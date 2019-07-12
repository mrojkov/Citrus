using System;

namespace RemoteScripting
{
	public static class NetworkSettings
	{
		public const int Port = 26616;
		public static readonly TimeSpan PingInterval = TimeSpan.FromSeconds(2);
	}
}
