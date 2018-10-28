using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ContextManager : MonoBehaviour
{
	public virtual void Start() {
		GameManager.Instance.ContextManager = this;
	}
	public abstract void HandleInput(InputPackage p);
}
