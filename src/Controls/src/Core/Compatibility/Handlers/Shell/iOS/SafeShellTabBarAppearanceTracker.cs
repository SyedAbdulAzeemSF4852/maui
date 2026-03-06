#nullable disable
using System;
using System.ComponentModel;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class SafeShellTabBarAppearanceTracker : IShellTabBarAppearanceTracker
	{
		UIColor _defaultBarTint;
		UIColor _defaultTint;
		UIColor _defaultUnselectedTint;
		UITabBarAppearance _tabBarAppearance;

		// iOS 26 Liquid Glass ignores UITabBarAppearance.Normal.IconColor for unselected items.
		// We work around this by tinting tab item images directly in SetAppearance.
		UIColor _iOS26UnselectedIconColor;
		UIColor _iOS26SelectedIconColor;

		// Track the last applied color values so we skip no-op re-applications
		// that would cause visual glitches during tab-switch animations.
		UIColor _lastAppliedUnselectedColor;
		UIColor _lastAppliedSelectedColor;

		public virtual void ResetAppearance(UITabBarController controller)
		{
			if (_defaultTint == null)
				return;

			var tabBar = controller.TabBar;
			tabBar.BarTintColor = _defaultBarTint;
			tabBar.TintColor = _defaultTint;
			tabBar.UnselectedItemTintColor = _defaultUnselectedTint;
		}

		public virtual void SetAppearance(UITabBarController controller, ShellAppearance appearance)
		{
			IShellAppearanceElement appearanceElement = appearance;
			var backgroundColor = appearanceElement.EffectiveTabBarBackgroundColor;
			var foregroundColor = appearanceElement.EffectiveTabBarForegroundColor; // Currently unused
			var disabledColor = appearanceElement.EffectiveTabBarDisabledColor; // Unused on iOS
			var unselectedColor = appearanceElement.EffectiveTabBarUnselectedColor;
			var titleColor = appearanceElement.EffectiveTabBarTitleColor;

			var tabBar = controller.TabBar;

			if (_defaultTint == null)
			{
				_defaultBarTint = tabBar.BarTintColor;
				_defaultTint = tabBar.TintColor;
				_defaultUnselectedTint = tabBar.UnselectedItemTintColor;
			}

			if (OperatingSystem.IsIOSVersionAtLeast(15) || OperatingSystem.IsTvOSVersionAtLeast(15))
				UpdateiOS15TabBarAppearance(controller, appearance);
			else
				UpdateTabBarAppearance(controller, appearance);

			// On iOS 26+, Liquid Glass ignores UITabBarAppearance icon colors
			// for unselected items. Work around this by tinting the images
			// directly. Also handles clearing: if colors were previously applied
			// but are now null, reset images to Automatic rendering mode.
			// We only re-apply when colors actually changed to avoid redundant
			// re-tinting during tab-switch animations.
			if ((OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26))
			 && (_iOS26UnselectedIconColor is not null || _iOS26SelectedIconColor is not null
			  || _lastAppliedUnselectedColor is not null || _lastAppliedSelectedColor is not null))
			{
				bool colorsChanged = !TabbedViewExtensions.ColorsEqual(_lastAppliedUnselectedColor, _iOS26UnselectedIconColor)
								  || !TabbedViewExtensions.ColorsEqual(_lastAppliedSelectedColor, _iOS26SelectedIconColor);

				if (colorsChanged)
				{
					ApplyiOS26TabBarItemIconColors(controller);
				}
			}
		}

		public virtual void UpdateLayout(UITabBarController controller)
		{
			if (!(OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26)))
			{
				return;
			}

			// Icon tinting: only needed on first layout after colors are set (or after a color
			// change). SetAppearance may fire before tab items load in ShellSectionRenderer, so
			// we re-try here in ViewDidLayoutSubviews which fires after images are ready.
			// Dispatched async to avoid replacing images mid-layout (misalignment).
			if ((_iOS26UnselectedIconColor is not null || _iOS26SelectedIconColor is not null)
			 && _lastAppliedUnselectedColor is null && _lastAppliedSelectedColor is null)
			{
				CoreFoundation.DispatchQueue.MainQueue.DispatchAsync(() =>
				{
					ApplyiOS26TabBarItemIconColors(controller);
				});
			}
		}

		#region IDisposable Support

		protected virtual void Dispose(bool disposing)
		{
		}

		public void Dispose()
		{
			Dispose(true);
		}

		#endregion

		[System.Runtime.Versioning.SupportedOSPlatform("ios15.0")]
		[System.Runtime.Versioning.SupportedOSPlatform("tvos15.0")]
		void UpdateiOS15TabBarAppearance(UITabBarController controller, ShellAppearance appearance)
		{
			IShellAppearanceElement appearanceElement = appearance;

			var backgroundColor = appearanceElement.EffectiveTabBarBackgroundColor;
			var foregroundColor = appearanceElement.EffectiveTabBarForegroundColor;
			var unselectedColor = appearanceElement.EffectiveTabBarUnselectedColor;
			var titleColor = appearanceElement.EffectiveTabBarTitleColor;

			controller.TabBar
				.UpdateiOS15TabBarAppearance(
					ref _tabBarAppearance,
					null,
					null,
					foregroundColor ?? titleColor,
					unselectedColor,
					backgroundColor,
					titleColor ?? foregroundColor,
					unselectedColor);

			if (OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26))
			{
				_iOS26UnselectedIconColor = unselectedColor?.ToPlatform();
				_iOS26SelectedIconColor = (foregroundColor ?? titleColor)?.ToPlatform();
			}
		}

		[System.Runtime.Versioning.SupportedOSPlatform("ios26.0")]
		[System.Runtime.Versioning.SupportedOSPlatform("maccatalyst26.0")]
		void ApplyiOS26TabBarItemIconColors(UITabBarController controller)
		{
			bool tinted = controller.TabBar.ApplyiOS26TabBarIconColors(
				_iOS26UnselectedIconColor, _iOS26SelectedIconColor);

			// Only mark as applied if items actually had images and were tinted.
			if (tinted)
			{
				_lastAppliedUnselectedColor = _iOS26UnselectedIconColor;
				_lastAppliedSelectedColor = _iOS26SelectedIconColor;
			}
		}

		void UpdateTabBarAppearance(UITabBarController controller, ShellAppearance appearance)
		{
			IShellAppearanceElement appearanceElement = appearance;
			var backgroundColor = appearanceElement.EffectiveTabBarBackgroundColor;
			var foregroundColor = appearanceElement.EffectiveTabBarForegroundColor;
			var unselectedColor = appearanceElement.EffectiveTabBarUnselectedColor;
			var titleColor = appearanceElement.EffectiveTabBarTitleColor;

			var tabBar = controller.TabBar;

			if (backgroundColor is not null && backgroundColor.IsNotDefault())
				tabBar.BarTintColor = backgroundColor.ToPlatform();

			if (unselectedColor is not null && unselectedColor.IsNotDefault())
			{
				tabBar.UnselectedItemTintColor = unselectedColor.ToPlatform();
				UITabBarItem.Appearance.SetTitleTextAttributes(new UIStringAttributes { ForegroundColor = unselectedColor.ToPlatform() }, UIControlState.Normal);
			}

			if (titleColor is not null && titleColor.IsNotDefault() ||
				foregroundColor is not null && foregroundColor.IsNotDefault())
			{
				tabBar.TintColor = (foregroundColor ?? titleColor).ToPlatform();
				UITabBarItem.Appearance.SetTitleTextAttributes(new UIStringAttributes { ForegroundColor = (titleColor ?? foregroundColor).ToPlatform() }, UIControlState.Selected);
			}
		}
	}
}
