using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SlidingObject : MonoBehaviour, IMoving {
	private Vector3 LastFramePosition;
	private MinMax Range;

	[SerializeField]
	private Transform Object;
	[SerializeField]
	private Transform Track;

	public float Direction;
	public float Speed;

	public void Move(float diff) {
		var pos = Object.transform.localPosition.y + diff;
		pos = Mathf.Clamp(pos, Range.Min, Range.Max);
		Object.transform.localPosition = new Vector2(0, pos);

		LastFramePosition = Object.transform.localPosition;
	}

	public void Start() {
		float r = Track.localScale.y / 2f;
		Range = new MinMax(-r, r);
	}

	public virtual void LateUpdate() {
		Move(Speed * Direction * Time.deltaTime);
		var newPosition = Object.transform.localPosition.y;
		if (newPosition >= Range.Max || newPosition <= Range.Min) {
			Direction *= -1f;
		}
	}

	public Vector3 GetMovementAmount(Vector3 position) {
		return Object.transform.position - LastFramePosition;
	}
}
