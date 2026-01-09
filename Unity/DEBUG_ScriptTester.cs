using UnityEngine;

namespace STCR {
public class DEBUG_ScriptTester : MonoBehaviour, IScriptRunner {
	public TextAsset script;
	public ContextProvider defaultContextProvider;
	public string segment;
	public bool trigger = true;
	
	private Script program;
	
	private void Start() {
		program = new Script(script.text);
	}

	private void Update() {
		if (!trigger) return;
		trigger = false;
		program.CallSegment(this, segment);
	}
	
	public ScriptContext Context() => defaultContextProvider.ctx;
}
}