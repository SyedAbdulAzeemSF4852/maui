using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.SwipeView)]
	public partial class SwipeViewTests : ControlsHandlerTestBase
	{
		MauiSwipeView GetPlatformControl(SwipeViewHandler handler) =>
			handler.PlatformView;

		Task<bool> HasChildren(SwipeViewHandler handler)
		{
			return InvokeOnMainThreadAsync(()
				=> GetPlatformControl(handler).Subviews.Length != 0);
		}

		[Fact]
		[Description("The Opacity property of a SwipeView should match with native Opacity")]
		public async Task VerifySwipeViewOpacityProperty()
		{
			var swipeView = new SwipeView
			{
				Opacity = 0.35f
			};
			var expectedValue = swipeView.Opacity;

			var handler = await CreateHandlerAsync<SwipeViewHandler>(swipeView);
			var nativeView = GetPlatformControl(handler);
			await InvokeOnMainThreadAsync(() =>
   			{
				   var nativeOpacityValue = (float)nativeView.Alpha;
				   Assert.Equal(expectedValue, nativeOpacityValue);
			   });
		}

		[Fact]
		[Description("The IsVisible property of a SwipeView should match with native IsVisible")]
		public async Task VerifySwipeViewIsVisibleProperty()
		{
			var swipeView = new SwipeView
			{
				IsVisible = false
			};
			var expectedValue = swipeView.IsVisible;

			var handler = await CreateHandlerAsync<SwipeViewHandler>(swipeView);
			var nativeView = GetPlatformControl(handler);
			await InvokeOnMainThreadAsync(() =>
   			{
				   var isVisible = !nativeView.Hidden;
				   Assert.Equal(expectedValue, isVisible);
			   });
		}

		// Platform helpers — called from shared SwipeViewTests.cs to assert native view state
		// Must be called from within AttachAndRun (already on main thread).
		// _contentView is always the last subview of MauiSwipeView after Open().

		bool PlatformSwipeViewHasOpenedItems(SwipeViewHandler handler) =>
			GetPlatformControl(handler).Subviews.Length >= 2;

		bool PlatformContentViewIsOpaque(SwipeViewHandler handler)
		{
			var platformView = GetPlatformControl(handler);
			var contentView = platformView.Subviews.LastOrDefault();
			if (contentView?.BackgroundColor == null)
				return false;
			return !contentView.BackgroundColor.IsEqual(UIColor.Clear);
		}

		bool PlatformContentViewIsOpaqueFromFix(SwipeViewHandler handler) =>
			PlatformContentViewIsOpaque(handler);
	}
}

