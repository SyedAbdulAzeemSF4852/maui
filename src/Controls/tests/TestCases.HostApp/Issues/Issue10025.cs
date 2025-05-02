namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 10025, "Assigning null to the SelectedItem of the CollectionView in the SelectionChanged event does not clear the selection as expected", PlatformAffected.UWP)]
public class Issue10025 : ContentPage
{
	CollectionView collectionView;
	public Issue10025()
	{
		collectionView = new CollectionView
		{
			SelectionMode = SelectionMode.Single,
			ItemsSource = new List<string> { "Item1", "Item2", "Item3", "Item4", "Item5" },
			ItemTemplate = new DataTemplate(() =>
			{
				Label label = new Label();
				label.SetBinding(Label.TextProperty, ".");
				label.SetBinding(Label.AutomationIdProperty, ".");
				return label;
			})
		};
		collectionView.SelectionChanged += SelectionChangedEvent;
		Content = collectionView;
	}
	private void SelectionChangedEvent(object sender, SelectionChangedEventArgs e)
	{
		collectionView.SelectedItem = null;
	}
}

