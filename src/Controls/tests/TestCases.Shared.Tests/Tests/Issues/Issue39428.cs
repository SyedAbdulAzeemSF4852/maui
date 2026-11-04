#if WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue39428 : _IssuesUITest
{
	public Issue39428(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Windows] Re-entrant activation during Window.Resumed";

	[Test]
	[Category(UITestCategories.Window)]
	public void ReentrantDeactivationDuringResumedDoesNotCorruptActivationState()
	{
		App.WaitForElement("RunTestButton");
		App.Tap("RunTestButton");

		var result = App.WaitForElement("ResultLabel").GetText();
		var status = App.WaitForElement("StatusLabel").GetText();

		Assert.That(result, Is.EqualTo("PASSED"),
			$"Re-entrant activation failed. Status: {status}, Result: {result}");
	}
}
#endif
