using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HousePet : MonoBehaviour
{
	[SerializeField]
	private HousePlayer Player;

	private float timeSinceLastWaypoint;
	private Waypoint waypoint;

    // Start is called before the first frame update
    void Start()
    {
        Player.NewRequiredPetWaypoint += NewRequiredWaypoint;
		timeSinceLastWaypoint = 10f;
    }

	public void AddWaypoint(bool exact, bool required) {
		Vector3 playerAngle = -Utils.AngleToVector(Player.transform.eulerAngles.z);
		waypoint = new Waypoint(Player.transform.position + (3 * Player.ForcePositionVector), exact, required);
		timeSinceLastWaypoint = 0;
	}

    // Update is called once per frame
    void LateUpdate()
    {
		transform.Rotate(new Vector3(0,0,(transform.eulerAngles.z - Player.transform.eulerAngles.z) * 0.015f));

		Vector3 diffToPlayer = (Vector2)transform.position - (Vector2)Player.transform.position;
		
		if (diffToPlayer.magnitude < 3) {
			// don't move closer, back away if player moves closer
			Vector3 ms = Vector3.Scale((Vector2)Player.MovementDelta, diffToPlayer.normalized);
			transform.position += ms;
		}
		else {
			Vector3 playerAngle = -Utils.AngleToVector(Player.transform.eulerAngles.z);
			Vector3 targetPosition = Player.transform.position + (3 * Player.ForcePositionVector);
			if(waypoint != null) {
				targetPosition = waypoint.Position;
			}

			Vector3 positionDiff = ((Vector2)targetPosition- (Vector2)transform.position);
			float positionDiffMagnitude = positionDiff.magnitude;
			float amountToMove = ((Vector2)Player.MovementDelta).magnitude;
			amountToMove = Mathf.Max(0.5f * Time.deltaTime, amountToMove);

			if (positionDiffMagnitude > 5) {
				amountToMove *= 1.25f;
			}

			if (positionDiffMagnitude < amountToMove) {
				transform.position = targetPosition;
				waypoint = null;			
			}
			else {
				transform.position += positionDiff.normalized * amountToMove;
			}
		}
    }

	public void NewRequiredWaypoint(object sender, Vector3 position) {
		AddWaypoint(true, true);
	}
}

public class Waypoint {
	public Vector3 Position { get; set; }
	public bool Required { get; set; }
	public bool ExactPosition { get; set; }

	public Waypoint(Vector3 position, bool exactPosition, bool required) {
		Position = position;
		ExactPosition = exactPosition;
		Required = required;
	}
}
