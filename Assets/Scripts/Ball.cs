using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AimAssistStatus {
	None,
	Light,
	Heavy
}

public class Ball : MonoBehaviour
{
	Vector3 movementDirection;
	private float BaseSpeed = 10;
	public float MoveSpeed { get; set; }
	public float Rotation { get; set; }

	private float curve;
	private Vector3 curveDirection;
	private bool isCurving;

	private LayerMask collidableLayermask;
	private CircleCollider2D ccollider;
	private float castRadius;

	private Vector3 lastPosition;
	private RaycastHit2D hitBetweenLastMove;

	private AimAssistStatus AimAssistType;
	private bool AlreadyAimAssisted = true;

	// Start is called before the first frame update
	void Start()
    {
		ccollider = GetComponent<CircleCollider2D>();
		castRadius = ccollider.radius * transform.localScale.x;
		GenerateRandomPositionAndDirection();
		collidableLayermask = 1 << LayerMask.NameToLayer("Collidable");
	}

    // Update is called once per frame
    void FixedUpdate()
    {
		Rotation -= (Rotation * 0.4f * Time.fixedDeltaTime);
		curve += Rotation / 200f * Time.deltaTime;
		Vector3 curveToAdd =  isCurving ? curve * curveDirection : Vector3.zero;
		transform.Rotate(0, 0, Rotation * Time.fixedDeltaTime);

		transform.position += (movementDirection * MoveSpeed + curveToAdd) * Time.fixedDeltaTime; ;

		hitBetweenLastMove = Physics2D.Linecast(lastPosition, transform.position, collidableLayermask);
		lastPosition = transform.position;
	}

	private void GenerateRandomPositionAndDirection() {
		MoveSpeed = BaseSpeed;
		curve = 0;

		transform.position = Vector3.zero;
		movementDirection = Vector3.right;
		Rotation = 0;

		AlreadyAimAssisted = true;

		//transform.position = new Vector2(0, Random.Range(-8f, 8f));
		//movementDirection = new Vector2(Random.Range(0.5f, 1f), Random.Range(0f, 0.4f)).normalized;
		//Rotation = Random.Range(-90f, 90f);
	}

	private void AimAssist() {

	}

	public void OnCollisionEnter2D(Collision2D collision) {
		ContactPoint2D cp = collision.GetContact(0);
		Vector3 normal = hitBetweenLastMove.collider == collision.collider ? hitBetweenLastMove.normal : cp.normal;

		Vector3 curveToAdd = isCurving ? curve * curveDirection : Vector3.zero;
		Vector3 actualMoveDirection = (movementDirection * MoveSpeed + curveToAdd).normalized;

		float dot = Vector2.Dot(actualMoveDirection, normal);

		var inc = actualMoveDirection; // For debug	

		if (dot < 0) {
			if (collision.collider.tag == "Player" && Mathf.Abs(normal.x) > 0.1f ) {
				var player = collision.collider.GetComponentInParent<Player>();
				Vector3 point = hitBetweenLastMove.collider == collision.collider ? hitBetweenLastMove.point : cp.point;
				movementDirection = player.GetBallTrajectory(point, actualMoveDirection);

				Rotation += player.YMove * 6 * MoveSpeed * -Mathf.Sign(movementDirection.x) * player.GetRotationModifier(point, actualMoveDirection);
				curveDirection = movementDirection.Rotate(90);
				if( Mathf.Sign(curveDirection.x * Rotation) * Mathf.Sign(movementDirection.x) < 0 ) {
					curveDirection.x = 0;
				}

				MoveSpeed += player.GetMSDelta(point, actualMoveDirection);
				if (AimAssistType != AimAssistStatus.None) {
					AlreadyAimAssisted = false;
				}

			} else {
				movementDirection = actualMoveDirection - 2 * dot * normal;
				if (Mathf.Abs(movementDirection.x) < 0.25f) {
					movementDirection = (movementDirection + Mathf.Sign(transform.position.x) * Vector3.left).normalized;
				}
	
				MoveSpeed += 0.25f;
			}

			isCurving = Mathf.Abs(Vector2.Dot(curveDirection, movementDirection)) < 0.5f;	
			curve = 0;
			AimAssist();
		}
		// Debug.Log($"normal: {normal}, incoming: {inc}, dot: {dot}, new: {movementDirection}");
	}

	public void OnCollisionStay2D(Collision2D collision) {
		
	}

	public void OnTriggerEnter2D(Collider2D collision) {
		if (collision.tag == "ScoreZone") {
			GenerateRandomPositionAndDirection();
		}
	}
}
