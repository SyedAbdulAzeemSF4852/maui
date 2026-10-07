using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class RefreshViewStub : StubBase, IRefreshView
	{
		bool _isRefreshEnabled = true;

		public bool IsRefreshing { get; set; }

		public bool IsRefreshEnabled
		{
			get => _isRefreshEnabled;
			set => SetProperty(ref _isRefreshEnabled, value);
		}

		public Paint RefreshColor { get; set; }

		public IView Content { get; set; }
	}
}