using UnityEngine;
using System.Collections;

public class MovementData  {
	public Vector3 Position { get; set; }
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

	public MovementData() { }

	public MovementData(Vector3 p) : this( 
			position: p,
			movementDirection: Vector3.zero,
			moveSpeed: BaseBall.BaseSpeed,
			rotation: 0,
			curve: 0,
			curveDirection: Vector2.zero,
			isCurving: false) { }

	public MovementData(Vector3 p, Vector3 md) : this(
			position: p,
			movementDirection: md,
			moveSpeed: BaseBall.BaseSpeed,
			rotation: 0,
			curve: 0,
			curveDirection: Vector2.zero,
			isCurving: false) { }

	public MovementData(MovementData d) : this(
		new Vector3(d.Position.x, d.Position.y),
		new Vector3(d.MovementDirection.x, d.MovementDirection.y),
		d.MoveSpeed,
		d.Rotation,
		d.Curve,
		new Vector3(d.CurveDirection.x, d.CurveDirection.y),
		d.IsCurving) {
		ActualMoveSpeed = d.ActualMoveSpeed;
	}

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

	public void AddRotation(float add, bool oppositeDirection) {
		float max = 600;
		float curveMod = 300f;
		if(oppositeDirection) {
			max *= 2f;
			curveMod *= 2.5f;
		}

		float newRotation = Mathf.Clamp(Rotation + add, -max, max);
		float rotationDiff = newRotation - Rotation;
		Rotation = newRotation;
		Curve += rotationDiff / curveMod;
		CalculateCurve(false);
	}

	public void CalculateCurve(bool resetCurve = true) {
		Vector3 cd = MovementDirection.Rotate(90);
		if (Mathf.Sign(cd.x * Rotation) * Mathf.Sign(MovementDirection.x) < 0) {
			cd.x = 0;
		}
		CurveDirection = cd;
		IsCurving = Mathf.Abs(Vector2.Dot(cd, MovementDirection)) < 0.5f;
		if(resetCurve) {
			Curve = 0;
		}
	}

	public void HandleNonPlayerCollision(float dot, Vector3 normal, Vector3 extraMovement, float msIncrease = 0.25f, Vector3? surfaceData = null) {
		float extraMs = extraMovement.magnitude;
		MovementDirection = ((ActualMovementDirection - 2 * dot * normal) * ActualMoveSpeed + (extraMovement * extraMs)).normalized;
		if (Mathf.Abs(MovementDirection.x) < 0.25f) {
			MovementDirection = (MovementDirection + Mathf.Sign(Position.x) * Vector3.left).normalized;
		}
		CalculateCurve();

		if (surfaceData != null) {
			Position = surfaceData.Value;
		}

		MoveSpeed = Mathf.Max(MoveSpeed + msIncrease, extraMs);
	}
}

public class SurfacePositionData {
	public float Radius { get; set; }
	public Vector3 Position { get; set; }

	public SurfacePositionData(Vector3 position, float radius) {
		Radius = radius;
		Position = position;
	}
}
