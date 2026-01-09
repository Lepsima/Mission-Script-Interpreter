using UnityEngine;

namespace STCR {
public class DEBUG_ScriptRunner : MonoBehaviour, IScriptRunner {
	public TextAsset script;
	public string segment;
	public bool trigger = true;
	
	private Script program;
	
	private void Start() {
		program = new Script(this, script.text);
	}

	private void Update() {
		if (!trigger) return;
		trigger = false;
		program.CallSegment(segment);
	}
}
}