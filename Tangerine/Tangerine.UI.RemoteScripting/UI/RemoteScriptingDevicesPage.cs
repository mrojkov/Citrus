using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Lime;
using RemoteScripting;
using Tangerine.Core;
using Task = System.Threading.Tasks.Task;

namespace Tangerine.UI.RemoteScripting
{
	internal class RemoteScriptingDevicesPage : RemoteScriptingWidgets.TabbedWidgetPage
	{
		private readonly float rowHeight = Theme.Metrics.DefaultEditBoxSize.Y;
		private readonly List<Device> devices = new List<Device>();
		private volatile bool isHostRunning;
		private CancellationTokenSource cancellationTokenSource;
		private CancellationToken cancellationToken;
		private RemoteScriptingWidgets.Toolbar mainToolbar;
		private ThemedScrollView devicesScrollView;
		private Widget deviceWidgetPlaceholder;
		private volatile int selectedDeviceIndex = -1;

		public Device SelectedDevice => selectedDeviceIndex >= 0 ? devices[selectedDeviceIndex] : null;

		public override void Initialize()
		{
			Tab = new ThemedTab { Text = "Devices" };
			Content = new Widget {
				Layout = new VBoxLayout(),
				Nodes = {
					(mainToolbar = new RemoteScriptingWidgets.Toolbar()),
					new Widget {
						Layout = new HBoxLayout(),
						Nodes = {
							(devicesScrollView = new ThemedScrollView {
								MinMaxWidth = 300,
								TabTravesable = new TabTraversable()
							}),
							(deviceWidgetPlaceholder = new Widget() {
								Layout = new HBoxLayout()
							})
						}
					}
				}
			};
			RefreshMainToolbar();
			Content.AddChangeWatcher(
				() => CompiledAssembly.Instance,
				_ => {
					RefreshMainToolbar();
				}
			);

			void SelectDeviceBasedOnMousePosition()
			{
				devicesScrollView.SetFocus();
				var index = (devicesScrollView.Content.LocalMousePosition().Y / rowHeight).Floor();
				if (index < devices.Count) {
					SelectDevice(index);
				}
			}
			var mouseDownGesture = new ClickGesture(0);
			mouseDownGesture.Began += SelectDeviceBasedOnMousePosition;
			devicesScrollView.Gestures.Add(mouseDownGesture);
			devicesScrollView.Content.CompoundPresenter.Insert(0, new SyncDelegatePresenter<Widget>(w => {
				w.PrepareRendererState();
				Renderer.DrawRect(
					0, rowHeight * selectedDeviceIndex,
					w.Width, rowHeight * (selectedDeviceIndex + 1),
					devicesScrollView.IsFocused() ? Theme.Colors.SelectedBackground : Theme.Colors.SelectedInactiveBackground
				);
			}));
		}

		private void RefreshMainToolbar()
		{
			mainToolbar.Tasks.Stop();
			mainToolbar.Content.Nodes.Clear();

			ToolbarButton startHostButton;
			ToolbarButton stopHostButton;
			mainToolbar.Content.Nodes.AddRange(
				startHostButton = new ToolbarButton("Start Host") { Clicked = StartHost },
				stopHostButton = new ToolbarButton("Stop Host") { Clicked = StopHost }
			);
			var assembly = CompiledAssembly.Instance;
			if (assembly != null) {
				foreach (var entryPoint in assembly.PortableAssembly.EntryPoints) {
					void RemoteRunEntryPoint()
					{
						if (TryGetActiveDevice(out var device)) {
							device.RemoteProcedureCall(assembly.RawBytes, entryPoint.ClassName, entryPoint.MethodName);
						}
					}
					var button = new ToolbarButton($"{entryPoint.Summary}") { Clicked = RemoteRunEntryPoint };
					mainToolbar.Content.Nodes.Add(button);
					mainToolbar.AddChangeWatcher(
						CalcSelectedDeviceHashCode,
						_ => button.Enabled = TryGetActiveDevice(out var _)
					);
				}
			}
			mainToolbar.AddChangeWatcher(
				() => isHostRunning,
				_ => {
					startHostButton.Enabled = !isHostRunning;
					stopHostButton.Enabled = isHostRunning;
				}
			);
		}

