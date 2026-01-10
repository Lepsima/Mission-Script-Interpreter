using System;
using System.Collections.Generic;

namespace STCR {
public partial class Script {
	private static readonly Dictionary<string, Func<object, object, bool>> boolOperations = new() { 
		{"is", (a, b) => a.Equals(b) },
		{"or", (a, b) => (bool)a || (bool)b }, 
		{"and", (a, b) => (bool)a && (bool)b }
	};
}
}