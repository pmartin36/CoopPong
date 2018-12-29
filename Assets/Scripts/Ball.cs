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
	private LayerMask playerAndColliderLayermask;

	private CircleCollider2D ccollider;
	private float ballRadius;

	private MovementData lastMovement;
	private RaycastHit2D hitBetweenLastMove;

	private AimAssist AimAssist;
	private bool aimAssistNeeded = true;

	private Collider2D lastUpdateCollider = null;

	// Start is called before the first frame update
	void Start()
    {
		ccollider = GetComponent<CircleCollider2D>();
		ballRadius = ccollider.radius * transform.localScale.x;

		collidableLayermask = 1 << LayerMask.NameToLayer("Collidable");
		targetLayermask = 1 << LayerMask.NameToLayer("Target");
		playerLayermask = 1 << LayerMask.NameToLayer("Player");
		playerAndColliderLayermask = collidableLayermask |  playerLayermask;

		GenerateRandomPositionAndDirection();
	}

    // Update is called once per frame
    void FixedUpdate()
    {
		if (projectedFlight != null && projectedFlight.Count > projectedFlightIndex - 1) {
			lastMovement = projectedFlight[projectedFlightIndex];
			Vector3 lastMoveDirection = lastMovement.ActualMovementDirection;

			projectedFlightIndex++;
			_movementData = projectedFlight[projectedFlightIndex];
			
			Vector3 nextPosition = _movementData.Position;
			transform.Rotate(0, 0, Rotation * Time.fixedDeltaTime);
			hitBetweenLastMove = Physics2D.CircleCastAll(transform.position, ballRadius, 
				lastMovement.ActualMovementDirection, lastMovement.ActualMoveSpeed, playerAndColliderLayermask).FirstOrDefault(h => h.collider != lastUpdateCollider);

			// Debug
			//for(int i = 0; i < 360; i+=30) {
			//	Debug.DrawLine(nextPosition, nextPosition + _movementData.ActualMovementDirection.Rotate(i) * ballRadius, projectedFlightIndex%2==0 ? Color.gray:Color.blue, 1f);
			//}

			if (hitBetweenLastMove.collider != null) {
				Collider2D collider = hitBetweenLastMove.collider;
				Vector3 normal = hitBetweenLastMove.normal;
				float dot = Vector2.Dot(lastMoveDirection, normal);
				if (collider.tag == "Player") {
					if (dot < 0 && Mathf.Abs(normal.x) > 0.1f) {
						var player = collider.GetComponentInParent<Player>();
						Vector3 point = hitBetweenLastMove.point;
						_movementData.MovementDirection = player.GetBallTrajectory(point, lastMoveDirection);

						Debug.Log(hitBetweenLastMove.centroid);
						_movementData.Position = hitBetweenLastMove.centroid;
						_movementData.Rotation += player.YMove * 6 * _movementData.MoveSpeed * -Mathf.Sign(_movementData.MovementDirection.x) * player.GetRotationModifier(point, lastMoveDirection);

						_movementData.MoveSpeed += player.GetMSDelta(point, lastMoveDirection);
						AimAssist = player.AimAssist;
						aimAssistNeeded = AimAssist != AimAssist.None;

						_movementData.CalculateCurve();
						projectedFlight = GetFlightPath(collider, AimAssist);
						if (projectedFlight[0].MovementDirection != _movementData.MovementDirection) {
							_movementData = projectedFlight[0];
						}
					}
				}
				else if (expectedCollisionIndices.Count < 1 || expectedCollisionIndices[0] != projectedFlightIndex) {
					// if there is an unexpected collision, recalculate trajectory
					Debug.Log($"Unexpected collision with {collider.gameObject.name} ({projectedFlightIndex} instead of {(expectedCollisionIndices.Count > 0 ? expectedCollisionIndices[0].ToString() : "unavailable")}), recalculate trajectory");

					IMoving m = collider.gameObject.GetComponent<IMoving>();
					Vector3 extra = Vector3.zero;
					if (m != null) {
						// verify going in same direction
						Vector3 diff = m.GetMovementAmount(nextPosition);
						float diffdot = Vector2.Dot(diff.normalized, lastMoveDirection);
						diff = diffdot < 0.5f ? diff : Vector3.zero;
						Debug.Log($"Adding {diff} with dot of {diffdot} and moveDirection of {lastMoveDirection}");
						_movementData.HandleNonPlayerCollision(dot, normal, diff, 0f, hitBetweenLastMove.centroid);	
					}
					else {
						_movementData.HandleNonPlayerCollision(dot, normal, extra, 0.25f, hitBetweenLastMove.centroid);
					}

					projectedFlight = GetFlightPath(collider, AimAssist);
				}
				else {
					expectedCollisionIndices.RemoveAt(0);
				}

				transform.position = _movementData.Position;
				lastUpdateCollider = collider;
			}
			else {
				if(expectedCollisionIndices.Count > 0 && expectedCollisionIndices[0] == projectedFlightIndex) {
					Debug.Log($"Expected collision at {expectedCollisionIndices[0]} but none occured - recalculating");
					_movementData = projectedFlight[projectedFlightIndex - 1];
					projectedFlight = GetFlightPath(null, AimAssist.None);

					transform.position = _movementData.Position;
				}
				else {
					transform.position = nextPosition;		
				}
				lastUpdateCollider = null;
			}

			Debug.DrawLine(lastMovement.Position, transform.position, Color.red, 0.5f);
		}
	}

	private void GenerateRandomPositionAndDirection() {
		Vector3 position = new Vector2(0, Random.Range(-8f, 8f));
		while (Physics2D.OverlapCircle(position, ballRadius, collidableLayermask) != null) {
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

		// _movementData.Position = new Vector3(0, 0f);
		// _movementData.MovementDirection = Vector3.right;
		// _movementData.MoveSpeed = BaseSpeed * 5f;
		
		transform.position = _movementData.Position;
		projectedFlight = GetFlightPath(null, AimAssist.None);
	}

	private List<MovementData> GetFlightPath(Collider2D origin, AimAssist aimAssist) {
		List<MovementData> flightData = new List<MovementData>();
		projectedFlightIndex = 0;
		expectedCollisionIndices = new List<int>();
		MovementData md = new MovementData(_movementData);
		
		float aimAssistRadius =  ballRadius * (aimAssist == AimAssist.None ? 1 :
							aimAssist == AimAssist.Light ? 4f : 6f);

		// must pass fully through the target to minimum aim assist adjustment
		// waitingOnLastTargetHit = have we passed fully through a target?[
		// closestLineHit = linecast with the minimum distance to the target
		bool waitingOnLastTargetHit = false;
		Collider2D colliderLast = origin;
		RaycastHit2D closestLineHit = new RaycastHit2D();

		int i = 1;
		int lastCollisionIndex = 0;
		flightData.Add(new MovementData(md));
		while (Mathf.Abs(md.Position.x) < 20f) { // TODO: Hardcoded x position?
			if(aimAssistNeeded) {
				var hit = Physics2D.OverlapCircle(md.Position, aimAssistRadius, targetLayermask);
				if(hit != null) {
					// determine how much we'd need to move over in order to hit target
					var linehit = Physics2D.Linecast(md.Position, hit.transform.position, targetLayermask);
					if (closestLineHit.collider == null || linehit.distance < closestLineHit.distance) {
						closestLineHit = linehit;
						waitingOnLastTargetHit = true;
					}
				}
				else if (waitingOnLastTargetHit) {
					// if we've passed through a target fully and we will be attempting aim assist
					waitingOnLastTargetHit = false;

					if (closestLineHit.distance > ballRadius) {
						MovementData lastMd = flightData[lastCollisionIndex];
						Vector2 lastPosition = lastMd.Position;
						float angle = Vector2.SignedAngle(md.Position - lastPosition, closestLineHit.point - lastPosition) * 1.25f;
						if (Mathf.Abs(angle) < 15f) {
							// adjust angle and recalculate
							// remove all the points we've added since the last collision and reset md to that last collision point
							lastMd.MovementDirection = lastMd.MovementDirection.Rotate(angle);
							lastMd.CalculateCurve();
							md = new MovementData(lastMd);

							int startRangeRemove = lastCollisionIndex + 1;
							flightData.RemoveRange(startRangeRemove, flightData.Count - startRangeRemove);					
							i = startRangeRemove;
							lastCollisionIndex = startRangeRemove;
							aimAssistNeeded = false;
						}
					}
				}
			}

			var tempPosition = md.Position;
			md.Update(Time.fixedDeltaTime);

			var ballHit = Physics2D.CircleCastAll(tempPosition, ballRadius, md.ActualMovementDirection, md.ActualMoveSpeed, collidableLayermask)
							.FirstOrDefault(h => h.collider != colliderLast);
			if(ballHit.collider != null) {
				Vector2 actualMoveDirection = md.ActualMovementDirection;
				Vector2 normal = ballHit.normal;
				float dot = Vector2.Dot(actualMoveDirection, normal);	
				float msIncrease = ballHit.collider.GetComponent<IMoving>() != null ? 0f : 0.25f;
				if (dot < 0) {
					// Debug.Log($"Est - n: {normal}, d: {dot}, am: {actualMoveDirection} c: {ballHit.collider.gameObject.name}");
					expectedCollisionIndices.Add(i);
					md.HandleNonPlayerCollision(dot, normal, Vector3.zero, msIncrease, ballHit.centroid);
					lastCollisionIndex = i;			
				}
				colliderLast = ballHit.collider;
			}
			else {
				colliderLast = null;
			}
	
			flightData.Add(new MovementData(md));
			i++;

			//Debug.DrawLine(tempPosition, md.Position, aimAssistNeeded ? Color.green : Color.cyan, i / 50f);
			Debug.DrawLine(tempPosition, md.Position, aimAssistNeeded ? Color.green : Color.cyan, (i/50f));
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
				//Vector3 poi = flightData.FindLast(p => Mathf.Abs(p.Position.x) - 15.9f < 0.1f).Position;
				Vector3 poi = flightData.Where(p => Mathf.Sign(p.Position.x * lastPositionOfFlight.x) > 0)
										.OrderBy(p => Mathf.Abs(Mathf.Abs(p.Position.x) - 15.9f)).First().Position; // fix this double abs?
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
		//ContactPoint2D cp = collision.GetContact(0);
		//Vector3 normal = hitBetweenLastMove.collider == collision.collider ? hitBetweenLastMove.normal : cp.normal;

		//Vector3 actualMoveDirection = _movementData.ActualMovementDirection;
		//float dot = Vector2.Dot(actualMoveDirection, normal);

		//activeCollision = collision.gameObject;

		//if (collision.collider.tag == "Player") {
		//	if (dot < 0 && Mathf.Abs(normal.x) > 0.1f) {
		//		var player = collision.collider.GetComponentInParent<Player>();
		//		Vector3 point = hitBetweenLastMove.collider == collision.collider ? hitBetweenLastMove.point : cp.point;
		//		_movementData.MovementDirection = player.GetBallTrajectory(point, actualMoveDirection);

		//		_movementData.Rotation += player.YMove * 6 * _movementData.MoveSpeed * -Mathf.Sign(_movementData.MovementDirection.x) * player.GetRotationModifier(point, actualMoveDirection);

		//		_movementData.MoveSpeed += player.GetMSDelta(point, actualMoveDirection);
		//		AimAssist = player.AimAssist;
		//		aimAssistNeeded = AimAssist != AimAssist.None;

		//		_movementData.CalculateCurve();
		//		projectedFlight = GetFlightPath(player.gameObject, AimAssist);
		//		if (projectedFlight[0].MovementDirection != _movementData.MovementDirection) {
		//			_movementData = projectedFlight[0];
		//		}
		//	}
		//}
		//else if (collision.gameObject.layer == LayerMask.NameToLayer("Collidable")) {
		//	if (expectedCollisionIndices.Count < 1 || expectedCollisionIndices[0] != projectedFlightIndex-1) {
		//		// if there is an unexpected collision, recalculate trajectory
		//		Debug.Log($"Unexpected collision with {collision.gameObject.name} ({projectedFlightIndex} instead of {(expectedCollisionIndices.Count > 0 ? expectedCollisionIndices[0].ToString() : "unavailable")}), recalculate trajectory");

		//		IMoving m = collision.gameObject.GetComponent<IMoving>();
		//		Vector3 extra = Vector3.zero;
		//		if (m != null) {
		//			// verify going in same direction
		//			Vector3 diff = m.GetMovementAmount(transform.position);
		//			float diffdot = Vector2.Dot(diff, _movementData.ActualMovementDirection);
		//			if (diffdot > 0) {
		//				extra = diff;
		//			}
		//		}

		//		_movementData.HandleNonPlayerCollision(dot, normal, extra);
		//		projectedFlight = GetFlightPath(collision.gameObject, AimAssist);
		//	}
		//	else {
		//		expectedCollisionIndices.RemoveAt(0);
		//	}
		//}

		// Debug.Log($"normal: {normal}, incoming: {inc}, dot: {dot}, new: {movementDirection}");
	}

	public void OnCollisionExit2D(Collision2D collision) {
		//if(collision.gameObject == activeCollision) {
		//	activeCollision = null;
		//}
	}

	public void OnTriggerEnter2D(Collider2D collision) {
		if (collision.tag == "ScoreZone") {
			GenerateRandomPositionAndDirection();
			GameManager.Instance.LevelManager.PlayerLifeLost();
		}
	}
}
