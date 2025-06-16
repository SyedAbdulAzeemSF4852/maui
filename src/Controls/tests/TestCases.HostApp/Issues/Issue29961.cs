using System.Reflection;
using Microsoft.Maui.Graphics.Platform;
using IImage = Microsoft.Maui.Graphics.IImage;

namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 29961, "The resize method returns an image that has already been disposed", PlatformAffected.Android)]
public class Issue29961 : ContentPage
{
	Label _convertedImageStatusLabel;
	public Issue29961()
	{
		_convertedImageStatusLabel = new Label
		{
			AutomationId = "ConvertedImage",
			Text = "Result Image Status: "
		};

		VerticalStackLayout VerticalStackLayout = new VerticalStackLayout
		{
			Padding = new Thickness(20),
			Spacing = 10,
			Children =
			{
				CreateButton("DownSize", OnDownSize),
				_convertedImageStatusLabel,
			}
		};

		Content = new ScrollView { Content = VerticalStackLayout };
	}

	Button CreateButton(string text, EventHandler handler)
	{
		Button button = new Button
		{
			AutomationId = $"Issue21886_1DownSizeBtn",
			Text = text,
			HorizontalOptions = LayoutOptions.Fill
		};

		button.Clicked += handler;
		return button;
	}

	async Task<IImage> LoadImageAsync()
	{
		var assembly = GetType().GetTypeInfo().Assembly;
		using var stream = assembly.GetManifestResourceStream("Controls.TestCases.HostApp.Resources.Images.royals.png");
		return await Task.FromResult(PlatformImage.FromStream(stream));
	}

	async void OnDownSize(object sender, EventArgs e)
	{
		var image = await LoadImageAsync();
		var res = image.Downsize(10);

		UpdateStatusLabels(res);
	}

	void UpdateStatusLabels(IImage resultImage)
	{
		_convertedImageStatusLabel.Text = TryAccessImage(resultImage)
		? "Success"
		: "Failure";
	}

	bool TryAccessImage(IImage downsizedImage)
	{
		if (Math.Round(downsizedImage.Width) == 10 && Math.Round(downsizedImage.Height) == 8)
		{
			return true;
		}

		return false;
	}
}