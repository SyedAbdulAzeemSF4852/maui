using Android.Content;

using Google.Android.Material.CheckBox;

namespace Microsoft.Maui.Platform;

public class MauiMaterialCheckBox : MaterialCheckBox
{
    public MauiMaterialCheckBox(Context context) : base(context)
    {

    }

    public override bool PerformClick()
    {
        if (Enabled == false)
        {
            return false;
        }

        return base.PerformClick();
    }

    /// <summary>Toggles between checked and unchecked, changing indeterminate to unchecked.</summary>
    public override void Toggle()
    {
        CheckedState = CheckedState == MaterialCheckBox.StateUnchecked
            ? MaterialCheckBox.StateChecked
            : MaterialCheckBox.StateUnchecked;
    }
}