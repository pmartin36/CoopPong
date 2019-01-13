using System;
using UnityEngine;

public interface IButtonEffected {
	float Amount { get; set; }
	GameObject GameObject { get; }
	ModifiableTResult<CommandLocation, Transform, TargetPosRot> OccupantTargetTransform { get; }

	void AddActor(Pet p, float amount);
	void RemoveActor(Pet p);

	bool IsEffectedByButtonLocation(CommandLocation bl);
}

public delegate void ModifiableTResult<T1, T2, TResult>(T1 a, T2 b, ref TResult c);