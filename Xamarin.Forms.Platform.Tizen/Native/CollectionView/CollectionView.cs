using System;
using System.Collections.Specialized;
using ElmSharp;
using EBox = ElmSharp.Box;
using EScroller = ElmSharp.Scroller;
using ESize = ElmSharp.Size;
using EPoint = ElmSharp.Point;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	public class CollectionView : EBox, ICollectionViewController
	{
		RecyclerPool _pool = new RecyclerPool();
		ICollectionViewLayoutManager _layoutManager;
		ItemAdaptor _adaptor;
		EBox _innerLayout;

		bool _requestLayoutItems = false;
		SnapPointsType _snapPoints;
		ESize _itemSize = new ESize(-1, -1);

		public CollectionView(EvasObject parent) : base(parent)
		{
			SetLayoutCallback(OnLayout);
			Scroller = CreateScroller(parent);
			Scroller.Show();
			PackEnd(Scroller);
			Scroller.Scrolled += OnScrolled;

			_innerLayout = new EBox(parent);
			_innerLayout.SetLayoutCallback(OnInnerLayout);
			_innerLayout.Show();
			Scroller.SetContent(_innerLayout);
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
		public int Span { get; set; }

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

		int ICollectionViewController.Count => Adaptor?.Count ?? 0;

		EPoint ICollectionViewController.ParentPosition => new EPoint
		{
			X = Scroller.Geometry.X - Scroller.CurrentRegion.X,
			Y = Scroller.Geometry.Y - Scroller.CurrentRegion.Y
		};

		ESize AllocatedSize { get; set; }

		Rect ViewPort => Scroller.CurrentRegion;

		public void ScrollTo(int index, ScrollToPosition position = ScrollToPosition.MakeVisible, bool animate = true)
		{
			var bound = LayoutManager.GetItemBound(index);
			if (LayoutManager.IsHorizontal)
			{
				if (position == ScrollToPosition.MakeVisible)
				{
					if (bound.Left < Scroller.CurrentRegion.Left)
					{
						position = ScrollToPosition.Start;
					}
					else if (bound.Right > Scroller.CurrentRegion.Right)
					{
						position = ScrollToPosition.End;
					}
					else
					{
						// Already visible
						return;
					}
				}
				if (bound.Width < AllocatedSize.Width)
				{
					switch (position)
					{
						case ScrollToPosition.Center:
							bound.X -= (AllocatedSize.Width - bound.Width) / 2;
							break;
						case ScrollToPosition.End:
							bound.X -= (AllocatedSize.Width - bound.Width);
							break;
					}
					bound.Width = AllocatedSize.Width;
				}
			}
			else
			{
				if (position == ScrollToPosition.MakeVisible)
				{
					if (bound.Top < Scroller.CurrentRegion.Top)
					{
						position = ScrollToPosition.Start;
					}
					else if (bound.Bottom > Scroller.CurrentRegion.Bottom)
					{
						position = ScrollToPosition.End;
					}
					else
					{
						// Already visible
						return;
					}
				}

				if (bound.Height < AllocatedSize.Height)
				{
					switch (position)
					{
						case ScrollToPosition.Center:
							bound.Y -= (AllocatedSize.Height - bound.Height) / 2;
							break;
						case ScrollToPosition.End:
							bound.Y -= (AllocatedSize.Height - bound.Height);
							break;
					}
					bound.Height = AllocatedSize.Height;
				}
			}
			Scroller.ScrollTo(bound, animate);
		}

		public void ScrollTo(object item, ScrollToPosition position = ScrollToPosition.MakeVisible, bool animate = true)
		{
			ScrollTo(Adaptor.GetItemIndex(item), position, animate);
		}

		ESize ICollectionViewController.GetItemSize()
		{
			if (Adaptor == null)
			{
				return new ESize(0, 0);
			}
			if (_itemSize.Width > 0 && _itemSize.Height > 0)
			{
				return _itemSize;
			}

			_itemSize = Adaptor.MeasureItem(AllocatedSize.Width, AllocatedSize.Height);
			_itemSize.Width = Math.Max(_itemSize.Width, 10);
			_itemSize.Height = Math.Max(_itemSize.Height, 10);

			if (_snapPoints != SnapPointsType.None)
			{
				Scroller.SetPageSize(_itemSize.Width, _itemSize.Height);
			}
			return _itemSize;
		}

		EvasObject ICollectionViewController.RealizeView(int index)
		{
			System.Console.WriteLine($"RealizeView {index}");
			if (Adaptor == null)
				return null;

			var view = _pool.GetRecyclerView();
			if (view != null)
			{
				view.Show();
			}
			else
			{
				view = Adaptor.CreateNativeView(this);
				_innerLayout.PackEnd(view);
			}

			Adaptor.SetBinding(view, index);
			return view;
		}

		void ICollectionViewController.UnrealizeView(EvasObject view)
		{
			System.Console.WriteLine($"UnrealizeView {view}");
			view.Hide();
			_pool.AddRecyclerView(view);
		}

		protected virtual EScroller CreateScroller(EvasObject parent)
		{
			return new EScroller(parent);
		}

		void OnLayoutManagerChanging()
		{
			System.Console.WriteLine($"@@@@ CollectionView.OnLayoutManagerChanging (1)");
			_layoutManager?.Reset();
		}

		void OnLayoutManagerChanged()
		{
			System.Console.WriteLine($"@@@@ CollectionView.OnLayoutManagerChanged (2)");
			if (_layoutManager == null)
				return;

			_layoutManager.CollectionView = this;
			_layoutManager.SizeAllocated(AllocatedSize);
			RequestLayoutItems();
		}

		void OnAdaptorChanging()
		{
			System.Console.WriteLine($"@@@@ CollectionView.OnAdaptorChanging (3)");
			_layoutManager?.Reset();
			if (Adaptor != null)
			{
				_pool.Clear(Adaptor);
				(Adaptor as INotifyCollectionChanged).CollectionChanged -= OnCollectionChanged;
			}
		}
		void OnAdaptorChanged()
		{
			System.Console.WriteLine($"@@@@ CollectionView.OnAdaptorChanged (4)");
			if (_adaptor == null)
				return;

			_itemSize = new ESize(-1, -1);
			(Adaptor as INotifyCollectionChanged).CollectionChanged += OnCollectionChanged;

			RequestLayoutItems();

			if (LayoutManager != null)
			{
				var itemSize = (this as ICollectionViewController).GetItemSize();
			}
		}

		void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Add)
			{
				int idx = e.NewStartingIndex;
				foreach (var item in e.NewItems)
				{
					LayoutManager.ItemInserted(idx++);
				}
			}
			else if (e.Action == NotifyCollectionChangedAction.Remove)
			{
				int idx = e.OldStartingIndex;
				foreach (var item in e.OldItems)
				{
					LayoutManager.ItemRemoved(idx);
				}
			}
			else if (e.Action == NotifyCollectionChangedAction.Move)
			{
				LayoutManager.ItemRemoved(e.OldStartingIndex);
				LayoutManager.ItemInserted(e.NewStartingIndex);
			}
			else if (e.Action == NotifyCollectionChangedAction.Replace)
			{
				LayoutManager.ItemUpdated(e.NewStartingIndex);
			}
			else if (e.Action == NotifyCollectionChangedAction.Reset)
			{
				LayoutManager.Reset();
			}
			RequestLayoutItems();
		}

		Rect _lastGeometry;
		void OnLayout()
		{
			System.Console.WriteLine($"CollectionView : OnLayout {Geometry}");
			if (_lastGeometry == Geometry)
			{
				return;
			}

			_lastGeometry = Geometry;
			Scroller.Geometry = Geometry;
			Scroller.ScrollBlock = ScrollBlock.None;
			AllocatedSize = Geometry.Size;
			_itemSize = new ESize(-1, -1);

			if (_adaptor != null && _layoutManager != null)
			{
				_layoutManager?.SizeAllocated(Geometry.Size);
				_layoutManager?.LayoutItems(ViewPort);
			}
		}

		void RequestLayoutItems()
		{
			if (!_requestLayoutItems)
			{
				_requestLayoutItems = true;
				Device.BeginInvokeOnMainThread(() =>
				{
					_requestLayoutItems = false;
					if (_adaptor != null && _layoutManager != null)
					{
						OnInnerLayout();
						_layoutManager?.LayoutItems(ViewPort, true);
					}
				});
			}
		}

		void OnInnerLayout()
		{

			System.Console.WriteLine($"CollectionView : OnInnerLayout Geometry : {Geometry}");

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
	}

	public interface ICollectionViewController
	{
		EPoint ParentPosition { get; }

		EvasObject RealizeView(int index);

		void UnrealizeView(EvasObject view);

		int Count { get; }

		ESize GetItemSize();
	}
}
