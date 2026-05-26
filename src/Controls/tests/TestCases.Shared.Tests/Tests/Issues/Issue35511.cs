#if IOS || MACCATALYST // BackButtonTitle is a UIKit-only feature; this issue does not affect Android or Windows.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35511 : _IssuesUITest
{
	public Issue35511(TestDevice device) : base(device) { }

	public override string Issue => "[iOS 26] BackButtonTitle is hidden when pushed page sets a custom TitleView";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void BackButtonTitleVisibleWhenPushedPageHasCustomTitleView()
	{
		App.WaitForElement("PushPageButton");
		App.Tap("PushPageButton");

		App.WaitForElement("PushedPageLabel");
		VerifyScreenshot();
	}
}
#endif
