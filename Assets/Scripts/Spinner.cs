using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spinner : MonoBehaviour {
    
	[Range(0, 50)]
	public float SpinSpeed;
	[Range(-1, 1)]
	public float SpinDirection;
	public float ArmLength;
	public float CenterSize;

	[Range(1,4)]
	public int NumArms;

	private Transform Center;
	private List<Transform> Arms;

    void Start()
    {
		Arms = new List<Transform>();
        for(int i = 0; i < transform.childCount; i++) {
			var child = transform.GetChild(i);
			if(child.tag == "SpinArm") {
				if( Arms.Count < NumArms ) {
					child.localScale = new Vector3(1, ArmLength);
					Arms.Add(child);
				}
				else {
					child.gameObject.SetActive(false);
				}			
			}
			else {
				Center = child;
				Center.localScale = Vector3.one * CenterSize;
			}
		}
    }

	public void Spin(float direction) {
		var euler = transform.rotation.eulerAngles + new Vector3(0, 0, direction * SpinSpeed * Time.deltaTime);
		transform.rotation = Quaternion.Euler(euler);
	}

	public virtual void LateUpdate() {
		Spin(SpinDirection);
	}
}