		private void RefreshDevicesView()
		{
			var content = devicesScrollView.Content;
			content.Nodes.Clear();
			content.Layout = new TableLayout {
				ColumnCount = 1,
				ColumnSpacing = 8,
				RowCount = devices.Count,
				ColumnDefaults = new List<DefaultLayoutCell> { new DefaultLayoutCell { StretchY = 0 } }
			};
			foreach (var device in devices) {
				var labelText = device.Name + (device.WasDisconnected ? " (disconnected)" : "");
				var item = new Widget {
					MinHeight = rowHeight,
					Padding = new Thickness(2, 10, 0, 0),
					Layout = new HBoxLayout(),
					Nodes = {
						new ThemedSimpleText(labelText) {
							Id = "Label",
							LayoutCell = new LayoutCell { VerticalAlignment = VAlignment.Center }
						},
					}
				};
				content.Nodes.Add(item);
			}
			SelectDevice(selectedDeviceIndex);
		}

		private void RegisterDevice(HostClient client)
		{
			var device = new Device(client) {
				Updated = RefreshDevicesView
			};
			devices.Add(device);
			RefreshDevicesView();
			if (devices.Count == 1) {
				SelectDevice(0);
			}
		}

		private bool TryGetActiveDevice(out Device device)
		{
			device = SelectedDevice;
			if (device != null && !device.WasDisconnected) {
				return true;
			}
			device = null;
			return false;
		}

		private long CalcSelectedDeviceHashCode()
		{
			var h = new Hasher();
			h.Begin();
			var device = SelectedDevice;
			h.Write(device?.GetHashCode() ?? 0);
			h.Write(device?.WasDisconnected ?? true);
			return h.End();
		}

		private void SelectDevice(int index)
		{
			void EnsureRowVisible(int row)
			{
				while ((row + 1) * rowHeight > devicesScrollView.ScrollPosition + devicesScrollView.Height) {
					devicesScrollView.ScrollPosition++;
				}
				while (row * rowHeight < devicesScrollView.ScrollPosition) {
					devicesScrollView.ScrollPosition--;
				}
			}
			deviceWidgetPlaceholder.Nodes.Clear();
			if (devices.Count > 0) {
				index = index.Clamp(0, devices.Count - 1);
				EnsureRowVisible(index);
				selectedDeviceIndex = index;
				deviceWidgetPlaceholder.PushNode(SelectedDevice.TabbedWidget);
			} else {
				selectedDeviceIndex = -1;
			}
			Window.Current.Invalidate();
		}

		private void StartHost()
		{
			Application.Exited += StopHost;

			cancellationTokenSource = new CancellationTokenSource();
			cancellationToken = cancellationTokenSource.Token;
			async void Hosting()
			{
				isHostRunning = true;
				var host = new Host(cancellationTokenSource);
				try {
					host.Start();
					var caughtClients = new HashSet<HostClient>();
					while (!cancellationToken.IsCancellationRequested) {
						foreach (var client in host.Clients) {
							if (!caughtClients.Contains(client)) {
								caughtClients.Add(client);
								RegisterDevice(client);
							}
						}
						foreach (var device in devices) {
							if (device.WasDisconnected) {
								continue;
							}
							device.ProcessMessages();
							if (!device.Client.IsConnected) {
								caughtClients.Remove(device.Client);
								device.Disconnect();
							}
						}
						await Task.Delay(1, cancellationToken);
					}
				} catch (ObjectDisposedException) {
					// Suppress
				} catch (TaskCanceledException) {
					// Suppress
				} catch (System.Exception exception) {
					// Suppress
					System.Console.WriteLine(exception);
				} finally {
					host.Stop();
					foreach (var device in devices) {
						if (device.WasDisconnected) {
							continue;
						}
						device.ProcessMessages();
						device.Disconnect();
					}
					isHostRunning = false;
					cancellationTokenSource = null;
				}
			}
			Hosting();
		}

