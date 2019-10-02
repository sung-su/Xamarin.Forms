using System;
using System.Linq;
using System.Collections.Specialized;
using System.Collections.Generic;
using ElmSharp;
using EBox = ElmSharp.Box;
using EScroller = ElmSharp.Scroller;
using ESize = ElmSharp.Size;
using EPoint = ElmSharp.Point;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	public class CarouselView : EBox, ICollectionViewController
	{
		RecyclerPool _pool = new RecyclerPool();
		ICollectionViewLayoutManager _layoutManager;

		ItemAdaptor _adaptor;
		EBox _innerLayout;
		EvasObject _emptyView;

		Dictionary<ViewHolder, int> _viewHolderIndexTable = new Dictionary<ViewHolder, int>();
		ViewHolder _lastSelectedViewHolder;
		int _selectedItemIndex = -1;
		//CollectionViewSelectionMode _selectionMode = CollectionViewSelectionMode.None;

		bool _requestLayoutItems = false;
		SnapPointsType _snapPoints;
		ESize _itemSize = new ESize(-1, -1);

		//TODO
		bool _isSwipeEnabled = true;
		bool _isDragging = false;
		//int _initialPosition;
		//EvasObjectEvent _mouseDown;
		//EvasObjectEvent _mouseUp;

		public CarouselView(EvasObject parent) : base(parent)
		{
			Console.WriteLine($"@@@ # CV CarouselView 1 in /*");
			SetLayoutCallback(OnLayout);
			Scroller = CreateScroller(parent);
			Scroller.Show();
			PackEnd(Scroller);
			Scroller.Scrolled += OnScrolled;

			//_mouseDown = new EvasObjectEvent(this, EvasObjectCallbackType.MouseDown);
			//_mouseUp = new EvasObjectEvent(this, EvasObjectCallbackType.MouseUp);
			//_mouseDown.On += (s, e) => { _isDragging = true; };
			//_mouseUp.On += (s, e) => { _isDragging = false; };
			Scroller.DragStart += (s, e) => { _isDragging = true; };
			Scroller.DragStop += (s, e) => { _isDragging = false; };

			_innerLayout = new EBox(parent);
			_innerLayout.SetLayoutCallback(OnInnerLayout);
			_innerLayout.Show();
			Scroller.SetContent(_innerLayout);
			Console.WriteLine($"@@@ # CV CarouselView 2 out */");
		}

		/*
		+ public Thickness PeekAreaInsets
		+ public List<View> VisibleViews
		+ public bool IsDragging
		+ public bool IsBounceEnabled
		+ public int NumberOfSideItems
		+ public bool IsSwipeEnabled //lock
		+ public bool IsScrollAnimated
		+ public object CurrentItem
		+ public LinearItemsLayout ItemsLayout
		*/

		public bool IsDragging => _isDragging;

		public bool IsSwipeEnabled => _isSwipeEnabled; // lock

		// remove
		//public CollectionViewSelectionMode SelectionMode
		//{
		//	get => _selectionMode;
		//	set
		//	{
		//		_selectionMode = value;
		//		UpdateSelectionMode();
		//	}
		//}

		public int SelectedItemIndex
		{
			get => _selectedItemIndex;
			set
			{
				if (_selectedItemIndex != value)
				{
					_selectedItemIndex = value;
					UpdateSelectedItemIndex();
				}
			}
		}

		public SnapPointsType SnapPointsType
		{
			get => _snapPoints;
			set
			{
				_snapPoints = value;
				UpdateSnapPointsType(_snapPoints);
			}
		}

		protected EScroller Scroller { get; }

		public ICollectionViewLayoutManager LayoutManager
		{
			get => _layoutManager;
			set
			{
				OnLayoutManagerChanging();
				_layoutManager = value;
				OnLayoutManagerChanged();
			}
		}

		public ItemAdaptor Adaptor
		{
			get => _adaptor;
			set
			{
				OnAdaptorChanging();
				_adaptor = value;
				OnAdaptorChanged();
			}
		}

		int ICollectionViewController.Count
		{
			get
			{
				if (Adaptor == null || Adaptor is IEmptyAdaptor)
					return 0;
				return Adaptor.Count;
			}
		}

		EPoint ICollectionViewController.ParentPosition => new EPoint
		{
			X = Scroller.Geometry.X - Scroller.CurrentRegion.X,
			Y = Scroller.Geometry.Y - Scroller.CurrentRegion.Y
		};

		ESize AllocatedSize { get; set; }

		Rect ViewPort => Scroller.CurrentRegion;

		public void ScrollTo(int index, ScrollToPosition position = ScrollToPosition.MakeVisible, bool animate = true)
		{
			Console.WriteLine($"@@@ ###### CV ScrollTo");
			var itemBound = LayoutManager.GetItemBound(index);
			int itemStart;
			int itemEnd;
			int scrollStart;
			int scrollEnd;
			int itemPadding = 0;
			int itemSize;
			int viewportSize;

			if (LayoutManager.IsHorizontal)
			{
				itemStart = itemBound.Left;
				itemEnd = itemBound.Right;
				itemSize = itemBound.Width;
				scrollStart = Scroller.CurrentRegion.Left;
				scrollEnd = Scroller.CurrentRegion.Right;
				viewportSize = AllocatedSize.Width;
			}
			else
			{
				itemStart = itemBound.Top;
				itemEnd = itemBound.Bottom;
				itemSize = itemBound.Height;
				scrollStart = Scroller.CurrentRegion.Top;
				scrollEnd = Scroller.CurrentRegion.Bottom;
				viewportSize = AllocatedSize.Height;
			}

			if (position == ScrollToPosition.MakeVisible)
			{
				if (itemStart < scrollStart)
				{
					position = ScrollToPosition.Start;
				}
				else if (itemEnd > scrollEnd)
				{
					position = ScrollToPosition.End;
				}
				else
				{
					// already visible
					return;
				}
			}

			if (itemSize < viewportSize)
			{
				switch (position)
				{
					case ScrollToPosition.Center:
						itemPadding = (viewportSize - itemSize) / 2;
						break;
					case ScrollToPosition.End:
						itemPadding = (viewportSize - itemSize);
						break;
				}
				itemSize = viewportSize;
			}

			if (LayoutManager.IsHorizontal)
			{
				itemBound.X -= itemPadding;
				itemBound.Width = itemSize;
			}
			else
			{
				itemBound.Y -= itemPadding;
				itemBound.Height = itemSize;
			}

			Scroller.ScrollTo(itemBound, animate);
		}

		public void ScrollTo(object item, ScrollToPosition position = ScrollToPosition.MakeVisible, bool animate = true)
		{
			ScrollTo(Adaptor.GetItemIndex(item), position, animate);
		}

		public void ItemMeasureInvalidated(int index)
		{
			Console.WriteLine($"@@@ ## CV ItemMeasureInvalidated 1 in /*");
			LayoutManager?.ItemMeasureInvalidated(index);
			Console.WriteLine($"@@@ ## CV ItemMeasureInvalidated 2 out */");
		}

		void ICollectionViewController.RequestLayoutItems() => RequestLayoutItems();

		ESize ICollectionViewController.GetItemSize()
		{
			return (this as ICollectionViewController).GetItemSize(LayoutManager.IsHorizontal ? AllocatedSize.Width * 100 : AllocatedSize.Width, LayoutManager.IsHorizontal ? AllocatedSize.Height : AllocatedSize.Height * 100);
		}

		ESize ICollectionViewController.GetItemSize(int widthConstraint, int heightConstraint)
		{
			if (Adaptor == null)
			{
				return new ESize(0, 0);
			}

			if (_itemSize.Width > 0 && _itemSize.Height > 0)
			{
				return _itemSize;
			}

			_itemSize = Adaptor.MeasureItem(widthConstraint, heightConstraint);
			_itemSize.Width = Math.Max(_itemSize.Width, 10);
			_itemSize.Height = Math.Max(_itemSize.Height, 10);

			if (_snapPoints != SnapPointsType.None)
			{
				Scroller.SetPageSize(_itemSize.Width, _itemSize.Height);
			}
			return _itemSize;
		}

		ESize ICollectionViewController.GetItemSize(int index, int widthConstraint, int heightConstraint)
		{
			if (Adaptor == null)
			{
				return new ESize(0, 0);
			}
			return Adaptor.MeasureItem(index, widthConstraint, heightConstraint);
		}

		ViewHolder ICollectionViewController.RealizeView(int index)
		{
			Console.WriteLine($"@@@ ## CV RealizeView 1 in /*");
			Console.WriteLine($"@@@ ## CV RealizeView 2 index=[{index}]");
			if (Adaptor == null)
				return null;

			Console.WriteLine($"@@@ ## CV RealizeView 3 adaptor exist");
			var holder = _pool.GetRecyclerView(Adaptor.GetViewCategory(index));
			Console.WriteLine($"@@@ ## CV RealizeView 4 get holder");
			if (holder != null)
			{
				Console.WriteLine($"@@@ ## CV RealizeView 5-1 reuse holder");
				holder.Show();
			}
			else
			{
				Console.WriteLine($"@@@ ## CV RealizeView 5-2 new holder");
				var content = Adaptor.CreateNativeView(index, this);
				holder = new ViewHolder(this);
				holder.RequestSelected += OnRequestItemSelection;
				holder.Content = content;
				holder.ViewCategory = Adaptor.GetViewCategory(index);
				_innerLayout.PackEnd(holder);
			}
			Console.WriteLine($"@@@ ## CV RealizeView 6 Adaptor.SetBinding(holder.Content, index) ");
			Adaptor.SetBinding(holder.Content, index);
			Console.WriteLine($"@@@ ## CV RealizeView 7 _viewHolderIndexTable[holder] = index");
			_viewHolderIndexTable[holder] = index;
			Console.WriteLine($"@@@ ## CV RealizeView 8");
			if (index == SelectedItemIndex)
			{
				Console.WriteLine($"@@@ ## CV RealizeView 9 index == selected item index");
				OnRequestItemSelection(holder, EventArgs.Empty);
			}
			Console.WriteLine($"@@@ ## CV RealizeView 10 return holder");
			Console.WriteLine($"@@@ ## CV RealizeView 11 out */");
			return holder;
		}

		void OnRequestItemSelection(object sender, EventArgs e)
		{
			//if (SelectionMode == CollectionViewSelectionMode.None)
			//	return;

			if (_lastSelectedViewHolder != null)
			{
				_lastSelectedViewHolder.State = ViewHolderState.Normal;
			}

			_lastSelectedViewHolder = sender as ViewHolder;
			if (_lastSelectedViewHolder != null)
			{
				_lastSelectedViewHolder.State = ViewHolderState.Selected;
				if (_viewHolderIndexTable.TryGetValue(_lastSelectedViewHolder, out int index))
				{
					_selectedItemIndex = index;
					Adaptor?.SendItemSelected(index);
				}
			}
		}

		void ICollectionViewController.UnrealizeView(ViewHolder view)
		{
			_viewHolderIndexTable.Remove(view);
			Adaptor.UnBinding(view.Content);
			view.ResetState();
			view.Hide();
			_pool.AddRecyclerView(view);
			if (_lastSelectedViewHolder == view)
			{
				_lastSelectedViewHolder = null;
			}
		}

		void ICollectionViewController.ContentSizeUpdated()
		{
			OnInnerLayout();
		}

		protected virtual EScroller CreateScroller(EvasObject parent)
		{
			return new EScroller(parent);
		}

		void UpdateSelectedItemIndex()
		{
			//if (SelectionMode == CollectionViewSelectionMode.None)
			//	return;

			ViewHolder holder = null;
			foreach (var item in _viewHolderIndexTable)
			{
				if (item.Value == SelectedItemIndex)
				{
					holder = item.Key;
					break;
				}
			}
			OnRequestItemSelection(holder, EventArgs.Empty);
		}

		//void UpdateSelectionMode()
		//{
		//	if (SelectionMode == CollectionViewSelectionMode.None)
		//	{
		//		if (_lastSelectedViewHolder != null)
		//		{
		//			_lastSelectedViewHolder.State = ViewHolderState.Normal;
		//			_lastSelectedViewHolder = null;
		//		}
		//		_selectedItemIndex = -1;
		//	}
		//}

		void OnLayoutManagerChanging()
		{
			Console.WriteLine($"@@@ #### CV OnLayoutManagerChanging 1 in /*");
			_layoutManager?.Reset();
			Console.WriteLine($"@@@ #### CV OnLayoutManagerChanging 2 out */");
		}

		void OnLayoutManagerChanged()
		{
			Console.WriteLine($"@@@ ##### CV OnLayoutManagerChanged 1 in /*");
			if (_layoutManager == null)
				return;
			Console.WriteLine($"@@@ ##### CV OnLayoutManagerChanged 2 layout manager exist");
			_itemSize = new ESize(-1, -1);
			Console.WriteLine($"@@@ ##### CV OnLayoutManagerChanged 3 _layoutManager.CollectionView = this");
			_layoutManager.CollectionView = this;
			Console.WriteLine($"@@@ ##### CV OnLayoutManagerChanged 4");
			_layoutManager.SizeAllocated(AllocatedSize);
			Console.WriteLine($"@@@ ##### CV OnLayoutManagerChanged 5 RequestLayoutItems() ");
			RequestLayoutItems();
			Console.WriteLine($"@@@ ##### CV OnLayoutManagerChanged 6 out */");
		}

		void OnAdaptorChanging()
		{
			Console.WriteLine($"@@@ ###### CV OnAdaptorChanging 1 in /*");
			if (Adaptor is IEmptyAdaptor)
			{
				Console.WriteLine($"@@@ ###### CV OnAdaptorChanging 2 adaptor set");
				RemoveEmptyView();
			}
			Console.WriteLine($"@@@ ###### CV OnAdaptorChanging 3");
			_layoutManager?.Reset();
			Console.WriteLine($"@@@ ###### CV OnAdaptorChanging 4");
			if (Adaptor != null)
			{
				Console.WriteLine($"@@@ ###### CV OnAdaptorChanging 5 adaptor null");
				_pool.Clear(Adaptor);
				(Adaptor as INotifyCollectionChanged).CollectionChanged -= OnCollectionChanged;
				Console.WriteLine($"@@@ ###### CV OnAdaptorChanging 6 Adaptor.CarouselView = null;");

				// check 
				Adaptor.CollectionView = null;
				Adaptor.CarouselView = null;
			}
			Console.WriteLine($"@@@ ###### CV OnAdaptorChanging 7 out */");
		}
		void OnAdaptorChanged()
		{
			Console.WriteLine($"@@@ ####### CV OnAdaptorChanged 1 in /*");
			if (_adaptor == null)
				return;
			Console.WriteLine($"@@@ ####### CV OnAdaptorChanged 2 adaptor exist");
			_itemSize = new ESize(-1, -1);
			Console.WriteLine($"@@@ ####### CV OnAdaptorChanged 3 Adaptor.CarouselView = this;");
			//Adaptor.CollectionView = this;
			Adaptor.CarouselView = this;
			Console.WriteLine($"@@@ ####### CV OnAdaptorChanged 4");
			(Adaptor as INotifyCollectionChanged).CollectionChanged += OnCollectionChanged;
			Console.WriteLine($"@@@ ####### CV OnAdaptorChanged 5 LayoutManager?.ItemSourceUpdated();");
			LayoutManager?.ItemSourceUpdated();
			Console.WriteLine($"@@@ ####### CV OnAdaptorChanged 6 RequestLayoutItems();");
			RequestLayoutItems();
			Console.WriteLine($"@@@ ####### CV OnAdaptorChanged 7");
			if (Adaptor is IEmptyAdaptor)
			{
				Console.WriteLine($"@@@ ####### CV OnAdaptorChanged 8 CreateEmptyView();");
				CreateEmptyView();
			}
			Console.WriteLine($"@@@ ####### CV OnAdaptorChanged 9 out */");
		}

		void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			// CollectionChanged could be called when Apaptor was changed on CollectionChanged event
			if (Adaptor is IEmptyAdaptor)
			{
				return;
			}

			if (e.Action == NotifyCollectionChangedAction.Add)
			{
				int idx = e.NewStartingIndex;
				if (idx == -1)
				{
					idx = Adaptor.Count - e.NewItems.Count;
				}
				foreach (var item in e.NewItems)
				{
					foreach (var viewHolder in _viewHolderIndexTable.Keys.ToList())
					{
						if (_viewHolderIndexTable[viewHolder] >= idx)
						{
							_viewHolderIndexTable[viewHolder]++;
						}
					}
					LayoutManager.ItemInserted(idx++);
				}
			}
			else if (e.Action == NotifyCollectionChangedAction.Remove)
			{
				int idx = e.OldStartingIndex;

				// Can't tracking remove if there is no data of old index
				if (idx == -1)
				{
					LayoutManager.ItemSourceUpdated();
				}
				else
				{
					foreach (var item in e.OldItems)
					{
						LayoutManager.ItemRemoved(idx);
						foreach (var viewHolder in _viewHolderIndexTable.Keys.ToList())
						{
							if (_viewHolderIndexTable[viewHolder] > idx)
							{
								_viewHolderIndexTable[viewHolder]--;
							}
						}
					}
				}
			}
			else if (e.Action == NotifyCollectionChangedAction.Move)
			{
				LayoutManager.ItemRemoved(e.OldStartingIndex);
				LayoutManager.ItemInserted(e.NewStartingIndex);
			}
			else if (e.Action == NotifyCollectionChangedAction.Replace)
			{
				// Can't tracking if there is no information old data
				if (e.OldItems.Count > 1 || e.NewStartingIndex == -1)
				{
					LayoutManager.ItemSourceUpdated();
				}
				else
				{
					LayoutManager.ItemUpdated(e.NewStartingIndex);
				}
			}
			else if (e.Action == NotifyCollectionChangedAction.Reset)
			{
				LayoutManager.Reset();
				LayoutManager.ItemSourceUpdated();
			}
			RequestLayoutItems();
		}

		Rect _lastGeometry;
		void OnLayout()
		{
			Console.WriteLine($"@@@ ######## CV OnLayout 1");
			if (_lastGeometry == Geometry)
			{
				return;
			}

			_lastGeometry = Geometry;
			Scroller.Geometry = Geometry;
			Scroller.ScrollBlock = ScrollBlock.None;
			AllocatedSize = Geometry.Size;
			_itemSize = new ESize(-1, -1);

			if (_adaptor == null)
				Console.WriteLine($"@@@ ######## CV OnLayout 2");
			if (_layoutManager == null)
				Console.WriteLine($"@@@ ######## CV OnLayout 3");

			if (_adaptor != null && _layoutManager != null)
			{
				_layoutManager?.SizeAllocated(Geometry.Size);
				_layoutManager?.LayoutItems(ViewPort);
			}
		}

		void RequestLayoutItems()
		{
			Console.WriteLine($"@@@ ######## CV RequestLayoutItems 1");
			if (!_requestLayoutItems)
			{
				Console.WriteLine($"@@@ ######## CV RequestLayoutItems 2");
				_requestLayoutItems = true;
				Device.BeginInvokeOnMainThread(() =>
				{
					Console.WriteLine($"@@@ ######## CV RequestLayoutItems 3");
					_requestLayoutItems = false;
					if (_adaptor != null && _layoutManager != null)
					{
						Console.WriteLine($"@@@ ######## CV RequestLayoutItems 4");
						OnInnerLayout();
						_layoutManager?.LayoutItems(ViewPort, true);
					}
					Console.WriteLine($"@@@ ######## CV RequestLayoutItems 5");
				});
				Console.WriteLine($"@@@ ######## CV RequestLayoutItems 6");
			}
			Console.WriteLine($"@@@ ######## CV RequestLayoutItems 7");
		}

		void OnInnerLayout()
		{
			Console.WriteLine($"@@@ ######## CV OnInnerLayout");
			// OnInnerLayout was called when child item was added
			// so, need to check scroll canvas size
			var size = _layoutManager.GetScrollCanvasSize();
			_innerLayout.MinimumWidth = size.Width;
			_innerLayout.MinimumHeight = size.Height;
		}

		void OnScrolled(object sender, EventArgs e)
		{
			_layoutManager.LayoutItems(Scroller.CurrentRegion);
		}

		void UpdateSnapPointsType(SnapPointsType snapPoints)
		{
			var itemSize = new ESize(0, 0);
			switch (snapPoints)
			{
				case SnapPointsType.None:
					Scroller.HorizontalPageScrollLimit = 0;
					Scroller.VerticalPageScrollLimit = 0;
					break;
				case SnapPointsType.MandatorySingle:
					Scroller.HorizontalPageScrollLimit = 1;
					Scroller.VerticalPageScrollLimit = 1;
					itemSize = (this as ICollectionViewController).GetItemSize();
					break;
				case SnapPointsType.Mandatory:
					Scroller.HorizontalPageScrollLimit = 0;
					Scroller.VerticalPageScrollLimit = 0;
					itemSize = (this as ICollectionViewController).GetItemSize();
					break;
			}
			Scroller.SetPageSize(itemSize.Width, itemSize.Height);
		}

		void CreateEmptyView()
		{
			Console.WriteLine($"@@@ ######## CV CreateEmptyView");
			_emptyView = Adaptor.CreateNativeView(this);
			_emptyView.Show();
			Adaptor.SetBinding(_emptyView, 0);
			_emptyView.Geometry = Geometry;
			_emptyView.MinimumHeight = Geometry.Height;
			_emptyView.MinimumWidth = Geometry.Width;
			Scroller.SetContent(_emptyView, true);
			_innerLayout.Hide();
		}

		void RemoveEmptyView()
		{
			Console.WriteLine($"@@@ ######## CV RemoveEmptyView");
			_innerLayout.Show();
			Scroller.SetContent(_innerLayout, true);
			Adaptor.RemoveNativeView(_emptyView);
			_emptyView = null;
		}
	}
}
