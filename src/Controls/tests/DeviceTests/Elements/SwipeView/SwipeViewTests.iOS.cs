using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using CoreGraphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
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

		[Fact(DisplayName = "SwipeView Keeps Tracking And Can Be Closed After Pointer Leaves Bounds Vertically")]
		public async Task SwipeViewTracksAndClosesAfterPointerLeavesBoundsVertically()
		{
			Grid content = new Grid
			{
				WidthRequest = 300,
				HeightRequest = 60,
				Background = new SolidPaint(Colors.White)
			};

			SwipeItem swipeItem = new SwipeItem
			{
				BackgroundColor = Colors.Red,
			};

			SwipeView swipeView = new SwipeView()
			{
				WidthRequest = 300,
				HeightRequest = 60,
				RightItems = new SwipeItems { swipeItem },
				Content = content
			};

			SetupBuilder();

			var handler = await CreateHandlerAsync<SwipeViewHandler>(swipeView);
			var nativeView = GetPlatformControl(handler);

			var handleTouchInteractions = typeof(MauiSwipeView).GetMethod(
				"HandleTouchInteractions", BindingFlags.NonPublic | BindingFlags.Instance);
			Assert.NotNull(handleTouchInteractions);

			var offsetField = typeof(MauiSwipeView).GetField(
				"_swipeOffset", BindingFlags.NonPublic | BindingFlags.Instance);
			Assert.NotNull(offsetField);

			double openOffset = 0;
			double offsetAfterAttemptedClose = 0;

			await InvokeOnMainThreadAsync(() =>
				nativeView.AttachAndRun(() =>
				{
					void Dispatch(GestureStatus status, double x, double y) =>
						handleTouchInteractions.Invoke(nativeView, new object[] { status, new CGPoint(x, y) });

					Dispatch(GestureStatus.Started, 280, 30);
					Dispatch(GestureStatus.Running, 130, 30);

					openOffset = (double)offsetField.GetValue(nativeView);

					Dispatch(GestureStatus.Running, 130, 500);
					Dispatch(GestureStatus.Running, 275, 500);

					offsetAfterAttemptedClose = (double)offsetField.GetValue(nativeView);
					Dispatch(GestureStatus.Completed, 275, 500);
				}));

			Assert.NotEqual(0, openOffset);
			Assert.True(Math.Abs(offsetAfterAttemptedClose) < Math.Abs(openOffset),
				$"Expected swipe offset to shrink after the closing drag (frozen bug would keep it near {openOffset}), but got {offsetAfterAttemptedClose}.");
		}
	}
}
