using UnityEngine;

public static class Extensions {

	/**
	* Vectors
	**/
	public static Vector3 Rotate(this Vector3 v, float degrees) {
		return Rotate( (Vector2) v, degrees );
	}
	public static Vector2 Rotate(this Vector2 v, float degrees) {
		float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
		float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

		float tx = v.x;
		float ty = v.y;
		v.x = (cos * tx) - (sin * ty);
		v.y = (sin * tx) + (cos * ty);
		return v;
	}

	public static Vector3 MoveTowards(this Vector3 v, Vector3 destination, float moveSpeed) {
		var diff = (destination - v);
		var mag = diff.magnitude;
		return v + diff.normalized * (mag > moveSpeed ? moveSpeed : mag);
		
	}

	/**
	 *  Etc 
	 **/
	public static void Destroy(this GameObject g, float time = 0) {
		Object.Destroy(g, time);
	}
}