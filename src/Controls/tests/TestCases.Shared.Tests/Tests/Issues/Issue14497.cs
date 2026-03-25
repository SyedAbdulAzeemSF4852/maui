using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue14497 : _IssuesUITest
{
	public Issue14497(TestDevice device) : base(device) { }

	public override string Issue => "Dynamically setting SearchHandler Query property does not update text in the search box";

	[Test]
	[Category(UITestCategories.Shell)]
	public void DynamicallySettingQueryUpdatesSearchBox()
	{
		App.WaitForElement("ChangeSearchText");
		App.Tap("ChangeSearchText");
		var text = App.GetShellSearchHandler().GetText();
		Assert.That(text, Is.EqualTo("Hello World"));
	}
}
