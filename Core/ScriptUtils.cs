using System.Globalization;

namespace STCR {
public static class ScriptUtils {
	internal const char VARIABLE = '$';
	internal const char KEYWORD = '&';
	internal const char EXTERNAL = '@';
	internal const char STRING = '"';
	internal const string NULL = "NULL";

	internal static bool IsVariable(string str) => str[0] == VARIABLE;
	internal static bool IsKeyword(string str) => str[0] == KEYWORD;

	internal static bool IsExternal(string str) => str[0] == EXTERNAL;
	
	internal static bool IsNull(string str) => str.Equals(NULL);
}
}