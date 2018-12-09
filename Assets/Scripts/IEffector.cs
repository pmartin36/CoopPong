using System;

[Flags]
public enum StatusEffect {
	Shrunk = 1,
	Jailed = 2,
	Blinded = 4,
	SoulSwap = 8
}

public interface IEffector {
	StatusEffect Effect { get; }
	event EventHandler Destroyed;
	void OnDestroy();
}