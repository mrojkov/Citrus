using System;
using System.Net;
using System.Net.Sockets;

namespace RemoteScripting
{
	public class Client : NetworkMember
	{
		public Client()
		{
			TcpClient = new TcpClient();
		}

		public async void Connect(IPAddress ipAddress)
		{
			try {
				await TcpClient.ConnectAsync(ipAddress, NetworkSettings.Port).ConfigureAwait(continueOnCapturedContext: false);
			} catch (Exception exception) {
				WasFailed = true;
				TcpClient?.Close();
				FailException = exception;
				return;
			}

			try {
				await ProcessConnectionAsync().ConfigureAwait(continueOnCapturedContext: false);
			} catch (Exception exception) {
				WasFailed = true;
				TcpClient?.Close();
				FailException = exception;
				return;
			}

			Stream.Close();
			TcpClient.Close();
		}
	}
}
