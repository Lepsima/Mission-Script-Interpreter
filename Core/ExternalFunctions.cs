using System;
using System.Collections.Generic;
using UnityEngine;

namespace STCR {
public partial class Script {
	private static readonly Dictionary<string, Func<Script, object[], object>> externalFunctions = new() { 
		{"@Print", (script, args) => {
			Debug.Log(args[0]);
			return null;
		}},
		{"@This", (script, args) => {
			return script;
		}},
	};
}
}