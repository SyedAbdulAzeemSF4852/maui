using System;
using System.Globalization;
using Foundation;
using Microsoft.Maui.Storage;
using UIKit;

namespace Microsoft.Maui.Platform;

public static class TimePickerExtensions
{
	public static void UpdateFormat(this MauiTimePicker mauiTimePicker, ITimePicker timePicker)
	{
		mauiTimePicker.UpdateTime(timePicker, null);
	}

	public static void UpdateFormat(this UIDatePicker picker, ITimePicker timePicker)
	{
		picker.UpdateTime(timePicker);
	}

	public static void UpdateFormat(this MauiTimePicker mauiTimePicker, ITimePicker timePicker, UIDatePicker? picker)
	{
		mauiTimePicker.UpdateTime(timePicker, picker);
	}

	public static void UpdateTime(this MauiTimePicker mauiTimePicker, ITimePicker timePicker)
	{
		mauiTimePicker.UpdateTime(timePicker, null);
	}

	public static void UpdateTime(this UIDatePicker picker, ITimePicker timePicker)
	{
		if (picker is not null)
		{
			picker.Date = new DateTime(1, 1, 1).Add(timePicker?.Time ?? TimeSpan.Zero).ToNSDate();
		}
	}

	public static void UpdateTime(this MauiTimePicker mauiTimePicker, ITimePicker timePicker, UIDatePicker? picker)
	{
		picker?.UpdateTime(timePicker);

		var cultureInfo = Culture.CurrentCulture;
		var format = timePicker.Format;
		var time = timePicker.Time;

		if (string.IsNullOrEmpty(format) || format == "t")
		{
			// "t" is the standard short-time specifier and the default TimePicker.Format value;
			// treat it like an empty format so it isn't misread as a literal 't' (12-hour) specifier.
			if (picker is not null)
			{
				picker.Locale = NSLocale.CurrentLocale;

				var formatter = new NSDateFormatter
				{
					Locale = picker.Locale!,
					TimeStyle = NSDateFormatterStyle.Short,
					DateStyle = NSDateFormatterStyle.None
				};

				mauiTimePicker.Text = formatter.StringFor(picker.Date);
			}
			else
			{
				mauiTimePicker.Text = time?.ToFormattedString(
					cultureInfo.DateTimeFormat.ShortTimePattern,
					cultureInfo);
			}
		}
		else
		{
			// Explicit Format belongs to the application.
			mauiTimePicker.Text = time?.ToFormattedString(format, cultureInfo);

			if (picker is not null)
			{
				var formattingCulture = GetFormattingCulture(format);
				picker.Locale = formattingCulture;
			}
		}

		mauiTimePicker.UpdateCharacterSpacing(timePicker);
	}

	// Selects the wheel locale from unescaped h, H, and t specifiers outside quoted literals.
	static NSLocale GetFormattingCulture(string format)
	{
		var has12HourSpecifier = false;
		var has24HourSpecifier = false;
		var quote = '\0';

		for (var i = 0; i < format.Length; i++)
		{
			var character = format[i];

			if (character == '\\')
			{
				// Escaped character: skip it, regardless of quote state.
				i++;
				continue;
			}

			if (quote != '\0')
			{
				if (character == quote)
				{
					quote = '\0';
				}
				continue;
			}

			if (character == '\'' || character == '"')
			{
				quote = character;
				continue;
			}

			if (character == 'h' || character == 't')
			{
				has12HourSpecifier = true;
			}
			else if (character == 'H')
			{
				has24HourSpecifier = true;
			}
		}

		if (has12HourSpecifier && !has24HourSpecifier)
		{
			return new NSLocale("en_US");
		}

		if (has24HourSpecifier && !has12HourSpecifier)
		{
			return new NSLocale("de_DE");
		}

		return NSLocale.CurrentLocale;
	}

	public static void UpdateTextAlignment(this MauiTimePicker textField, ITimePicker timePicker)
	{
		UISemanticContentAttribute updateValue = textField.SemanticContentAttribute;

		textField.TextAlignment = (updateValue == UISemanticContentAttribute.ForceRightToLeft) ? UITextAlignment.Right : UITextAlignment.Left;
	}

	internal static void UpdateIsOpen(this UIDatePicker picker, ITimePicker timePicker)
	{
		if (timePicker.IsOpen)
			picker.BecomeFirstResponder();
		else
			picker.ResignFirstResponder();
	}

	internal static void UpdateIsOpen(this MauiTimePicker mauiTimePicker, ITimePicker timePicker)
	{
		if (timePicker.IsOpen)
			mauiTimePicker.BecomeFirstResponder();
		else
			mauiTimePicker.ResignFirstResponder();
	}
}