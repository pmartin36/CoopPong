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

	private List<MovementData> projectedFlight;
	private List<int> expectedCollisionIndices;
	private int projectedFlightIndex = 0;

	private LayerMask collidableLayermask;
	private LayerMask targetLayermask;
	private LayerMask playerLayermask;
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

		collidableLayermask = 1 << LayerMask.NameToLayer("Collidable");
		targetLayermask = 1 << LayerMask.NameToLayer("Target");
		playerLayermask = 1 << LayerMask.NameToLayer("Player");
		collidableAndTargetLayermask = collidableLayermask |  targetLayermask;

		GenerateRandomPositionAndDirection();
	}

    // Update is called once per frame
    void FixedUpdate()
    {
		if(projectedFlight != null && projectedFlight.Count > projectedFlightIndex - 1) {
			projectedFlightIndex++;
			_movementData = projectedFlight[projectedFlightIndex];
			
			var nextPosition = _movementData.Position;
			transform.Rotate(0, 0, Rotation * Time.fixedDeltaTime);
			hitBetweenLastMove = Physics2D.Linecast(lastPosition, nextPosition, playerLayermask);
			
			if(hitBetweenLastMove.collider == null && expectedCollisionIndices.Count > 0 && expectedCollisionIndices[0]+1 == projectedFlightIndex) {
				Debug.Log($"Expected collision at {expectedCollisionIndices[0]} but none occured - recalculating");
				if(projectedFlightIndex > 1)
					_movementData = projectedFlight[projectedFlightIndex-2];
				projectedFlight = GetFlightPath(null, AimAssist.None);
			}
			else {
				transform.position = nextPosition;
				Debug.DrawLine(lastPosition, transform.position, Color.red, 0.5f);
				lastPosition = transform.position;
			}
		}
	}

	private void GenerateRandomPositionAndDirection() {
		Vector3 position = new Vector2(0, Random.Range(-8f, 8f));
		while (Physics2D.OverlapCircle(position, castRadius, collidableLayermask) != null) {
			position = new Vector2(0, Random.Range(-8f, 8f));
		}

		_movementData = new MovementData(
			position: position,
			movementDirection: new Vector2(Random.Range(0.5f, 1f), Random.Range(-0.4f, 0.4f)).normalized,
			moveSpeed: BaseSpeed,
			rotation: 0,
			curve: 0,
			curveDirection: Vector2.zero,
			isCurving: false);

		//transform.position = new Vector2(0, Random.Range(-8f, 8f));
		//movementDirection = new Vector2(Random.Range(0.5f, 1f), Random.Range(0f, 0.4f)).normalized;
		//Rotation = Random.Range(-90f, 90f);

		projectedFlight = GetFlightPath(null, AimAssist.None);
		transform.position = projectedFlight.First().Position;
	}

	private List<MovementData> GetFlightPath(GameObject origin, AimAssist aimAssist) {
		projectedFlightIndex = 0;
		expectedCollisionIndices = new List<int>();
		MovementData md = new MovementData(_movementData);

		List<MovementData> flightData = new List<MovementData>();
		float radius =  castRadius * (aimAssist == AimAssist.None ? 1 :
							aimAssist == AimAssist.Light ? 4f : 6f);

		// must pass fully through the target to minimum aim assist adjustment
		// waitingOnLastTargetHit = have we passed fully through a target?[
		// closestLineHit = linecast with the minimum distance to the target
		bool waitingOnLastTargetHit = false;
		RaycastHit2D closestLineHit = new RaycastHit2D();

		flightData.Add(new MovementData(md));

		int i = 1;
		int lastCollisionIndex = 0; 
		while (Mathf.Abs(md.Position.x) < 20f) { // TODO: Hardcoded x position?
			var tempPosition = md.Position;
			md.Update(Time.fixedDeltaTime);

			var hits = Physics2D.CircleCastAll(tempPosition, radius, md.ActualMovementDirection, md.ActualMoveSpeed, collidableAndTargetLayermask);
			var filteredHits = hits.Where( hit => hit.collider != null ).ToArray();
			if(filteredHits.Length > 0) {

			}
			foreach (RaycastHit2D hit in filteredHits) {
				if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Collidable")) {
					Vector2 actualMoveDirection = md.ActualMovementDirection;
					Vector2 normal = hit.normal;
					float dot = Vector2.Dot(actualMoveDirection, normal);				
					if (dot < 0) {
						// I would like to not have to do this overlap circle but can't seem to get it to work with just math
						var oc = Physics2D.OverlapCircle(md.Position, castRadius, collidableLayermask);
						if(oc != null) {
							expectedCollisionIndices.Add(i);
							md.HandleNonPlayerCollision(dot, normal);
							md.CalculateCurve();
							flightData.Add(new MovementData(md));
							lastCollisionIndex = i;
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

			//Debug.DrawLine(tempPosition, md.Position, aimAssistNeeded ? Color.green : Color.cyan, i / 50f);
			Debug.DrawLine(tempPosition, md.Position, aimAssistNeeded ? Color.green : Color.cyan, 1 + (i/50f));

			// if we've passed through a target fully and we will be attempting aim assist
			if (filteredHits.Length < 1 && waitingOnLastTargetHit) {
				waitingOnLastTargetHit = false;

				if(closestLineHit.distance > castRadius) {
					MovementData lastMd = flightData[lastCollisionIndex];
					Vector2 lastPosition = lastMd.Position;
					float angle = Vector2.SignedAngle(md.Position - lastPosition, closestLineHit.point - lastPosition) * 1.25f;
					if(Mathf.Abs(angle) < 15f) {
						// adjust angle and recalculate
						// remove all the points we've added since the last collision and reset md to that last collision point
						lastMd.MovementDirection = lastMd.MovementDirection.Rotate(angle);
						lastMd.CalculateCurve();
						md = new MovementData(lastMd);			

						int startRangeRemove = lastCollisionIndex + 1;
						flightData.RemoveRange(startRangeRemove, flightData.Count - startRangeRemove);
						i = startRangeRemove;
						lastCollisionIndex = 0;
						aimAssistNeeded = false;
					}
				}
			} else {
				flightData.Add(new MovementData(md));
				i++;
			}	
		}

		// FOR DEBUG
		//string s = "";
		//foreach (var index in expectedCollisionIndices) {
		//	s += index + ", ";
		//}
		//if (s.Length > 0) Debug.Log(s);

		Vector3 lastPositionOfFlight = flightData.Last().Position;
		if (Mathf.Abs(lastPositionOfFlight.x) > 16f) {
			var lm = GameManager.Instance.LevelManager;
			Player targetedPlayer = lastPositionOfFlight.x < 0f ? lm.LeftPlayer : lm.RightPlayer;
			if (!targetedPlayer.PlayerControlled) {
				Vector3 poi = flightData.FindLast(p => Mathf.Abs(p.Position.x) - 15.9f < 0.1f).Position;
				targetedPlayer.GoToLocation(poi);
			}
			if(!targetedPlayer.OtherPlayer.PlayerControlled) {
				Vector3 poi = new Vector3(targetedPlayer.OtherPlayer.transform.position.x, Random.Range(-1f, 1f));
				targetedPlayer.OtherPlayer.GoToLocation(poi);
			}
		}

		return flightData;
	}

	public void OnCollisionEnter2D(Collision2D collision) {
		ContactPoint2D cp = collision.GetContact(0);
		Vector3 normal = hitBetweenLastMove.collider == collision.collider ? hitBetweenLastMove.normal : cp.normal;

		Vector3 actualMoveDirection = _movementData.ActualMovementDirection;
		float dot = Vector2.Dot(actualMoveDirection, normal);

		// Debug.Log($"Collision at: {projectedFlightIndex}");

		if (collision.collider.tag == "Player") {
			if (dot < 0 && Mathf.Abs(normal.x) > 0.1f) {
				var player = collision.collider.GetComponentInParent<Player>();
				Vector3 point = hitBetweenLastMove.collider == collision.collider ? hitBetweenLastMove.point : cp.point;
				_movementData.MovementDirection = player.GetBallTrajectory(point, actualMoveDirection);

				_movementData.Rotation += player.YMove * 6 * _movementData.MoveSpeed * -Mathf.Sign(_movementData.MovementDirection.x) * player.GetRotationModifier(point, actualMoveDirection);

				_movementData.MoveSpeed += player.GetMSDelta(point, actualMoveDirection);
				AimAssist = player.AimAssist;
				aimAssistNeeded = AimAssist != AimAssist.None;

				_movementData.CalculateCurve();
				projectedFlight = GetFlightPath(player.gameObject, AimAssist);
				if (projectedFlight[0].MovementDirection != _movementData.MovementDirection) {
					_movementData = projectedFlight[0];
				}
			}
		}
		else if (collision.gameObject.layer == LayerMask.NameToLayer("Collidable")) {
			if(expectedCollisionIndices.Count < 1 || expectedCollisionIndices[0] != projectedFlightIndex) {
				// if there is an unexpected collision, recalculate trajectory
				Debug.Log($"Unexpected collision ({projectedFlightIndex} instead of {(expectedCollisionIndices.Count > 0 ? expectedCollisionIndices[0].ToString() : "unavailable")}), recalculate trajectory");
				_movementData.HandleNonPlayerCollision(dot, normal);
				projectedFlight = GetFlightPath(collision.gameObject, AimAssist);
			}
			else {
				expectedCollisionIndices.RemoveAt(0);
			}
		}

		// Debug.Log($"normal: {normal}, incoming: {inc}, dot: {dot}, new: {movementDirection}");
	}

	public void OnCollisionStay2D(Collision2D collision) {
		
	}

	public void OnTriggerEnter2D(Collider2D collision) {
		if (collision.tag == "ScoreZone") {
			GenerateRandomPositionAndDirection();
			GameManager.Instance.LevelManager.PlayerLifeLost();
		}
	}
}
