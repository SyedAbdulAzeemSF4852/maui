#nullable disable
using AndroidX.RecyclerView.Widget;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	internal class CarouselViewOnScrollListener : RecyclerViewScrollListener<CarouselView, IItemsViewSource>
	{
		readonly CarouselView _carouselView;
		readonly CarouselViewLoopManager _carouselViewLoopManager;
		RecyclerView _lastRecyclerView;
		int _lastDx;
		int _lastDy;
		
		public CarouselViewOnScrollListener(ItemsView itemsView, ItemsViewAdapter<CarouselView, IItemsViewSource> itemsViewAdapter, CarouselViewLoopManager carouselViewLoopManager) : base((CarouselView)itemsView, itemsViewAdapter, true)
		{
			_carouselView = itemsView as CarouselView;
			_carouselViewLoopManager = carouselViewLoopManager;
		}

		public override void OnScrollStateChanged(RecyclerView recyclerView, int state)
		{
			base.OnScrollStateChanged(recyclerView, state);

			if (_carouselView.IsSwipeEnabled)
			{
				if (state == RecyclerView.ScrollStateDragging)
					_carouselView.SetIsDragging(true);
				else
					_carouselView.SetIsDragging(false);
			}

			_carouselView.IsScrolling = state != RecyclerView.ScrollStateIdle;
			// When scroll animation completes (settling -> idle) and we have cached scroll data, 
			if (state == RecyclerView.ScrollStateIdle && _carouselView.IsScrollAnimated && _lastRecyclerView != null)
			{
				ProcessScrolled(_lastRecyclerView, _lastDx, _lastDy);
				_lastRecyclerView = null;
				_lastDx = 0;
				_lastDy = 0;
			}
		}

		public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
		{
			// Defer scroll processing during animated transitions to prevent erratic property updates.
			if (_carouselView.IsScrollAnimated && recyclerView.ScrollState == RecyclerView.ScrollStateSettling)
			{
				// This prevents multiple rapid CurrentItem changes during the smooth scroll animation
				_lastRecyclerView = recyclerView;
				_lastDx = dx;
				_lastDy = dy;
			}
			else
			{
				// For non-animated scrolls, user dragging, or idle state, process immediately
				ProcessScrolled(recyclerView, dx, dy);
			}
		}

		void ProcessScrolled(RecyclerView recyclerView, int dx, int dy)
		{
			if (recyclerView is not null)
			{
				base.OnScrolled(recyclerView, dx, dy);
			}
			if (_carouselView.Loop)
			{
				//We could have a race condition where we are scrolling our collection to center the first item
				//We save that ScrollToEventARgs and call it again
				_carouselViewLoopManager.CheckPendingScrollToEvents(recyclerView);
			}
		}

		protected override (int First, int Center, int Last) GetVisibleItemsIndex(RecyclerView recyclerView)
		{
			var firstVisibleItemIndex = -1;
			var lastVisibleItemIndex = -1;
			var centerItemIndex = -1;

			if (recyclerView.GetLayoutManager() is LinearLayoutManager linearLayoutManager)
			{
				var firstView = recyclerView.FindViewHolderForAdapterPosition(linearLayoutManager.FindFirstVisibleItemPosition());
				var lastView = recyclerView.FindViewHolderForAdapterPosition(linearLayoutManager.FindLastVisibleItemPosition());
				var centerView = recyclerView.GetCenteredView();
				firstVisibleItemIndex = GetIndexFromTemplatedCell(firstView?.ItemView);
				lastVisibleItemIndex = GetIndexFromTemplatedCell(lastView?.ItemView);
				centerItemIndex = GetIndexFromTemplatedCell(centerView);
			}

			return (firstVisibleItemIndex, centerItemIndex, lastVisibleItemIndex);
		}

		int GetIndexFromTemplatedCell(global::Android.Views.View view)
		{
			int itemIndex = -1;

			if (view is ItemContentView templatedCell && ItemsViewAdapter != null)
			{
				var bContext = (templatedCell?.View as VisualElement)?.BindingContext;
				itemIndex = ItemsViewAdapter.GetPositionForItem(bContext);
			}

			return itemIndex;
		}
	}
}