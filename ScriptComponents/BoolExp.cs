namespace STCR {
internal class BoolExp : Value {
	public object other;
	public string operation;
	
	public BoolExp(string a, string b, string op) : base(a) {
		other = ConvertObject(b);
		operation = op;
		type = ValueType.Operation;
	}
	
	public BoolExp(Value a, Value b, string op) : base(a) {
		other = ConvertObject(b);
		operation = op;
		type = ValueType.Operation;
	}
}
}