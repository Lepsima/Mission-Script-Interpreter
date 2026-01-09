using System.Collections;
using UnityEngine;

namespace STCR {
public interface IScriptRunner {
	public Coroutine StartCoroutine(IEnumerator routine);

	public ScriptContext Context();
}
}