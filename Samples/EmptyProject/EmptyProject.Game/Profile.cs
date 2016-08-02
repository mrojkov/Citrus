using ProtoBuf;

namespace EmptyProject
{
	[ProtoContract]
	public enum ExampleEnum
	{
		[ProtoEnum]
		ExampleItem = 0,
	}

	[ProtoContract]
	public class ExampleClass
	{
		[ProtoMember(1)]
		public ExampleEnum Type = ExampleEnum.ExampleItem;
	}

	[ProtoContract]
	public class Profile
	{
		public static Profile Instance;

		[ProtoMember(1)]
		public ExampleClass ExampleField = new ExampleClass();

		[ProtoAfterDeserialization]
		public void AfterDeserialization()
		{
		}
	}
}