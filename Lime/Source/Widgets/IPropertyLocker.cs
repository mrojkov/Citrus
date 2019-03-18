using System.Reflection;

namespace Lime
{
	public interface IPropertyLocker
	{
		bool IsPropertyLocked(PropertyInfo property, bool isExternal);
		bool IsPropertyLocked(string propertyName, bool isExternal);
	}
}
