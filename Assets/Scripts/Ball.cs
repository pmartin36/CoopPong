using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ball : MonoBehaviour
{
	private float BaseSpeed = 10;

	private MovementData _movementData;
	public float Rotation {
		get {
			return _movementData.Rotation;
		}
	}

	private List<MovementData> flightCollisions;
	private int collisionsSinceLastPlayerCollision = 0;

	private LayerMask collidableLayermask;
	private LayerMask targetLayermask;
	private LayerMask collidableAndTargetLayermask;

	private CircleCollider2D ccollider;
	private float castRadius;

	private Vector3 lastPosition;
	private RaycastHit2D hitBetweenLastMove;

	private AimAssist AimAssist;
	private bool aimAssistNeeded = true;

	// Start is called before the first frame update
	void Start()
    {
		ccollider = GetComponent<CircleCollider2D>();
		castRadius = ccollider.radius * transform.localScale.x;

		GenerateRandomPositionAndDirection();

		collidableLayermask = 1 << LayerMask.NameToLayer("Collidable");
		targetLayermask = 1 << LayerMask.NameToLayer("Target");
		collidableAndTargetLayermask = collidableLayermask |  targetLayermask;

		aimAssistNeeded = false;
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
		AimAssist = AimAssist.None;
		_movementData = new MovementData(
			position: new Vector3(0f, 0.75f, 0),
			//position: new Vector3(0f, 0.75f, 0),
			movementDirection: new Vector3(16f, 9f, 0).normalized,
			moveSpeed: BaseSpeed,
			rotation: 0,
			curve: 0,
			curveDirection: Vector2.zero,
			isCurving: false);
		transform.position = _movementData.Position;

		//transform.position = new Vector2(0, Random.Range(-8f, 8f));
		//movementDirection = new Vector2(Random.Range(0.5f, 1f), Random.Range(0f, 0.4f)).normalized;
		//Rotation = Random.Range(-90f, 90f);

		GetFlightPath(null);
	}

	private List<MovementData> GetFlightPath(GameObject origin) {			
		MovementData md = new MovementData(_movementData);

		List<MovementData> flightData = new List<MovementData>();
		float radius =  castRadius * (AimAssist == AimAssist.None ? 1 :
							AimAssist == AimAssist.Light ? 4f : 6f);

		RaycastHit2D closestLineHit = new RaycastHit2D();
		bool waitingOnLastTargetHit = false;

		flightData.Add(new MovementData(md));

		int i = 0;
		int last_i = 0;
		while (i < 25 || Mathf.Abs(md.Position.x) < 20f) { // TODO: Hardcoded x position?
			var tempPosition = md.Position;
			md.Update(Time.fixedDeltaTime);

			var hits = Physics2D.CircleCastAll(tempPosition, radius, md.ActualMovementDirection, md.ActualMoveSpeed, collidableAndTargetLayermask);
			var filteredHits = hits.Where( hit => hit.collider != null && hit.collider.gameObject != origin ).ToArray();
			foreach (RaycastHit2D hit in filteredHits) {
				if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Collidable")) {
					Vector2 actualMoveDirection = md.ActualMovementDirection;
					Vector2 normal = hit.normal;
					float dot = Vector2.Dot(actualMoveDirection, normal);				
					if (dot < 0) {
						// I would like to not have to do this overlap circle but can't seem to get it to work with just math
						var oc = Physics2D.OverlapCircle(md.Position, castRadius, collidableLayermask);
						if(oc != null) {
							md.HandleNonPlayerCollision(dot, normal);
							md.CalculateCurve();
							flightData.Add(new MovementData(md));
							last_i = i;
						}
					}
				}
				else if(aimAssistNeeded && hit.collider.gameObject.layer == LayerMask.NameToLayer("Target")) {
					// determine how much we'd need to move over in order to hit target
					var linehit = Physics2D.Linecast(md.Position, hit.transform.position, targetLayermask);

					if ( closestLineHit.collider == null || linehit.distance < closestLineHit.distance ) {
						closestLineHit = linehit;
						waitingOnLastTargetHit = true;
					}
				}
			}

			Debug.DrawLine(tempPosition, md.Position, aimAssistNeeded ? Color.green : Color.cyan, 3f);

			if (filteredHits.Length < 1 && waitingOnLastTargetHit) {
				waitingOnLastTargetHit = false;

				if(closestLineHit.distance > castRadius) {
					MovementData lastMd = flightData.Last();
					Vector2 lastPosition = lastMd.Position;
					float angle = Vector2.SignedAngle(md.Position - lastPosition, closestLineHit.point - lastPosition);
					if(Mathf.Abs(angle) < 15f) {
						lastMd.MovementDirection = lastMd.MovementDirection.Rotate(angle * 1.25f);
						lastMd.CalculateCurve();
						md = new MovementData(lastMd);			

						i = last_i;
						last_i = 0;
						aimAssistNeeded = false;
					}
				}
			} else {
				i++;
			}	
		}

		if(md.Position != flightData.Last().Position) {
			flightData.Add(new MovementData(md));
		}

		return flightData;
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
				aimAssistNeeded = AimAssist != AimAssist.None;

				_movementData.CalculateCurve();
				flightCollisions = GetFlightPath(player.gameObject);
				if(flightCollisions[0].MovementDirection != _movementData.MovementDirection) {
					_movementData = flightCollisions[0];
				}
				collisionsSinceLastPlayerCollision = 0;

				var lm = GameManager.Instance.LevelManager;
				var otherPlayer = player.Side == PlayerSide.Left ? lm.RightPlayer : lm.LeftPlayer;
				if(!otherPlayer.PlayerControlled) {
					otherPlayer.GoToLocation(
						flightCollisions.Last().Position
						//flightCollisions.FindLast( p => Mathf.Abs(p.Position.x) - 16f < 0.1f ).Position
					);
				}
			} else {
				collisionsSinceLastPlayerCollision++;
				if( flightCollisions != null && flightCollisions.Count > collisionsSinceLastPlayerCollision ) {
					_movementData.HandleNonPlayerCollision(dot, normal, flightCollisions[collisionsSinceLastPlayerCollision]);
				} 
				else {
					_movementData.HandleNonPlayerCollision(dot, normal);
				}		
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
