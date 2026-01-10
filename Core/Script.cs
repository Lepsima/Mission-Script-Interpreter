using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static STCR.Instruction.InstructionType;
using static STCR.Value.ValueType;

namespace STCR {
public partial class Script {
	private const string EXTERNAL_CALL_OUTPUT = "$EXT_MAIN_OUT";
	private const string EXTERNAL_SUBCALL_OUTPUT = "$EXT_SUB_OUT";
	private const string Null = "NULL";

	private int pointer;
	private float sleepTimer;
	
	private readonly Dictionary<string, Value> variables = new();
	private readonly Dictionary<string, string> variableEvents = new();
	private readonly Dictionary<string, Action> specialEvents = new();
	
	private readonly HashSet<string> variableTriggers = new();
	private readonly Stack<int> functionOriginStack = new();

	private readonly ScriptSource source;
	private readonly ScriptContext ctx;
	private IScriptRunner runner;
	private Coroutine routine;

	public Script(int id) : this(ScriptCompiler.GetSourceByID(id)) { }
	
	public Script(ScriptSource source) {
		this.source = source;
		ctx = source.ctx;
	}
	
#region - Execution -
	
	public void CallSegment(IScriptRunner runner, string name) {
		if (!source.TryGetSegment(name, out int segmentIndex)) {
			Debug.LogError("Call Segment error: Segment with name: " + name + " does not exist");
			return;
		}

		this.runner = runner;
		pointer = segmentIndex;
		pointer++;
	
		routine ??= runner.StartCoroutine(Routine());
	}

	private IEnumerator Routine() {
		while (true) {
			if (sleepTimer > Time.time) {
				yield return null;
				continue;
			}
			
			try {
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
		foreach (string trigger in variableTriggers) {
			if (trigger[0] == '@') {
				CallExternal(trigger);
			}
			else {
				CallInternal(trigger, pointer);
			}
		}
		
		variableTriggers.Clear();
		Instruction ins = source.GetInstruction(pointer);

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
				object result = CallExternal(ins.key);
				SetVariable(EXTERNAL_CALL_OUTPUT, result);
				break;
			}
			
			case InternalCall: {
				CallInternal(ins.key, pointer + 1);
				break;
			}
		}

		pointer++;
	}
	
	private void SleepFor(float time) {
		sleepTimer = Time.time + time;
	}
	
	private bool EvaluateOperation(BoolExp data) {
		object a = ConvertOperationParameter(data.value);
		object b = ConvertOperationParameter(data.other);
		
		Func<object, object, bool> func = boolOperations[data.operation];
		return func.Invoke(a, b);
		
		object ConvertOperationParameter(object value) {
			return value switch {
				BoolExp operationA => EvaluateOperation(operationA),
				string str => GetObjectValue(str),
				_ => value
			};
		}
	}
	
	private void ExecuteCommand(string command, Value arg) {
		string[] args = arg.type switch {
			Static => new[] { arg.ToString() },
			ValueArray => (string[])arg.value,
			Operation => new[] { EvaluateOperation((BoolExp)arg) + "" },
			Variable => new[] { VariableToString(arg) },
			External => new[] { SubcallExternal(arg) },
			_ => null
		};

		commands[command].Invoke(this, args);
	}
	
#endregion
	
#region - Variables -
	private string VariableToString(Value value) {
		return GetVariable(value.ToString())?.ToString();
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
	
	private void ClearVariables() {
		variableEvents.Clear();
		variables.Clear();
	}
	
	private object GetObjectValue(string arg) {
		if (arg == null) return null;
		if (arg.Length == 0) return "";
		
		return arg[0] switch {
			'$' => GetVariable(arg).value,
			'@' => CallExternal(arg),
			'"' => arg[1..^1],
			_ => arg
		};
	}
	
	private string GetStringValue(string arg) {
		if (arg == null) return null;
		if (arg.Length == 0) return "";
		
		return arg[0] switch {
			'$' => GetVariable(arg).ToString(),
			'@' => CallExternal(arg) as string,
			'"' => arg[1..^1],
			_ => arg
		};
	}
	
	private bool TryGetObjectValue<T>(string arg, out T value) {
		object obj = GetObjectValue(arg);

		if (obj is T t) {
			value = t;
			return true;
		}

		value = default;
		return false;
	}
	
	private bool TryGetStringValue(string arg, out string value) {
		value = GetStringValue(arg);
		return value != null;
	}
#endregion
	
#region - Events -
	public void TriggerSpecialEvent(string name) {
		if (specialEvents.TryGetValue(name, out Action action)) {
			action?.Invoke();
		}
	}
	
	private void SetVariableEvent(string name, string varEvent) {
		variableEvents[name] = varEvent;
	}

	private void SetSpecialEvent(string name, Action action) {
		specialEvents[name] = action;
	}
	
	private void ClearEvents() {
		specialEvents.Clear();
	}
#endregion
	
#region - Functions -
	private void CallInternal(string name, int origin = -1) {
		if (!source.TryGetFunction(name, out int newPointer)) return;
		pointer = newPointer;
		functionOriginStack.Push(origin);
	}
	
	// Calls external function and returns the output variable
	private string SubcallExternal(Value value) {
		object ret = CallExternal(value.ToString());
		SetVariable(EXTERNAL_SUBCALL_OUTPUT, ret);
		return EXTERNAL_SUBCALL_OUTPUT;
	}
	
	private object CallExternal(string name, string arg) {
		if (!externalFunctions.TryGetValue(name, out Func<Script, object[], object> func))
			return null;

		if (string.IsNullOrEmpty(arg))
			return func(this, new object[] { "" });
		
		object[] args = arg.Split(',').Select(GetObjectValue).ToArray();
		return func(this, args);
	}
	
	private object CallExternal(string stringCall) {
		string[] split = stringCall.Split('(');
		string name = split[0];
		string arg = split.Length == 1 ? "" : split[1][..^1];
		return CallExternal(name, arg);
	}
	
	private object CallExternal(Value value) {
		string call = value.type switch {
			Static => value.ToString(),
			Variable => VariableToString(value),
			_ => null
		};

		return call == null ? null : CallExternal(call);
	}
#endregion
}
}