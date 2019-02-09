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

	public void Move(float diff) {
		Vector3 diffFromCenter = ObjectOnTrack.transform.position - Track.position;

		float pushDirection = Mathf.Sign(diff);
		float positionDirection = Mathf.Sign(Vector3.Dot(diffFromCenter, TrackDirection));

		float distFromCenter = diffFromCenter.magnitude;
		if(Mathf.Abs(positionDirection * distFromCenter + diff) > Range.Max) {
			diff = pushDirection * Mathf.Max(0, Range.Max - distFromCenter);
		}

		ObjectOnTrack.transform.localPosition += TrackDirection * diff;
		LastFramePosition = ObjectOnTrack.transform.localPosition;
	}

	public Vector3 GetMovementAmount(Vector3 position) {
		return ObjectOnTrack.transform.position - LastFramePosition;
	}
}
