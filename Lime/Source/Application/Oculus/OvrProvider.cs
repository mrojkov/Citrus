#if WIN
using OculusWrap;
using System.Linq;
using static OculusWrap.OVRTypes;

namespace Lime.Oculus
{
	public class OvrProvider
	{
		public bool Initialized { get; private set; }
		public Hmd Hmd { get; private set; }
		public OculusMirrorTexture MirrorTexture { get; private set; }
		private static readonly Vector2 WindowSize = new Vector2(1080, 1600);
		private uint frameIndex = 0;
		private Wrap Wrap;
		private RenderChain renderChain;
		private OculusWrap.LayerEyeFov LayerFov;
		private Layers Layers = new Layers();
		private Posef[] eyeRenderPoses;
		private OculusTextureSwapChain[] eyeRenderTexture;
		private StereoCamera stereoCamera;
		private IntVector2 textureSize;

		private static OvrProvider instance;
		public static OvrProvider Instance
		{
			get
			{
				if (instance == null) {
					instance = new OvrProvider();
				}
				return instance;
			}
		}

		private OvrProvider() { }

		~OvrProvider()
		{
			DisposeResources();
		}

		public void SetCurrentCamera(StereoCamera camera)
		{
			stereoCamera = camera;
			stereoCamera.LeftEye.ViewProjCalculator = CreateViewProjectionCalculator(0);
			stereoCamera.RightEye.ViewProjCalculator = CreateViewProjectionCalculator(1);
		}

		private IViewProjectionCalculator CreateViewProjectionCalculator(int eyeIndex)
		{
			return new GenericViewProjectionCalculator(
				(near, far) => Wrap.Matrix4f_Projection(Hmd.DefaultEyeFov[eyeIndex], near, far, ProjectionModifier.None).ToLime().Transpose(),
				() => {
					if (eyeRenderPoses != null) {
						var poses = eyeRenderPoses.Select(p => p.ToLime()).ToArray();
						return (Matrix44.CreateRotation(poses[eyeIndex].Orientation) *
							Matrix44.CreateTranslation(poses[eyeIndex].Position)).CalcInverted();
					} else {
						return Matrix44.Identity;
					}
				});
		}

		internal void Initialize()
		{
			if (Initialized) return;
			Wrap = new Wrap();
			// Define initialization parameters
			InitParams initializationParameters = new InitParams();
			initializationParameters.Flags = InitFlags.RequestVersion;
			initializationParameters.RequestedMinorVersion = 17;
#if DEBUG
			initializationParameters.Flags |= InitFlags.Debug;
#endif
			// Initialize the Oculus runtime.
			var success = Wrap.Initialize(initializationParameters);
			if (!success) {
				throw new OvrExcepton("Failed to initialize the Oculus runtime library.");
			}

			// Use the head mounted display.
			GraphicsLuid graphicsLuid;
			Hmd = Wrap.Hmd_Create(out graphicsLuid);
			if (Hmd == null) {
				throw new OvrExcepton("Oculus Rift not detected.");
			}

			if (Hmd.ProductName == string.Empty) {
				throw new OvrExcepton("The HMD is not enabled.");
			}
			try {
				LayerFov = Layers.AddLayerEyeFov();
				eyeRenderTexture = new OculusTextureSwapChain[2];
				for (int i = 0; i < 2; i++) {
					var eyeType = (EyeType)i;
					textureSize = Hmd.GetFovTextureSize(eyeType, Hmd.DefaultEyeFov[i], 1).ToLime();
					eyeRenderTexture[i] = new OculusTextureSwapChain(this, textureSize, eyeType);
				}
				LayerFov.Header.Type = LayerType.EyeFov;
				var result = Hmd.SetTrackingOriginType(TrackingOrigin.FloorLevel);
				CheckError(result, "Failed to set tracking origin type.");
				renderChain = new RenderChain();
				Initialized = true;
			} catch {
				DisposeResources();
				throw;
			}
		}

