﻿using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;


namespace UITests
{
	public class Issue2399 : IssuesUITest
	{
		const string AllEventsHaveDetached = "AllEventsHaveDetached";

		public Issue2399(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Label Renderer Dispose never called";

		[Test]
		[Category(UITestCategories.Label)]
		public void WaitForAllEffectsToDetach()
		{
			this.IgnoreIfPlatforms([TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement(AllEventsHaveDetached);
			var text = RunningApp.FindElement(AllEventsHaveDetached).GetText();
			ClassicAssert.NotNull(text);
			ClassicAssert.AreEqual("Success", text);
		}
	}
}