		private void StopHost()
		{
			Application.Exited -= StopHost;
			cancellationTokenSource?.Cancel();
		}

		public override void OnDispose() => StopHost();

		public class Device
		{
			public readonly HostClient Client;
			public readonly RemoteScriptingWidgets.TabbedWidget TabbedWidget;
			public readonly ApplicationOutputPage ApplicationOutput;

			public Action Updated;

			public string Name { get; private set; }
			public bool WasDisconnected { get; private set; }

			public Device(HostClient client)
			{
				Client = client;
				Name = Guid.NewGuid().ToString().Substring(0, 18);

				TabbedWidget = new RemoteScriptingWidgets.TabbedWidget(
					new RemoteScriptingWidgets.TabbedWidgetPage[] {
						ApplicationOutput = new ApplicationOutputPage()
					}
				);
			}

			public void Disconnect()
			{
				WasDisconnected = true;
				Updated?.Invoke();
			}

			public void RemoteProcedureCall(byte[] assemblyRawBytes, string className, string methodName)
			{
				var remoteProcedureCall = new RemoteProcedureCall {
					AssemblyRawBytes = assemblyRawBytes,
					ClassName = className,
					MethodName = methodName
				};
				Client.SendMessage(new NetworkRemoteProcedureCall(remoteProcedureCall));
			}

			public void ProcessMessages()
			{
				while (Client.TryReceiveMessage(out var message)) {
					switch (message.MessageType) {
						case NetworkMessageType.DeviceName:
							SetName((NetworkDeviceName)message);
							break;
						case NetworkMessageType.Text:
							var networkText = (NetworkText)message;
							AppendApplicationOutput(networkText.Text);
							break;
						case NetworkMessageType.RemoteFileRequest:
							var fileRequest = (NetworkRemoteFileRequest)message;
							AppendApplicationOutput($"Remote file request: \"{fileRequest.Data.Path}\"");
							var absFilePath = Path.Combine(ProjectPreferences.Instance.RemoteStoragePath, fileRequest.Data.Path);
							if (File.Exists(absFilePath)) {
								async void SendFileAsync()
								{
									byte[] bytes;
									using (var fileStream = File.Open(absFilePath, FileMode.Open)) {
										bytes = new byte[fileStream.Length];
										await fileStream.ReadAsync(bytes, 0, (int)fileStream.Length);
									}
									var remoteFile = new RemoteFile {
										Path = fileRequest.Data.Path,
										Bytes = bytes
									};
									Client.SendMessage(new NetworkRemoteFile(remoteFile));
									AppendApplicationOutput($"Requested file \"{fileRequest.Data.Path}\" was sended.");
								}
								SendFileAsync();
							} else {
								var remoteFile = new RemoteFile {
									Path = fileRequest.Data.Path,
									Bytes = null
								};
								Client.SendMessage(new NetworkRemoteFile(remoteFile));
								AppendApplicationOutput($"Can not send requested file: \"{fileRequest.Data.Path}\". File not found!");
							}
							break;
						default:
							throw new NotSupportedException($"Unknown message type: {message.MessageType}");
					}
				}
			}

			private void SetName(NetworkDeviceName message)
			{
				Name = message.Name;
				Updated?.Invoke();
			}

			private void AppendApplicationOutput(string message)
			{
				ApplicationOutput.Append(message);
			}

			public class ApplicationOutputPage : RemoteScriptingWidgets.TabbedWidgetPage
			{
				private RemoteScriptingWidgets.TextView textView;

				public override void Initialize()
				{
					Tab = new ThemedTab { Text = "Application Output" };
					Content = new Widget {
						Layout = new VBoxLayout(),
						Nodes = {
							(textView = new RemoteScriptingWidgets.TextView(maxRowsCount: 1500))
						}
					};
				}

				public void Append(string text) => textView.AppendLine(text);
			}
		}
	}
}
