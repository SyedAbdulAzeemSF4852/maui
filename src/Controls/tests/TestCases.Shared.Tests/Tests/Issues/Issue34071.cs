using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34071 : _IssuesUITest
{
	public Issue34071(TestDevice device) : base(device) { }

	public override string Issue => "[Windows] The Shell's foreground color is not applied to the ToolbarItems";

	[Test]
	[Category(UITestCategories.Shell)]
	public void ShellForegroundColorShouldApplyToToolbarItems()
	{
		App.WaitForElement("Issue34071DescriptionLabel");
		VerifyScreenshot();
	}
}
