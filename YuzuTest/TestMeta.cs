using Microsoft.VisualStudio.TestTools.UnitTesting;

using ProtoBuf;

using Yuzu;
using Yuzu.Metadata;

namespace YuzuTest.Metadata
{
	[TestClass]
	public class TestMeta
	{
		[TestMethod]
		public void TestAttributes()
		{
			var opt1 = new CommonOptions {
				Meta = new MetaOptions { }
			};
			var opt2 = new CommonOptions {
				Meta = new MetaOptions {
					RequiredAttribute = typeof(ProtoMemberAttribute),
					OptionalAttribute = null,
					MemberAttribute = null,
					GetAlias = attr => (attr as ProtoMemberAttribute).Tag.ToString(),
				}
			};
			var m1 = Meta.Get(typeof(Sample1), opt1);
			var m2 = Meta.Get(typeof(Sample1), opt2);
			Assert.AreNotEqual(m1, m2);

			Assert.AreEqual(2, m1.Items.Count);
			Assert.AreEqual("X", m1.Items[0].Tag(opt1));
			Assert.AreEqual("Y", m1.Items[1].Tag(opt1));

			Assert.AreEqual(2, m2.Items.Count);
			Assert.AreEqual("1", m2.Items[0].Tag(opt2));
			Assert.AreEqual("2", m2.Items[1].Tag(opt2));
		}

		[YuzuMust(YuzuItemKind.Field)]
		internal class MustField
		{
			[YuzuOptional]
			public int F1 = 0;
			public int P1 { get; set; }
			public int F2 = 0;
		}

		[YuzuMust(YuzuItemKind.Property)]
		internal class MustProperty
		{
			public int F1 = 0;
			[YuzuOptional]
			public int P1 { get; set; }
			public int P2 { get; set; }
		}

		[YuzuMust]
		internal class MustPrivate
		{
			[YuzuRequired]
			public int F1 = 0;
			[YuzuRequired]
			public int P1 { get; set; }
			private int F2 = 0;
			private int P2 { get; set; }
		}

		[TestMethod]
		public void TestMust()
		{
			var opt1 = new CommonOptions();
			XAssert.Throws<YuzuException>(() => Meta.Get(typeof(MustField), opt1), "F2");
			XAssert.Throws<YuzuException>(() => Meta.Get(typeof(MustProperty), opt1), "P2");
			Assert.AreEqual(2, Meta.Get(typeof(MustPrivate), opt1).Items.Count);
		}

		[YuzuAll]
		internal class AllDefault
		{
			public int F1 = 0;
			public int P1 { get; set; }
			private int F2 = 0;
			private int P2 { get; set; }
			[YuzuRequired]
			public int R = 0;
		}

		[YuzuAll(YuzuItemOptionality.Required, YuzuItemKind.Field)]
		internal class AllRequiredFields
		{
			public int F1 = 0;
			public int P1 { get; set; }
			private int F2 = 0;
			private int P2 { get; set; }
			[YuzuMember]
			public int M = 0;
		}

		[TestMethod]
		public void TestAll()
		{
			var opt1 = new CommonOptions();
			var m1 = Meta.Get(typeof(AllDefault), opt1);
			Assert.AreEqual(3, m1.Items.Count);
			Assert.IsTrue(m1.Items[0].IsOptional);
			Assert.IsTrue(m1.Items[1].IsOptional);
			Assert.IsFalse(m1.Items[2].IsOptional);

			var m2 = Meta.Get(typeof(AllRequiredFields), opt1);
			Assert.AreEqual(2, m2.Items.Count);
			Assert.IsFalse(m2.Items[0].IsOptional);
			Assert.IsTrue(m2.Items[1].IsOptional);
		}

		[YuzuAll]
		internal class Exclude
		{
			public int F1 = 0;
			public int P1 { get; set; }
			[YuzuExclude]
			public int F2 = 0;
			[YuzuExclude]
			public int P2 { get; set; }
			[YuzuRequired, YuzuExclude]
			public int R = 0;
		}

		[TestMethod]
		public void TestExclude()
		{
			var opt1 = new CommonOptions();
			var m1 = Meta.Get(typeof(Exclude), opt1);
			Assert.AreEqual(2, m1.Items.Count);
			Assert.AreEqual("F1", m1.Items[0].Tag(opt1));
			Assert.AreEqual("P1", m1.Items[1].Tag(opt1));
		}

		[YuzuAllowReadingFromAncestor]
		internal class Sample1Bad: Sample1
		{
			[YuzuMember]
			public int F1 = 0;
		}

		[TestMethod]
		public void TestAllowReadingFromAncestorBad()
		{
			XAssert.Throws<YuzuException>(() => Meta.Get(typeof(Sample1Bad), new CommonOptions()), "3");
		}
	}
}
