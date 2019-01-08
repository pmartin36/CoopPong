using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spinner : MonoBehaviour, IMoving {
    private float LastFrameSpinAmount;

	[Range(0, 50)]
	public float SpinSpeed;
	[Range(-1, 1)]
	public float SpinDirection;

	private Transform Center;
	private List<Transform> Arms;

	private float ArmLength;

    void Start()
    {
		Arms = new List<Transform>();
		for (int i = 0; i < transform.childCount; i++) {
			var child = transform.GetChild(i);
			if (child.CompareTag("SpinArm")) {
				Arms.Add(child);
			}
			else {
				Center = child;
			}
		}
		ArmLength = Arms[0].transform.lossyScale.y;
	}

	public void Spin(float direction) {
		float spin = direction * SpinSpeed * Time.deltaTime;
		var euler = transform.rotation.eulerAngles + new Vector3(0, 0, spin);
		transform.rotation = Quaternion.Euler(euler);

		LastFrameSpinAmount = spin;
	}

	public virtual void LateUpdate() {
		Spin(SpinDirection);
	}

	public Vector3 GetMovementAmount(Vector3 position) {
		Vector3 relativePosition = position - transform.position;
		float distFromCenter = relativePosition.magnitude;

		if(distFromCenter > ArmLength) {
			return Vector3.zero;
		}

		float currentAngle = Vector2.SignedAngle(Vector2.right, relativePosition);
		float previousAngle = (currentAngle - LastFrameSpinAmount);

		Vector3 lastPosition = new Vector3(
			Mathf.Cos(previousAngle * Mathf.Deg2Rad) * distFromCenter + transform.position.x,
			Mathf.Sin(previousAngle * Mathf.Deg2Rad) * distFromCenter + transform.position.y);
		Vector3 currentPosition = new Vector3(
			Mathf.Cos(currentAngle * Mathf.Deg2Rad) * distFromCenter + transform.position.x,
			Mathf.Sin(currentAngle * Mathf.Deg2Rad) * distFromCenter + transform.position.y);
		return (currentPosition - lastPosition) / Time.fixedDeltaTime;
	}
}
