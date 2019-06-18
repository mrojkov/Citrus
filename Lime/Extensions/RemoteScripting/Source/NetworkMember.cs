using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteScripting
{
	public abstract class NetworkMember
	{
		private readonly ConcurrentQueue<NetworkMessage> messagesReceiveQueue = new ConcurrentQueue<NetworkMessage>();
		private readonly ConcurrentQueue<NetworkMessage> messagesSendQueue = new ConcurrentQueue<NetworkMessage>();
		private readonly CancellationTokenSource cancellationTokenSource;
		private readonly CancellationToken cancellationToken;
		private volatile bool isSending;

		protected TcpClient TcpClient { get; set; }
		protected NetworkStream Stream => TcpClient.GetStream();

		public bool WasFailed { get; protected set; }
		public Exception FailException { get; protected set; }
		public bool IsConnected => TcpClient?.Client != null && TcpClient.Connected;
		public bool IsSending => isSending || messagesSendQueue.Count > 0;

		protected NetworkMember()
		{
			cancellationTokenSource = new CancellationTokenSource();
			cancellationToken = cancellationTokenSource.Token;
		}

		public void SendMessage(NetworkMessage message) => messagesSendQueue.Enqueue(message);

		public bool TryReceiveMessage(out NetworkMessage message) => messagesReceiveQueue.TryDequeue(out message);

		protected internal async Task ProcessConnectionAsync()
		{
			while (!cancellationToken.IsCancellationRequested) {
				if (Stream.DataAvailable) {
					var message = await NetworkMessage.ReadMessageFromStreamAsync(Stream);
					messagesReceiveQueue.Enqueue(message);
				}
				while (messagesSendQueue.TryDequeue(out var message)) {
					isSending = true;
					await NetworkMessage.WriteMessageToStreamAsync(Stream, message);
					isSending = false;
				}
				await Task.Delay(1, cancellationToken);
			}
		}

		public void Close()
		{
			cancellationTokenSource.Cancel();
			TcpClient?.Close();
		}
	}
}
