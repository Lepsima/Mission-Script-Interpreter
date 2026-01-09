using UnityEngine;

namespace STCR {
public abstract class BaseContextBuilder : MonoBehaviour {
	public int version;
	public string context;
	
	public virtual ScriptContext BuildContext() {
		ScriptContext ctx = new() {
			version = version,
			context = context,
		};
		return ctx;
	}
}
}