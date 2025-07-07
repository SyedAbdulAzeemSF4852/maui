using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls;

public partial class Image
{
	internal new static void RemapForControls()
	{
		// Auto-set WidthRequest and HeightRequest for images with intrinsic dimensions
		// This ensures downsized images are displayed at their correct size on all platforms
		ImageHandler.Mapper.AppendToMapping<Image, IImageHandler>(nameof(Source), MapSourceAndAutoSize);
	}

	static void MapSourceAndAutoSize(IImageHandler handler, Image image)
	{
		// First try to get dimensions from the ImageSource before mapping
		TrySetDimensionsFromImageSource(image);

		// Then let the default source mapping happen
		ImageHandler.MapSource(handler, image);

		// For cases where we couldn't read dimensions from the source, try platform-specific fallback
		Microsoft.Maui.Dispatching.Dispatcher.GetForCurrentThread()?.DispatchDelayed(TimeSpan.FromMilliseconds(100), () =>
		{
			TrySetDimensionsFromPlatformView(handler, image);
		});
	}

	static void TrySetDimensionsFromImageSource(Image image)
	{
		// Only auto-size if width and height are not explicitly set by the user
		if (image.IsSet(WidthRequestProperty) || image.IsSet(HeightRequestProperty))
			return;

		var source = image.Source;
		if (source == null)
			return;

		// Handle file-based image sources
		if (source is FileImageSource fileSource)
		{
			TrySetDimensionsFromFile(image, fileSource.File);
		}
		else if (source is StreamImageSource streamSource)
		{
			TrySetDimensionsFromStream(image, streamSource);
		}
		else if (source is UriImageSource uriSource)
		{
			// For URI sources, we'll have to wait for the image to load
			// This will be handled by the platform-specific code after loading
		}
	}

