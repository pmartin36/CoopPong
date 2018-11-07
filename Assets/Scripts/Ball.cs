using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct FlightData {
	public Vector3 position;
	public RaycastHit2D[] hit;

	public FlightData(Vector3 p, RaycastHit2D[] h) {
		position = p;
		hit = h;
	}
}

public class MovementData {
	public Vector3 Position { get; set; }
	public Vector3 MovementDirection { get; set; }
	public float MoveSpeed { get; set; }

	public float Rotation { get; set; }
	public float Curve { get; set; }
	public Vector3 CurveDirection { get; set; }

	public bool IsCurving { get; set; }

	public Vector3 ActualMovementDirection {
		get {
			Vector3 curveToAdd = IsCurving ? Curve * CurveDirection : Vector3.zero;
			return (MovementDirection * MoveSpeed + curveToAdd).normalized;
		}
	}
	public float ActualMoveSpeed { get; private set; }

	public MovementData() {

	}

	public MovementData(MovementData d) : this (  
		new Vector3(d.Position.x, d.Position.y),
		new Vector3(d.MovementDirection.x, d.MovementDirection.y),
		d.MoveSpeed,
		d.Rotation,
		d.Curve,
		new Vector3(d.CurveDirection.x, d.CurveDirection.y),
		d.IsCurving) { }

	public MovementData(Vector3 position, Vector3 movementDirection, float moveSpeed, float rotation, float curve, Vector3 curveDirection, bool isCurving) {
		Position = position;
		MovementDirection = movementDirection;
		MoveSpeed = moveSpeed;
		Rotation = rotation;
		Curve = curve;
		CurveDirection = curveDirection;
		IsCurving = isCurving;
	}

	public void Update(float timestep) {
		Rotation -= (Rotation * 0.4f * timestep);
		Curve += Rotation / 200f * timestep;
		Vector3 curveToAdd = IsCurving ? Curve * CurveDirection : Vector3.zero;

		Vector3 delta = (MovementDirection * MoveSpeed + curveToAdd) * timestep;

		Position += delta;
		ActualMoveSpeed = delta.magnitude;
	}

	public void CalculateCurve() {
		Vector3 cd = MovementDirection.Rotate(90);
		if (Mathf.Sign(cd.x * Rotation) * Mathf.Sign(MovementDirection.x) < 0) {
			cd.x = 0;
		}
		CurveDirection = cd;
		IsCurving = Mathf.Abs(Vector2.Dot(cd, MovementDirection)) < 0.5f;
		Curve = 0;
	}

	public void HandleNonPlayerCollision(float dot, Vector3 normal) {
		MovementDirection = ActualMovementDirection - 2 * dot * normal;
		if (Mathf.Abs(MovementDirection.x) < 0.25f) {
			MovementDirection = (MovementDirection + Mathf.Sign(Position.x) * Vector3.left).normalized;
		}

		MoveSpeed += 0.25f;
	}
}

public class Ball : MonoBehaviour
{
	private float BaseSpeed = 10;

	private MovementData _movementData;
	public float Rotation {
		get {
			return _movementData.Rotation;
		}
	}

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
		_movementData.Update(Time.fixedDeltaTime);
		transform.position = _movementData.Position;
		Debug.DrawLine(lastPosition, transform.position, Color.red, 2f);

		transform.Rotate(0, 0, Rotation * Time.fixedDeltaTime);
		hitBetweenLastMove = Physics2D.Linecast(lastPosition, transform.position, collidableLayermask);
		lastPosition = transform.position;
	}

	private void GenerateRandomPositionAndDirection() {
		_movementData = new MovementData(
			position: Vector3.zero,
			movementDirection: Vector3.right,
			moveSpeed: BaseSpeed,
			rotation: 0,
			curve: 0,
			curveDirection: Vector2.zero,
			isCurving: false);
		transform.position = _movementData.Position;

		AlreadyAimAssisted = true;

		//transform.position = new Vector2(0, Random.Range(-8f, 8f));
		//movementDirection = new Vector2(Random.Range(0.5f, 1f), Random.Range(0f, 0.4f)).normalized;
		//Rotation = Random.Range(-90f, 90f);
	}

	private List<FlightData> GetFlightPath(GameObject origin) {			
		bool aimAssistNeeded = true;
		MovementData md = new MovementData(_movementData);

		List<FlightData> flightData = new List<FlightData>();
		float radius =  castRadius * (AimAssist == AimAssist.None ? 1 :
							AimAssist == AimAssist.Light ? 1.5f : 2);
			
		int i = 0;
		while (i < 25 || Mathf.Abs(md.Position.x) < 15.8f) { // TODO: Hardcoded x position?
			var tempPosition = md.Position;
			md.Update(Time.fixedDeltaTime);

			var hits = Physics2D.CircleCastAll(tempPosition, radius, md.ActualMovementDirection, md.ActualMoveSpeed, collidableAndTargetLayermask);
			var filteredHits = hits.Where( hit => hit.collider != null && hit.collider.gameObject != origin && hit.collider.transform.parent?.gameObject != origin ).ToArray();
			foreach (RaycastHit2D hit in filteredHits) {
				Vector2 actualMoveDirection = md.ActualMovementDirection;
				Vector2 normal = hit.normal;
				float dot = Vector2.Dot(actualMoveDirection, normal);				
				if (dot < 0) {
					// I would like to not have to do this overlap circle but can't seem to get it to work with just math
					var oc = Physics2D.OverlapCircle(md.Position, castRadius, collidableAndTargetLayermask);
					if(oc != null) {
						md.HandleNonPlayerCollision(dot, normal);
						md.CalculateCurve();
					}
				}
			}		
			Debug.DrawLine(tempPosition, md.Position, Color.green, 5f);

			if(filteredHits.Length > 0) {
				flightData.Add(new FlightData(md.Position, hits));
			}
			i++;
		}

		return flightData;
	}

	private void PerformAimAssist(List<FlightData> fd) {	
		if(AimAssist != AimAssist.None) {

		}
	}

	public void OnCollisionEnter2D(Collision2D collision) {
		ContactPoint2D cp = collision.GetContact(0);
		Vector3 normal = hitBetweenLastMove.collider == collision.collider ? hitBetweenLastMove.normal : cp.normal;

		Vector3 actualMoveDirection = _movementData.ActualMovementDirection;

		float dot = Vector2.Dot(actualMoveDirection, normal);

		var inc = actualMoveDirection; // For debug	

		if (dot < 0) {
			if (collision.collider.tag == "Player" && Mathf.Abs(normal.x) > 0.1f ) {
				var player = collision.collider.GetComponentInParent<Player>();
				Vector3 point = hitBetweenLastMove.collider == collision.collider ? hitBetweenLastMove.point : cp.point;
				_movementData.MovementDirection = player.GetBallTrajectory(point, actualMoveDirection);

				_movementData.Rotation += player.YMove * 6 * _movementData.MoveSpeed * -Mathf.Sign(_movementData.MovementDirection.x) * player.GetRotationModifier(point, actualMoveDirection);

				_movementData.MoveSpeed += player.GetMSDelta(point, actualMoveDirection);
				AimAssist = player.AimAssist;
				if (AimAssist != AimAssist.None) {
					AlreadyAimAssisted = false;
				}

				_movementData.CalculateCurve();
				List<FlightData> fd = GetFlightPath(player.gameObject);
				PerformAimAssist(fd);
			} else {
				_movementData.HandleNonPlayerCollision(dot, normal);
				_movementData.CalculateCurve();
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
