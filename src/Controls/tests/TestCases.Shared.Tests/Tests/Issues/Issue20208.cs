using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework.Legacy;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Tests.Issues
{
    internal class Issue20208 : _IssuesUITest
    {
		public Issue20208(TestDevice device) : base(device) { }
		public override string Issue => "When minimumheightrequest is set on label, the label is not vertically centered in Windows";
		[Test]
		[Category(UITestCategories.Label)]
		public void LabelMinHeight()
		{
			App.WaitForElement("Label");
			//VerifyScreenshot();
		}
	}
}
