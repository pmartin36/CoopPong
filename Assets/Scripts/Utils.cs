using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Utils {
	public static Vector2 RandomVectorInBox(Bounds b, float padding = 1) {
		return new Vector2(
			Random.Range(b.min.x+padding, b.max.x-padding),
			Random.Range(b.min.y+padding, b.max.y-padding)
		);
	}

	public static Vector3 AngleToVector(float angle) {
		return new Vector2(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle)).normalized;
	}

	public static float xyToAngle(float x, float y) {
		return Mathf.Atan2(y, x) * Mathf.Rad2Deg;
	}

	public static float VectorToAngle(Vector2 vector) {
		return xyToAngle(vector.x, vector.y);
	}

	public static Vector2 QuadraticBezier(Vector2 start, Vector2 end, Vector2 ctrl, float t) {
		return (1-t)*(1-t)*start + 2*t*(1-t)*ctrl + t*t*end;
	}

	public static float NegativeMod(float a, float b) {
		return a - b * Mathf.Floor(Mathf.Abs(a / b));
	}
}
