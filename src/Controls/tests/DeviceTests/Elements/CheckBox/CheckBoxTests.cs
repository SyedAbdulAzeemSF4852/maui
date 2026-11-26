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

		CheckBoxHandler CreateThreeStateHandler(CheckBox checkBox) =>
			CreateHandler<CheckBoxHandler>(checkBox);

		static CheckBoxState ToCheckState(bool? value) => value switch
		{
			true => CheckBoxState.Checked,
			false => CheckBoxState.Unchecked,
			null => CheckBoxState.Indeterminate
		};

		static bool? GetCheckedValue(CheckBox checkBox) => checkBox.CheckState switch
		{
			CheckBoxState.Checked => true,
			CheckBoxState.Indeterminate => null,
			_ => false
		};

		[Theory]
		[InlineData(true, false, true, null, false)]
		[InlineData(true, true, null, false, true)]
		[InlineData(true, null, false, true, null)]
		[InlineData(false, false, true, false, true)]
		[InlineData(false, true, false, true, false)]
		public Task CompleteStateMapsAndActivationNotifiesOnce(bool threeState, bool? initial, bool? first, bool? second, bool? third)
		{
			SetupBuilder();
			return InvokeOnMainThreadAsync(() =>
			{
				var checkBox = new CheckBox { IsThreeState = threeState, CheckState = ToCheckState(initial) };
				var events = 0;
				var commands = 0;
				checkBox.CheckStateChanged += (_, e) =>
				{
					Assert.Equal(checkBox.CheckState, e.Value);
					events++;
				};
				checkBox.Command = new Command(() => commands++);
				var handler = CreateThreeStateHandler(checkBox);

				Assert.Equal(initial, GetNativeCheckedState(handler));
				Assert.Equal(initial, GetCheckedValue(checkBox));
				Assert.Equal(0, events);
				Assert.Equal(0, commands);

				var activations = 0;
				var expectedCommands = 0;
				var previousIsChecked = initial == true;
				foreach (var expected in new[] { first, second, third })
				{
					ActivateNativeCheckBox(handler);
					activations++;
					if (previousIsChecked != (expected == true))
						expectedCommands++;
					previousIsChecked = expected == true;

					Assert.Equal(expected, GetCheckedValue(checkBox));
					Assert.Equal(expected, GetNativeCheckedState(handler));
					Assert.Equal(activations, events);
					Assert.Equal(expectedCommands, commands);
				}

				bool? assignedState = threeState ? null : false;
				var expectedNotifications = activations + (third == assignedState ? 0 : 1);
				if ((third == true) != (assignedState == true))
					expectedCommands++;
				checkBox.CheckState = CheckBoxState.Indeterminate;
				checkBox.CheckState = CheckBoxState.Indeterminate;
				Assert.Equal(assignedState, GetCheckedValue(checkBox));
				Assert.Equal(assignedState, GetNativeCheckedState(handler));
				Assert.Equal(expectedNotifications, events);
				Assert.Equal(expectedCommands, commands);
			});
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public Task ActivationUsesUpdatedThreeStateFlag(bool initialThreeState)
		{
			SetupBuilder();
			return InvokeOnMainThreadAsync(() =>
			{
				var checkBox = new CheckBox { IsThreeState = initialThreeState };
				var handler = CreateThreeStateHandler(checkBox);
				var events = 0;
				var commands = 0;
				checkBox.CheckStateChanged += (_, _) => events++;
				checkBox.Command = new Command(() => commands++);

				checkBox.IsThreeState = !initialThreeState;
				Assert.Equal(0, events);
				Assert.Equal(0, commands);
				ActivateNativeCheckBox(handler);
				Assert.True(checkBox.IsChecked);
				Assert.Equal((bool?)true, GetNativeCheckedState(handler));
				Assert.Equal(1, events);
				Assert.Equal(1, commands);

				ActivateNativeCheckBox(handler);
				bool? expected = initialThreeState ? false : null;
				Assert.Equal(expected, GetCheckedValue(checkBox));
				Assert.Equal(expected, GetNativeCheckedState(handler));
				Assert.Equal(2, events);
				Assert.Equal(2, commands);
			});
		}

		[Fact]
		public Task DisablingThreeStateAfterActivationResetsMixedOnce()
		{
			SetupBuilder();
			return InvokeOnMainThreadAsync(() =>
			{
				var checkBox = new CheckBox { IsThreeState = true };
				var handler = CreateThreeStateHandler(checkBox);
				var events = 0;
				var commands = 0;
				checkBox.CheckStateChanged += (_, e) =>
				{
					Assert.Equal(checkBox.CheckState, e.Value);
					events++;
				};
				checkBox.Command = new Command(() => commands++);

				ActivateNativeCheckBox(handler);
				ActivateNativeCheckBox(handler);
				Assert.Equal(CheckBoxState.Indeterminate, checkBox.CheckState);
				Assert.Null(GetNativeCheckedState(handler));
				Assert.Equal(2, events);
				Assert.Equal(2, commands);

				checkBox.IsThreeState = false;
				checkBox.IsThreeState = false;
				Assert.False(checkBox.IsChecked);
				Assert.Equal((bool?)false, GetNativeCheckedState(handler));
				Assert.Equal(3, events);
				Assert.Equal(2, commands);

				ActivateNativeCheckBox(handler);
				Assert.True(checkBox.IsChecked);
				Assert.Equal((bool?)true, GetNativeCheckedState(handler));
				Assert.Equal(4, events);
				Assert.Equal(3, commands);

				ActivateNativeCheckBox(handler);
				Assert.False(checkBox.IsChecked);
				Assert.Equal((bool?)false, GetNativeCheckedState(handler));
				Assert.Equal(5, events);
				Assert.Equal(4, commands);
			});
		}

		[Theory(
#if ANDROID
			Skip = "Native PerformClick bypasses disabled touch handling; disabled Android activation requires physical-input coverage."
#endif
			)]
		[InlineData(false, true)]
		[InlineData(true, null)]
		[InlineData(null, false)]
		public Task DisabledActivationPreservesCheckedState(bool? initial, bool? enabledResult)
		{
			SetupBuilder();
			return InvokeOnMainThreadAsync(() =>
			{
				var checkBox = new CheckBox { IsThreeState = true, CheckState = ToCheckState(initial), IsEnabled = false };
				var events = 0;
				var commands = 0;
				checkBox.CheckStateChanged += (_, _) => events++;
				checkBox.Command = new Command(() => commands++);
				var handler = CreateThreeStateHandler(checkBox);
				Assert.Equal(initial, GetNativeCheckedState(handler));

				ActivateNativeCheckBox(handler);

				Assert.Equal(initial, GetCheckedValue(checkBox));
				Assert.Equal(initial, GetNativeCheckedState(handler));
				Assert.Equal(0, events);
				Assert.Equal(0, commands);

				checkBox.IsEnabled = true;
				ActivateNativeCheckBox(handler);

				Assert.Equal(enabledResult, GetCheckedValue(checkBox));
				Assert.Equal(enabledResult, GetNativeCheckedState(handler));
				Assert.Equal(1, events);
				Assert.Equal((initial == true) != (enabledResult == true) ? 1 : 0, commands);
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