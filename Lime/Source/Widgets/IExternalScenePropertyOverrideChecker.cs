using System.Reflection;

namespace Lime
{
	public interface IExternalScenePropertyOverrideChecker
	{
		bool IsPropertyOverridden(PropertyInfo property, bool isExternal);
	}
}
