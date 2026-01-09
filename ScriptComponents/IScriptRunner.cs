using System.Collections;
using UnityEngine;

namespace STCR {
public partial interface IScriptRunner {
	public Coroutine StartCoroutine(IEnumerator routine);
}
}