	static void TrySetDimensionsFromFile(Image image, string filePath)
	{
		try
		{
			if (File.Exists(filePath))
			{
				using var fileStream = File.OpenRead(filePath);
				var dimensions = GetImageDimensionsFromStream(fileStream);
				if (dimensions.HasValue)
				{
					image.WidthRequest = dimensions.Value.Width;
					image.HeightRequest = dimensions.Value.Height;
				}
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Image.Mapper: Error reading file dimensions: {ex.Message}");
		}
	}

	static void TrySetDimensionsFromStream(Image image, StreamImageSource streamSource)
	{
		try
		{
			// We need to get the stream from the StreamImageSource
			// This is a bit tricky as the Stream property is a Func<CancellationToken, Task<Stream>>
			var streamTask = streamSource.Stream?.Invoke(System.Threading.CancellationToken.None);
			if (streamTask != null)
			{
				streamTask.ContinueWith(task =>
				{
					if (task.IsCompletedSuccessfully && task.Result != null)
					{
						try
						{
							var dimensions = GetImageDimensionsFromStream(task.Result);
							if (dimensions.HasValue)
							{
								Microsoft.Maui.Dispatching.Dispatcher.GetForCurrentThread()?.Dispatch(() =>
								{
									// Only set if not already set by user
									if (!image.IsSet(WidthRequestProperty) && !image.IsSet(HeightRequestProperty))
									{
										image.WidthRequest = dimensions.Value.Width;
										image.HeightRequest = dimensions.Value.Height;
									}
								});
							}
						}
						finally
						{
							task.Result?.Dispose();
						}
					}
				}, TaskScheduler.Default);
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Image.Mapper: Error reading stream dimensions: {ex.Message}");
		}
	}






	static (double Width, double Height)? GetImageDimensionsFromStream(Stream stream)
	{
		try
		{
			// Save original position
			var originalPosition = stream.Position;
			stream.Position = 0;

			// Read the image header to get dimensions
			// This is a simple PNG header reader - can be extended for other formats
			var buffer = new byte[24]; // Enough for PNG header
			if (stream.Read(buffer, 0, buffer.Length) >= 24)
			{
				// Check for PNG signature
				if (buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47)
				{
					// PNG format - read IHDR chunk
					var width = (buffer[16] << 24) | (buffer[17] << 16) | (buffer[18] << 8) | buffer[19];
					var height = (buffer[20] << 24) | (buffer[21] << 16) | (buffer[22] << 8) | buffer[23];

					// Restore position
					stream.Position = originalPosition;

					return (width, height);
				}
				// Check for JPEG signature
				else if (buffer[0] == 0xFF && buffer[1] == 0xD8)
				{
					// For JPEG, we need to find the SOF0 marker - this is more complex
					// For now, return null and let the platform handle it
					stream.Position = originalPosition;
					return null;
				}
			}

			// Restore position if we couldn't read the format
			stream.Position = originalPosition;
			return null;
		}
		catch
		{
			return null;
		}
	}

	static void TrySetDimensionsFromPlatformView(IImageHandler handler, Image image)
	{
		// Only auto-size if width and height are not explicitly set by the user
		if (image.IsSet(WidthRequestProperty) || image.IsSet(HeightRequestProperty))
			return;

#if ANDROID
            TrySetDimensionsFromPlatformViewAndroid(handler, image);
#elif WINDOWS
            TrySetDimensionsFromPlatformViewWindows(handler, image);
#elif IOS || MACCATALYST
            // iOS handles this correctly by default, so no action needed
#endif
	}

#if ANDROID
        static void TrySetDimensionsFromPlatformViewAndroid(IImageHandler handler, Image image)
        {
            if (handler?.PlatformView is not Android.Widget.ImageView imageView)
                return;

            var drawable = imageView.Drawable;
            if (drawable == null)
                return;

            // Get image dimensions
            var imageWidth = drawable.IntrinsicWidth;
            var imageHeight = drawable.IntrinsicHeight;

            // Try to get dimensions from bitmap if intrinsic dimensions are not available
            if ((imageWidth <= 0 || imageHeight <= 0) && drawable is Android.Graphics.Drawables.BitmapDrawable bitmapDrawable)
            {
                var bitmap = bitmapDrawable.Bitmap;
                if (bitmap != null && !bitmap.IsRecycled)
                {
                    imageWidth = bitmap.Width;
                    imageHeight = bitmap.Height;
                }
            }

            // For wrapped drawables (like from Glide), try to unwrap
            if ((imageWidth <= 0 || imageHeight <= 0) && drawable is Android.Graphics.Drawables.LayerDrawable layerDrawable && layerDrawable.NumberOfLayers > 0)
            {
                var innerDrawable = layerDrawable.GetDrawable(0);
                if (innerDrawable is Android.Graphics.Drawables.BitmapDrawable innerBitmapDrawable)
                {
                    var bitmap = innerBitmapDrawable.Bitmap;
                    if (bitmap != null && !bitmap.IsRecycled)
                    {
                        imageWidth = bitmap.Width;
                        imageHeight = bitmap.Height;
                    }
                }
            }

            // Only set dimensions if we have valid ones and they're reasonable (not screen-sized)
            if (imageWidth > 0 && imageHeight > 0 && imageWidth < 2000 && imageHeight < 2000)
            {   
                // Convert pixel dimensions to density-independent pixels for MAUI
                var context = imageView.Context;
                if (context != null)
                {
                    var density = context.Resources?.DisplayMetrics?.Density ?? 1.0f;
                    var dipWidth = imageWidth / density;
                    var dipHeight = imageHeight / density;
                    
                    image.WidthRequest = dipWidth;
                    image.HeightRequest = dipHeight;
                }
            }
        }
#endif

#if WINDOWS
        static void TrySetDimensionsFromPlatformViewWindows(IImageHandler handler, Image image)
        {
            if (handler?.PlatformView is not Microsoft.UI.Xaml.Controls.Image winImage)
                return;

            var source = winImage.Source;
            if (source == null)
                return;

            // Get image dimensions from the Windows ImageSource
            var imageWidth = 0.0;
            var imageHeight = 0.0;

            if (source is Microsoft.UI.Xaml.Media.Imaging.BitmapSource bitmapSource)
            {
                imageWidth = bitmapSource.PixelWidth;
                imageHeight = bitmapSource.PixelHeight;
            }

            // Only set dimensions if we have valid ones and they're reasonable (not screen-sized)
            if (imageWidth > 0 && imageHeight > 0 && imageWidth < 2000 && imageHeight < 2000)
            {   
                image.WidthRequest = imageWidth;
                image.HeightRequest = imageHeight;
            }
        }
#endif
}
