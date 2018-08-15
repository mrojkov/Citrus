#if ANDROID
using System;

using Android.Hardware;
using Android.Runtime;
using Android.Views;
using AndroidApp = Android.App.Application;
using AndroidContext = Android.Content.Context;

namespace Lime
{
	class AccelerometerListener : Java.Lang.Object, ISensorEventListener
	{
		private static AccelerometerListener listener;
		private static IWindowManager windowManager =
			AndroidApp.Context.GetSystemService(AndroidContext.WindowService).JavaCast<IWindowManager>();
		private Input input;

		private static bool isActive = true;

		public static bool IsActive
		{
			get { return isActive; }
			set {
				isActive = value;
				if (isActive) {
					StartListening(Lime.Application.Input);
				} else {
					StopListening();
				}
			}
		}

		public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy) { }

		public void OnSensorChanged(SensorEvent e)
		{
			if (e.Values == null) {
				throw new InvalidOperationException();
			}

			var vector = new Vector3();
			if (e.Values.Count > 0) {
				vector.X = e.Values[0];
			}
			if (e.Values.Count > 1) {
				vector.Y = e.Values[1];
			}
			if (e.Values.Count > 2) {
				vector.Z = e.Values[2];
			}

			// Translate into iOS accelerometer units: g-forces instead of m/s^2
			var a = -vector / 9.81f;
			input.NativeAcceleration = a;

			switch (windowManager.DefaultDisplay.Rotation) {
				case SurfaceOrientation.Rotation0:
					input.Acceleration = new Vector3(a.X, a.Y, a.Z);
					break;
				case SurfaceOrientation.Rotation180:
					input.Acceleration = new Vector3(-a.X, -a.Y, a.Z);
					break;
				case SurfaceOrientation.Rotation270:
					input.Acceleration = new Vector3(a.Y, -a.X, a.Z);
					break;
				case SurfaceOrientation.Rotation90:
					input.Acceleration = new Vector3(-a.Y, a.X, a.Z);
					break;
			}
		}

		public static void StartListening(Input input)
		{
			if (listener == null && isActive) {
				listener = new AccelerometerListener { input = input };
				var activity = ActivityDelegate.Instance.Activity;
				var sensorManager = (SensorManager)activity.GetSystemService(AndroidContext.SensorService);
				sensorManager.RegisterListener(
					listener,
					sensorManager.GetDefaultSensor(SensorType.Accelerometer),
					SensorDelay.Ui);
			}
		}

		public static void StopListening()
		{
			if (listener != null) {
				var activity = ActivityDelegate.Instance.Activity;
				var sensorManager = (SensorManager)activity.GetSystemService(AndroidContext.SensorService);
				sensorManager.UnregisterListener(listener);
				listener = null;
			}
		}
	}
}
#endif