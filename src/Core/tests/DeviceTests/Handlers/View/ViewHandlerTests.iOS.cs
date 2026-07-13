using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ViewHandlerTests
	{
		[Fact]
		public async Task NonUIControlDisablesUserInteractionWhenIsEnabledFalse()
		{
			var view = new StubBase();
			var handler = await CreateHandlerAsync(view);

			await InvokeOnMainThreadAsync(() =>
			{
				Assert.True(handler.PlatformView.UserInteractionEnabled);

				view.IsEnabled = false;
				handler.UpdateValue(nameof(IView.IsEnabled));

				Assert.False(handler.PlatformView.UserInteractionEnabled);

				view.IsEnabled = true;
				handler.UpdateValue(nameof(IView.IsEnabled));

				Assert.True(handler.PlatformView.UserInteractionEnabled);
			});
		}

		[Fact]
		public async Task NonUIControlKeepsInputTransparentAfterIsEnabledToggles()
		{
			var view = new StubBase { InputTransparent = true };
			var handler = await CreateHandlerAsync(view);

			await InvokeOnMainThreadAsync(() =>
			{
				Assert.False(handler.PlatformView.UserInteractionEnabled);

				view.IsEnabled = false;
				handler.UpdateValue(nameof(IView.IsEnabled));

				Assert.False(handler.PlatformView.UserInteractionEnabled);

				view.IsEnabled = true;
				handler.UpdateValue(nameof(IView.IsEnabled));

				Assert.False(handler.PlatformView.UserInteractionEnabled);
			});
		}
	}
}