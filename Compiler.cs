using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions.Must;
using static STCR.Instruction.InstructionType;

namespace STCR {
public partial class Script {
	private int version;
	private Instruction[] instructions;
	
	private readonly Dictionary<string, int> segments = new();
	private readonly Dictionary<string, int> checkpoints = new();
	private readonly Dictionary<string, int> functions = new();

	public Script(string code) {
		// Split to lines
		List<string> lines = code.Split(
			new[] { Environment.NewLine },
			StringSplitOptions.RemoveEmptyEntries
		).ToList();

		try {
			VerifyVersion(ref lines); // Clean
			CompileLines(ref lines);  // Compile
		}
		catch (ScriptException e) {
			// Output error and compiled script for debugging
			string output = "Compiled script: \n";
			int ln = 0;
			
			foreach (string line in lines) {
				output += ln + ": " + line + "\n";
				ln++;
			}
			
			Debug.LogError(e);
			Debug.LogError(output);
		}
	}

	private bool IsCompatibleVersion() {
		return version > -1;
	}
	
	private void VerifyVersion(ref List<string> lines) {
		// Remove comments or white spaces and trim everything
		for (int i = lines.Count - 1; i >= 0; i--) {
			if (lines[i].StartsWith("//") || string.IsNullOrWhiteSpace(lines[i])) {
				lines.RemoveAt(i);
				continue;
			}
			
			lines[i] = lines[i].Trim();

			// Auto-generates the jump out of the if condition
			if (lines[i].StartsWith("else")) {
				lines.Insert(i, "stopif");
			}
		}
		
		// Get header information
		if (!lines[0].StartsWith("STCR")) {
			throw new ScriptException(
				"The file must contain as a first line: 'STCR v0' replacing '0' with the script's version");
		}
		
		int versionStart = lines[0].IndexOf('v');
		string versionStr = lines[0][(versionStart + 1)..];
		lines.RemoveAt(0);
		
		// Parse and validate version
		if (!int.TryParse(versionStr, out version)) {
			throw new ScriptException("The STCR version is not a valid integer.");
		}
		
		if (!IsCompatibleVersion()) {
			throw new ScriptException("The specified STCR version is no longer compatible.");
		}
	}

	private void CompileLines(ref List<string> lines) {
		instructions = new Instruction[lines.Count];
		
		// Goes from bottom to top so if/else's have a reference of where to jump to
		int previousIfIndex = -1;
		int finalIfIndex = -1;
		
		for (int i = lines.Count - 1; i >= 0; i--) {
			string line = lines[i];
			string[] args = line.Split(' ');

			// Handle each instruction type
			switch (args[0]) {
				case "###":
					CompileSegment(args, i);
					break;
				
				case "checkpoint":
					CompileCheckpoint(args, i);
					break;

				case "func":
					CompileFunction(args, i);
					break;
				
				case "jump":
					instructions[i] = new Instruction(Jump, args[1]);
					break;
				
				case "call":
					Instruction.InstructionType type = args[1].StartsWith('@')
						? ExternalCall
						: InternalCall;
					
					instructions[i] = new Instruction(type, line[5..]);
					break;

				// Start of the if condition, jump to next elseif when failed
				case "if":
					CompileConditional(line, "if", previousIfIndex, i);
					previousIfIndex = -1;
					finalIfIndex = -1;
					break;

				// Jump to here when last if/elseif fails, and check again
				case "elseif":
					CompileConditional(line, "elseif", previousIfIndex, i);
					previousIfIndex = i;
					break;
				
				// All if's should jump here when failed, will execute without condition
				case "else":
					instructions[i] = new Instruction(Ignore);
					previousIfIndex = i;
					break;
				
				// Auto-generates before each else/elseif to jump out when complete
				case "stopif":
					instructions[i] = new Instruction(Jump, finalIfIndex);
					break;
				
				// Endpoint of the if condition, all jumps lead here
				case "endif":
					previousIfIndex = i;
					finalIfIndex = i;
					instructions[i] = new Instruction(Ignore);
					break;
				
				default: {
					Value value = new(args.Length != 2 ? args.Skip(1).ToArray() : args[1]);
					instructions[i] = new Instruction(Command, args[0], value);
					break;
				}
			}
		}
		
		// Convert checkpoint references to proper line jumps
		foreach (Instruction i in instructions.Where(i => i.type == Jump && i.key != null)) {
			i.jump = checkpoints[i.key];
			i.key = "";
		}
	}
	
	private void CompileSegment(string[] args, int i) {
		if (!segments.TryAdd(args[1], i)) {
			throw new ScriptException($"Segment with name '{args[1]}' already exists.");
		}
		instructions[i] = new Instruction(Segment, args[1]);
	}

	private void CompileCheckpoint(string[] args, int i) {
		if (!checkpoints.TryAdd(args[1], i)) {
			throw new ScriptException($"Checkpoint with name '{args[1]}' already exists.");
		}
		instructions[i] = new Instruction(Checkpoint, args[1]);
	}

	private void CompileFunction(string[] args, int i) {
		if (args[1].Equals("END")) {
			instructions[i] = new Instruction(CallEnd);
			return;
		}

		if (functions.TryAdd(args[1], i)) {
			instructions[i] = new Instruction(Func, args[1]);
			return;
		}

		throw new ScriptException($"Function with name '{args[1]}' already exists.");
	}

	private void CompileConditional(string line, string conditional, int jumpTo, int i) {
		try {
			line = line[conditional.Length..].Trim();
			
			BoolExp expression = BuildBoolExpression(line);
			instructions[i] = new Instruction(JumpIf, expression, jumpTo);
		}
		catch (ScriptException) {
			throw new ScriptException($"Boolean expression at line {i} is invalid.");
		}
	}

	private static BoolExp BuildBoolExpression(string str) {
		int index = 0;

		// Simple 3-parameter expression, no recursive parenthesis
		if (!str.Contains('(')) {
			string[] split = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (split.Length != 3) {
				foreach (string s in split) {
					Debug.LogError(s);
				}
				
				Debug.LogError(str);
				throw new ScriptException();
			}
			
			string sa = split[0], sb = split[2], op = split[1];
			return new BoolExp(sa, sb, op);
		}

		// Handle recursive parenthesis
		BoolExp expression = null;
		string operation = null;
		
		while (index < str.Length) {
			int closeIndex;
			
			// Get operator
			if (!str[index].Equals('(')) {
				closeIndex = str.IndexOf('(', index);
				if (closeIndex == -1) {
					throw new ScriptException();
				}
				
				operation = str.Substring(index, closeIndex - index).Trim();
				index = closeIndex;
				continue;
			}
			
			// Evaluate inside of parenthesis
			closeIndex = str.IndexOf(')', index);
			if (closeIndex == -1) {
				throw new ScriptException();
			}

			int start = index + 1;
			string inside = str.Substring(start,  closeIndex - start);
			BoolExp other = BuildBoolExpression(inside);

			expression = expression != null ? new BoolExp(expression, other, operation) : other;
			index = closeIndex + 1;
		}

		return expression ?? throw new ScriptException();
	}
}
}