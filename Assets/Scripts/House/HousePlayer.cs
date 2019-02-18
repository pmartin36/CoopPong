using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HousePlayer : MonoBehaviour
{
	private static float BaseHeight = -1.25f;
	private Rigidbody2D rigid;
	public static event EventHandler NewLeadPlayer;

	public bool LeadPlayer { get; set; } = true;

	public float RotationDelta { get; private set; }
	public Vector3 MovementDelta { get; private set; }

	private MeshRenderer board;
	public Color BoardColor { 
		get => board.material.color;
		set => board.material.color = value;
	}

	public PlayerStyle Style { get; private set; }

	float forcePositionOffsetFromCenter;
	private Vector3 lastPosition;

	private CapsuleCollider2D capsuleCollider;
	private BoxCollider2D paddleCollider;
	private Coroutine pushPaddleCoroutine;

	public void Awake() {
		board = GetComponent<MeshRenderer>();
		rigid = GetComponent<Rigidbody2D>();
		capsuleCollider = GetComponent<CapsuleCollider2D>();
		forcePositionOffsetFromCenter = capsuleCollider.bounds.extents.x;
		lastPosition = transform.position;

		foreach(Transform child in transform) {
			paddleCollider = child.GetComponent<BoxCollider2D>();
		}
	}

	public void Start() {
		NewLeadPlayer?.Invoke(this, null);
	}

	public void SetStyle(PlayerStyle s) {
		Style = s;
		BoardColor = s.BoardColor;
	}

	public void Update() {
		transform.position = new Vector3(transform.position.x, transform.position.y, BaseHeight +  0.1f * Mathf.Sin(Time.time * 2));
	}

	public void HandleInput(float vertical, float horizontal, bool enter, bool boost, bool ctrl) {	
		if(enter) {
			var targetLayerMask = 1 << LayerMask.NameToLayer("Target");
			if (LeadPlayer) {
				Collider2D hit = Physics2D.OverlapCapsule(transform.position, capsuleCollider.size, CapsuleDirection2D.Horizontal, transform.eulerAngles.z, targetLayerMask);
				if(hit != null) {
					GameManager.Instance.HouseManager.EnterLevel(hit.GetComponent<HouseEnterLevel>().Level);
					return;
				}
			}

			Vector2 tangent = Utils.AngleToVector(transform.eulerAngles.z).Rotate(90);
			Vector2 scaledTangent = tangent * capsuleCollider.size.y * 1.2f;
			// capsuleCast seems bugged, using BoxCast instead
			RaycastHit2D rayHit = Physics2D.BoxCast(
				(Vector2)paddleCollider.transform.position - (scaledTangent / 2f), 
				paddleCollider.size/2f, 
				transform.eulerAngles.z, 
				scaledTangent, 
				scaledTangent.magnitude, 
				~(targetLayerMask | 1 << LayerMask.NameToLayer("Player")));

			Debug.DrawLine(
				(Vector2)transform.position - (scaledTangent /2f), 
				(Vector2)transform.position + (scaledTangent / 2f),
				Color.red,
				1f);
			if (rayHit.collider != null) {			
				var dir = rayHit.normal;
				rigid.AddForceAtPosition(dir * 250f, transform.position);

				if(pushPaddleCoroutine != null) StopCoroutine(pushPaddleCoroutine);
				pushPaddleCoroutine = StartCoroutine(
					PushPaddleAndPullBack(
						Mathf.Sign(Vector2.Dot(tangent, rayHit.centroid)), 
						((Vector2)paddleCollider.transform.position - rayHit.centroid).magnitude));
			}
		}

		var angleDegree = transform.rotation.eulerAngles.z;
		Vector3 angleVector = Utils.AngleToVector(angleDegree);
		Vector3 forcePosition = transform.position + angleVector * Mathf.Sign(vertical) * -forcePositionOffsetFromCenter;

		float frontThrustAmount = 1000 * (boost ? 1.7f : 1f);
		float turnThrustAmount = 75 * (boost ? 1.7f : 1f);

		// vertical
		Vector3 forceAdded = vertical * angleVector * 1000f * Time.deltaTime;
		Debug.DrawRay(forcePosition, angleVector, Color.green, 0.5f);

		// horizontal
		forceAdded += angleVector.Rotate(90) * horizontal * 75f * Time.deltaTime;

		if(boost) {
			forceAdded *= 1.7f;
			BoardColor *= new Color(1, 0.95f, 0.95f);
		}
		else {
			BoardColor = Style.BoardColor;
		}
		rigid.AddForceAtPosition(forceAdded, forcePosition);

		if (rigid.velocity.magnitude > 8) {
			rigid.velocity = rigid.velocity.normalized * 8f;
		}

		MovementDelta = transform.position - lastPosition;
		lastPosition = transform.position;
	}

	IEnumerator PushPaddleAndPullBack(float direction, float distance) {
		Vector3 targetLocalCentroid = new Vector3(0, -direction * distance, paddleCollider.transform.localPosition.z);
		Vector3 directionVector = (targetLocalCentroid.y * Vector3.up).normalized;
		var paddle = paddleCollider.transform;	
		Vector2 last = transform.position;

		float time = 0;
		var lsm = ((Vector2)paddle.localPosition).sqrMagnitude;
		var start = paddle.localPosition;
		var modifiedStart = paddle.localPosition;
		while (lsm < 1) {
			Vector3 diff = ((Vector2)transform.position - last) * directionVector;
			targetLocalCentroid += diff;
			modifiedStart += diff;

			paddle.localPosition = Vector3.Lerp(modifiedStart, targetLocalCentroid, time * 10);

			last = transform.position;
			lsm = ((Vector2)paddle.localPosition).sqrMagnitude;

			time += Time.deltaTime;
			yield return null;
		}

		time = 0f;
		var end = start;
		start = paddle.localPosition;
		while (time < 1f) {
			paddle.localPosition = Vector3.Lerp(start, end, time);
			time += Time.deltaTime;
			yield return null;
		}
		paddle.localPosition = end;
	}
}
