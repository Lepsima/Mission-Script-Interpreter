using System;
using System.Collections.Generic;
using UnityEngine;

namespace STCR {
public class DEBUG_DefaultContextProvider : MonoBehaviour {
	public ScriptContext ctx;

	private void Awake() {
		ctx = new ScriptContext();
		ctx.contextName = "Context Name";
	}
}
}