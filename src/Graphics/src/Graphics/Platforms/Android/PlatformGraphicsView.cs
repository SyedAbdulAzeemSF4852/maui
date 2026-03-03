using System;
using Android.Content;
using Android.Graphics;
using Android.Text;
using Android.Util;
using Android.Views;

namespace Microsoft.Maui.Graphics.Platform
{
	public class PlatformGraphicsView : View
	{
		private int _width, _height;
		private readonly PlatformCanvas _canvas;
		private readonly ScalingCanvas _scalingCanvas;
		private IDrawable _drawable;
		private readonly float _scale = 1;
		private Color _backgroundColor;

		public PlatformGraphicsView(Context context, IAttributeSet attrs, IDrawable drawable = null) : base(context, attrs)
		{
			_scale = Resources.DisplayMetrics.Density;
			_canvas = new PlatformCanvas(context);
			_scalingCanvas = new ScalingCanvas(_canvas);
			Drawable = drawable;
		}

		public PlatformGraphicsView(Context context, IDrawable drawable = null) : base(context)
		{
			_scale = Resources.DisplayMetrics.Density;
			_canvas = new PlatformCanvas(context);
			_scalingCanvas = new ScalingCanvas(_canvas);
			Drawable = drawable;
		}

		public Color BackgroundColor
		{
			get => _backgroundColor;
			set
			{
				_backgroundColor = value;
				Invalidate();
			}
		}

		public IDrawable Drawable
		{
			get => _drawable;
			set
			{
				_drawable = value;
				Invalidate();
			}
		}

		public override void Draw(Canvas androidCanvas)
		{
			if (_drawable == null)
				return;

			var dirtyRect = new RectF(0, 0, _width, _height);

			_canvas.Canvas = androidCanvas;
			if (_backgroundColor != null)
			{
				_canvas.FillColor = _backgroundColor;
				_canvas.FillRectangle(dirtyRect);
				_canvas.FillColor = Colors.White;
			}

			_scalingCanvas.ResetState();
			// Round pixel dimensions to the nearest integer dp value, then compute an adjusted
			// scale factor so those integer dp values map to the exact pixel allocation.
			// This gives the drawable clean integer dimensions with no sub-pixel gaps at view edges.
			var logicalWidth = MathF.Round(_width / _scale);
			var logicalHeight = MathF.Round(_height / _scale);
			if (logicalWidth > 0 && logicalHeight > 0)
			{
				_scalingCanvas.Scale(_width / logicalWidth, _height / logicalHeight);
				dirtyRect.Width = logicalWidth;
				dirtyRect.Height = logicalHeight;
			}
			else
			{
				_scalingCanvas.Scale(_scale, _scale);
				dirtyRect.Width = _width / _scale;
				dirtyRect.Height = _height / _scale;
			}

			_drawable.Draw(_scalingCanvas, dirtyRect);
			_canvas.Canvas = null;
		}

		protected override void OnSizeChanged(int width, int height, int oldWidth, int oldHeight)
		{
			base.OnSizeChanged(width, height, oldWidth, oldHeight);
			_width = width;
			_height = height;
		}
	}
}
