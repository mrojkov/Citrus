using Yuzu;

namespace EmptyProject
{
	public class Profile
	{
		public static Profile Instance;

		[YuzuAfterDeserialization]
		public void AfterDeserialization()
		{ }
	}
}