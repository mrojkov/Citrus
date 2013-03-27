using System;
using MonoTouch.Foundation;

namespace TestFlight {
	
	[BaseType (typeof (NSObject))]
	public partial interface TestFlight {
		[Static, Export ("addCustomEnvironmentInformation:forKey:")]
		void AddCustomEnvironmentInformation (string information, string key);
		
		[Static, Export ("takeOff:")]
		void TakeOff (string teamToken);
		
		// The values that the dictionary accepts:
		// NSString ("reinstallCrashHandlers") -> NSNumber.Boolean, set to true to reinstall the crash handlers, in case some other library does
		[Static, Export ("setOptions:")]
		void SetOptions (NSDictionary options);
		
		[Static, Export ("passCheckpoint:")]
		void PassCheckpoint (string checkpointName);
		
		[Static, Export ("openFeedbackView")]
		void OpenFeedbackView ();
		
		[Static, Export ("submitFeedback:")]
		void SubmitFeedback (string feedback);
		
		[Static, Export ("setDeviceIdentifier:")]
		void SetDeviceIdentifier (string deviceIdentifer);
	}
}