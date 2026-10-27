using Microsoft.Maui.Handlers;
using System.Threading.Tasks;
using CoreGraphics;
using ObjCRuntime;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class RefreshViewHandlerTests
	{
		[Theory]
		[InlineData(false, false)]
		[InlineData(false, true)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		public async Task RefreshPreservesHorizontalScrollPosition(bool userInitiated, bool scrollDuringRefresh)
		{
			double startX = 0;
			double endX = 0;
			bool nativeRefreshingAfterStart = false;
			bool nativeRefreshingAfterEnd = true;

			await InvokeOnMainThreadAsync(async () =>
			{
				using var refreshView = new MauiRefreshView
				{
					Frame = new CGRect(0, 0, 300, 300)
				};
				using var scrollView = new UIScrollView(new CGRect(0, 0, 300, 300))
				{
					ContentSize = new CGSize(1200, 300)
				};

				refreshView.AddSubview(scrollView);
				await refreshView.AttachAndRun(() =>
				{
					refreshView.Frame = new CGRect(0, 0, 300, 300);
					scrollView.Frame = new CGRect(0, 0, 300, 300);
					scrollView.ContentSize = new CGSize(1200, 300);
					refreshView.RefreshControl.Frame = new CGRect(0, 0, 300, 60);
					refreshView.UpdateIsRefreshEnabled(true);
					Assert.Same(scrollView, refreshView.RefreshControl.Superview);

					// Capture results here; assertions must not throw through UIKit's native callback.
					UIView.PerformWithoutAnimation(() =>
					{
						scrollView.ContentOffset = new CGPoint(120, userInitiated ? -60 : 0);
						if (userInitiated)
							refreshView.RefreshControl.BeginRefreshing();

						refreshView.IsRefreshing = true;
						nativeRefreshingAfterStart = refreshView.RefreshControl.Refreshing;
						startX = (double)scrollView.ContentOffset.X;

						if (scrollDuringRefresh)
							scrollView.ContentOffset = new CGPoint(240, scrollView.ContentOffset.Y);

						refreshView.IsRefreshing = false;
						nativeRefreshingAfterEnd = refreshView.RefreshControl.Refreshing;
						endX = (double)scrollView.ContentOffset.X;
					});
				});
			});

			Assert.True(nativeRefreshingAfterStart, "The attached native refresh control must start refreshing.");
			Assert.False(nativeRefreshingAfterEnd, "The native refresh control must stop refreshing.");
			Assert.Equal(120, startX);
			Assert.Equal(scrollDuringRefresh ? 240 : 120, endX);
		}

		MauiRefreshView GetNativeRefreshView(RefreshViewHandler RefreshViewHandler) =>
			(MauiRefreshView)RefreshViewHandler.PlatformView;

		bool GetPlatformIsRefreshing(RefreshViewHandler RefreshViewHandler) =>
			GetNativeRefreshView(RefreshViewHandler).IsRefreshing;
	}
}