#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	public partial class CheckBox
	{
		bool _stateTransaction;
		bool _stateBindingInput;
		object _requestedStateValue;
		BindableProperty _stateWriteProperty;
		BindableProperty _stateBindingFeedback;
		Queue<StateWrite> _stateWrites;
		StateProjection _booleanProjection;
		StateProjection _enumProjection;

		internal static bool IsStateProperty(BindableProperty property) =>
			property == IsCheckedProperty || property == CheckStateProperty;

		internal bool SuppressStateBindingWriteBack =>
			_stateBindingInput && _stateWriteProperty == CheckStateProperty &&
			!Equals(_requestedStateValue, CheckState);

		internal void SetStateValue(BindableProperty property, object value, SetValueFlags attributes,
			SetValuePrivateFlags privateAttributes, SetterSpecificity specificity)
		{
			if (property == _stateBindingFeedback &&
				(privateAttributes & SetValuePrivateFlags.FromBinding) != 0 && Equals(GetValue(property), value))
			{
				var projection = GetStateProjection(property);
				if (projection?.Source != null && projection.Specificity == specificity)
				{
					projection.OriginalValue = value;
					projection.HadOriginal = true;
				}
				else
				{
					GetOrCreateContext(property).Values.SetValue(specificity, value);
				}
				return;
			}

			ProcessStateWrite(new StateWrite(property, value, attributes, privateAttributes, specificity));
		}

		internal void ClearStateValue(BindableProperty property, SetterSpecificity specificity) =>
			ProcessStateWrite(new StateWrite(property, null, SetValueFlags.None, SetValuePrivateFlags.None, specificity, true));

		void ProcessStateWrite(StateWrite write)
		{
			if (_stateTransaction)
			{
				(_stateWrites ??= new Queue<StateWrite>()).Enqueue(write);
				return;
			}

			_stateTransaction = true;
			try
			{
				ExecuteStateWrite(write);
				while (_stateWrites?.Count > 0)
					ExecuteStateWrite(_stateWrites.Dequeue());
			}
			finally
			{
				_stateWrites?.Clear();
				_stateWriteProperty = null;
				_requestedStateValue = null;
				_stateBindingInput = false;
				_stateTransaction = false;
			}
		}

		void ExecuteStateWrite(StateWrite write)
		{
			_stateWriteProperty = write.Property;
			_requestedStateValue = write.Value;
			_stateBindingInput = (write.PrivateAttributes & SetValuePrivateFlags.FromBinding) != 0;
			var context = GetContext(write.Property);
			var projection = GetStateProjection(write.Property);
			if (write.IsClear)
			{
				if (context == null)
				{
					return;
				}
				if (projection?.Source != null)
				{
					if (projection.Specificity == write.Specificity)
					{
						projection.HadOriginal = false;
						projection.OriginalValue = null;
					}
					else
					{
						context.Values.Remove(write.Specificity);
					}
					return;
				}
				if (context.Values.GetValue(write.Specificity) == null &&
					context.Values.GetSpecificity() != SetterSpecificity.FromHandler)
				{
					return;
				}
				ClearValueCoreActual(write.Property, write.Specificity);
				return;
			}

			try
			{
				SetValueCoreActual(write.Property, write.Value, write.Attributes, write.PrivateAttributes, write.Specificity);
			}
			finally
			{
				if (GetContext(write.Property) is { } settingContext)
				{
					settingContext.Attributes &= ~BindableContextAttributes.IsBeingSet;
					settingContext.DelayedSetters = null;
				}
			}
		}

		internal bool PrepareStateValue(BindablePropertyContext context, object value, SetterSpecificity specificity)
		{
			var projection = GetStateProjection(context.Property);
			if (projection?.Source != null)
			{
				var currentSpecificity = context.Values.GetSpecificity();
				if (currentSpecificity == SetterSpecificity.FromHandler &&
					specificity != SetterSpecificity.FromHandler &&
					specificity < context.Values.GetClearedSpecificity())
				{
					// A lower-priority setter must not remove a native projection over a local value.
					context.Values.SetValue(specificity, value);
					return false;
				}
				if (specificity >= currentSpecificity || currentSpecificity == SetterSpecificity.FromHandler)
				{
					RestoreStateProjection(context, projection);
				}
			}
			return true;
		}

		StateProjection GetStateProjection(BindableProperty property) =>
			property == IsCheckedProperty ? _booleanProjection : _enumProjection;

		void RestoreStateProjection(BindablePropertyContext context, StateProjection projection)
		{
			context.Values.Remove(projection.Specificity);
			if (projection.HadOriginal)
			{
				var value = projection.OriginalValue;
				if (context.Property.CoerceValue is { } coerce)
				{
					value = coerce(this, value);
				}
				context.Values.SetValue(projection.Specificity, value);
			}
			projection.Source = null;
			projection.OriginalValue = null;
			projection.HadOriginal = false;
		}

		void SetStateProjection(BindableProperty property, object value, SetterSpecificity specificity, BindableProperty source)
		{
			var context = GetOrCreateContext(property);
			var original = context.Values.GetValue();
			var projection = GetStateProjection(property);
			if (projection?.Source == source)
			{
				RestoreStateProjection(context, projection);
			}
			var currentSpecificity = context.Values.GetSpecificity();
			if (specificity != SetterSpecificity.FromHandler && currentSpecificity == SetterSpecificity.FromHandler)
			{
				context.Values.Remove(SetterSpecificity.FromHandler);
				currentSpecificity = context.Values.GetSpecificity();
			}
			if (specificity < currentSpecificity)
			{
				SynchronizeState(property, context.Values.GetValue());
			}
			else if (!specificity.IsDefault || !Equals(context.Values.GetValue(), value))
			{
				projection ??= new StateProjection();
				if (property == IsCheckedProperty)
				{
					_booleanProjection = projection;
				}
				else
				{
					_enumProjection = projection;
				}
				projection.Source = source;
				projection.Specificity = specificity;
				projection.OriginalValue = context.Values.GetValue(specificity);
				projection.HadOriginal = projection.OriginalValue != null;
				context.Values.SetValue(specificity, value);
				context.Attributes &= ~BindableContextAttributes.IsDefaultValueCreated;
			}

			value = context.Values.GetValue();
			if (Equals(original, value) || (context.Attributes & BindableContextAttributes.IsBeingSet) != 0)
			{
				return;
			}
			if (!IsApplyingBindings && !SuppressStateBindingWriteBack && context.Bindings.GetValue() is { } binding)
			{
				ApplyStateBinding(property, binding);
			}
			if (Equals(context.Values.GetValue(), value))
			{
				base.OnBindablePropertySet(property, original, value, true, true);
			}
		}

		internal void ApplyStateBinding(BindableProperty property, BindingBase binding)
		{
			var previous = _stateBindingFeedback;
			_stateBindingFeedback = property;
			try
			{
				binding.Apply(true);
			}
			finally
			{
				_stateBindingFeedback = previous;
			}
		}

		sealed class StateProjection
		{
			public BindableProperty Source;
			public SetterSpecificity Specificity;
			public object OriginalValue;
			public bool HadOriginal;
		}

		sealed class StateWrite
		{
			public readonly BindableProperty Property;
			public object Value;
			public readonly SetValueFlags Attributes;
			public readonly SetValuePrivateFlags PrivateAttributes;
			public readonly SetterSpecificity Specificity;
			public readonly bool IsClear;

			public StateWrite(BindableProperty property, object value, SetValueFlags attributes,
				SetValuePrivateFlags privateAttributes, SetterSpecificity specificity, bool isClear = false)
			{
				Property = property;
				Value = value;
				Attributes = attributes;
				PrivateAttributes = privateAttributes;
				Specificity = specificity;
				IsClear = isClear;
			}
		}
	}
}
