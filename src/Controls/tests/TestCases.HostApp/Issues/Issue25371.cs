using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 25371, "OnNavigatedTo not called when navigating back to a specific page", PlatformAffected.iOS | PlatformAffected.macOS)]
	public class Issue25371 : NavigationPage
	{
		public Issue25371() : base(new FirstPage25371())
		{
		}
	}
	public class FirstPage25371 : ContentPage
	{
		Label label;
		public FirstPage25371()
		{
			var stackLayout = new StackLayout();
			label = new Label()
			{
				Text = "Welcome to Main page",
			};

			var button = new Button() { Text = "MoveToNextPage", AutomationId = "MoveToNextPage" };
			button.Clicked += Button_Clicked;
			stackLayout.Children.Add(label);
			stackLayout.Children.Add(button);
			Content = stackLayout;

		}

		protected override void OnNavigatedTo(NavigatedToEventArgs args)
		{
			var stackLayout = (StackLayout)Content;
			stackLayout.Add(new Label
			{
				AutomationId = "FirstPageLabel",
				Text = "OnNavigatedTo method is called"
			});

			base.OnNavigatedTo(args);
		}

		private void Button_Clicked(object sender, EventArgs e)
		{
			var stackLayout = (StackLayout)Content;
			// remove the navigated label when leaving the page
			if (stackLayout.OfType<Label>().FirstOrDefault(l => l.AutomationId == "FirstPageLabel") is { } label)
			{
				stackLayout.Children.Remove(label);
			}

			Navigation.PushAsync(new SecondPage25371());
		}
	}

	public class SecondPage25371 : ContentPage
	{
		public SecondPage25371()
		{
			var label = new Label()
			{
				AutomationId = "SecondPageLabel",
				Text = "Welcome to Second Page",
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center
			};

			Content = label;
		}
	}
}

