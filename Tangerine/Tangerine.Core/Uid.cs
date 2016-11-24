namespace Tangerine.Core
{
	public struct Uid
	{
		private static object sync = new object();
		private static long generator;

		private long value;

		public static Uid Generate()
		{
			lock (sync) {
				return new Uid { value = ++generator };
			}
		}

		public override string ToString()
		{
			return value.ToString();
		}

		public override int GetHashCode()
		{
			return value.GetHashCode();
		}
	}
}