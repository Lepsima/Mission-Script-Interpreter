using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

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
			if (args[0][0] == '$') { 
				script.SetVariableEvent(args[0], args[1]);
			}
			
			// Special events, external activation
			else {
				bool isNull = args[1].Equals(Null);
				Action action = isNull ? null : () => script.CallInternal(args[1]);
				script.SetSpecialEvent(args[0], action);
			}
		}},
		
		// Sleeps the program until the specified SPECIAL EVENT is called, NO variable events allowed
		{ "waitEvent", (script, args) => {
			// Sleep until special event triggers
			script.SleepFor(999_999f);
			script.SetSpecialEvent(args[0], () => script.SleepFor(-1));
		}},
		
		// Sleeps the program for X seconds
		{ "wait", (script, args) => {
			string timeStr = args[0][0] switch {
				'$' => script.GetVariable(args[0]).ToString(),
				'@' => script.StringToExternalCall(args[0]) as string,
				_ => args[0]
			};

			if (timeStr == null) return;
			script.SleepFor(float.Parse(timeStr, CultureInfo.InvariantCulture));
			
		}},
		
		// Sets a VARIABLE to a specific value, the source can be static, variable or external
		{ "set", (script, args) => {
			if (args.Length != 2) {
				throw new ScriptException("'set' command: needs exactly two arguments");
			}
			
			if (args[0][0] != '$') {
				throw new ScriptException("'set' command: first argument must be a variable");
			}
			
			object a = args[1][0] switch {
				'$' => script.GetVariable(args[1]),
				'@' => script.StringToExternalCall(args[1]),
				_ => args[1]
			};

			script.SetVariable(args[0], a);
		}},
	};
}
}