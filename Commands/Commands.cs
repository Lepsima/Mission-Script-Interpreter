using System;
using System.Collections.Generic;
using System.Globalization;

namespace STCR {
public partial class Script {
	private static readonly Dictionary<string, Action<Script, string[]>> commands = new() {
		// Clears everything related to variables, their values and events
		{ "clearVariables", (script, _) => {
			script.ClearVariables();
		}},

		// Clears SPECIAL events
		{ "clearEvents", (script, _) => {
			script.ClearEvents();
		}},
		
		// When an event triggers, the specified internal function is called
		{ "onEvent", (script, args) => {
			// Variable events, triggered on value change
			if (IsVariable(args[0])) { 
				script.SetVariableEvent(args[0], args[1]);
				return;
			}
			
			// Special events, external activation
			Action action = IsNull(args[1]) ? null : () => script.CallInternal(args[1]);
			script.SetSpecialEvent(args[0], action);
		}},
		
		// Sleeps the program until the specified SPECIAL EVENT is called, NO variable events allowed
		{ "waitEvent", (script, args) => {
			if (!IsKeyword(args[0])) {
				throw new ScriptException("'waitEvent' command: external KEYWORD starting with '&' is required");
			}
			
			// Sleep until special event triggers
			script.SleepFor(999_999f);
			script.SetSpecialEvent(args[0], () => script.SleepFor(-1));
		}},
		
		// Sleeps the program for X seconds
		{ "wait", (script, args) => {
			if (script.ArgToFloat(args[0], out float time)) {
				script.SleepFor(time);
			}
		}},
		
		// Sets a VARIABLE to a specific value, the source can be static, variable or external
		{ "set", (script, args) => {
			if (args.Length != 2) {
				throw new ScriptException("'set' command: needs exactly two arguments");
			}
			
			if (!IsVariable(args[0])) {
				throw new ScriptException("'set' command: first argument must be a variable");
			}
			
			object value = script.GetObjectValue(args[1]);
			script.SetVariable(args[0], value);
		}},
	};
}
}