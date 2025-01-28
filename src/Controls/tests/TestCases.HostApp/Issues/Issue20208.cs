using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Maui.Controls.Sample.Issues;
using Microsoft.Maui.Layouts;

namespace Controls.TestCases.HostApp.Issues
{
	[Issue(IssueTracker.Github, 20208, "When minimumheightrequest is set on label, the label is not vertically centered in Windows", PlatformAffected.UWP)]
	public class Issue20208 : TestContentPage
	{
		public Issue20208()
		{

		}

		protected override void Init()
		{
			FlexLayout flexLayout = new FlexLayout
			{
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center,
				BackgroundColor = Colors.Red,
				HeightRequest = 50,
			};
			Label label = new Label
			{
				AutomationId = "Label",
				VerticalOptions = LayoutOptions.Fill,
				VerticalTextAlignment = TextAlignment.Center,
				MinimumHeightRequest = 39,
				BackgroundColor = Colors.Yellow,
				Text = "Hello, World!"
			};

			flexLayout.Children.Add(label);

			Content = flexLayout;
		}
	}
}
