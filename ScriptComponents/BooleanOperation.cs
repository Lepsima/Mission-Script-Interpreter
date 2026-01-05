namespace STCR {
internal class BooleanOperation : Value {
	public object other;
	public string operation;
	
	public BooleanOperation(string a, string b, string op) : base(a) {
		other = b;
		operation = op;
		type = ValueType.Operation;
	}
	
	public BooleanOperation(Value a, Value b, string op) : base(a) {
		other = b;
		operation = op;
		type = ValueType.Operation;
	}
}
}