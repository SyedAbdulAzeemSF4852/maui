using Microsoft.Maui.Handlers;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Hosting;
using ObjCRuntime;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class RefreshViewHandlerTests
	{
		[Fact]
		public async Task DisablingRefreshRestoresAlwaysBounceVertical()
		{
			EnsureHandlerCreated(builder => builder.ConfigureMauiHandlers(
				handlers => handlers.AddHandler<ScrollViewStub, ScrollViewHandler>()));

			var scrollView = new ScrollViewStub();
			var refreshView = new RefreshViewStub
			{
				Content = scrollView,
			};

			await InvokeOnMainThreadAsync(() =>
			{
				CreateHandler(refreshView);
				var platformScrollView = ((ScrollViewHandler)scrollView.Handler).PlatformView;

				Assert.True(platformScrollView.AlwaysBounceVertical);

				refreshView.IsRefreshEnabled = false;

				Assert.False(platformScrollView.AlwaysBounceVertical);
			});
		}

		MauiRefreshView GetNativeRefreshView(RefreshViewHandler RefreshViewHandler) =>
			(MauiRefreshView)RefreshViewHandler.PlatformView;

		bool GetPlatformIsRefreshing(RefreshViewHandler RefreshViewHandler) =>
			GetNativeRefreshView(RefreshViewHandler).IsRefreshing;
	}
}