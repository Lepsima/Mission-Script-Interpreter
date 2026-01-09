using System;
using System.Collections.Generic;
using UnityEngine;

namespace STCR {
public abstract class ContextProvider : MonoBehaviour {
	public ScriptContext ctx;

	protected virtual void Awake() {
		ctx = new ScriptContext { contextName = "Context Name" };
	}
}
}