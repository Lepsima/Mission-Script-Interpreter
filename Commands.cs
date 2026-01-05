using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace STCR {
public partial class Script {
	private static readonly Dictionary<string, Action<Script, string[]>> commands = new() {
		// Clears everything related to variables, their values and events
		{ "clearVariables", (s, _) => {
			s.ClearVariables();
		}},

		// Clears SPECIAL events
		{ "clearEvents", (s, _) => {
			s.ClearEvents();
		}},
		
		// When an event triggers, the specified internal function is called
		{ "onEvent", (s, args) => {
			// Variable events, triggered on value change
			if (args[0][0] == '$') { 
				s.SetVariableEvent(args[0], args[1]);
			}
			
			// Special events, external activation
			else {
				bool isNull = args[1].Equals(Null);
				Action action = isNull ? null : () => s.CallInternal(args[1]);
				s.SetSpecialEvent(args[0], action);
			}
		}},
		
		// Sleeps the program until the specified SPECIAL EVENT is called, NO variable events allowed
		{ "waitEvent", (s, args) => {
			// Sleep until special event triggers
			s.SleepFor(999_999f);
			s.SetSpecialEvent(args[0], () => s.SleepFor(-1));
		}},
		
		// Sleeps the program for X seconds
		{ "wait", (s, args) => {
			string timeStr = args[0][0] switch {
				'$' => s.GetVariable(args[0]).AsString(),
				'@' => s.StringToExternalCall(args[0]) as string,
				_ => args[0]
			};

			if (timeStr == null) return;
			s.SleepFor(float.Parse(timeStr, CultureInfo.InvariantCulture));
			
		}},
		
		// Sets a VARIABLE to a specific value, the source can be static, variable or external
		{ "set", (s, args) => {
			if (args.Length != 2) {
				throw new ScriptException("'set' command: needs exactly two arguments");
			}
			
			if (args[0][0] != '$') {
				throw new ScriptException("'set' command: first argument must be a variable");
			}
			
			object a = args[1][0] switch {
				'$' => s.GetVariable(args[1]),
				'@' => s.StringToExternalCall(args[1]),
				_ => args[1]
			};

			s.SetVariable(args[0], a);
		}},
	};
}
}