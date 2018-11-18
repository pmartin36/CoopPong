using System;

[Flags]
public enum StatusEffect {
	Slowed = 1,
	Jailed = 2,
	Blinded = 4
}

public interface IEffector {
	StatusEffect Effect { get; }
	event EventHandler Destroyed;
	void OnDestroy();
}