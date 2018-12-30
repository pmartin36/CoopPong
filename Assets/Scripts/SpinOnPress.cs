using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpinOnPress : Spinner, IButtonEffected {
	public float Amount { get; set; }
	public ButtonLocation[] PositiveActors;
	public ButtonLocation[] NegativeActors;

	public void AddActor(ButtonLocation location, float amount) {
		if(PositiveActors.Any(a => a == location)) {
			Amount += amount;
		}
		else if (NegativeActors.Any(a => a == location)) {
			Amount -= amount;
		}
	}

	public override void LateUpdate() {
		Spin(Amount);
		Amount = 0;
	}
}
