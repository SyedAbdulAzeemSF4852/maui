﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue18961 : _IssuesUITest
	{
		const string ScrollButton = "ScrollButton";
		const string LastEntry = "TestEntry20";

		public Issue18961(TestDevice device) : base(device) { }

		public override string Issue => "Modal Page margin correct after Keyboard opens";

		[Test]
		public async Task ModalPageMarginCorrectAfterKeyboardOpens()
		{
			this.IgnoreIfPlatforms(new[]
			{
				TestDevice.iOS, TestDevice.Mac, TestDevice.Windows
			});

			App.WaitForElement("WaitForStubControl");

			// 1. Ensure that the keyboard is closed before we start.
			App.DismissKeyboard();

			// 2. Scroll to the end.
			App.WaitForElement(ScrollButton);
			App.Click(ScrollButton);
			await Task.Delay(1000); // Wait for the scroll animation to finish.
			App.ScrollDown("TestScrollView"); // Ensure that we are at the end of the scroll.

			// 3. Focus latest Entry
			App.WaitForElement(LastEntry);
			App.EnterText(LastEntry, "test");
			App.Click(LastEntry);
			await Task.Delay(1000);

			// 4. The keyboard has opened and the Entry have been translated above the keyboard.
			App.Screenshot("The keyboard has opened and the Entry have been translated above the keyboard.");

			// 5. Close the keyboard to see if sizes adjust back.
			App.DismissKeyboard();
			await Task.Delay(1000);

			// 6. Verify the latest Entry text.
			var text = App.FindElement(LastEntry).GetText();
			Assert.AreEqual(text, "test");

			// 7. Make sure that everything has returned to the initial size once the keyboard has closed.
			App.Screenshot("Make sure that everything has returned to the initial size once the keyboard has closed.");
		}
	}
}