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
	protected Transform ObjectOnTrack;
	[SerializeField]
	protected Transform Track;

	public float Direction;
	public float Speed;
	protected Vector3 TrackDirection;

	public void Move(float diff) {
		var pos = ObjectOnTrack.transform.localPosition.y + diff;
		pos = Mathf.Clamp(pos, Range.Min, Range.Max);
		ObjectOnTrack.transform.localPosition = TrackDirection * pos;

		LastFramePosition = ObjectOnTrack.transform.localPosition;
	}

	public virtual void Start() {
		float r = Track.localScale.x / 2f;
		Range = new MinMax(-r, r);
		TrackDirection = Utils.AngleToVector(Track.rotation.eulerAngles.z);
	}

	public virtual void LateUpdate() {
		Move(Speed * Direction * Time.deltaTime);
		var newPosition = ObjectOnTrack.transform.localPosition.y;
		if (newPosition >= Range.Max || newPosition <= Range.Min) {
			Direction *= -1f;
		}
	}

	public Vector3 GetMovementAmount(Vector3 position) {
		return ObjectOnTrack.transform.position - LastFramePosition;
	}
}
