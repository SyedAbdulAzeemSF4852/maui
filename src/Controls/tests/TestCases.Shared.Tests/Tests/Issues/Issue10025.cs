using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue10025 : _IssuesUITest
{
	public Issue10025(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Assigning null to the SelectedItem of the CollectionView in the SelectionChanged event does not clear the selection as expected";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectedItemClearsOnNullAssignment()
	{
		App.Tap("Item2");
		VerifyScreenshot();
	}
}