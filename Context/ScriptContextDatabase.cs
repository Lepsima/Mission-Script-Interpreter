using System.Collections.Generic;
using UnityEngine;

namespace STCR {
public class ScriptContextDatabase : MonoBehaviour {
	private static ScriptContextDatabase Instance;

	private readonly Dictionary<string, ScriptContext> scriptContexts = new();
	private ContextBuilder[] builders;

	private void Awake() {
		if (Instance != null) {
			throw new UnityException("There can only be one instance of ContextDatabase");
		}

		Instance = this;
		builders = GetComponentsInChildren<ContextBuilder>();
		
		foreach (ContextBuilder builder in builders) {
			ScriptContext ctx = builder.BuildContext();
			scriptContexts[ctx.context.ToLower()] = ctx;
		}
	}

	private void OnDestroy() {
		Instance = null;
	}

	public static ScriptContext GetContext(string context) {
		return Instance.scriptContexts.GetValueOrDefault(context);
	}
}
}