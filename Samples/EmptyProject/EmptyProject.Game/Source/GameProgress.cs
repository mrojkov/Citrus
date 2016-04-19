using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using Lime;

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
	public class GameProgress
	{
		[ProtoMember(1)]
		public ExampleClass ExampleField = new ExampleClass();

		[ProtoAfterDeserialization]
		public void AfterDeserialization()
		{
		}
	}
}