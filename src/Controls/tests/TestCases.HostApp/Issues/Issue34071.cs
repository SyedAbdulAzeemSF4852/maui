namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34071, "[Windows] The Shell's foreground color is not applied to the ToolbarItems", PlatformAffected.UWP)]
public class Issue34071 : TestShell
{
	protected override void Init()
	{
		Shell.SetForegroundColor(this, Colors.Purple);

		ContentPage page = new ContentPage { Title = "Issue 34071" };

		page.ToolbarItems.Add(new ToolbarItem
		{
			Text = "Action",
			IconImageSource = "calculator.png",
			Order = ToolbarItemOrder.Primary,
			AutomationId = "PrimaryItem",
		});

		page.Content = new Label
		{
			Text = "Test passes if the toolbar item text and icon respect the Shell ForegroundColor (purple).",
			AutomationId = "Issue34071DescriptionLabel",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			HorizontalTextAlignment = TextAlignment.Center,
			Margin = new Thickness(16),
		};

		AddContentPage(page);
	}
}
