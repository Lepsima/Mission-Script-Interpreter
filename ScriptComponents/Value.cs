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
	
	public string AsString() => value.ToString();

	public Value(object val) {
		value = ConvertObject(val);
		
		type = value switch {
			string str => GetType(str),
			string[] => ValueType.ValueArray,
			_ => type
		};
	}

	protected static object ConvertObject(object value) {
		if (value is not string str) return value;
		if (value.Equals("NULL")) return null;
		if (bool.TryParse(str, out bool b)) return b;
		return str;
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

	public override int GetHashCode() {
		return HashCode.Combine(value);
	}
}
}