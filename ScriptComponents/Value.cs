using System;

namespace STCR {
internal class Value {
	public enum ValueType {
		Static, // Value, string, number, etc
		ValueArray, // Multiple values
		Operation, // 1000 > 100 (ONLY FOR BooleanOperations)
		Variable, // $Target
		External // @Scan()
	}

	public ValueType type;
	public readonly object value;
	
	public string AsString() {
		return (string)value;
	}
	
	public Value() {
		value = null;
		type = ValueType.Static;
	}

	public Value(object value) {
		this.value = value;

		type = value switch {
			string str => GetType(str),
			string[] => ValueType.ValueArray,
			_ => type
		};
	}

	private static ValueType GetType(string value) {
		return value[0] switch {
			'$' => ValueType.Variable,
			'@' => ValueType.External,
			_ => ValueType.Static
		};
	}
	
	
	public override bool Equals(object obj) {
		return Equals(obj, value) || Equals(obj, this);
	}

	protected bool Equals(Value other) {
		return Equals(value, other.value);
	}

	public override int GetHashCode() {
		return HashCode.Combine(value);
	}
}
}