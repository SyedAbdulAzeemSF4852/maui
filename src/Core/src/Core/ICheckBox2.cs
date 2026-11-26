namespace Microsoft.Maui
{
	/// <summary>
	/// Extends the Boolean checkbox contract with an optional indeterminate state.
	/// </summary>
	public interface ICheckBox2 : ICheckBox
	{
		/// <summary>
		/// Gets or sets the complete state. The inherited <see cref="ICheckBox.IsChecked"/>
		/// is true only when this value is <see cref="CheckBoxState.Checked"/>.
		/// </summary>
		CheckBoxState CheckState { get; set; }

		/// <summary>
		/// Gets whether application code can assign indeterminate and user interaction cycles through three states.
		/// </summary>
		bool IsThreeState { get; }
	}
}
