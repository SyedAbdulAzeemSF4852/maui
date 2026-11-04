using System;
#if WINDOWS
using System.Runtime.InteropServices;
#endif

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 39428, "[Windows] Re-entrant activation during Window.Resumed", PlatformAffected.UWP)]
public class Issue39428 : ContentPage
{
	readonly Label _resultLabel;
	readonly Label _statusLabel;
	Window _window;
	bool _minimizeInsideNextResumed;
	int _activated;
	int _deactivated;
	string _exceptionMessage;

#if WINDOWS
	const int SW_MINIMIZE = 6;
	const int SW_RESTORE = 9;

	[DllImport("user32.dll")]
	static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
#endif

	public Issue39428()
	{
		_resultLabel = new Label
		{
			AutomationId = "ResultLabel",
			Text = "Waiting"
		};

		_statusLabel = new Label
		{
			AutomationId = "StatusLabel",
			Text = "Activated: 0, Deactivated: 0"
		};

		var runButton = new Button
		{
			AutomationId = "RunTestButton",
			Text = "Run re-entrant activation test"
		};
		runButton.Clicked += OnRunTestClicked;

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 12,
			Children =
			{
				runButton,
				_resultLabel,
				_statusLabel
			}
		};

		Loaded += OnLoaded;
	}

	void OnLoaded(object sender, EventArgs e)
	{
		_window = Window;
		_window.Activated += OnActivated;
		_window.Deactivated += OnDeactivated;
		_window.Resumed += OnResumed;
#if WINDOWS
		Microsoft.UI.Xaml.Application.Current.UnhandledException += OnUnhandledException;
#endif
	}

	protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
	{
		base.OnNavigatedFrom(args);

		if (_window is not null)
		{
			_window.Activated -= OnActivated;
			_window.Deactivated -= OnDeactivated;
			_window.Resumed -= OnResumed;
		}
#if WINDOWS
		Microsoft.UI.Xaml.Application.Current.UnhandledException -= OnUnhandledException;
#endif
		Loaded -= OnLoaded;
	}

	void OnActivated(object sender, EventArgs e)
	{
		_activated++;
		UpdateStatus();
	}

	void OnDeactivated(object sender, EventArgs e)
	{
		_deactivated++;
		UpdateStatus();
	}

	void OnResumed(object sender, EventArgs e)
	{
#if WINDOWS
		if (_minimizeInsideNextResumed)
		{
			_minimizeInsideNextResumed = false;
			ShowWindow(GetWindowHandle(), SW_MINIMIZE);
		}
#endif
	}

#if WINDOWS
	void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
	{
		_exceptionMessage = e.Exception.Message;
		e.Handled = true;
	}

	IntPtr GetWindowHandle() =>
		WinRT.Interop.WindowNative.GetWindowHandle((Microsoft.UI.Xaml.Window)_window.Handler.PlatformView);
#endif

	async void OnRunTestClicked(object sender, EventArgs e)
	{
#if WINDOWS
		_activated = 0;
		_deactivated = 0;
		_exceptionMessage = null;
		UpdateStatus();
		_resultLabel.Text = "Running";

		var windowHandle = GetWindowHandle();
		ShowWindow(windowHandle, SW_MINIMIZE);

		_minimizeInsideNextResumed = true;
		ShowWindow(windowHandle, SW_RESTORE);

		ShowWindow(windowHandle, SW_RESTORE);

		_resultLabel.Text = _exceptionMessage is null && _window.IsActivated
			? "PASSED"
			: $"FAILED: {_exceptionMessage ?? "Window is not activated"}";
#else
		_resultLabel.Text = "Windows only";
		await Task.CompletedTask;
#endif
	}

	void UpdateStatus() =>
		_statusLabel.Text = $"Activated: {_activated}, Deactivated: {_deactivated}";
}
