#if WIN
using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using Lime;
using Tangerine.Core;

namespace Tangerine
{
	public class TangerineSingleInstanceKeeper
	{
		private const string MutexName = "TangerineMutex";
		private const string PipeServerName = "TangerinePipe";
		private static Mutex mutex;

		public static TangerineSingleInstanceKeeper Instance { get; private set; }
		public static event Action<string[]> AnotherInstanceArgsRecieved;

		private readonly NamedPipeManager pipeManager;

		public static void Initialize(string[] args)
		{
			if (Instance != null) {
				throw new InvalidOperationException();
			}
			Instance = new TangerineSingleInstanceKeeper(args);
		}

		private TangerineSingleInstanceKeeper(string[] args)
		{
			bool isOnlyInstance;
			mutex = new Mutex(true, MutexName, out isOnlyInstance);
			if (!isOnlyInstance) {
				if (args.Length > 0) {
					var stream = new MemoryStream();
					TangerinePersistence.Instance.WriteObject(string.Empty, stream, args, Persistence.Format.Json);
					var serializedArgs = Encoding.UTF8.GetString(stream.ToArray());
					var manager = new NamedPipeManager(PipeServerName);
					manager.Write(serializedArgs);
				}

				System.Environment.Exit(0);
			}

			pipeManager = new NamedPipeManager(PipeServerName);
			pipeManager.StartServer();
			pipeManager.ReceiveString += OnAnotherInstanceArgsRecieved;
		}

		public void ReleaseInstance()
		{
			mutex.ReleaseMutex();
			pipeManager?.StopServer();
		}

		private static void OnAnotherInstanceArgsRecieved(string serializedArgs)
		{
			var stream = new MemoryStream(Encoding.UTF8.GetBytes(serializedArgs));
			try {
				var persistence = new Persistence();
				var args = persistence.ReadObject<string[]>(string.Empty, stream);
				Application.InvokeOnMainThread(() => {
					WindowsFormActivator.Activate((Window)Application.MainWindow);
					AnotherInstanceArgsRecieved?.Invoke(args);
				});
			} catch {
				// ignored
			}
		}

		private static class WindowsFormActivator
		{
			[System.Runtime.InteropServices.DllImport("user32.dll")]
			private static extern int ShowWindow(IntPtr hWnd, uint Msg);

			private const uint SW_RESTORE = 0x09;

			public static void Activate(Window window)
			{
				if (window.Form.WindowState == System.Windows.Forms.FormWindowState.Minimized) {
					ShowWindow(window.Form.Handle, SW_RESTORE);
				}
				window.Activate();
			}
		}

		private class NamedPipeManager
		{
			private const string ExitString = "__EXIT__";

			private readonly CancellationTokenSource source;
			private readonly CancellationToken token;
			private readonly string name;
			private Thread thread;

			public event Action<string> ReceiveString;

			public NamedPipeManager(string name)
			{
				this.name = name;
				source = new CancellationTokenSource();
				token = source.Token;
			}

			public void StartServer()
			{
				thread = new Thread(pipeName =>
				{
					while (true) {
						string text;
						using (var server = new NamedPipeServerStream((string)pipeName)) {
							server.WaitForConnection();
							using (var reader = new StreamReader(server)) {
								text = reader.ReadToEnd();
							}
						}
						if (text == ExitString || token.IsCancellationRequested) {
							break;
						}
						OnReceiveString(text);
					}
				});
				thread.Start(name);
			}

			public void StopServer()
			{
				source.Cancel();
				Write(ExitString);
				const int CancelationTimeout = 30;
				thread.Join(CancelationTimeout);
			}

			public bool Write(string text, int connectTimeout = 300)
			{
				using (var client = new NamedPipeClientStream(name)) {
					try {
						client.Connect(connectTimeout);
					} catch {
						return false;
					}

					if (!client.IsConnected) {
						return false;
					}

					using (var writer = new StreamWriter(client)) {
						writer.Write(text);
						writer.Flush();
					}
				}
				return true;
			}

			private void OnReceiveString(string text) => ReceiveString?.Invoke(text);
		}
	}
}
#endif
