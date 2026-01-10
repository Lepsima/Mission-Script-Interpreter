using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace STCR {
public class ScriptCompiler : MonoBehaviour {
	private static ScriptCompiler Instance;
	private readonly Dictionary<int, ScriptSource> scripts = new();

	private void Awake() {
		if (Instance != null) {
			throw new UnityException("There can only be one instance of ScriptCompiler");
		}
		
		Instance = this;
		
		// Compile all scripts
		IEnumerable<TextAsset> assets = Manager.GetAll<TextAsset>();
		assets.ForEach(i => scripts.Add(i.GetInstanceID(), new ScriptSource(i)));
	}

	public static ScriptSource GetSourceByID(int id) {
		return Instance.scripts[id];
	}
	
	public static bool TryGetSourceByID(int id, out ScriptSource source) {
		return Instance.scripts.TryGetValue(id, out source);
	}
}
}