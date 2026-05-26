namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35511, "[iOS 26] BackButtonTitle is hidden when pushed page sets a custom TitleView", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue35511 : TestNavigationPage
{
	protected override void Init()
	{
		var pushButton = new Button
		{
			Text = "Push Page With TitleView",
			AutomationId = "PushPageButton"
		};

		var rootPage = new ContentPage
		{
			Title = "Root",
			Content = new VerticalStackLayout
			{
				Padding = 20,
				Spacing = 12,
				Children = { pushButton }
			}
		};
		NavigationPage.SetBackButtonTitle(rootPage, "Main");

		pushButton.Clicked += (_, _) => Navigation.PushAsync(new Issue35511PushedPage());

		PushAsync(rootPage);
	}
}

public class Issue35511PushedPage : ContentPage
{
	public Issue35511PushedPage()
	{
		Title = "Custom Title";

		NavigationPage.SetTitleView(this, new HorizontalStackLayout
		{
			Spacing = 8,
			AutomationId = "CustomTitleView",
			Children =
			{
				new Image { Source = "dotnet_bot.png", WidthRequest = 24, HeightRequest = 24 },
				new Label { Text = "Custom Title", FontSize = 16, VerticalOptions = LayoutOptions.Center }
			}
		});

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 12,
			Children =
			{
				new Label { Text = "This test passes if the back button title \"Main\" is visible next to the back arrow, else fails.", AutomationId = "PushedPageLabel" }
			}
		};
	}
}
