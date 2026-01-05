using UnityEngine;

namespace STCR {
public class DEBUG_ScriptTester : MonoBehaviour {
	public TextAsset script;
	public string segment;
	
	private Script program;
	
	private void Start() {
		program = new Script(script.text);
		program.CallSegment(this, segment);
	}
}
}