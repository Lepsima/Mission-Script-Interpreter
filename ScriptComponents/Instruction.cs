namespace STCR {
internal class Instruction {
	public enum InstructionType {
		Command, // Key, Value
		Segment, // Key
		Checkpoint, // Key
		Jump, // Jump
		JumpIf, // Value, Jump
		Func, // Key
		CallEnd, //
		InternalCall, // Key
		ExternalCall, // Key
		Ignore,
	}

	public readonly InstructionType type;
	public string key;
	public Value value;
	public int jump;
	
	public Instruction(InstructionType type) {
		this.type = type;
	}
	
	public Instruction(InstructionType type, int jump) {
		this.type = type;
		this.jump = jump;
	}

	public Instruction(InstructionType type, string key) {
		this.type = type;
		this.key = key;
	}

	public Instruction(InstructionType type, string key, Value value) {
		this.type = type;
		this.key = key;
		this.value = value;
	}
	
	public Instruction(InstructionType type, Value value, int jump) {
		this.type = type;
		this.value = value;
		this.jump = jump;
	}
}
}