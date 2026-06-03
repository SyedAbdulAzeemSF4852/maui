#nullable disable
using System;
using AndroidX.RecyclerView.Widget;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public class RecyclerViewScrollListener<TItemsView, TItemsViewSource> : RecyclerView.OnScrollListener
		where TItemsView : ItemsView
		where TItemsViewSource : IItemsViewSource
	{
		protected ItemsViewAdapter<TItemsView, TItemsViewSource> ItemsViewAdapter;
		bool _disposed;
		int _horizontalOffset, _verticalOffset;
		TItemsView _itemsView;
		bool _pendingRemainingItemsThresholdReached;
		readonly bool _getCenteredItemOnXAndY = false;
		bool _hasCompletedFirstLayout = false;

		public RecyclerViewScrollListener(TItemsView itemsView, ItemsViewAdapter<TItemsView, TItemsViewSource> itemsViewAdapter) : this(itemsView, itemsViewAdapter, false)
		{

		}

		public RecyclerViewScrollListener(TItemsView itemsView, ItemsViewAdapter<TItemsView, TItemsViewSource> itemsViewAdapter, bool getCenteredItemOnXAndY)
		{
			_itemsView = itemsView;
			UpdateAdapter(itemsViewAdapter);
			_getCenteredItemOnXAndY = getCenteredItemOnXAndY;
		}

		internal void UpdateAdapter(ItemsViewAdapter<TItemsView, TItemsViewSource> itemsViewAdapter)
		{
			ItemsViewAdapter = itemsViewAdapter;
			// Reset flag when adapter changes to handle ItemsSource updates
			_hasCompletedFirstLayout = false;
		}

		public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
		{
			base.OnScrolled(recyclerView, dx, dy);

			var itemCount = recyclerView.GetAdapter()?.ItemCount ?? 0;
			_horizontalOffset = itemCount == 0 ? 0 : _horizontalOffset + dx;
			_verticalOffset = itemCount == 0 ? 0 : _verticalOffset + dy;

			// Prevent the Scrolled event from firing on the very first layout callback only.
			// This is the initial OnScrolled(0,0) call when the view is first laid out.
			// After that, layout is marked as complete and all subsequent scroll events are allowed.
			if (!_hasCompletedFirstLayout && !recyclerView.IsLaidOut && dx == 0 && dy == 0)
			{
				return;
			}

			// Mark that first layout has been processed - all future scrolls should fire events
			_hasCompletedFirstLayout = true;

			var (First, Center, Last) = GetVisibleItemsIndex(recyclerView);
			var itemsViewScrolledEventArgs = new ItemsViewScrolledEventArgs
			{
				HorizontalDelta = recyclerView.FromPixels(dx),
				VerticalDelta = recyclerView.FromPixels(dy),
				HorizontalOffset = recyclerView.FromPixels(_horizontalOffset),
				VerticalOffset = recyclerView.FromPixels(_verticalOffset),
				FirstVisibleItemIndex = First,
				CenterItemIndex = Center,
				LastVisibleItemIndex = Last
			};

			_itemsView.SendScrolled(itemsViewScrolledEventArgs);

			// Don't send RemainingItemsThresholdReached event for non-linear layout managers
			// This can also happen if a layout pass has not happened yet
			if (Last == -1 || ItemsViewAdapter is null || _itemsView.RemainingItemsThreshold == -1)
			{
				return;
			}

			var itemsSource = ItemsViewAdapter.ItemsSource;
			int headerValue = itemsSource.HasHeader ? 1 : 0;
			int footerValue = itemsSource.HasFooter ? 1 : 0;

			// Calculate actual data item count (excluding header and footer positions)
			int actualItemCount = ItemsViewAdapter.ItemCount - footerValue - headerValue;

			// Ensure we're within the data items region (not in header/footer)
			if (Last < headerValue || Last > actualItemCount)
			{
				return;
			}

			// Check if we're at or within threshold distance from the last data item
			bool isThresholdReached = (Last == actualItemCount - 1) || (actualItemCount - 1 - Last <= _itemsView.RemainingItemsThreshold);

			if (isThresholdReached)
			{
				HandleRemainingItemsThresholdReached();
			}
		}

		public override void OnScrollStateChanged(RecyclerView recyclerView, int newState)
		{
			base.OnScrollStateChanged(recyclerView, newState);

			// If we have a pending threshold reached event and the RecyclerView is now idle,
			// it's safe to trigger the event without the risk of modifying the adapter during a scroll callback
			if (_pendingRemainingItemsThresholdReached && newState == RecyclerView.ScrollStateIdle)
			{
				_pendingRemainingItemsThresholdReached = false;
				if (!_disposed && _itemsView is not null)
				{
					_itemsView.SendRemainingItemsThresholdReached();
				}
			}
		}

		void HandleRemainingItemsThresholdReached()
		{
			// Mark that we need to trigger the threshold reached event
			// This will be handled when the RecyclerView transitions to idle state
			// to avoid the "Cannot call this method in a scroll callback" exception
			_pendingRemainingItemsThresholdReached = true;
		}

		protected virtual (int First, int Center, int Last) GetVisibleItemsIndex(RecyclerView recyclerView)
		{
			int firstVisibleItemIndex = -1, lastVisibleItemIndex = -1, centerItemIndex = -1;

			if (recyclerView.GetLayoutManager() is LinearLayoutManager linearLayoutManager)
			{
				firstVisibleItemIndex = linearLayoutManager.FindFirstVisibleItemPosition();
				lastVisibleItemIndex = linearLayoutManager.FindLastVisibleItemPosition();
				centerItemIndex = recyclerView.CalculateCenterItemIndex(firstVisibleItemIndex, linearLayoutManager, _getCenteredItemOnXAndY);
			}

			var adapter = ItemsViewAdapter;
			var itemsSource = adapter.ItemsSource;
			int itemsCount = adapter.ItemCount;
			bool hasHeader = itemsSource.HasHeader;
			bool hasFooter = itemsSource.HasFooter;

			if (itemsSource is ILogicalDataIndexMapper logicalDataIndexMapper)
			{
				return (
					logicalDataIndexMapper.GetLogicalDataIndex(firstVisibleItemIndex, snapForward: true),
					logicalDataIndexMapper.GetLogicalDataIndex(centerItemIndex, snapForward: true),
					logicalDataIndexMapper.GetLogicalDataIndex(lastVisibleItemIndex, snapForward: false)
				);
			}

			// Adjust for footer: if the last visible item is the footer, decrement to get the last data item index
			if (hasFooter && lastVisibleItemIndex == itemsCount - 1)
			{
				lastVisibleItemIndex--;
			}

			// Non-grouped items adjustment
			if (hasHeader)
			{
				firstVisibleItemIndex--;
				lastVisibleItemIndex--;
				centerItemIndex--;
			}

			int maxValidIndex = Math.Max(0, itemsSource.Count - 1);
			firstVisibleItemIndex = Math.Clamp(firstVisibleItemIndex, 0, maxValidIndex);
			lastVisibleItemIndex = Math.Clamp(lastVisibleItemIndex, 0, maxValidIndex);
			centerItemIndex = Math.Clamp(centerItemIndex, 0, maxValidIndex);

			return (firstVisibleItemIndex, centerItemIndex, lastVisibleItemIndex);
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				_itemsView = null;
				ItemsViewAdapter = null;
				_pendingRemainingItemsThresholdReached = false;
			}

			_disposed = true;

			base.Dispose(disposing);
		}
	}
}
