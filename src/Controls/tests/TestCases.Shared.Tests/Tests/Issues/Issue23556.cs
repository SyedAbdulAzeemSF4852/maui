using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Tests.Issues
{
	internal class Issue23556 : _IssuesUITest
	{
		public Issue23556(TestDevice device) : base(device)
		{
		}
		public override string Issue => "when 'ScrollToPosition' is set to 'MakeVisible' & 'Start', the item is not fully visible";
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ScrollToStart()
		{
			App.WaitForElement("collection");
			App.Tap("button");
			App.WaitForElement("Proboscis Monkey");
			VerifyScreenshot();
		}
	}
}
