using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.CheckBox)]
	public partial class CheckBoxTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<CheckBox, CheckBoxHandler>();
				});
			});
		}

		CheckBoxHandler CreateThreeStateHandler(CheckBox checkBox)
		{
#if ANDROID
			return CreateHandler<Material3CheckBoxHandler>(checkBox);
#else
			return CreateHandler<CheckBoxHandler>(checkBox);
#endif
		}

		[Theory]
		[InlineData(false, true)]
		[InlineData(true, false)]
		[InlineData(null, false)]
		public Task NullableStateMapsAndActivationNotifiesOnce(bool? initial, bool expected)
		{
			SetupBuilder();
			return InvokeOnMainThreadAsync(() =>
			{
				var checkBox = new CheckBox { IsThreeState = true, IsChecked = initial };
				var events = 0;
				var commands = 0;
				checkBox.CheckedChanged += (_, e) =>
				{
					Assert.Equal(checkBox.IsChecked, e.CheckedValue);
					events++;
				};
				checkBox.Command = new Command(() => commands++);
				var handler = CreateThreeStateHandler(checkBox);

				Assert.Equal(initial, GetNativeCheckedState(handler));
				Assert.Equal(initial, checkBox.IsChecked);
				Assert.Equal(0, events);
				Assert.Equal(0, commands);

				ActivateNativeCheckBox(handler);

				Assert.Equal((bool?)expected, checkBox.IsChecked);
				Assert.Equal(checkBox.IsChecked, GetNativeCheckedState(handler));
				Assert.Equal(1, events);
				Assert.Equal(1, commands);

				ActivateNativeCheckBox(handler);

				Assert.Equal((bool?)!expected, checkBox.IsChecked);
				Assert.Equal(checkBox.IsChecked, GetNativeCheckedState(handler));
				Assert.Equal(2, events);
				Assert.Equal(2, commands);

				checkBox.IsChecked = null;
				checkBox.IsChecked = null;
				Assert.Null(GetNativeCheckedState(handler));
				Assert.Equal(3, events);
				Assert.Equal(3, commands);
			});
		}

		[Fact]
		public Task DisabledActivationPreservesIndeterminateState()
		{
			SetupBuilder();
			return InvokeOnMainThreadAsync(() =>
			{
				var checkBox = new CheckBox { IsThreeState = true, IsChecked = null, IsEnabled = false };
				var events = 0;
				var commands = 0;
				checkBox.CheckedChanged += (_, _) => events++;
				checkBox.Command = new Command(() => commands++);
				var handler = CreateThreeStateHandler(checkBox);
				Assert.Null(GetNativeCheckedState(handler));

				ActivateNativeCheckBox(handler);

				Assert.Null(checkBox.IsChecked);
				Assert.Null(GetNativeCheckedState(handler));
				Assert.Equal(0, events);
				Assert.Equal(0, commands);

				checkBox.IsEnabled = true;
				ActivateNativeCheckBox(handler);

				Assert.False(checkBox.IsChecked);
				Assert.Equal((bool?)false, GetNativeCheckedState(handler));
				Assert.Equal(1, events);
				Assert.Equal(1, commands);
			});
		}

		[Theory("Checkbox Background Updates Correctly With BackgroundColor Property"
#if WINDOWS
			,Skip = "Failing"
#endif
			)]
		[InlineData("#FF0000")]
		[InlineData("#00FF00")]
		[InlineData("#0000FF")]
		public async Task UpdatingCheckBoxBackgroundColorUpdatesBackground(string colorStr)
		{
			var color = Color.Parse(colorStr);

			var checkBox = new CheckBox
			{
				BackgroundColor = Colors.HotPink
			};

			checkBox.BackgroundColor = color;

			await ValidateHasColor<CheckBoxHandler>(checkBox, color);
		}

		[Fact]
		[Description("The Opacity property of a CheckBox should match with native Opacity")]
		public async Task VerifyCheckBoxOpacityProperty()
		{
			var checkBox = new CheckBox
			{
				Opacity = 0.35f
			};
			var expectedValue = checkBox.Opacity;

			var handler = await CreateHandlerAsync<CheckBoxHandler>(checkBox);
			await InvokeOnMainThreadAsync(async () =>
			{
				var nativeOpacityValue = await GetPlatformOpacity(handler);
				Assert.Equal(expectedValue, nativeOpacityValue);
			});
		}

		[Fact]
		[Description("The IsVisible property of a CheckBox should match with native IsVisible")]
		public async Task VerifyCheckBoxIsVisibleProperty()
		{
			var checkBox = new CheckBox();
			checkBox.IsVisible = false;
			var expectedValue = checkBox.IsVisible;

			var handler = await CreateHandlerAsync<CheckBoxHandler>(checkBox);
			await InvokeOnMainThreadAsync(async () =>
   			{
				   var isVisible = await GetPlatformIsVisible(handler);
				   Assert.Equal(expectedValue, isVisible);
			   });
		}
	}
}