		public void InitializeMirrorTexture(Size size)
		{
			MirrorTexture?.Dispose();
			MirrorTexture = new OculusMirrorTexture(size);
		}

		internal void DisposeResources()
		{
			Layers?.Dispose();
			for (int eyeIndex = 0; eyeIndex < 2; ++eyeIndex) {
				eyeRenderTexture[eyeIndex]?.Dispose();
			}
			Hmd?.Dispose();
			Wrap?.Dispose();
			Initialized = false;
		}

		internal void CheckError(Result result, string message)
		{
			if (result >= Result.Success)
				return;
			var errorInformation = Wrap.GetLastError();
			var formattedMessage = string.Format("{0}. \nMessage: {1} (Error code={2})", message, errorInformation.ErrorString, errorInformation.Result);
			throw new OvrExcepton(formattedMessage);
		}

		internal void GetEyePoses(out double sensorSampleTime, out Posef[] eyeRenderPoses)
		{
			var eyeRenderDesc = new EyeRenderDesc[2];
			eyeRenderDesc[0] = Hmd.GetRenderDesc(EyeType.Left, Hmd.DefaultEyeFov[0]);
			eyeRenderDesc[1] = Hmd.GetRenderDesc(EyeType.Right, Hmd.DefaultEyeFov[1]);

			// Get eye poses, feeding in correct IPD offset
			eyeRenderPoses = new Posef[2];
			Vector3f[] hmdToEyeOffsets = { eyeRenderDesc[0].HmdToEyeOffset, eyeRenderDesc[1].HmdToEyeOffset };

			Hmd.GetEyePoses(frameIndex, true, hmdToEyeOffsets, ref eyeRenderPoses, out sensorSampleTime);
		}

		public void Render(WindowWidget widget)
		{
			if (eyeRenderTexture == null) return;
			var oldSize = widget.Size;
			widget.Size = WindowSize;
			try {
				double sensorSampleTime;
				GetEyePoses(out sensorSampleTime, out eyeRenderPoses);
				Renderer.Viewport = new WindowRect { X = 0, Y = 0, Size = textureSize };
				for (int eyeIndex = 0; eyeIndex < 2; eyeIndex++) {
					stereoCamera?.SetActive(eyeIndex);
					eyeRenderTexture[eyeIndex].SetAsRenderTarget();
					widget.RenderChainBuilder?.AddToRenderChain(renderChain);
					widget.RenderToTexture(eyeRenderTexture[eyeIndex].CurrentTexture, renderChain);
					eyeRenderTexture[eyeIndex].Commit();
				}

				for (int eyeIndex = 0; eyeIndex < 2; eyeIndex++) {
					LayerFov.ColorTexture[eyeIndex] = eyeRenderTexture[eyeIndex].TextureChain.TextureSwapChainPtr;
					LayerFov.Viewport[eyeIndex].Position = new Vector2i(0, 0);
					var size = eyeRenderTexture[eyeIndex].Size;
					LayerFov.Viewport[eyeIndex].Size = new Sizei(size.X, size.Y);
					LayerFov.Fov[eyeIndex] = Hmd.DefaultEyeFov[eyeIndex];
					LayerFov.RenderPose[eyeIndex] = eyeRenderPoses[eyeIndex];
					LayerFov.SensorSampleTime = sensorSampleTime;
				}

				var result = Hmd.SubmitFrame(frameIndex, Layers);
				CheckError(result, "Unable to submit frame.");
				var sessionStatus = new SessionStatus();
				Hmd.GetSessionStatus(ref sessionStatus);
				if (sessionStatus.ShouldQuit == 1)
					throw new OvrExcepton("SessionStatus: ShouldQuit.");
				frameIndex++;
			} catch {
				DisposeResources();
				throw;
			} finally {
				widget.Size = oldSize;
			}
		}

		public struct EyePose
		{
			public Vector3 Position { get; set; }
			public Quaternion Orientation { get; set; }
		}
	}
}
#endif