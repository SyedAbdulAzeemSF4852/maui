using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Provides data for <see cref="CheckBox.CheckStateChanged"/>.</summary>
	public class CheckStateChangedEventArgs : EventArgs
	{
		/// <summary>Initializes event data with the new checkbox state.</summary>
		/// <param name="value">The new state.</param>
		public CheckStateChangedEventArgs(CheckBoxState value) => Value = value;

		/// <summary>Gets the complete state of the checkbox.</summary>
		public CheckBoxState Value { get; }
	}
}
