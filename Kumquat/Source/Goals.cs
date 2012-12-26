
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Kumquat
{
	[ProtoContract]
	public class Goals
	{
		[ProtoContract]
		public class Record
		{
			[ProtoMember(1)]
			public Enum Goal;

			[ProtoMember(2)]
			public bool IsStrike;

			public Record(Enum goal)
			{
				Goal = goal;
			}
		}

		[ProtoMember(1)]
		public List<Record> Records = new List<Record>();

		public void Add(Enum goal)
		{
			Records.Add(new Record(goal));
		}

		public void Strike(Enum goal)
		{
			foreach (var record in Records)
				if (record.Goal == goal)
					record.IsStrike = true;
		}
	}
}
