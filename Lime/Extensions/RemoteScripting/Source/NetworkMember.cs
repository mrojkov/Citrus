using System;
using System.Collections.Concurrent;
using System.Diagnostics;
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
		private volatile bool wasVerified;
		private volatile bool isSending;

		protected TcpClient TcpClient { get; set; }
		protected NetworkStream Stream => TcpClient.GetStream();

		public bool WasFailed { get; protected set; }
		public Exception FailException { get; protected set; }
		public bool IsConnected => TcpClient?.Client != null && TcpClient.Connected;
		public bool WasVerified => wasVerified;
		public bool IsSending => isSending || messagesSendQueue.Count > 0;

		protected NetworkMember()
		{
			cancellationTokenSource = new CancellationTokenSource();
			cancellationToken = cancellationTokenSource.Token;
		}

		public void SendMessage(NetworkMessage message) => messagesSendQueue.Enqueue(message);

		public bool TryReceiveMessage(out NetworkMessage message) => messagesReceiveQueue.TryDequeue(out message);

		internal void EnqueueReceivedMessage(NetworkMessage message) => messagesReceiveQueue.Enqueue(message);

		protected internal async Task ProcessConnectionAsync()
		{
			var stopWatch = new Stopwatch();
			stopWatch.Start();
			while (!cancellationToken.IsCancellationRequested) {
				while (Stream.DataAvailable) {
					var message = await NetworkMessage.ReadMessageFromStreamAsync(Stream).ConfigureAwait(continueOnCapturedContext: false);
					if (!(message is NetworkPing)) {
						messagesReceiveQueue.Enqueue(message);
					}
					wasVerified = true;
				}
				if (stopWatch.Elapsed > NetworkSettings.PingInterval) {
					SendMessage(new NetworkPing());
					stopWatch.Restart();
				}
				while (messagesSendQueue.TryDequeue(out var message)) {
					isSending = true;
					await NetworkMessage.WriteMessageToStreamAsync(Stream, message).ConfigureAwait(continueOnCapturedContext: false);
					isSending = false;
				}
				await Task.Delay(1, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
		}

		public void Close()
		{
			cancellationTokenSource.Cancel();
			TcpClient?.Close();
		}
	}
}
