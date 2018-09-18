using NUnit.Framework;

namespace Lime.Tests.Source.Types
{
	[TestFixture]
	public class BitSet32Tests
	{
		[Test]
		public void AnyTest()
		{
			var bitSet = new BitSet32(0);
			Assert.That(bitSet.Any(), Is.False);
			bitSet[0] = true;
			Assert.That(bitSet.Any(), Is.True);
		}

		[Test]
		public void AllTest()
		{
			var bitSet = new BitSet32(uint.MaxValue);
			Assert.That(bitSet.All(), Is.True);
			bitSet[0] = false;
			Assert.That(bitSet.All(), Is.False);
		}

		[Test]
		public void EqualsTest()
		{
			var bitSet1 = new BitSet32(0);
			var bitSet2 = new BitSet32(1);
			Assert.That(bitSet1.Equals(bitSet2), Is.False);
			Assert.That(bitSet1 != bitSet2);
			bitSet1[0] = true;
			Assert.That(bitSet1[0].Equals(bitSet2[0]));
			Assert.That(bitSet1 == bitSet2);
			Assert.That(bitSet1.Equals((object)bitSet2));
			Assert.That(bitSet1.GetHashCode(), Is.EqualTo(bitSet2.GetHashCode()));
		}

		[Test]
		public void ToStringTest()
		{
			Assert.That(new BitSet32(0).ToString(), Is.EqualTo("0"));
			Assert.That(new BitSet32(1).ToString(), Is.EqualTo("1"));
			Assert.That(new BitSet32(2).ToString(), Is.EqualTo("10"));
			Assert.That(new BitSet32(1025).ToString(), Is.EqualTo("10000000001"));
			Assert.That(new BitSet32(uint.MaxValue).ToString(), Is.EqualTo("11111111111111111111111111111111"));
		}
	}
}
