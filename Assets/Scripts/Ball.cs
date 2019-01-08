using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ball : BaseBall
{
	private List<MovementData> projectedFlight;
	private List<int> expectedCollisionIndices;
	private int projectedFlightIndex = 0;

	private LayerMask targetLayermask;

	private MovementData lastMovement;
	private RaycastHit2D hitBetweenLastMove;

	private AimAssist AimAssist;
	private bool aimAssistNeeded = true;

	public bool RemoteControlled { get; set; }
	public List<Player> CpuControlledPlayers {
		get {
			return GameManager.Instance.LevelManager.CpuControlledPlayers;
		}
	}

	protected override void SetBallRadius(float radius) {
		base.SetBallRadius(radius);
		if(MovementData != null) {
			projectedFlight = GetFlightPath(null, AimAssist.None);
		}
	}

	// Start is called before the first frame update
	public override void Start()
    {
		base.Start();
		targetLayermask = 1 << LayerMask.NameToLayer("Target");
		GenerateRandomPositionAndDirection();
	}

    // Update is called once per frame
    public override void FixedUpdate()
    {
		if (RemoteControlled) {
			base.FixedUpdate();
			foreach(Player p in CpuControlledPlayers) {
				p.GoToLocation(transform.position.y);
			}
		}
		else if (projectedFlight != null && projectedFlight.Count > projectedFlightIndex - 1) {
			lastMovement = projectedFlight[projectedFlightIndex];
			Vector3 lastMoveDirection = lastMovement.ActualMovementDirection;

			projectedFlightIndex++;
			MovementData = projectedFlight[projectedFlightIndex];
			
			Vector3 nextPosition = MovementData.Position;
			transform.Rotate(0, 0, Rotation * Time.fixedDeltaTime);
			hitBetweenLastMove = Physics2D.CircleCastAll(transform.position, ballRadius, 
				lastMovement.ActualMovementDirection, lastMovement.ActualMoveSpeed, playerAndColliderLayermask).FirstOrDefault(h => h.collider != lastUpdateCollider);

			// Debug
			//for(int i = 0; i < 360; i+=30) {
			//	Debug.DrawLine(nextPosition, nextPosition + _movementData.ActualMovementDirection.Rotate(i) * ballRadius, projectedFlightIndex%2==0 ? Color.gray:Color.blue, 1f);
			//}

			if (hitBetweenLastMove.collider != null) {
				HandleCollisions(nextPosition, lastMoveDirection);
			}
			else {
				if(expectedCollisionIndices.Count > 0 && expectedCollisionIndices[0] == projectedFlightIndex) {
					// Debug.Log($"Expected collision at {expectedCollisionIndices[0]} but none occured - recalculating");
					MovementData = projectedFlight[projectedFlightIndex - 1];
					projectedFlight = GetFlightPath(null, AimAssist.None);

					transform.position = MovementData.Position;
				}
				else {
					transform.position = nextPosition;		
				}
				lastUpdateCollider = null;
			}

			Debug.DrawLine(lastMovement.Position, transform.position, Color.red, 0.5f);
		}
	}

	protected virtual void HandleCollisions(Vector3 nextPosition, Vector3 lastMoveDirection) {
		Collider2D collider = hitBetweenLastMove.collider;
		Vector3 normal = hitBetweenLastMove.normal;
		float dot = Vector2.Dot(lastMoveDirection, normal);

		if (collider.CompareTag("Player")) {
			HandlePlayerCollision(dot, normal, hitBetweenLastMove, lastMoveDirection);
		}
		else if (expectedCollisionIndices.Count < 1 || expectedCollisionIndices[0] != projectedFlightIndex) {
			// if there is an unexpected collision, recalculate trajectory
			// Debug.Log($"Unexpected collision with {collider.gameObject.name} ({projectedFlightIndex} instead of {(expectedCollisionIndices.Count > 0 ? expectedCollisionIndices[0].ToString() : "unavailable")}), recalculate trajectory");

			IMoving m = collider.gameObject.GetComponent<IMoving>();
			Vector3 extra = Vector3.zero;
			if (m != null) {
				// verify going in same direction
				Vector3 diff = m.GetMovementAmount(nextPosition);
				float diffdot = Vector2.Dot(diff.normalized, lastMoveDirection);
				diff = diffdot < 0.5f ? diff : Vector3.zero;
				// Debug.Log($"Adding {diff} with dot of {diffdot} and moveDirection of {lastMoveDirection}");
				MovementData.HandleNonPlayerCollision(dot, normal, diff, 0f, hitBetweenLastMove.centroid);
			}
			else {
				MovementData.HandleNonPlayerCollision(dot, normal, extra, 0.25f, hitBetweenLastMove.centroid);
			}

			projectedFlight = GetFlightPath(collider, AimAssist);
		}
		else {
			expectedCollisionIndices.RemoveAt(0);
		}

		transform.position = MovementData.Position;
		lastUpdateCollider = collider;
	}

	protected override void HandlePlayerCollision(float dot, Vector2 normal, RaycastHit2D hit, Vector3 lastMoveDirection) {
		if (dot < 0 && Mathf.Abs(normal.x) > 0.1f) {
			var player = hit.collider.GetComponentInParent<Player>();
			Vector3 point = hit.point;
			MovementData.MovementDirection = player.GetBallTrajectory(this, point, lastMoveDirection);

			// Debug.Log(hitBetweenLastMove.centroid);
			MovementData.Position = hit.centroid;
			MovementData.Rotation += player.YMove * 6 * MovementData.MoveSpeed * -Mathf.Sign(MovementData.MovementDirection.x) * player.GetRotationModifier(point, lastMoveDirection);

			MovementData.MoveSpeed += player.GetMSDelta(point, lastMoveDirection);
			AimAssist = player.AimAssist;
			aimAssistNeeded = AimAssist != AimAssist.None;

			MovementData.CalculateCurve();
			projectedFlight = GetFlightPath(hit.collider, AimAssist);
			if (projectedFlight[0].MovementDirection != MovementData.MovementDirection) {
				MovementData = projectedFlight[0];
			}

			if(IsBigBall) {
				player.ApplyMoveSlow();
			}
		}
	}

	private void GenerateRandomPositionAndDirection() {
		Vector3 position = new Vector2(0, Random.Range(-8f, 8f));
		while (Physics2D.OverlapCircle(position, ballRadius, collidableLayermask) != null) {
			position = new Vector2(0, Random.Range(-8f, 8f));
		}

		MovementData = new MovementData(
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
		
		RemoteControlled = false;
		transform.position = MovementData.Position;
		projectedFlight = GetFlightPath(null, AimAssist.None);
	}

	private List<MovementData> GetFlightPath(Collider2D origin, AimAssist aimAssist) {
		List<MovementData> flightData = new List<MovementData>();
		projectedFlightIndex = 0;
		expectedCollisionIndices = new List<int>();
		MovementData md = new MovementData(MovementData);
		
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
				targetedPlayer.GoToLocation(poi.y);
			}
			if(!targetedPlayer.OtherPlayer.PlayerControlled) {
				Vector3 poi = new Vector3(targetedPlayer.OtherPlayer.transform.position.x, Random.Range(-1f, 1f));
				targetedPlayer.OtherPlayer.GoToLocation(poi.y);
			}
		}

		return flightData;
	}

	public void ApplyBigBall() {
		StopCoroutine("BigBall");
		StartCoroutine("BigBall");
	}

	public void GraduateBallSize(float size) {
		StartCoroutine(ChangeBallSize(size, 1f, new WaitForSeconds(0.1f)));
	}

	public void RemoteControl(float v) {
		RemoteControlled = true;
		MovementData.AddCurve(v * 30);
	}

	public void OnTriggerEnter2D(Collider2D collision) {
		if (collision.CompareTag("ScoreZone")) {
			GenerateRandomPositionAndDirection();
			GameManager.Instance.LevelManager.PlayerLifeLost();
		}
	}

	public IEnumerator BigBall() {
		GraduateBallSize(0.5f);
		yield return new WaitForSeconds(1 * 60f);
		GraduateBallSize(0.25f);
	}
}
