namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 30065, "DatePicker Ignores FlowDirection When Set to RightToLeft or MatchParent", PlatformAffected.iOS)]
public class Issue30065 : ContentPage
{
	public Issue30065()
	{
		DatePicker datePicker = new DatePicker
		{
			AutomationId = "RtlDatePicker",
			WidthRequest = 300,
			FlowDirection = FlowDirection.RightToLeft,
		};
		VerticalStackLayout verticalStackLayout = new VerticalStackLayout()
		{
			VerticalOptions = LayoutOptions.Center,
			Children = { datePicker }
		};
		Content = verticalStackLayout;
	}
}