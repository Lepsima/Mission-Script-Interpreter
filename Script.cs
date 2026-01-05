using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static STCR.Instruction.InstructionType;
using static STCR.Value.ValueType;

namespace STCR {
public partial class Script {
	private const string EXTERNAL_OUTPUT_VARIABLE = "$EXT_OUT";
	private const string Null = "NULL";

	private int pointer;
	private float sleepTimer;
	
	private readonly Dictionary<string, Value> variables = new();
	private readonly Dictionary<string, string> variableEvents = new();
	private readonly Dictionary<string, Action> specialEvents = new();
	
	private readonly HashSet<string> variableTriggers = new();
	private readonly Stack<int> functionOriginStack = new();

	private Coroutine routine;
	
	public void CallSegment(MonoBehaviour handler, string name) {
		pointer = segments[name];
		pointer++;
	
		routine ??= handler.StartCoroutine(Routine());
	}

	public void TriggerSpecialEvent(string name) {
		if (specialEvents.TryGetValue(name, out Action action)) {
			action?.Invoke();
		}
	}

	private void CallInternal(string name, int origin = -1) {
		if (!functions.TryGetValue(name, out int newPointer)) return;
		pointer = newPointer;
		functionOriginStack.Push(origin);
	}

	private object CallExternal(string name, string arg) {
		Debug.Log(name + "(\"" + arg + "\")");
		return null;
	}

	private void SleepFor(float time) {
		sleepTimer = Time.time + time;
	}
	
	public IEnumerator Routine() {
		while (true) {
			if (sleepTimer > Time.time) {
				yield return null;
				continue;
			}
			
			try {
				RunTriggers();
				Run();
			}
			catch (ScriptException e) {
				Debug.Log(e);
				break;
			}
		}
		
		routine = null;
	}

	private void Run() {
		Instruction ins = instructions[pointer];

		switch (ins.type) {
			case Checkpoint or Func or Ignore:
				break;
			
			case Segment:
				throw new ScriptException("End of segment");
			
			case Command:
				ExecuteCommand(ins.key, ins.value);
				break;
			
			case Jump:
				pointer = ins.jump;
				return;
			
			case JumpIf:
				if (EvaluateOperation((BoolExp)ins.value)) break;
				pointer = ins.jump;
				return;
			
			case CallEnd:
				if (functionOriginStack.Count == 0) {
					throw new ScriptException($"Function end not expected at line {pointer}");
				}
				pointer = functionOriginStack.Pop();
				return;
			
			case ExternalCall: {
				object result = StringToExternalCall(ins.key);
				SetVariable(EXTERNAL_OUTPUT_VARIABLE, result);
				break;
			}
			
			case InternalCall: {
				CallInternal(ins.key, pointer + 1);
				break;
			}
		}

		pointer++;
	}

	private void RunTriggers() {
		foreach (string trigger in variableTriggers) {
			if (trigger[0] == '@') {
				StringToExternalCall(trigger);
			}
			else {
				CallInternal(trigger, pointer);
			}
		}
		
		variableTriggers.Clear();
	}

	private void ClearVariables() {
		variableEvents.Clear();
		variables.Clear();
	}

	private void ClearEvents() {
		specialEvents.Clear();
	}

	private string VariableToString(Value value) {
		return GetVariable(value.AsString())?.ToString();
	}

	private void SetVariable(string name, object value) {
		Value val = variables.GetValueOrDefault(name);
		if (val != null && val.Equals(value)) return;
		
		if (variableEvents.TryGetValue(name, out string varEvent)) {
			variableTriggers.Add(varEvent);
		}
		
		variables[name] = new Value(value);
	}

	private Value GetVariable(string name) {
		return variables.GetValueOrDefault(name);
	}

	private void SetVariableEvent(string name, string varEvent) {
		variableEvents[name] = varEvent;
	}

	private void SetSpecialEvent(string name, Action action) {
		specialEvents[name] = action;
	}
	
	private object ValueToExternalCall(Value value) {
		string call = value.type switch {
			Static => value.AsString(),
			Variable => VariableToString(value),
			_ => null
		};

		return call == null ? null : StringToExternalCall(call);
	}

	private object StringToExternalCall(string text) {
		string[] split = text.Split('(');
		string name = split[0];
		string arg = split[1][..^1];
		return CallExternal(name, arg);
	}
	
	// TODO: WIP
	private bool EvaluateOperation(BoolExp data) {
		object a = ConvertOperationParameter(data.value);
		object b = ConvertOperationParameter(data.other);
		
		Func<object, object, bool> func = boolOperations[data.operation];
		return func.Invoke(a, b);
	}

	private object ConvertOperationParameter(object value) {
		return value switch {
			BoolExp operationA => EvaluateOperation(operationA),
			string str when str[0] == '$' => GetVariable(str),
			_ => value
		};
	}
	
	private void ExecuteCommand(string command, Value arg) {
		string[] args = arg.type switch {
			Static => new[] { arg.AsString() },
			ValueArray => (string[])arg.value,
			Operation => new[] { EvaluateOperation((BoolExp)arg) + "" },
			Variable => new[] { VariableToString(arg) },
			External => new[] { ValueToExternalCall(arg) as string },
			_ => null
		};

		commands[command].Invoke(this, args);
	}
}
}