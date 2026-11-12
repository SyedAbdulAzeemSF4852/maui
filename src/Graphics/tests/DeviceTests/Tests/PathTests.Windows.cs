using System;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Maui.Controls.Shapes;
using Xunit;

namespace Microsoft.Maui.Graphics.DeviceTests;

public class PathTests
{
	[Fact]
	public void GeometryGroupWithMoveOnlyFigureAndLineConverts()
	{
		var group = new GeometryGroup();
		group.Children.Add(new PathGeometry(new PathFigureCollection { new PathFigure() }));
		group.Children.Add(new LineGeometry(new Point(15, 15), new Point(19, 19)));
		var path = ((IShape)group).PathForBounds(new Rect(0, 0, 100, 100));
		var device = CanvasDevice.GetSharedDevice();

		foreach (var fillMode in new[] { CanvasFilledRegionDetermination.Winding, CanvasFilledRegionDetermination.Alternate })
		{
			using var platformGeometry = global::Microsoft.Maui.Graphics.Platform.GraphicsExtensions.AsCanvasGeometry(path, device, fillMode);
			using var win2DGeometry = global::Microsoft.Maui.Graphics.Win2D.W2DExtensions.AsPath(path, device, fillMode);

			Assert.NotNull(platformGeometry);
			Assert.NotNull(win2DGeometry);
			Assert.Equal(MathF.Sqrt(32), platformGeometry.ComputePathLength(), 3);
			Assert.Equal(MathF.Sqrt(32), win2DGeometry.ComputePathLength(), 3);
		}
	}
}
