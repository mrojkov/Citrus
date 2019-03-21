using System.Reflection;

namespace Lime
{
	public interface IPropertyLocker
	{
		bool IsPropertyLocked(string propertyName, bool isExternal);
	}
}
