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

		var time = timePicker.Time;
		var format = timePicker.Format;
		var formattingCulture = GetFormattingCulture(format, cultureInfo);

		// Apply the same culture to both the text display and the picker
		mauiTimePicker.Text = time?.ToFormattedString(format ?? string.Empty, formattingCulture);

		if (picker is not null)
		{
			picker.Locale = IsStandardTimeFormat(format)
				? NSLocale.CurrentLocale
				: new NSLocale(formattingCulture.TwoLetterISOLanguageName);
		}

		mauiTimePicker.UpdateCharacterSpacing(timePicker);
	}

	internal static CultureInfo GetFormattingCulture(string? format, CultureInfo cultureInfo)
	{
		if (IsStandardTimeFormat(format))
		{
			return cultureInfo;
		}

		bool has12HourSpecifier = false;
		bool has24HourSpecifier = false;
		char quote = '\0';
		var customFormat = format!;

		for (int i = 0; i < customFormat.Length; i++)
		{
			char character = customFormat[i];

			if (character == '\\' && quote == '\0')
			{
				i++;
				continue;
			}

			if (character is '\'' or '"')
			{
				if (quote == '\0')
				{
					quote = character;
				}
				else if (quote == character)
				{
					quote = '\0';
				}

				continue;
			}

			if (quote != '\0')
			{
				continue;
			}

			has12HourSpecifier |= character is 'h' or 't';
			has24HourSpecifier |= character == 'H';
		}

		if (has12HourSpecifier)
		{
			return new CultureInfo("en-US");
		}

		return has24HourSpecifier ? new CultureInfo("de-DE") : cultureInfo;
	}

	internal static bool IsStandardTimeFormat(string? format) =>
		string.IsNullOrEmpty(format) || format.Length == 1;

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