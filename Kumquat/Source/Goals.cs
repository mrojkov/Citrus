
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
			public int Goal;

			[ProtoMember(2)]
			public bool IsStrike;

			public Record(Enum goal)
			{
                Goal = Convert.ToInt32(goal);
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
            int value = Convert.ToInt32(goal);
            foreach (var record in Records)
                if (record.Goal == value)
					record.IsStrike = true;
		}
	}
}
