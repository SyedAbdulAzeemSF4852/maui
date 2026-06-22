using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    public class Issue26835 : _IssuesUITest
    {
        public Issue26835(TestDevice device) : base(device) { }

        public override string Issue => "[iOS & Mac] Headers and Footers Get Cropped in CollectionView with Horizontal Grid and Horizontal Orientation";

        [Test]
        [Category(UITestCategories.CollectionView)]
        public void HeaderAndFooterShouldNotBeCroppedInHorizontalGrid()
        {
            // Verify the CollectionView is present
            App.WaitForElement("HorizontalGridCollectionView");
            
            // Verify header elements are visible and not cropped
            App.WaitForElement("HeaderTitle");
            App.WaitForElement("HeaderSubtitle");  
            App.WaitForElement("HeaderLine3");
            
            // Scroll to the right to reveal the footer at the end of the collection
            for (int i = 0; i < 8; i++)
            {
                App.ScrollRight("HorizontalGridCollectionView", ScrollStrategy.Auto, 0.8, 500);
            }
            
            // Verify footer elements are visible and not cropped
            App.WaitForElement("FooterTitle");
            App.WaitForElement("FooterSubtitle");
            App.WaitForElement("FooterLine3");
            
            // Scroll back to verify header is still visible
            for (int i = 0; i < 8; i++)
            {
                App.ScrollLeft("HorizontalGridCollectionView", ScrollStrategy.Auto, 0.8, 500);
            }
            
            // Verify header is still visible after scrolling
            App.WaitForElement("HeaderTitle");
            App.WaitForElement("HeaderSubtitle");
            App.WaitForElement("HeaderLine3");
        }
    }
}