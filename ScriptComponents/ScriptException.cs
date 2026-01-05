using System;

namespace STCR {
public class ScriptException : Exception {
	public ScriptException() { }
	public ScriptException(string message) : base(message) { }
}
}