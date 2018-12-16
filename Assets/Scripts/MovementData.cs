using UnityEngine;
using System.Collections;

public class MovementData  {
	public Vector2 Position { get; set; }
	public Vector3 MovementDirection { get; set; }
	public float MoveSpeed { get; set; }

	public float Rotation { get; set; }
	public float Curve { get; set; }
	public Vector3 CurveDirection { get; set; }

	public bool IsCurving { get; set; }

	public Vector3 ActualMovementDirection {
		get
		{
			Vector3 curveToAdd = IsCurving ? Curve * CurveDirection : Vector3.zero;
			return (MovementDirection * MoveSpeed + curveToAdd).normalized;
		}
	}
	public float ActualMoveSpeed { get; private set; }

	public MovementData() {

	}

	public MovementData(MovementData d) : this(
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

		Vector2 delta = (MovementDirection * MoveSpeed + curveToAdd) * timestep;

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

	public void HandleNonPlayerCollision(float dot, Vector3 normal, Vector3 extraMovement, MovementData mdOverride = null) {
		if (mdOverride != null) {
			MovementDirection = mdOverride.MovementDirection;
		}
		else {
			MovementDirection = ActualMovementDirection - 2 * dot * normal + extraMovement;
			if (Mathf.Abs(MovementDirection.x) < 0.25f) {
				MovementDirection = (MovementDirection + Mathf.Sign(Position.x) * Vector3.left).normalized;
			}
			CalculateCurve();
		}
		
		MoveSpeed += 0.25f + extraMovement.magnitude;
	}
}
