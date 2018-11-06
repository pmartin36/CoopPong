using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AimAssist {
	None,
	Light,
	Heavy
}

public struct FlightData {
	public Vector3 position;
	public RaycastHit2D[] hit;

	public FlightData(Vector3 p, RaycastHit2D[] h) {
		position = p;
		hit = h;
	}
}

public class MovementData {
	public Vector3 MovementDirection { get; set; }
	public float MoveSpeed { get; set; }

	public float Rotation { get; set; }
	public float Curve { get; set; }
	public Vector3 CurveDirection { get; set; }

	public bool IsCurving { get; set; }

	public MovementData(Vector3 movementDirection, float moveSpeed, float rotation, float curve, Vector3 curveDirection, bool isCurving) {
		MovementDirection = movementDirection;
		MoveSpeed = moveSpeed;
		Rotation = rotation;
		Curve = curve;
		CurveDirection = curveDirection;
		IsCurving = isCurving;
	}
}

public class Ball : MonoBehaviour
{
	Vector3 movementDirection;
	private float BaseSpeed = 10;

	[HideInInspector]
	public float MoveSpeed;
	[HideInInspector]
	public float Rotation;

	private float curve;
	private Vector3 curveDirection;
	private bool isCurving;

	private MovementData _movementData;

	private LayerMask collidableLayermask;
	private LayerMask collidableAndTargetLayermask;
	private CircleCollider2D ccollider;
	private float castRadius;

	private Vector3 lastPosition;
	private RaycastHit2D hitBetweenLastMove;

	private AimAssist AimAssist;
	private bool AlreadyAimAssisted = true;

	// Start is called before the first frame update
	void Start()
    {
		ccollider = GetComponent<CircleCollider2D>();
		castRadius = ccollider.radius * transform.localScale.x;
		GenerateRandomPositionAndDirection();
		collidableLayermask = 1 << LayerMask.NameToLayer("Collidable");
		collidableAndTargetLayermask = collidableLayermask | ( 1 << LayerMask.NameToLayer("Target") );
	}

    // Update is called once per frame
    void FixedUpdate()
    {
		transform.position = UpdatePosition(ref Rotation, ref curve, transform.position, MoveSpeed, Time.fixedDeltaTime, movementDirection, isCurving, curveDirection);
		Debug.DrawLine(lastPosition, transform.position, Color.red, 2f);

		transform.Rotate(0, 0, Rotation * Time.fixedDeltaTime);
		hitBetweenLastMove = Physics2D.Linecast(lastPosition, transform.position, collidableLayermask);
		lastPosition = transform.position;
	}

	private Vector3 UpdatePosition(ref float rotation, ref float cur, Vector3 incomingPosition, float ms, float timestep, Vector3 mDir, bool is_curving, Vector3 cd) {
		rotation -= (rotation * 0.4f * timestep);
		cur += rotation / 200f * timestep;
		Vector3 curveToAdd = is_curving ? cur * cd : Vector3.zero;
		
		return incomingPosition + (mDir * ms + curveToAdd) * timestep;
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

	private List<FlightData> GetFlightPath(Collider2D originPlayer) {
		Vector3 position = transform.position;
		Vector3 mDir = movementDirection;
		float ms = MoveSpeed;
				
		bool aimAssistNeeded = true;

		float rotation = Rotation;
		float curv = curve;
		bool is_curv = isCurving;
		Vector3 curveDir = curveDirection;

		List<FlightData> flightData = new List<FlightData>();

		float radius = ccollider.radius * transform.lossyScale.x;
		radius *= AimAssist == AimAssist.None ? 1 :
				  AimAssist == AimAssist.Light ? 1.5f : 2;

			
		int i = 0;
		while (i < 100) { // TODO: Hardcoded x position?
			var tempPosition = position;
			position = UpdatePosition(ref rotation, ref curv, position, ms, Time.fixedDeltaTime, mDir, is_curv, curveDir);

			var hits = Physics2D.CircleCastAll(position, radius, Vector2.zero, 0, collidableAndTargetLayermask);
			foreach (RaycastHit2D hit in hits) {
				if (hit.collider == null || hit.collider == originPlayer) continue;

				
			}

			Debug.DrawLine(tempPosition, position, Color.green, 2f);


			i++;
		}

		return flightData;
	}

	private void PerformAimAssist() {	
		if(AimAssist != AimAssist.None) {

		}
	}

	private Vector3 HandleNonPlayerCollision(float dot, Vector3 normal, Vector3 inc_md, ref float ms) {
		Vector3 new_md = inc_md - 2 * dot * normal;
		if (Mathf.Abs(movementDirection.x) < 0.25f) {
			movementDirection = (movementDirection + Mathf.Sign(transform.position.x) * Vector3.left).normalized;
		}

		ms += 0.25f;
		return new_md;
	}

	private Vector3 CalculateCurve(Vector3 inc_md, float rot, ref bool is_curv, ref float curv) {
		Vector3 cd = inc_md.Rotate(90);
		if (Mathf.Sign(cd.x * rot) * Mathf.Sign(inc_md.x) < 0) {
			curveDirection.x = 0;
		}
		is_curv = Mathf.Abs(Vector2.Dot(cd, inc_md)) < 0.5f;
		curv = 0;
		return cd;
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

				MoveSpeed += player.GetMSDelta(point, actualMoveDirection);
				if (AimAssist != AimAssist.None) {
					AlreadyAimAssisted = false;
				}

			} else {
				movementDirection = HandleNonPlayerCollision(dot, normal, actualMoveDirection, ref MoveSpeed);
			}

			curveDirection = CalculateCurve(movementDirection, Rotation, ref isCurving, ref curve);

			if (collision.collider.tag == "Player" && Mathf.Abs(normal.x) > 0.1f) {
				GetFlightPath(collision.collider);
				PerformAimAssist();
			}
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
