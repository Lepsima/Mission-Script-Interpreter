using System.Collections.Generic;
using UnityEngine;

namespace STCR {
public class ContextDatabase : MonoBehaviour {
	public ContextBuilder[] builders;

	private readonly Dictionary<string, ScriptContext> scriptContexts = new();
	private static ContextDatabase Instance;

	private void Awake() {
		if (Instance != null) {
			throw new UnityException("There can only be one instance of ContextDatabase");
		}

		Instance = this;

		foreach (ContextBuilder builder in builders) {
			ScriptContext ctx = builder.BuildContext();
			scriptContexts[ctx.context] = ctx;
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