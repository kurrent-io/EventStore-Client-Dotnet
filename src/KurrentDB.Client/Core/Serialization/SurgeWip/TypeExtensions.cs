using System.Diagnostics;
using static System.StringComparison;

namespace Kurrent.Surge;

public static class TypeExtensions {
	static readonly Type MissingType = Type.Missing.GetType();

	[DebuggerStepThrough]
	public static bool IsMissing(this Type source) => source == MissingType;
}
