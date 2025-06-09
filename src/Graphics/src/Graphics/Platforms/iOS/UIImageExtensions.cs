using System;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Graphics.Platform
{
	public static class UIImageExtensions
	{
		public static UIImage ScaleImage(this UIImage target, float maxWidth, float maxHeight, bool disposeOriginal = false)
		{
			float originalWidth = (float)target.Size.Width;
			float originalHeight = (float)target.Size.Height;

			float widthRatio = maxWidth / originalWidth;
			float heightRatio = maxHeight / originalHeight;

			float scale = Math.Min(widthRatio, heightRatio);

			var targetWidth = (float)Math.Round(originalWidth * scale);
			var targetHeight = (float)Math.Round(originalHeight * scale);

			System.Diagnostics.Debug.WriteLine($"Scaling image from {originalWidth}x{originalHeight} to {targetWidth}x{targetHeight}");

			return ScaleImage(target, new CGSize(targetWidth, targetHeight), disposeOriginal);
		}

		public static UIImage ScaleImage(this UIImage target, CGSize size, bool disposeOriginal = false)
		{
			UIGraphics.BeginImageContext(size);
			target.Draw(new CGRect(CGPoint.Empty, size));
			var image = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();

			if (disposeOriginal)
			{
				target.Dispose();
			}

			return image;
		}

		public static UIImage NormalizeOrientation(this UIImage target, bool disposeOriginal = false)
		{
			if (target.Orientation == UIImageOrientation.Up)
			{
				return target;
			}

			UIGraphics.BeginImageContextWithOptions(target.Size, false, target.CurrentScale);
			target.Draw(CGPoint.Empty);
			var image = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();

			if (disposeOriginal)
			{
				target.Dispose();
			}

			return image;
		}
	}
}
