using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using ObjCRuntime;
using UIKit;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class RefreshViewHandlerTests
	{
		[Fact]
		public async Task RefreshingStopsOnDetachAndResumesOnReattach()
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var scrollView = new RefreshLifecycleScrollViewStub();
				var scrollHandler = CreateHandler<ScrollViewHandler>(scrollView);
				var refreshView = new RefreshViewStub { Content = scrollView };
				var handler = CreateHandler(refreshView);
				var platformView = GetNativeRefreshView(handler);

				try
				{
					await platformView.AttachAndRun(async () =>
					{
						Assert.NotNull(platformView.Window);
						var parent = platformView.Superview;
						Assert.NotNull(parent);

						refreshView.IsRefreshing = true;
						handler.UpdateValue(nameof(IRefreshView.IsRefreshing));
						await AssertEventually(() => platformView.RefreshControl.Refreshing,
							message: "Native refresh did not start while attached.");

						// Keep the handler connected and move the same native view.
						platformView.RemoveFromSuperview();

						Assert.Null(platformView.Window);
						Assert.Same(handler, refreshView.Handler);
						Assert.Same(refreshView, handler.VirtualView);
						Assert.True(refreshView.IsRefreshing);
						Assert.True(platformView.IsRefreshing);
						Assert.False(platformView.RefreshControl.Refreshing,
							"Detaching must stop native refresh while preserving logical refresh.");

						parent.AddSubview(platformView);

						Assert.NotNull(platformView.Window);
						Assert.Same(platformView, handler.PlatformView);
						await AssertEventually(() => platformView.RefreshControl.Refreshing,
							message: "Native refresh did not resume after reattachment.");

						refreshView.IsRefreshing = false;
						handler.UpdateValue(nameof(IRefreshView.IsRefreshing));
						Assert.False(platformView.IsRefreshing);
						await AssertEventually(() => !platformView.RefreshControl.Refreshing,
							message: "Native refresh did not stop when refresh completed.");
					});
				}
				finally
				{
					refreshView.IsRefreshing = false;
					handler.UpdateValue(nameof(IRefreshView.IsRefreshing));
					((IElementHandler)handler).DisconnectHandler();
					((IElementHandler)scrollHandler).DisconnectHandler();
				}
			});
		}

		[Fact]
		public async Task RefreshingRequestedWhileDetachedStartsOnlyAfterAttachment()
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var scrollView = new RefreshLifecycleScrollViewStub();
				var scrollHandler = CreateHandler<ScrollViewHandler>(scrollView);
				var refreshView = new RefreshViewStub { Content = scrollView };
				var handler = CreateHandler(refreshView);
				var platformView = GetNativeRefreshView(handler);

				try
				{
					Assert.Null(platformView.Window);
					Assert.Same(platformView, scrollHandler.PlatformView.Superview);
					Assert.False(platformView.RefreshControl.Refreshing);

					refreshView.IsRefreshing = true;
					handler.UpdateValue(nameof(IRefreshView.IsRefreshing));

					// AttachAndRun attaches and waits for layout before its callback.
					// Check native state here so attachment cannot hide an early start.
					Assert.Null(platformView.Window);
					Assert.True(refreshView.IsRefreshing);
					Assert.True(platformView.IsRefreshing);
					Assert.False(platformView.RefreshControl.Refreshing,
						"A detached refresh request must not begin native refresh.");

					await platformView.AttachAndRun(async () =>
					{
						Assert.NotNull(platformView.Window);
						await AssertEventually(() => platformView.RefreshControl.Refreshing,
							message: "Deferred native refresh did not start after attachment.");

						refreshView.IsRefreshing = false;
						handler.UpdateValue(nameof(IRefreshView.IsRefreshing));
						Assert.False(platformView.IsRefreshing);
						await AssertEventually(() => !platformView.RefreshControl.Refreshing,
							message: "Native refresh did not stop when refresh completed.");
					});
				}
				finally
				{
					refreshView.IsRefreshing = false;
					handler.UpdateValue(nameof(IRefreshView.IsRefreshing));
					((IElementHandler)handler).DisconnectHandler();
					((IElementHandler)scrollHandler).DisconnectHandler();
				}
			});
		}

		sealed class RefreshLifecycleScrollViewStub : ScrollViewStub, IScrollView
		{
			// Refresh inset/offset changes can complete a native scroll animation.
			void IScrollView.ScrollFinished()
			{
			}
		}

		MauiRefreshView GetNativeRefreshView(RefreshViewHandler RefreshViewHandler) =>
			(MauiRefreshView)RefreshViewHandler.PlatformView;

		bool GetPlatformIsRefreshing(RefreshViewHandler RefreshViewHandler) =>
			GetNativeRefreshView(RefreshViewHandler).IsRefreshing;
	}
}