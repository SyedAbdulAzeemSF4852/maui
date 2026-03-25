namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 14497, "Dynamically setting SearchHandler Query property does not update text in the search box", PlatformAffected.Android | PlatformAffected.iOS)]
public class Issue14497 : Shell
{
	public Issue14497()
	{
		Items.Add(new ShellContent
		{
			Title = "Home",
			ContentTemplate = new DataTemplate(typeof(Issue14497Page)),
			Route = "MainPage"
		});
	}
}

public class Issue14497Page : ContentPage
{
	readonly Issue14497SearchHandler _searchHandler;

	public Issue14497Page()
	{
		_searchHandler = new Issue14497SearchHandler
		{
			ShowsResults = false
		};

		Label instructions = new Label
		{
			Text = "Tap the button below. If the SearchHandler updates to 'Hello World', the test has passed."
		};

		Button button = new Button
		{
			Text = "Change Search Text",
			AutomationId = "ChangeSearchText"
		};

		button.Clicked += (s, e) => _searchHandler.Query = "Hello World";

		Content = new VerticalStackLayout
		{
			Children = { instructions, button }
		};

		Shell.SetSearchHandler(this, _searchHandler);
	}
}

public class Issue14497SearchHandler : SearchHandler
{
}
