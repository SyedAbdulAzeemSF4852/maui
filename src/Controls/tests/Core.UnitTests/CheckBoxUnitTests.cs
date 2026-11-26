using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using NSubstitute;
using Xunit;
using static Microsoft.Maui.Controls.Core.UnitTests.VisualStateTestHelpers;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class CheckBoxUnitTests : BaseTestFixture
	{
		[Fact]
		public void TestConstructor()
		{
			var checkBox = new CheckBox();
			Assert.False(checkBox.IsChecked);
			Assert.False(checkBox.IsThreeState);
			Assert.Equal(CheckBoxState.Unchecked, checkBox.CheckState);
		}

		[Fact]
		public void NativeWriteBackUsesThreeStateCapability()
		{
			var capable = Substitute.For<ICheckBox2>();
			Microsoft.Maui.Handlers.CheckBoxHandler.SetCheckedState(capable, null);
			capable.Received().CheckState = CheckBoxState.Indeterminate;
			((ICheckBox)capable).DidNotReceive().IsChecked = Arg.Any<bool>();

			var legacy = Substitute.For<ICheckBox>();
			Microsoft.Maui.Handlers.CheckBoxHandler.SetCheckedState(legacy, null);
			legacy.DidNotReceive().IsChecked = Arg.Any<bool>();
			Microsoft.Maui.Handlers.CheckBoxHandler.SetCheckedState(legacy, true);
			legacy.Received().IsChecked = true;
		}

		[Theory]
		[InlineData(CheckBoxState.Unchecked, CheckBoxState.Checked, 1)]
		[InlineData(CheckBoxState.Checked, CheckBoxState.Indeterminate, 1)]
		[InlineData(CheckBoxState.Indeterminate, CheckBoxState.Unchecked, 0)]
		[InlineData(CheckBoxState.Unchecked, CheckBoxState.Indeterminate, 0)]
		[InlineData(CheckBoxState.Indeterminate, CheckBoxState.Checked, 1)]
		[InlineData(CheckBoxState.Checked, CheckBoxState.Unchecked, 1)]
		public void StateChangesNotifyOnceWithBooleanCompatibility(CheckBoxState initial, CheckBoxState next, int booleanChanges)
		{
			var checkBox = new CheckBox { IsThreeState = true, CheckState = initial };
			var stateChanges = 0;
			var checkedChanges = 0;
			var commands = 0;
			var parameter = new object();
			checkBox.CheckStateChanged += (_, e) =>
			{
				Assert.Equal(next, e.Value);
				Assert.Equal(next, checkBox.CheckState);
				Assert.Equal(next == CheckBoxState.Checked, checkBox.IsChecked);
				stateChanges++;
			};
			checkBox.CheckedChanged += (_, e) =>
			{
				Assert.Equal(checkBox.IsChecked, e.Value);
				checkedChanges++;
			};
			checkBox.CommandParameter = parameter;
			checkBox.Command = new Command<object>(value =>
			{
				Assert.Same(parameter, value);
				commands++;
			});

			checkBox.CheckState = next;
			checkBox.CheckState = next;

			Assert.Equal(1, stateChanges);
			Assert.Equal(booleanChanges, checkedChanges);
			Assert.Equal(booleanChanges, commands);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void BooleanAssignmentClearsIndeterminate(bool value)
		{
			var checkBox = new CheckBox { IsThreeState = true, CheckState = CheckBoxState.Indeterminate };
			checkBox.SetValue(CheckBox.IsCheckedProperty, value);
			Assert.Equal(value ? CheckBoxState.Checked : CheckBoxState.Unchecked, checkBox.CheckState);
			Assert.Equal(value, ((ICheckBox)checkBox).IsChecked);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void DisallowedIndeterminateIsNotRestoredWhenThreeStateIsEnabled(bool initiallyChecked)
		{
			var checkBox = new CheckBox { IsChecked = initiallyChecked };
			var states = 0;
			var commands = 0;
			checkBox.CheckStateChanged += (_, _) => states++;
			checkBox.Command = new Command(() => commands++);
			checkBox.CheckState = CheckBoxState.Indeterminate;
			checkBox.CheckState = CheckBoxState.Indeterminate;
			Assert.Equal(CheckBoxState.Unchecked, checkBox.CheckState);
			checkBox.IsThreeState = true;
			Assert.Equal(CheckBoxState.Unchecked, checkBox.CheckState);
			Assert.False(checkBox.IsChecked);
			Assert.Equal(initiallyChecked ? 1 : 0, states);
			Assert.Equal(states, commands);
		}

		[Fact]
		public void ClearingNormalizedStateDoesNotRestoreIndeterminate()
		{
			var checkBox = new CheckBox { CheckState = CheckBoxState.Indeterminate };
			checkBox.ClearValue(CheckBox.CheckStateProperty);
			checkBox.IsThreeState = true;
			Assert.Equal(CheckBoxState.Unchecked, checkBox.CheckState);
		}

		[Fact]
		public void ThreeStateFlagBindingUpdatesBothDirectionsAndClearsMixedState()
		{
			var source = new CheckBox { IsThreeState = true };
			var target = new CheckBox();
			target.SetBinding(CheckBox.IsThreeStateProperty, static (CheckBox value) => value.IsThreeState, source: source);
			Assert.True(target.IsThreeState);
			target.CheckState = CheckBoxState.Indeterminate;
			target.IsThreeState = false;
			Assert.False(source.IsThreeState);
			Assert.Equal(CheckBoxState.Unchecked, target.CheckState);
			source.IsThreeState = true;
			Assert.True(target.IsThreeState);
			Assert.Equal(CheckBoxState.Unchecked, target.CheckState);
			target.CheckState = CheckBoxState.Indeterminate;
			source.IsThreeState = false;
			Assert.False(target.IsThreeState);
			Assert.Equal(CheckBoxState.Unchecked, target.CheckState);
		}

		[Fact]
		public void HandlerUpdatesPreserveStateBinding()
		{
			var source = new CheckBox { IsThreeState = true };
			var target = new CheckBox { IsThreeState = true };
			target.SetBinding(CheckBox.CheckStateProperty, static (CheckBox value) => value.CheckState, source: source);
			((ICheckBox2)target).CheckState = CheckBoxState.Indeterminate;
			Assert.Equal(CheckBoxState.Indeterminate, source.CheckState);
			source.CheckState = CheckBoxState.Checked;
			Assert.True(target.IsChecked);
			target.IsThreeState = false;
			Assert.True(target.IsChecked);
			source.CheckState = CheckBoxState.Unchecked;
			Assert.False(target.IsChecked);
		}

		[Fact]
		public void DisablingThreeStateResetsMixedOnceWithoutReplacingStateBinding()
		{
			var source = new CheckBox { IsThreeState = true };
			var target = new CheckBox { IsThreeState = true };
			target.SetBinding(CheckBox.CheckStateProperty,
				static (CheckBox value) => value.CheckState, source: source);
			((ICheckBox2)target).CheckState = CheckBoxState.Indeterminate;
			Assert.Equal(CheckBoxState.Indeterminate, source.CheckState);
			var states = new List<CheckBoxState>();
			var checkedEvents = 0;
			var commands = 0;
			target.CheckStateChanged += (_, e) => states.Add(e.Value);
			target.CheckedChanged += (_, _) => checkedEvents++;
			target.Command = new Command(() => commands++);

			target.IsThreeState = false;
			target.IsThreeState = false;
			target.IsThreeState = true;

			Assert.Equal(CheckBoxState.Unchecked, target.CheckState);
			Assert.Equal(CheckBoxState.Unchecked, source.CheckState);
			Assert.Equal(new[] { CheckBoxState.Unchecked }, states);
			Assert.Equal(0, checkedEvents);
			Assert.Equal(0, commands);

			source.CheckState = CheckBoxState.Checked;
			Assert.Equal(CheckBoxState.Checked, target.CheckState);
			((ICheckBox2)target).CheckState = CheckBoxState.Indeterminate;
			Assert.Equal(CheckBoxState.Indeterminate, source.CheckState);
			Assert.Equal(new[] { CheckBoxState.Unchecked, CheckBoxState.Checked, CheckBoxState.Indeterminate }, states);
			Assert.Equal(2, checkedEvents);
			Assert.Equal(2, commands);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void BindingAndChangedCallbacksSeeConsistentControlProperties(bool booleanWrite)
		{
			var checkBox = new CheckBox { IsThreeState = true };
			var model = new StateObserver(checkBox);
			checkBox.SetBinding(CheckBox.CheckStateProperty,
				new Binding(nameof(StateObserver.State), BindingMode.TwoWay, source: model));
			checkBox.SetBinding(CheckBox.IsCheckedProperty,
				new Binding(nameof(StateObserver.IsChecked), BindingMode.TwoWay, source: model));
			checkBox.PropertyChanged += (_, e) =>
			{
				if (e.PropertyName is nameof(CheckBox.CheckState) or nameof(CheckBox.IsChecked))
					Assert.Equal(checkBox.CheckState == CheckBoxState.Checked, checkBox.IsChecked);
			};

			if (booleanWrite)
				checkBox.IsChecked = true;
			else
				checkBox.CheckState = CheckBoxState.Checked;
			Assert.True(model.IsChecked);
			Assert.Equal(CheckBoxState.Checked, model.State);
			checkBox.CheckState = CheckBoxState.Indeterminate;
			Assert.False(model.IsChecked);
			Assert.Equal(CheckBoxState.Indeterminate, model.State);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void NotifyingDualBindingsDoNotClearIndeterminateThroughBooleanFeedback(bool typedBinding)
		{
			var checkBox = new CheckBox { IsThreeState = true };
			var model = new StateObserver(checkBox);
			if (typedBinding)
			{
				checkBox.SetBinding(CheckBox.CheckStateProperty,
					new TypedBinding<StateObserver, CheckBoxState>(
						static observer => (observer.State, true),
						static (observer, value) => observer.State = value,
						new[] { new Tuple<Func<StateObserver, object>, string>(static observer => observer, nameof(StateObserver.State)) })
					{ Source = model, Mode = BindingMode.TwoWay });
				checkBox.SetBinding(CheckBox.IsCheckedProperty,
					new TypedBinding<StateObserver, bool>(
						static observer => (observer.IsChecked, true),
						static (observer, value) => observer.IsChecked = value,
						new[] { new Tuple<Func<StateObserver, object>, string>(static observer => observer, nameof(StateObserver.IsChecked)) })
					{ Source = model, Mode = BindingMode.TwoWay });
			}
			else
			{
				checkBox.SetBinding(CheckBox.CheckStateProperty,
					new Binding(nameof(StateObserver.State), BindingMode.TwoWay, source: model));
				checkBox.SetBinding(CheckBox.IsCheckedProperty,
					new Binding(nameof(StateObserver.IsChecked), BindingMode.TwoWay, source: model));
			}

			checkBox.CheckState = CheckBoxState.Checked;
			var states = new List<CheckBoxState>();
			checkBox.CheckStateChanged += (_, e) => states.Add(e.Value);
			checkBox.CheckState = CheckBoxState.Indeterminate;
			Assert.Equal(CheckBoxState.Indeterminate, checkBox.CheckState);
			Assert.False(checkBox.IsChecked);
			Assert.Equal(CheckBoxState.Indeterminate, model.State);
			Assert.False(model.IsChecked);
			Assert.Equal(new[] { CheckBoxState.Indeterminate }, states);

			// An explicit equal Boolean write must still clear mixed; only binding feedback is ignored.
			checkBox.IsChecked = false;
			Assert.Equal(CheckBoxState.Unchecked, checkBox.CheckState);
			Assert.Equal(CheckBoxState.Unchecked, model.State);

			model.State = CheckBoxState.Checked;
			Assert.True(checkBox.IsChecked);
			Assert.True(model.IsChecked);
			model.State = CheckBoxState.Indeterminate;
			Assert.Equal(CheckBoxState.Indeterminate, checkBox.CheckState);
			Assert.False(checkBox.IsChecked);
			Assert.False(model.IsChecked);
			model.IsChecked = true;
			Assert.Equal(CheckBoxState.Checked, checkBox.CheckState);
			Assert.Equal(CheckBoxState.Checked, model.State);
		}

		sealed class StateObserver : BindableObject
		{
			readonly CheckBox _checkBox;
			CheckBoxState _state;
			bool _isChecked;
			public StateObserver(CheckBox checkBox) => _checkBox = checkBox;
			public CheckBoxState State
			{
				get => _state;
				set
				{
					Assert.Equal(_checkBox.CheckState == CheckBoxState.Checked, _checkBox.IsChecked);
					if (_state == value)
						return;
					_state = value;
					OnPropertyChanged();
				}
			}
			public bool IsChecked
			{
				get => _isChecked;
				set
				{
					Assert.Equal(_checkBox.CheckState == CheckBoxState.Checked, _checkBox.IsChecked);
					if (_isChecked == value)
						return;
					_isChecked = value;
					OnPropertyChanged();
				}
			}
		}

		[Fact]
		public void SingleStateBindingKeepsDerivedBooleanConsistentInBothDirections()
		{
			var model = new SingleStateViewModel();
			var checkBox = new CheckBox { IsThreeState = true };
			checkBox.SetBinding(CheckBox.CheckStateProperty,
				new Binding(nameof(SingleStateViewModel.State), BindingMode.TwoWay, source: model));
			model.PropertyChanged += (_, _) =>
				Assert.Equal(model.State == CheckBoxState.Checked, model.IsChecked);
			checkBox.CheckStateChanged += (_, e) =>
			{
				Assert.Equal(model.State, e.Value);
				Assert.Equal(model.IsChecked, checkBox.IsChecked);
			};

			checkBox.IsChecked = true;
			Assert.Equal(CheckBoxState.Checked, model.State);
			Assert.True(model.IsChecked);

			checkBox.CheckState = CheckBoxState.Indeterminate;
			Assert.Equal(CheckBoxState.Indeterminate, model.State);
			Assert.False(model.IsChecked);

			model.State = CheckBoxState.Checked;
			Assert.Equal(CheckBoxState.Checked, checkBox.CheckState);
			Assert.True(checkBox.IsChecked);

			model.State = CheckBoxState.Unchecked;
			Assert.Equal(CheckBoxState.Unchecked, checkBox.CheckState);
			Assert.False(checkBox.IsChecked);
		}

		sealed class SingleStateViewModel : BindableObject
		{
			CheckBoxState _state;
			public CheckBoxState State
			{
				get => _state;
				set
				{
					if (_state == value)
						return;
					_state = value;
					OnPropertyChanged();
					OnPropertyChanged(nameof(IsChecked));
				}
			}
			public bool IsChecked => State == CheckBoxState.Checked;
		}

		[Theory]
		[InlineData(false, false, BindingMode.OneWay)]
		[InlineData(false, true, BindingMode.OneWay)]
		[InlineData(true, false, BindingMode.OneWay)]
		[InlineData(true, true, BindingMode.OneWay)]
		[InlineData(false, false, BindingMode.TwoWay)]
		[InlineData(false, true, BindingMode.TwoWay)]
		[InlineData(true, false, BindingMode.TwoWay)]
		[InlineData(true, true, BindingMode.TwoWay)]
		public void StateBindingBeforeFlagBindingPreservesModel(bool typedBinding, bool deferredContext, BindingMode stateBindingMode)
		{
			var model = new CheckBox { IsThreeState = true, CheckState = CheckBoxState.Indeterminate };
			var checkBox = new CheckBox { IsChecked = true };
			var writes = new List<CheckBoxState>();
			model.CheckStateChanged += (_, e) => writes.Add(e.Value);

			if (!deferredContext)
			{
				checkBox.BindingContext = model;
			}
			if (typedBinding)
			{
				checkBox.SetBinding(CheckBox.CheckStateProperty, static (CheckBox value) => value.CheckState,
					mode: stateBindingMode);
				checkBox.SetBinding(CheckBox.IsThreeStateProperty, static (CheckBox value) => value.IsThreeState,
					mode: BindingMode.TwoWay);
			}
			else
			{
				checkBox.SetBinding(CheckBox.CheckStateProperty, new Binding(nameof(CheckBox.CheckState), stateBindingMode));
				checkBox.SetBinding(CheckBox.IsThreeStateProperty, new Binding(nameof(CheckBox.IsThreeState), BindingMode.TwoWay));
			}
			if (deferredContext)
			{
				checkBox.BindingContext = model;
			}

			Assert.Empty(writes);
			Assert.Equal(CheckBoxState.Indeterminate, model.CheckState);
			Assert.True(checkBox.IsThreeState);
			Assert.Equal(CheckBoxState.Indeterminate, checkBox.CheckState);
			Assert.False(checkBox.IsChecked);
		}

		[Theory]
		[InlineData(BindingMode.OneTime)]
		[InlineData(BindingMode.OneWayToSource)]
		public void EnablingThreeStateDoesNotReapplyNonReadingStateBinding(BindingMode mode)
		{
			var model = new SingleStateViewModel { State = CheckBoxState.Checked };
			var checkBox = new CheckBox { IsChecked = true };
			checkBox.SetBinding(CheckBox.CheckStateProperty,
				new Binding(nameof(SingleStateViewModel.State), mode, source: model));
			model.State = CheckBoxState.Indeterminate;
			Assert.Equal(CheckBoxState.Checked, checkBox.CheckState);
			var modelChanges = 0;
			model.PropertyChanged += (_, e) =>
			{
				if (e.PropertyName == nameof(SingleStateViewModel.State))
				{
					modelChanges++;
				}
			};
			var states = new List<CheckBoxState>();
			checkBox.CheckStateChanged += (_, e) => states.Add(e.Value);

			checkBox.IsThreeState = true;

			Assert.Equal(CheckBoxState.Checked, checkBox.CheckState);
			Assert.True(checkBox.IsChecked);
			Assert.Equal(CheckBoxState.Indeterminate, model.State);
			Assert.Equal(0, modelChanges);
			Assert.Empty(states);
		}

		[Fact]
		public void NormalizedNativeWriteStillUpdatesStateBinding()
		{
			var model = new CheckBox { IsThreeState = true, CheckState = CheckBoxState.Checked };
			var checkBox = new CheckBox();
			checkBox.SetBinding(CheckBox.CheckStateProperty,
				new Binding(nameof(CheckBox.CheckState), BindingMode.TwoWay, source: model));

			((ICheckBox2)checkBox).CheckState = CheckBoxState.Indeterminate;

			Assert.Equal(CheckBoxState.Unchecked, checkBox.CheckState);
			Assert.Equal(CheckBoxState.Unchecked, model.CheckState);
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(false, true)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		public void ReentrantProjectionResetPreservesBooleanEventsAndCommands(bool threeState, bool enumWrite)
		{
			var checkBox = new CheckBox { IsThreeState = threeState };
			var states = new List<CheckBoxState>();
			var checkedValues = new List<bool>();
			var commands = new List<bool>();
			checkBox.PropertyChanged += (_, e) =>
			{
				if (e.PropertyName == nameof(CheckBox.IsChecked) && checkBox.IsChecked)
				{
					checkBox.IsChecked = false;
				}
			};
			checkBox.CheckStateChanged += (_, e) => states.Add(e.Value);
			checkBox.CheckedChanged += (_, e) => checkedValues.Add(e.Value);
			checkBox.Command = new Command(() => commands.Add(checkBox.IsChecked));

			if (enumWrite)
			{
				((ICheckBox2)checkBox).CheckState = CheckBoxState.Checked;
			}
			else
			{
				((ICheckBox)checkBox).IsChecked = true;
			}

			Assert.False(checkBox.IsChecked);
			Assert.Equal(CheckBoxState.Unchecked, checkBox.CheckState);
			Assert.Equal(new[] { CheckBoxState.Checked, CheckBoxState.Unchecked }, states);
			Assert.Equal(new[] { true, false }, checkedValues);
			Assert.Equal(new[] { true, false }, commands);
		}

		[Fact]
		public void ClearingProjectedBooleanDoesNotClearIndeterminate()
		{
			var checkBox = new CheckBox { IsThreeState = true, CheckState = CheckBoxState.Indeterminate };
			var changes = 0;
			checkBox.PropertyChanged += (_, e) =>
			{
				if (e.PropertyName is nameof(CheckBox.IsChecked) or nameof(CheckBox.CheckState))
				{
					changes++;
				}
			};

			checkBox.ClearValue(CheckBox.IsCheckedProperty);

			Assert.Equal(CheckBoxState.Indeterminate, checkBox.CheckState);
			Assert.False(checkBox.IsChecked);
			Assert.Equal(0, changes);
		}

		[Fact]
		public void ReentrantCheckBoxResetDoesNotInterruptEntryBinding()
		{
			var model = new Entry { Text = "initial" };
			var entry = new Entry();
			entry.SetBinding(Entry.TextProperty, new Binding(nameof(Entry.Text), BindingMode.TwoWay, source: model));
			var checkBox = new CheckBox();
			var events = new List<bool>();
			checkBox.CheckedChanged += (_, e) => events.Add(e.Value);
			checkBox.PropertyChanged += (_, e) =>
			{
				if (e.PropertyName != nameof(CheckBox.IsChecked))
				{
					return;
				}
				entry.Text = checkBox.IsChecked ? "checked" : "unchecked";
				if (checkBox.IsChecked)
				{
					checkBox.IsChecked = false;
				}
			};

			((ICheckBox2)checkBox).CheckState = CheckBoxState.Checked;

			Assert.Equal(new[] { true, false }, events);
			Assert.Equal("unchecked", model.Text);
			model.Text = "source update";
			Assert.Equal("source update", entry.Text);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void ReentrantCounterpartResetUpdatesOriginalBindingAndNotification(bool stateWrite)
		{
			var checkBox = new CheckBox();
			var model = new CheckBox();
			var sourceProperty = stateWrite ? CheckBox.CheckStateProperty : CheckBox.IsCheckedProperty;
			var counterpartName = stateWrite ? nameof(CheckBox.IsChecked) : nameof(CheckBox.CheckState);
			checkBox.SetBinding(sourceProperty,
				new Binding(sourceProperty.PropertyName, BindingMode.TwoWay, source: model));
			var values = new List<object>();
			checkBox.PropertyChanged += (_, e) =>
			{
				if (e.PropertyName == sourceProperty.PropertyName)
				{
					values.Add(checkBox.GetValue(sourceProperty));
				}
				if (e.PropertyName == counterpartName && checkBox.IsChecked)
				{
					if (stateWrite)
					{
						checkBox.IsChecked = false;
					}
					else
					{
						checkBox.CheckState = CheckBoxState.Unchecked;
					}
				}
			};

			if (stateWrite)
			{
				checkBox.CheckState = CheckBoxState.Checked;
			}
			else
			{
				checkBox.IsChecked = true;
			}

			Assert.False(checkBox.IsChecked);
			Assert.False(model.IsChecked);
			Assert.Equal(CheckBoxState.Unchecked, model.CheckState);
			Assert.Equal(stateWrite
				? new object[] { CheckBoxState.Checked, CheckBoxState.Unchecked }
				: new object[] { true, false }, values);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void DisablingThreeStateDoesNotRestoreQueuedMixedRequest(bool stateWrite)
		{
			var checkBox = new CheckBox { IsThreeState = true };
			var states = new List<CheckBoxState>();
			checkBox.CheckStateChanged += (_, e) => states.Add(e.Value);
			checkBox.PropertyChanged += (_, e) =>
			{
				if (e.PropertyName == nameof(CheckBox.IsChecked) && checkBox.IsChecked)
				{
					checkBox.CheckState = CheckBoxState.Indeterminate;
					checkBox.IsThreeState = false;
					checkBox.IsThreeState = true;
				}
			};

			if (stateWrite)
			{
				checkBox.CheckState = CheckBoxState.Checked;
			}
			else
			{
				checkBox.IsChecked = true;
			}

			Assert.True(checkBox.IsThreeState);
			Assert.False(checkBox.IsChecked);
			Assert.Equal(CheckBoxState.Unchecked, checkBox.CheckState);
			Assert.Equal(new[] { CheckBoxState.Checked, CheckBoxState.Unchecked }, states);
		}

		[Fact]
		public void ClearingBooleanProjectionAllowsLaterStateStyle()
		{
			var checkBox = new CheckBox { IsChecked = true };
			checkBox.ClearValue(CheckBox.IsCheckedProperty);
			checkBox.Style = new Style(typeof(CheckBox))
			{
				Setters = { new Setter { Property = CheckBox.CheckStateProperty, Value = CheckBoxState.Checked } }
			};

			Assert.Equal(CheckBoxState.Checked, checkBox.CheckState);
			Assert.True(checkBox.IsChecked);
		}

		[Fact]
		public void LocalIndeterminateWinsOverBooleanStyleAndClearingRevealsStyle()
		{
			var checkBox = new CheckBox { IsThreeState = true, CheckState = CheckBoxState.Indeterminate };
			checkBox.Style = new Style(typeof(CheckBox))
			{
				Setters = { new Setter { Property = CheckBox.IsCheckedProperty, Value = true } }
			};
			Assert.Equal(CheckBoxState.Indeterminate, checkBox.CheckState);
			Assert.False(checkBox.IsChecked);

			checkBox.ClearValue(CheckBox.CheckStateProperty);

			Assert.Equal(CheckBoxState.Checked, checkBox.CheckState);
			Assert.True(checkBox.IsChecked);
		}

		[Fact]
		public void IgnoredStyleDoesNotRemoveNativeProjectionOverLocalBoolean()
		{
			var checkBox = new CheckBox { IsThreeState = true, IsChecked = true };
			((ICheckBox2)checkBox).CheckState = CheckBoxState.Indeterminate;
			checkBox.Style = new Style(typeof(CheckBox))
			{
				Setters = { new Setter { Property = CheckBox.IsCheckedProperty, Value = false } }
			};

			Assert.Equal(CheckBoxState.Indeterminate, checkBox.CheckState);
			Assert.False(checkBox.IsChecked);
			checkBox.ClearValue(CheckBox.CheckStateProperty);
			Assert.Equal(CheckBoxState.Checked, checkBox.CheckState);
			Assert.True(checkBox.IsChecked);
		}

		[Fact]
		public void DisablingThreeStateAlsoNormalizesSavedProjectionFallback()
		{
			var checkBox = new CheckBox { IsThreeState = true, CheckState = CheckBoxState.Indeterminate };
			checkBox.IsChecked = true;
			checkBox.IsThreeState = false;
			checkBox.IsThreeState = true;
			checkBox.ClearValue(CheckBox.IsCheckedProperty);

			Assert.Equal(CheckBoxState.Unchecked, checkBox.CheckState);
			Assert.False(checkBox.IsChecked);
		}

		[Fact]
		public void ReentrantClearFromIndeterminatePublishesUncheckedOnce()
		{
			var checkBox = new CheckBox { IsThreeState = true, CheckState = CheckBoxState.Indeterminate };
			var values = new List<CheckBoxState>();
			var states = new List<CheckBoxState>();
			checkBox.PropertyChanging += (_, e) =>
			{
				if (e.PropertyName == nameof(CheckBox.CheckState))
					checkBox.IsThreeState = false;
			};
			checkBox.PropertyChanged += (_, e) =>
			{
				if (e.PropertyName == nameof(CheckBox.CheckState))
					values.Add(checkBox.CheckState);
			};
			checkBox.CheckStateChanged += (_, e) => states.Add(e.Value);

			checkBox.ClearValue(CheckBox.CheckStateProperty);

			Assert.False(checkBox.IsThreeState);
			Assert.Equal(new[] { CheckBoxState.Unchecked }, values);
			Assert.Equal(new[] { CheckBoxState.Unchecked }, states);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void DisablingThreeStateDuringPropertyChangingNormalizesPendingValue(bool initial)
		{
			var checkBox = new CheckBox { IsThreeState = true, IsChecked = initial };
			var states = new List<CheckBoxState>();
			var commands = 0;
			checkBox.PropertyChanging += (_, e) =>
			{
				if (e.PropertyName == nameof(CheckBox.CheckState))
					checkBox.IsThreeState = false;
			};
			checkBox.CheckStateChanged += (_, e) => states.Add(e.Value);
			checkBox.Command = new Command(() => commands++);
			checkBox.CheckState = CheckBoxState.Indeterminate;
			Assert.False(checkBox.IsThreeState);
			Assert.Equal(CheckBoxState.Unchecked, checkBox.CheckState);
			Assert.False(checkBox.IsChecked);
			Assert.Equal(initial ? new[] { CheckBoxState.Unchecked } : Array.Empty<CheckBoxState>(), states);
			Assert.Equal(states.Count, commands);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void ClearValuePublishesOnlyTheNormalizedFallback(bool initial)
		{
			var checkBox = new CheckBox
			{
				IsThreeState = true,
				Style = new Style(typeof(CheckBox))
				{
					Setters = { new Setter { Property = CheckBox.CheckStateProperty, Value = CheckBoxState.Indeterminate } }
				}
			};
			checkBox.CheckState = initial ? CheckBoxState.Checked : CheckBoxState.Unchecked;
			var changes = 0;
			var commands = 0;
			checkBox.PropertyChanging += (_, e) =>
			{
				if (e.PropertyName == nameof(CheckBox.CheckState))
					checkBox.IsThreeState = false;
			};
			checkBox.CheckStateChanged += (_, e) =>
			{
				Assert.Equal(CheckBoxState.Unchecked, e.Value);
				Assert.False(checkBox.IsChecked);
				changes++;
			};
			checkBox.Command = new Command(() => commands++);
			checkBox.ClearValue(CheckBox.CheckStateProperty);
			Assert.Equal(CheckBoxState.Unchecked, checkBox.CheckState);
			Assert.False(checkBox.IsChecked);
			Assert.Equal(initial ? 1 : 0, changes);
			Assert.Equal(changes, commands);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void PropertyChangingNormalizationDoesNotPublishMixedToBinding(bool initial)
		{
			var source = new CheckBox { IsThreeState = true, IsChecked = initial };
			var target = new CheckBox { IsThreeState = true };
			target.SetBinding(CheckBox.CheckStateProperty, static (CheckBox value) => value.CheckState, source: source);
			target.PropertyChanging += (_, e) =>
			{
				if (e.PropertyName == nameof(CheckBox.CheckState))
					target.IsThreeState = false;
			};
			source.CheckStateChanged += (_, e) => Assert.NotEqual(CheckBoxState.Indeterminate, e.Value);
			target.CheckState = CheckBoxState.Indeterminate;
			Assert.Equal(CheckBoxState.Unchecked, source.CheckState);
			Assert.Equal(CheckBoxState.Unchecked, target.CheckState);
			Assert.False(source.IsChecked);
			Assert.False(target.IsChecked);
		}

		[Fact]
		public void DisablingThreeStateNormalizesHiddenStyleWithoutTransientChangedNotifications()
		{
			var checkBox = new CheckBox
			{
				IsThreeState = true,
				Style = new Style(typeof(CheckBox))
				{
					Setters = { new Setter { Property = CheckBox.CheckStateProperty, Value = CheckBoxState.Indeterminate } }
				}
			};
			checkBox.CheckState = CheckBoxState.Checked;
			checkBox.IsThreeState = false;
			var values = new List<CheckBoxState>();
			checkBox.PropertyChanged += (_, e) =>
			{
				if (e.PropertyName == nameof(CheckBox.CheckState))
					values.Add(checkBox.CheckState);
			};
			checkBox.ClearValue(CheckBox.CheckStateProperty);
			Assert.Equal(new[] { CheckBoxState.Unchecked }, values);
			checkBox.IsThreeState = true;
			Assert.Equal(CheckBoxState.Unchecked, checkBox.CheckState);
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(false, true)]
		[InlineData(true, true)]
		public void DisablingThreeStateNormalizesReentrantAssignments(bool fromCommand, bool queueCheckedAfterMixed)
		{
			var checkBox = new CheckBox { IsThreeState = true };
			var states = new List<CheckBoxState>();
			var commands = 0;
			var reentered = false;
			void Reenter()
			{
				if (reentered || !checkBox.IsChecked)
					return;
				reentered = true;
				checkBox.CheckState = CheckBoxState.Indeterminate;
				if (queueCheckedAfterMixed)
					checkBox.CheckState = CheckBoxState.Checked;
				checkBox.IsThreeState = false;
			}
			checkBox.CheckStateChanged += (_, e) =>
			{
				states.Add(e.Value);
				if (!fromCommand)
					Reenter();
			};
			checkBox.Command = new Command(() =>
			{
				commands++;
				if (fromCommand)
					Reenter();
			});
			checkBox.CheckState = CheckBoxState.Checked;
			Assert.True(reentered);
			Assert.False(checkBox.IsThreeState);
			Assert.Equal(queueCheckedAfterMixed, checkBox.IsChecked);
			Assert.Equal(queueCheckedAfterMixed
				? new[] { CheckBoxState.Checked, CheckBoxState.Unchecked, CheckBoxState.Checked }
				: new[] { CheckBoxState.Checked, CheckBoxState.Unchecked }, states);
			Assert.Equal(states.Count, commands);
		}

		[Fact]
		public void MixedChangeHonorsCanExecute()
		{
			var commands = 0;
			var checkBox = new CheckBox { IsThreeState = true, IsChecked = true, Command = new Command(() => commands++, () => false) };
			checkBox.CheckState = CheckBoxState.Indeterminate;
			Assert.Equal(CheckBoxState.Indeterminate, checkBox.CheckState);
			Assert.Equal(0, commands);
		}

		[Theory]
		[InlineData("True", CheckBoxState.Checked)]
		[InlineData("False", CheckBoxState.Unchecked)]
		[InlineData("Indeterminate", CheckBoxState.Indeterminate)]
		public void DragAndDropRoundTripsState(string text, CheckBoxState expected)
		{
			var checkBox = new CheckBox { IsThreeState = true };
			Assert.True(checkBox.TrySetValue(text));
			Assert.Equal(expected, checkBox.CheckState);
			Assert.Equal(text, checkBox.GetStringValue());
			Assert.False(checkBox.TrySetValue("not a state"));
			Assert.Equal(expected, checkBox.CheckState);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task DragAndDropRoundTripsIndeterminateState(bool threeState)
		{
			var source = new CheckBox { IsThreeState = true, CheckState = CheckBoxState.Indeterminate };
			var target = new CheckBox { IsThreeState = threeState, IsChecked = true };
			var drag = new DragGestureRecognizer();
			var drop = new DropGestureRecognizer { AllowDrop = true };
			source.GestureRecognizers.Add(drag);
			target.GestureRecognizers.Add(drop);
			var data = drag.SendDragStarting(source).Data;
			Assert.Equal("Indeterminate", data.Text);
			await drop.SendDrop(new DropEventArgs(new DataPackageView(data)));
			Assert.Equal(threeState ? CheckBoxState.Indeterminate : CheckBoxState.Unchecked, target.CheckState);
			Assert.Equal(CheckBoxState.Indeterminate, source.CheckState);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void RadioButtonRetainsBooleanEventContract(bool value)
		{
			var radio = new RadioButton { IsChecked = !value };
			var events = 0;
			radio.CheckedChanged += (_, e) => { Assert.Equal(value, e.Value); events++; };
			radio.IsChecked = value;
			radio.IsChecked = value;
			Assert.Equal(1, events);
		}

		[Fact]
		public void TestOnEvent()
		{
			var checkBox = new CheckBox();
			var fired = false;
			checkBox.CheckedChanged += (sender, e) => fired = true;
			checkBox.IsChecked = true;
			Assert.True(fired);
		}

		[Fact]
		public void TestOnEventNotDoubleFired()
		{
			var checkBox = new CheckBox();
			bool fired = false;
			checkBox.IsChecked = true;
			checkBox.CheckedChanged += (sender, args) => fired = true;
			checkBox.IsChecked = true;
			Assert.False(fired);
		}

		[Fact]
		public void CheckedChangedEventArgs_ShouldHaveCorrectValue()
		{
			var checkBox = new CheckBox();
			CheckedChangedEventArgs eventArgs = null;
			checkBox.CheckedChanged += (sender, e) => eventArgs = e;
			checkBox.IsChecked = true;
			Assert.NotNull(eventArgs);
			Assert.True(eventArgs.Value);
			checkBox.IsChecked = false;
			Assert.False(eventArgs.Value);
		}

		[Fact]
		public void CheckedChangedEvent_ShouldFireOnlyWhenValueChanges()
		{
			var checkBox = new CheckBox();
			int eventFireCount = 0;
			checkBox.CheckedChanged += (sender, e) => eventFireCount++;
			checkBox.IsChecked = false;
			Assert.Equal(0, eventFireCount);
			checkBox.IsChecked = true;
			Assert.Equal(1, eventFireCount);
			checkBox.IsChecked = true;
			Assert.Equal(1, eventFireCount);
			checkBox.IsChecked = false;
			Assert.Equal(2, eventFireCount);
		}

		[Fact]
		public void CheckedVisualStates()
		{
			var vsgList = CreateTestStateGroups();
			string checkedStateName = CheckBox.IsCheckedVisualState;
			var checkedState = new VisualState() { Name = checkedStateName };
			var stateGroup = vsgList[0];
			stateGroup.States.Add(checkedState);
			var element = new CheckBox();
			VisualStateManager.SetVisualStateGroups(element, vsgList);
			element.IsChecked = true;
			Assert.Equal(checkedStateName, stateGroup.CurrentState.Name);
			element.IsChecked = false;
			Assert.NotEqual(checkedStateName, stateGroup.CurrentState.Name);
		}

		[Fact]
		public void IndeterminateVisualStateTransitionsAndDisabledPrecedence()
		{
			var groups = CreateTestStateGroups();
			groups[0].States.Add(new VisualState { Name = CheckBox.IsCheckedVisualState });
			groups[0].States.Add(new VisualState { Name = CheckBox.IsIndeterminateVisualState });
			var checkBox = new CheckBox { IsThreeState = true };
			VisualStateManager.SetVisualStateGroups(checkBox, groups);
			checkBox.CheckState = CheckBoxState.Checked;
			Assert.Equal(CheckBox.IsCheckedVisualState, groups[0].CurrentState.Name);
			checkBox.CheckState = CheckBoxState.Indeterminate;
			Assert.Equal(CheckBox.IsIndeterminateVisualState, groups[0].CurrentState.Name);
			checkBox.CheckState = CheckBoxState.Checked;
			Assert.Equal(CheckBox.IsCheckedVisualState, groups[0].CurrentState.Name);
			checkBox.CheckState = CheckBoxState.Indeterminate;
			Assert.Equal(CheckBox.IsIndeterminateVisualState, groups[0].CurrentState.Name);
			checkBox.IsEnabled = false;
			Assert.Equal(DisabledStateName, groups[0].CurrentState.Name);
			checkBox.CheckState = CheckBoxState.Checked;
			Assert.Equal(DisabledStateName, groups[0].CurrentState.Name);
			checkBox.CheckState = CheckBoxState.Indeterminate;
			Assert.Equal(DisabledStateName, groups[0].CurrentState.Name);
			checkBox.IsEnabled = true;
			Assert.Equal(CheckBox.IsIndeterminateVisualState, groups[0].CurrentState.Name);
			checkBox.CheckState = CheckBoxState.Unchecked;
			Assert.Equal(NormalStateName, groups[0].CurrentState.Name);
			checkBox.CheckState = CheckBoxState.Indeterminate;
			checkBox.IsThreeState = false;
			Assert.Equal(NormalStateName, groups[0].CurrentState.Name);
		}

		[Fact]
		public void MissingIndeterminateVisualStateFallsBackToNormal()
		{
			var groups = CreateTestStateGroups();
			groups[0].States.Add(new VisualState { Name = CheckBox.IsCheckedVisualState });
			var checkBox = new CheckBox { IsThreeState = true };
			VisualStateManager.SetVisualStateGroups(checkBox, groups);
			checkBox.IsChecked = true;
			Assert.Equal(CheckBox.IsCheckedVisualState, groups[0].CurrentState.Name);
			checkBox.CheckState = CheckBoxState.Indeterminate;
			Assert.Equal(NormalStateName, groups[0].CurrentState.Name);
		}

		[Fact]
		public void CheckBoxClickWhenCommandCanExecuteFalse()
		{
			bool invoked = false;
			var checkBox = new CheckBox { Command = new Command(() => invoked = true, () => false) };
			checkBox.IsChecked = true;
			Assert.False(invoked);
		}

		[Fact]
		public void CheckBoxClickWhenCommandCanExecuteTrue()
		{
			bool invoked = false;
			var checkBox = new CheckBox { Command = new Command(() => invoked = true, () => true) };
			checkBox.IsChecked = true;
			Assert.True(invoked);
		}

		[Fact]
		public void Command_ShouldExecuteWithCorrectParameter()
		{
			object receivedParameter = null;
			var expectedParameter = "TestParameter";
			var checkBox = new CheckBox
			{
				Command = new Command<object>(param => receivedParameter = param, param => true),
				CommandParameter = expectedParameter
			};
			checkBox.IsChecked = true;
			Assert.Equal(expectedParameter, receivedParameter);
		}

		[Fact]
		public void Command_ShouldNotExecuteWhenNull()
		{
			var checkBox = new CheckBox { Command = null };
			checkBox.IsChecked = true;
			Assert.True(checkBox.IsChecked);
		}

		[Fact]
		public void CommandParameter_ShouldSupportNullValue()
		{
			object receivedParameter = "NotNull";
			var checkBox = new CheckBox
			{
				Command = new Command<object>(param => receivedParameter = param, param => true),
				CommandParameter = null
			};
			checkBox.IsChecked = true;
			Assert.Null(receivedParameter);
		}

		[Fact]
		public void Command_ShouldExecuteOnlyOnCheckedStateChange()
		{
			int executeCount = 0;
			var checkBox = new CheckBox { Command = new Command(() => executeCount++, () => true) };
			checkBox.IsChecked = true;
			Assert.Equal(1, executeCount);
			checkBox.IsChecked = true;
			Assert.Equal(1, executeCount);
			checkBox.IsChecked = false;
			Assert.Equal(2, executeCount);
		}

		[Fact]
		public void Command_And_CheckedChanged_ShouldBothFire()
		{
			bool commandExecuted = false;
			bool eventFired = false;
			var checkBox = new CheckBox { Command = new Command(() => commandExecuted = true, () => true) };
			checkBox.CheckedChanged += (sender, e) => eventFired = true;
			checkBox.IsChecked = true;
			Assert.True(commandExecuted);
			Assert.True(eventFired);
		}
	}
}
