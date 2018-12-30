using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class SlideOnPress : SlidingObject, IButtonEffected {
	public float Amount { get; set; }
	public ButtonLocation[] PositiveActors;
	public ButtonLocation[] NegativeActors;

	public void AddActor(ButtonLocation location, float amount) {
		if (PositiveActors.Any(a => a == location)) {
			Amount += amount;
		}
		else if (NegativeActors.Any(a => a == location)) {
			Amount -= amount;
		}
	}

	public override void LateUpdate() {
		Move(Speed * Amount * Time.deltaTime);
		Amount = 0;
	}
}

