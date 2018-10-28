using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
	Vector3 movementDirection;
	private float BaseSpeed = 20;
	public float MoveSpeed { get; set; }
	public float Rotation { get; set; }

	private float curve;
	private Vector3 curveDirection;
	private bool isCurving;

	private LayerMask collidableLayermask;
	private CircleCollider2D ccollider;
	private float castRadius;

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
		curve += Rotation / 2000f * MoveSpeed * Time.deltaTime;
		Vector3 curveToAdd =  isCurving ? curve * curveDirection : Vector3.zero;
		transform.Rotate(0, 0, Rotation * Time.fixedDeltaTime);

		var idealMove = (movementDirection * MoveSpeed + curveToAdd) * Time.fixedDeltaTime;
		transform.position += idealMove;

		//Vector3 actualDir = idealMove.normalized;
		//float actualMS = idealMove.magnitude;
		//RaycastHit2D hit = Physics2D.CircleCast(transform.position, castRadius, actualDir, actualMS, collidableLayermask);
		//if (hit.collider == null) {
		//	transform.position += idealMove;
		//}
		//else {
		//	Debug.Log($"About to collide @{Time.time}");
		//	//transform.position += actualDir * hit.distance;
		//	HandleCollision(hit, actualDir, actualMS);
		//}
	}

	public void HandleCollision(RaycastHit2D hit, Vector3 actualMoveDirection, float actualMS) {
		Vector3 normal = hit.normal;
		float dot = Vector2.Dot(actualMoveDirection, normal);
		var inc = actualMoveDirection; // For debug
		Debug.Log($"Collided with {hit.collider.name}, normal: { normal}, incoming: {inc}, dot: { dot}, new: {movementDirection}");

		if (dot < 0) {	
			if (Mathf.Min(normal.x, normal.y) > 0.01f && dot < -0.90f) {
				actualMoveDirection *= -1f;
			}
			else {
				movementDirection = actualMoveDirection - 2 * dot * normal;
			}

			if (Mathf.Abs(movementDirection.x) < 0.25f) {
				Debug.Log("Artifically adding x direction");
				movementDirection = (movementDirection + Mathf.Sign(transform.position.x) * Vector3.left).normalized;
			}

			if (hit.collider.tag == "Player") {
				var player = hit.collider.GetComponent<Player>();
				Rotation += player.YMove * 60 * -Mathf.Sign(movementDirection.x);

				// curveDirection = movementDirection.Rotate(-90 * Mathf.Sign(movementDirection.x));
				curveDirection = new Vector3(0, Mathf.Sign(movementDirection.x));
			}

			curve = 0;
			float absDot = Mathf.Abs(Vector2.Dot(curveDirection, movementDirection));
			isCurving = absDot < 0.7f;
			Debug.Log("Rotation: {Rotation}, AbsDot: {absDot}, Rotation: {isCurving}");

			transform.position += actualMoveDirection * (hit.distance);
			MoveSpeed += 0.25f;
		}
	}

	private void GenerateRandomPositionAndDirection() {
		MoveSpeed = BaseSpeed;
		transform.position = new Vector2(0, Random.Range(-8f, 8f));
		movementDirection = new Vector2(Random.Range(0.5f, 1f), Random.Range(0f, 0.4f)).normalized;
		Rotation = Random.Range(-90f, 90f);
	}

	public void OnCollisionEnter2D(Collision2D collision) {
		Vector3 normal = collision.GetContact(0).normal;

		Vector3 curveToAdd = isCurving ? curve * curveDirection : Vector3.zero;
		Vector3 actualMoveDirection = (movementDirection * MoveSpeed + curveToAdd).normalized;

		float dot = Vector2.Dot(actualMoveDirection, normal);

		if (dot < 0) {
			var inc = actualMoveDirection; // For debug
			if (Mathf.Min(normal.x, normal.y) > 0.01f && dot < -0.90f) {
				actualMoveDirection *= -1f;
			}
			else {
				movementDirection = actualMoveDirection - 2 * dot * normal;
			}

			if (Mathf.Abs(movementDirection.x) < 0.25f) {
				Debug.Log("Artifically adding x direction");
				movementDirection = (movementDirection + Mathf.Sign(transform.position.x) * Vector3.left).normalized;
			}
			Debug.Log($"normal: {normal}, incoming: {inc}, dot: {dot}, new: {movementDirection}");

			if (collision.collider.tag == "Player") {
				var player = collision.collider.GetComponent<Player>();
				Rotation += player.YMove * 60 * -Mathf.Sign(movementDirection.x);

				curveDirection = new Vector3(0, Mathf.Sign(movementDirection.x));
				Debug.Log($"Collided with {collision.collider.name} @{Time.time}, Rotation: {Rotation}, Rotation: {isCurving}");
			}

			curve = 0;
			isCurving = Mathf.Abs(Vector2.Dot(curveDirection, movementDirection)) < 0.5f;
			MoveSpeed += 0.25f;
		}
	}

	public void OnCollisionStay2D(Collision2D collision) {
		
	}

	public void OnTriggerEnter2D(Collider2D collision) {
		if (collision.tag == "ScoreZone") {
			GenerateRandomPositionAndDirection();
		}
	}
}
