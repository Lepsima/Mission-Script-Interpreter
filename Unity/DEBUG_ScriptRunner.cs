using UnityEngine;

namespace STCR {
public class DEBUG_ScriptRunner : MonoBehaviour, IScriptRunner {
	public TextAsset script;
	public string segment;
	public bool trigger = true;
	
	private Script program;
	
	private void Start() {
		// Compile
		ScriptSource source = new(script);
		
		// Create instance
		program = new Script(source);
	}

	private void Update() {
		if (!trigger) return;
		trigger = false;
		program.CallSegment(this, segment);
	}
}
}