#if WIN
using OculusWrap;
using System.Linq;
using static OculusWrap.OVRTypes;

namespace Lime.Oculus
{
	public class OvrProvider
	{
		private Wrap Wrap;
		private RenderChain renderChain;
		private OculusWrap.LayerEyeFov LayerFov;
		private Layers Layers = new Layers();
		private uint frameIndex = 0;
		private double sensorSampleTime;
		private Posef[] eyeRenderPoses;
		public Hmd Hmd;
		public IntVector2 TextureSize { get; private set; }
		public OculusTextureSwapChain[] EyeRenderTexture { get; private set; }
		public StereoCamera StereoCamera { get; private set; }
		public Viewport3D Viewport;

		private OvrProvider() { }

		~OvrProvider()
		{
			ReleaseResources();
		}

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

		public void SetupViewportAndCamera(Viewport3D vp, StereoCamera camera)
		{
			StereoCamera = camera;
			StereoCamera.LeftEye.ViewProjCalculator = CreateViewProjectionCalculator(0);
			StereoCamera.RightEye.ViewProjCalculator = CreateViewProjectionCalculator(1);
			Viewport = vp;
		}

		private IViewProjectionCalculator CreateViewProjectionCalculator(int eyeIndex)
		{
			return new GenericViewProjectionCalculator(
				(near, far) => GetProjection(eyeIndex, near, far),
				() => GetView(eyeIndex));
		}

		public void Initialize()
		{
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
				EyeRenderTexture = new OculusTextureSwapChain[2];
				for (int i = 0; i < 2; i++) {
					var eyeType = (EyeType)i;
					TextureSize = Hmd.GetFovTextureSize(eyeType, Hmd.DefaultEyeFov[i], 1).ToLime();
					EyeRenderTexture[i] = new OculusTextureSwapChain(this, TextureSize, eyeType);
				}
				LayerFov.Header.Type = LayerType.EyeFov;
				var result = Hmd.SetTrackingOriginType(TrackingOrigin.FloorLevel);
				WriteErrorDetails(result, "Failed to set tracking origin type.");
				renderChain = new RenderChain();
			} catch {
				ReleaseResources();
				throw;
			}
		}

		public void ReleaseResources()
		{
			Layers?.Dispose();
			for (int eyeIndex = 0; eyeIndex < 2; ++eyeIndex) {
				EyeRenderTexture[eyeIndex]?.Dispose();
			}
			Hmd?.Dispose();
			Wrap?.Dispose();
		}

		public void WriteErrorDetails(Result result, string message)
		{
			if (result >= Result.Success)
				return;

			// Retrieve the error message from the last occurring error.
			var errorInformation = Wrap.GetLastError();

			var formattedMessage = string.Format("{0}. \nMessage: {1} (Error code={2})", message, errorInformation.ErrorString, errorInformation.Result);
			throw new OvrExcepton(formattedMessage);
		}

		public Matrix44 GetProjection(int eyeIndex, float near, float far)
		{
			return Wrap.Matrix4f_Projection(Hmd.DefaultEyeFov[eyeIndex], near, far, ProjectionModifier.None).ToLime().Transpose();
		}

		internal void GetEyePoses(out double sensorSampleTime, out Posef[] eyeRenderPoses)
		{
			var eyeRenderDesc = new EyeRenderDesc[2];
			eyeRenderDesc[0] = Hmd.GetRenderDesc(EyeType.Left, Hmd.DefaultEyeFov[0]);
			eyeRenderDesc[1] = Hmd.GetRenderDesc(EyeType.Right, Hmd.DefaultEyeFov[1]);

			// Get eye poses, feeding in correct IPD offset
			eyeRenderPoses = new Posef[2];
			Vector3f[] hmdToEyeOffsets = { eyeRenderDesc[0].HmdToEyeOffset, eyeRenderDesc[1].HmdToEyeOffset };

			// Keeping sensorSampleTime as close to ovr_GetTrackingState as possible - fed into the layer
			Hmd.GetEyePoses(frameIndex, true, hmdToEyeOffsets, ref eyeRenderPoses, out sensorSampleTime);
		}

		public void Render(Widget widget)
		{
			if (EyeRenderTexture == null) return;
			try {
				GetEyePoses(out sensorSampleTime, out eyeRenderPoses);
				for (int eyeIndex = 0; eyeIndex < 2; eyeIndex++) {
					Renderer.Viewport = new WindowRect { X = 0, Y = 0, Size = TextureSize };
					StereoCamera?.SetActive(eyeIndex);
					EyeRenderTexture[eyeIndex].SetAsRenderTarget();
					widget.RenderChainBuilder?.AddToRenderChain(renderChain);
					widget.RenderToTexture(EyeRenderTexture[eyeIndex].CurrentTexture, renderChain);
					EyeRenderTexture[eyeIndex].Commit();
				}

				for (int eyeIndex = 0; eyeIndex < 2; eyeIndex++) {
					LayerFov.ColorTexture[eyeIndex] = EyeRenderTexture[eyeIndex].TextureChain.TextureSwapChainPtr;
					LayerFov.Viewport[eyeIndex].Position = new Vector2i(0, 0);
					var size = EyeRenderTexture[eyeIndex].Size;
					LayerFov.Viewport[eyeIndex].Size = new Sizei(size.X, size.Y);
					LayerFov.Fov[eyeIndex] = Hmd.DefaultEyeFov[eyeIndex];
					LayerFov.RenderPose[eyeIndex] = eyeRenderPoses[eyeIndex];
					LayerFov.SensorSampleTime = sensorSampleTime;
				}

				var result = Hmd.SubmitFrame(frameIndex, Layers);
				WriteErrorDetails(result, "Unable to submit frame.");
				var sessionStatus = new SessionStatus();
				Hmd.GetSessionStatus(ref sessionStatus);
				if (sessionStatus.ShouldQuit == 1)
					throw new OvrExcepton("SessionStatus: ShouldQuit.");
				frameIndex++;
			} catch {
				ReleaseResources();
				throw;
			}
		}

		public Matrix44 GetView(int eyeIndex)
		{
			if (eyeRenderPoses != null) {
				var poses = eyeRenderPoses.Select(p => p.ToLime()).ToArray();
				var tState = Hmd.GetTrackingState(sensorSampleTime, true);
				return (Matrix44.CreateRotation(poses[eyeIndex].Orientation) *
					Matrix44.CreateTranslation(poses[eyeIndex].Position)).CalcInverted();
			} else {
				return Matrix44.Identity;
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