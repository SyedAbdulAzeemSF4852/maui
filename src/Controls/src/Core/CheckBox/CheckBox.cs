#nullable disable
using System;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Represents a control that a user can select or clear.
	/// </summary>
	/// <remarks>
	/// Use <see cref="IsChecked"/> for Boolean selection and <see cref="CheckState"/> for the
	/// complete state, including indeterminate when <see cref="IsThreeState"/> is true.
	/// </remarks>
	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	public partial class CheckBox : View, IElementConfiguration<CheckBox>, IBorderElement, IColorElement, ICheckBox2, ICommandElement
	{
		readonly Lazy<PlatformConfigurationRegistry<CheckBox>> _platformConfigurationRegistry;
		CheckBoxState _committedState;
		bool _reportedIsChecked;
		bool _notifyingProjection;
		int _commitVersion;
		
		/// <summary>
		/// The visual state name for the checked state of the <see cref="CheckBox"/>.
		/// </summary>
		/// <value>The string "IsChecked".</value>
		public const string IsCheckedVisualState = "IsChecked";

		/// <summary>
		/// The visual state name for the indeterminate state of the <see cref="CheckBox"/>.
		/// </summary>
		/// <value>The string "IsIndeterminate".</value>
		public const string IsIndeterminateVisualState = "IsIndeterminate";

		/// <summary>Bindable property for <see cref="IsChecked"/>. This is a bindable property.</summary>
		public static readonly BindableProperty IsCheckedProperty =
			BindableProperty.Create(nameof(IsChecked), typeof(bool), typeof(CheckBox), BooleanBoxes.FalseBox,
				defaultBindingMode: BindingMode.TwoWay,
				propertyChanging: (bindable, oldValue, value) =>
				{
					var checkBox = (CheckBox)bindable;
					if (checkBox.CheckState != ((bool)value ? CheckBoxState.Checked : CheckBoxState.Unchecked))
						checkBox.OnPropertyChanging(nameof(CheckState));
				});

		/// <summary>Bindable property for <see cref="CheckState"/>.</summary>
		public static readonly BindableProperty CheckStateProperty =
			BindableProperty.Create(nameof(CheckState), typeof(CheckBoxState), typeof(CheckBox), CheckBoxState.Unchecked,
				defaultBindingMode: BindingMode.TwoWay,
				validateValue: (bindable, value) => (CheckBoxState)value is
					CheckBoxState.Unchecked or CheckBoxState.Checked or CheckBoxState.Indeterminate,
				coerceValue: (bindable, value) =>
					!((CheckBox)bindable).IsThreeState && (CheckBoxState)value == CheckBoxState.Indeterminate
						? CheckBoxState.Unchecked : value,
				propertyChanging: (bindable, oldValue, value) =>
				{
					var checkBox = (CheckBox)bindable;
					if (checkBox.IsChecked != ((CheckBoxState)value == CheckBoxState.Checked))
						checkBox.OnPropertyChanging(nameof(IsChecked));
				});

		/// <summary>Bindable property for <see cref="IsThreeState"/>.</summary>
		public static readonly BindableProperty IsThreeStateProperty =
			BindableProperty.Create(nameof(IsThreeState), typeof(bool), typeof(CheckBox), BooleanBoxes.FalseBox,
				defaultBindingMode: BindingMode.TwoWay);

		/// <summary>Bindable property for the <see cref="Command"/> property.</summary>
		public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(CheckBox), null, propertyChanging: CommandElement.OnCommandChanging, propertyChanged: CommandElement.OnCommandChanged);

		/// <summary>Bindable property for the <see cref="CommandParameter"/> property.</summary>
		public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(CheckBox), null, propertyChanged: CommandElement.OnCommandParameterChanged);

		/// <summary>
		/// Gets or sets the command executed when the Boolean <see cref="IsChecked"/> value changes.
		/// This is a bindable property.
		/// </summary>
		/// <remarks>
		/// Transitions between unchecked and indeterminate do not execute the command.
		/// Use <see cref="CheckStateChanged"/> to observe every complete state change.
		/// </remarks>
		public ICommand Command
		{
			get => (ICommand)GetValue(CommandProperty);
			set => SetValue(CommandProperty, value);
		}

		/// <summary>
		/// Gets or sets the parameter to pass to the <see cref="Command"/> when it is executed. This is a bindable property.
		/// </summary>
		public object CommandParameter
		{
			get => GetValue(CommandParameterProperty);
			set => SetValue(CommandParameterProperty, value);
		}

		/// <summary>Bindable property for <see cref="Color"/>. This is a bindable property.</summary>
		public static readonly BindableProperty ColorProperty = ColorElement.ColorProperty;

		/// <summary>
		/// Gets or sets the color of the checkbox.
		/// This is a bindable property.
		/// </summary>
		/// <value>The <see cref="Graphics.Color"/> of the checkbox. The default is platform-specific.</value>
		public Color Color
		{
			get => (Color)GetValue(ColorProperty);
			set => SetValue(ColorProperty, value);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CheckBox"/> class.
		/// </summary>
		public CheckBox() => _platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<CheckBox>>(() => new PlatformConfigurationRegistry<CheckBox>(this));

		/// <summary>
		/// Gets or sets a value indicating whether the <see cref="CheckBox"/> is checked.
		/// This is a bindable property.
		/// </summary>
		/// <value>True only when <see cref="CheckState"/> is <see cref="CheckBoxState.Checked"/>. The default is false.</value>
		/// <remarks>Setting false clears indeterminate, even when this property already returns false.</remarks>
		public bool IsChecked
		{
			get => (bool)GetValue(IsCheckedProperty);
			set => SetValue(IsCheckedProperty, BooleanBoxes.Box(value));
		}

		/// <summary>Gets or sets the complete checkbox state. This is a bindable property.</summary>
		/// <remarks>
		/// Indeterminate is normalized to unchecked while <see cref="IsThreeState"/> is false.
		/// The normalized request is not remembered or restored when three-state support is enabled later.
		/// Enable <see cref="IsThreeState"/> before assigning indeterminate. Enabling the flag
		/// re-reads an existing one-way or two-way state binding from its current source.
		/// Bind this property, rather than both state properties, when a view model needs a single source of truth.
		/// </remarks>
		public CheckBoxState CheckState
		{
			get => (CheckBoxState)GetValue(CheckStateProperty);
			set => SetValue(CheckStateProperty, value);
		}

		/// <summary>
		/// Gets or sets whether application code and user interaction can set the indeterminate state.
		/// This is a bindable property.
		/// </summary>
		/// <remarks>
		/// The default is false. When true, user interaction cycles from unchecked to checked
		/// to indeterminate, then back to unchecked. Otherwise, user interaction remains binary.
		/// Disabling three-state support resets an indeterminate <see cref="CheckState"/> to unchecked.
		/// </remarks>
		public bool IsThreeState
		{
			get => (bool)GetValue(IsThreeStateProperty);
			set => SetValue(IsThreeStateProperty, BooleanBoxes.Box(value));
		}

		void OnBooleanRequested(bool value)
		{
			if (!_notifyingProjection && value == IsChecked &&
				CheckState != (value ? CheckBoxState.Checked : CheckBoxState.Unchecked))
			{
				_notifyingProjection = true;
				try
				{
					OnPropertyChanging(nameof(CheckState));
				}
				finally
				{
					_notifyingProjection = false;
				}
			}
		}

		void SynchronizeState(BindableProperty source, object value)
		{
			var specificity = GetContext(source)?.Values.GetSpecificity() ?? SetterSpecificity.DefaultValue;
			if (source == IsCheckedProperty)
			{
				SetStateProjection(CheckStateProperty,
					(bool)value ? CheckBoxState.Checked : CheckBoxState.Unchecked, specificity, source);
			}
			else
			{
				SetStateProjection(IsCheckedProperty,
					BooleanBoxes.Box((CheckBoxState)value == CheckBoxState.Checked), specificity, source);
			}
		}

		void UpdateThreeState()
		{
			if (GetContext(CheckStateProperty) is not { } context)
			{
				return;
			}

			if (IsThreeState)
			{
				if (context.Bindings.GetValue() is { } binding &&
					binding.GetRealizedMode(CheckStateProperty) is BindingMode.OneWay or BindingMode.TwoWay)
				{
					binding.Apply(false);
				}
				return;
			}

			if (_enumProjection is { Source: not null, HadOriginal: true, OriginalValue: CheckBoxState.Indeterminate })
			{
				_enumProjection.OriginalValue = CheckBoxState.Unchecked;
			}
			if (_stateWrites != null)
			{
				foreach (var setter in _stateWrites)
				{
					if (!setter.IsClear && setter.Property == CheckStateProperty && Equals(setter.Value, CheckBoxState.Indeterminate))
					{
						setter.Value = CheckBoxState.Unchecked;
					}
				}
			}

			for (var i = 0; i < context.Values.Count - 1; i++)
			{
				if ((CheckBoxState)context.Values.GetValueAt(i) == CheckBoxState.Indeterminate)
					context.Values[context.Values.GetSpecificityAt(i)] = CheckBoxState.Unchecked;
			}

			if (CheckState == CheckBoxState.Indeterminate)
				SetValue(CheckStateProperty, CheckBoxState.Unchecked, EffectiveStateSpecificity());
		}

		SetterSpecificity EffectiveStateSpecificity()
		{
			var specificity = GetContext(CheckStateProperty)?.Values.GetSpecificity() ?? SetterSpecificity.DefaultValue;
			return specificity == SetterSpecificity.DefaultValue ? SetterSpecificity.ManualValueSetter : specificity;
		}

		private protected override void OnBindablePropertySet(BindableProperty property, object original, object value, bool changed, bool willFirePropertyChanged)
		{
			base.OnBindablePropertySet(property, original, value, changed, willFirePropertyChanged);
			if (IsStateProperty(property) || (property == IsThreeStateProperty && !_stateTransaction))
			{
				CommitState();
			}
		}

		void CommitState()
		{
			var state = CheckState;
			if (state == _committedState)
				return;

			_committedState = state;
			var version = ++_commitVersion;
			var isChecked = state == CheckBoxState.Checked;
			Handler?.UpdateValue(nameof(ICheckBox.Foreground));
			CheckStateChanged?.Invoke(this, new CheckStateChangedEventArgs(state));
			if (version != _commitVersion)
				return;

			if (isChecked != _reportedIsChecked)
			{
				_reportedIsChecked = isChecked;
				CheckedChanged?.Invoke(this, new CheckedChangedEventArgs(isChecked));
				if (version != _commitVersion)
					return;

				if (Command?.CanExecute(CommandParameter) == true)
					Command.Execute(CommandParameter);
			}

			if (version == _commitVersion)
				ChangeVisualState();
		}

		protected internal override void ChangeVisualState()
		{
			if (IsEnabled && CheckState != CheckBoxState.Unchecked)
			{
				string stateName = IsChecked ? IsCheckedVisualState : IsIndeterminateVisualState;
				bool isStateAvailable = false;
				var visualStates = VisualStateManager.GetVisualStateGroups(this);
				foreach (var group in visualStates)
				{
					if (group.Name is not "CommonStates")
					{
						continue;
					}

					foreach (var state in group.States)
					{
						if (state.Name == stateName)
						{
							isStateAvailable = true;
							break;
						}
					}

					break;
				}

				if (isStateAvailable)
				{
					VisualStateManager.GoToState(this, stateName);
				}
				else
				{
					VisualStateManager.GoToState(this, VisualStateManager.CommonStates.Normal);
				}
			}
			else
				base.ChangeVisualState();
		}

		/// <summary>
		/// Occurs when the <see cref="IsChecked"/> property changes.
		/// </summary>
		public event EventHandler<CheckedChangedEventArgs> CheckedChanged;

		/// <summary>Occurs for each complete state change, after both state properties are updated.</summary>
		/// <remarks>Raised before <see cref="CheckedChanged"/>, which only reports Boolean changes.</remarks>
		public event EventHandler<CheckStateChangedEventArgs> CheckStateChanged;

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, CheckBox> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		void IBorderElement.OnBorderColorPropertyChanged(Color oldValue, Color newValue)
		{
		}

		Color IBorderElement.BorderColor => Colors.Transparent;
		int IBorderElement.CornerRadius => 0;
		double IBorderElement.BorderWidth => 0;
		int IBorderElement.CornerRadiusDefaultValue => 0;
		Color IBorderElement.BorderColorDefaultValue => Colors.Transparent;
		double IBorderElement.BorderWidthDefaultValue => 0;
		bool IBorderElement.IsCornerRadiusSet() => false;
		bool IBorderElement.IsBackgroundColorSet() => IsSet(BackgroundColorProperty);
		bool IBorderElement.IsBackgroundSet() => IsSet(BackgroundProperty);
		bool IBorderElement.IsBorderColorSet() => false;
		bool IBorderElement.IsBorderWidthSet() => false;
		void ICommandElement.CanExecuteChanged(object sender, EventArgs e) =>
			RefreshIsEnabledProperty();

		protected override bool IsEnabledCore =>
			base.IsEnabledCore && CommandElement.GetCanExecute(this, CommandProperty);
		public Paint Foreground => Color?.AsPaint();

		bool ICheckBox.IsChecked
		{
			get => IsChecked;
			set => SetValue(IsCheckedProperty, BooleanBoxes.Box(value), SetterSpecificity.FromHandler);
		}

		CheckBoxState ICheckBox2.CheckState
		{
			get => CheckState;
			set => SetValue(CheckStateProperty, value, SetterSpecificity.FromHandler);
		}

		bool ICheckBox2.IsThreeState => IsThreeState;

		ICommand ICommandElement.Command => Command;

		object ICommandElement.CommandParameter => CommandParameter;

		WeakCommandSubscription ICommandElement.CleanupTracker { get; set; }

		private protected override string GetDebuggerDisplay()
		{
			return $"{base.GetDebuggerDisplay()}, CheckState = {CheckState}";
		}

		internal override bool TrySetValue(string text)
		{
			if (string.Equals(text, "Indeterminate", StringComparison.OrdinalIgnoreCase))
			{
				CheckState = CheckBoxState.Indeterminate;
				return true;
			}

			if (bool.TryParse(text, out bool result))
			{
				IsChecked = result;
				return true;
			}

			return false;
		}
	}
}