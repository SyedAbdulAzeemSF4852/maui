#nullable enable
#if __IOS__ || MACCATALYST
using PlatformView = Microsoft.Maui.Platform.MauiCheckBox;
#elif __ANDROID__
using PlatformView = AndroidX.AppCompat.Widget.AppCompatCheckBox;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.CheckBox;
#elif (NETSTANDARD || !PLATFORM)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	/// <summary>
	/// Represents the view handler for the abstract <see cref="ICheckBox"/> view and its platform-specific implementation.
	/// </summary>
	/// <seealso href="https://learn.microsoft.com/dotnet/maui/user-interface/handlers/">Conceptual documentation on handlers</seealso>
	public partial class CheckBoxHandler : ICheckBoxHandler
	{
		public static IPropertyMapper<ICheckBox, ICheckBoxHandler> Mapper = new PropertyMapper<ICheckBox, ICheckBoxHandler>(ViewHandler.ViewMapper)
		{
#if MONOANDROID
			[nameof(ICheckBox.Background)] = MapBackground,
#endif
			[nameof(ICheckBox2.IsThreeState)] = MapIsThreeState,
			[nameof(ICheckBox.IsChecked)] = MapIsChecked,
			[nameof(ICheckBox2.CheckState)] = MapCheckState,
			[nameof(ICheckBox.Foreground)] = MapForeground,
		};

		public static CommandMapper<ICheckBox, CheckBoxHandler> CommandMapper = new(ViewCommandMapper)
		{
		};

		public CheckBoxHandler() : base(Mapper, CommandMapper)
		{

		}

		public CheckBoxHandler(IPropertyMapper? mapper)
			: base(mapper ?? Mapper, CommandMapper)
		{
		}

		public CheckBoxHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
		}

		ICheckBox ICheckBoxHandler.VirtualView => VirtualView;

		PlatformView ICheckBoxHandler.PlatformView => PlatformView;

		internal static void SetCheckedState(ICheckBox checkBox, bool? value)
		{
			if (checkBox is ICheckBox2 checkBox2)
			{
				var state = value switch
				{
					true => CheckBoxState.Checked,
					false => CheckBoxState.Unchecked,
					null => CheckBoxState.Indeterminate
				};
				if (checkBox2.CheckState != state)
					checkBox2.CheckState = state;
			}
			else if (value.HasValue)
			{
				checkBox.IsChecked = value.Value;
			}
		}

		internal static CheckBoxState GetCheckState(ICheckBox checkBox) =>
			checkBox is ICheckBox2 checkBox2 ? checkBox2.CheckState :
				checkBox.IsChecked ? CheckBoxState.Checked : CheckBoxState.Unchecked;

		/// <summary>Maps the complete checkbox state through the existing checked-state mapping.</summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="check">The associated checkbox.</param>
		public static void MapCheckState(ICheckBoxHandler handler, ICheckBox check) =>
			handler.UpdateValue(nameof(ICheckBox.IsChecked));

#if MONOANDROID
		/// <summary>
		/// Maps the abstract <see cref="IView.Background"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="check">The associated <see cref="ICheckBox"/> instance.</param>
		public static partial void MapBackground(ICheckBoxHandler handler, ICheckBox check);
#endif

		/// <summary>
		/// Maps the abstract <see cref="ICheckBox.IsChecked"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="check">The associated <see cref="ICheckBox"/> instance.</param>
		public static partial void MapIsChecked(ICheckBoxHandler handler, ICheckBox check);

		/// <summary>Maps whether user interaction cycles through three states.</summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="check">The associated checkbox.</param>
		public static partial void MapIsThreeState(ICheckBoxHandler handler, ICheckBox check);

		/// <summary>
		/// Maps the abstract <see cref="ICheckBox.Foreground"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="check">The associated <see cref="ICheckBox"/> instance.</param>
		public static partial void MapForeground(ICheckBoxHandler handler, ICheckBox check);
	}
}