#if ANDROID
using System;
using Android.Hardware;
using Android.Content;

namespace Lime
{
	class AccelerometerListener : Java.Lang.Object, ISensorEventListener
	{
		private static AccelerometerListener listener;
		private Input input;

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
			input.Acceleration = a;
		}

		public static void StartListening(Input input)
		{
			if (listener == null) {
				listener = new AccelerometerListener() { input = input };
				var activity = Lime.ActivityDelegate.Instance.Activity;
				var sensorManager = (SensorManager)activity.GetSystemService(Context.SensorService);
				sensorManager.RegisterListener(listener, sensorManager.GetDefaultSensor(SensorType.Accelerometer), SensorDelay.Ui);
			}
		}

		public static void StopListening()
		{
			if (listener != null) {
				var activity = Lime.ActivityDelegate.Instance.Activity;
				var sensorManager = (SensorManager)activity.GetSystemService(Context.SensorService);
				sensorManager.UnregisterListener(listener);
				listener = null;
			}
		}
	}
}
#endif