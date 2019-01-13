using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpinOnPress : Spinner, IButtonEffected {
	public float Amount { get; set; }

	public ModifiableTResult<CommandLocation, Transform, TargetPosRot> OccupantTargetTransform =>
		(CommandLocation location, Transform otransform, ref TargetPosRot posRot) => {
			float x_sign = (location == CommandLocation.BottomRight || location == CommandLocation.TopRight) ? 1 : -1;
			posRot.Position = transform.position + new Vector3(otransform.lossyScale.x / 2f * x_sign, 0);
	};
	public GameObject GameObject { get => gameObject; }

	public CommandLocation[] PositiveActors;
	public CommandLocation[] NegativeActors;


	public void AddActor(Pet p, float amount) {
		var location = p.Command;
		if(PositiveActors.Any(a => a == location)) {
			Amount += amount;
		}
		else if (NegativeActors.Any(a => a == location)) {
			Amount -= amount;
		}
	}

	public void RemoveActor(Pet p) { }

	public override void LateUpdate() {
		Spin(Amount);
		Amount = 0;
	}

	public bool IsEffectedByButtonLocation(CommandLocation bl) {
		return PositiveActors.Any(a => a == bl) || NegativeActors.Any(a => a == bl);
	}
}
