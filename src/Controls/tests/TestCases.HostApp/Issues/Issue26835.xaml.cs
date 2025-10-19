using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
    [Issue(IssueTracker.Github, 26835, "[iOS & Mac] Headers and Footers Get Cropped in CollectionView with Horizontal Grid and Horizontal Orientation",
        PlatformAffected.iOS | PlatformAffected.macOS)]
    public partial class Issue26835 : ContentPage
    {
        public ObservableCollection<string> Items { get; set; }

        public Issue26835()
        {
            InitializeComponent();
            
            // Create test data with enough items to demonstrate horizontal scrolling
            Items = new ObservableCollection<string>();
            for (int i = 1; i <= 30; i++)
            {
                Items.Add($"Item {i}");
            }
            
            CollectionView.ItemsSource = Items;
            
            // Additional verification: Set AutomationIds for testing
            CollectionView.AutomationId = "HorizontalGridCollectionView";
        }
    }
}