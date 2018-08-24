using System.Threading;
using Yuzu;
using Yuzu.Json;

namespace Tangerine.Core
{
	public class TangerineYuzu
	{
		public static ThreadLocal<Lime.Yuzu> Instance = new ThreadLocal<Lime.Yuzu>(() => new Lime.Yuzu(
			new CommonOptions {
				TagMode = TagMode.Aliases,
				AllowEmptyTypes = true,
				CheckForEmptyCollections = true,
				AllowUnknownFields = true,
			},
			new JsonSerializeOptions {
				ArrayLengthPrefix = false,
				Indent = "\t",
				FieldSeparator = "\n",
				SaveRootClass = true,
				Unordered = true,
				MaxOnelineFields = 8,
				BOM = true,
			}
		));
	}
}
