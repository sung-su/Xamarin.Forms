using System;
using ElmSharp;
using EBox = ElmSharp.Box;
using EScroller = ElmSharp.Scroller;
using ESize = ElmSharp.Size;
using EPoint = ElmSharp.Point;
using System.Collections.Specialized;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	public class CollectionView : EBox, ICollectionViewController
	{
		ICollectionViewLayoutManager _layoutManager;
		ItemAdaptor _adaptor;
		EBox _innerLayout;
		bool _requestLayoutItems = false;
		RecyclerPool _pool = new RecyclerPool();

		public CollectionView(EvasObject parent) : base(parent)
		{
			System.Console.WriteLine("CollectionView created");
			SetLayoutCallback(OnLayout);
			Scroller = CreateScroller(parent);
			Scroller.Show();
			PackEnd(Scroller);
			Scroller.Scrolled += OnScrolled;

			_innerLayout = new EBox(parent);
			_innerLayout.SetLayoutCallback(OnInnerLayout);
			_innerLayout.Show();
			Scroller.SetContent(_innerLayout);

			//Scroller.HorizontalPageScrollLimit = 1;
			//Scroller.VerticalPageScrollLimit = 1;

			//(new SmartEvent(Scroller, Scroller.RealHandle, "scroll,anim,stop")).On += OnScrollAnimationCompleted;

		}

		bool _onSnaping;

		int _lastPage = 0;
		void OnScrollAnimationCompleted(object sender, EventArgs e)
		{
			if (LayoutManager == null)
				return;
			if (_onSnaping)
				return;

			_onSnaping = true;

			System.Console.WriteLine($"anim,stop H : {Scroller.HorizontalPageIndex} V: {Scroller.VerticalPageIndex}");

			if (LayoutManager.IsHorizontal)
			{
				if (Scroller.HorizontalPageIndex % 2 == 0)
				{
					int diff = 1;
					if (_lastPage > Scroller.HorizontalPageIndex)
					{
						diff = -1;
					}
						Scroller.ScrollTo(Scroller.HorizontalPageIndex + diff, Scroller.VerticalPageIndex, false);
					System.Console.WriteLine($"anim,stop RequestEnd H : {Scroller.HorizontalPageIndex} V: {Scroller.VerticalPageIndex}");
				}
				_lastPage = Scroller.HorizontalPageIndex;
			}
			else
			{
				if (Scroller.VerticalPageIndex % 2 == 0)
				{
					int diff = 1;
					if (_lastPage > Scroller.VerticalPageIndex)
					{
						diff = -1;
					}

					Scroller.ScrollTo(Scroller.HorizontalPageIndex, Scroller.VerticalPageIndex + diff, false);
					System.Console.WriteLine($"anim,stop RequestEnd H : {Scroller.HorizontalPageIndex} V: {Scroller.VerticalPageIndex}");
				}
				_lastPage = Scroller.VerticalPageIndex;
			}

			_onSnaping = false;
		}

		protected virtual EScroller CreateScroller(EvasObject parent)
		{
			return new EScroller(parent);
		}

		public EScroller Scroller { get; }

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

		ESize AllocatedSize { get; set; }

		Rect ViewPort => Scroller.CurrentRegion;

		int ICollectionViewController.Count => Adaptor?.Count ?? 0;

		EPoint ICollectionViewController.ParentPosition => new EPoint
		{
			X = Scroller.Geometry.X - Scroller.CurrentRegion.X,
			Y = Scroller.Geometry.Y - Scroller.CurrentRegion.Y
		};

		void OnLayoutManagerChanging()
		{
			_layoutManager?.Reset();
		}
		void OnLayoutManagerChanged()
		{
			if (_layoutManager == null)
				return;

			_layoutManager.CollectionView = this;
			_layoutManager.SizeAllocated(AllocatedSize);
			RequestLayoutItems();
		}

		void OnAdaptorChanging()
		{
			_layoutManager?.Reset();
			if (Adaptor != null)
			{
				_pool.Clear(Adaptor);
				(Adaptor as INotifyCollectionChanged).CollectionChanged -= OnCollectionChanged;
			}
		}
		void OnAdaptorChanged()
		{
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

			// elm-scroller updates the CurrentRegion after render
			/*
			Device.BeginInvokeOnMainThread(() =>
			{
				if (Scroller != null)
				{
					OnScrolled(Scroller, EventArgs.Empty);
				}
			});
			*/
		}

		void OnScrolled(object sender, EventArgs e)
		{
			_layoutManager.LayoutItems(Scroller.CurrentRegion);
		}

		ESize _itemSize = new ESize(-1, -1);
		ESize ICollectionViewController.GetItemSize()
		{
			if (_itemSize.Width > 0 && _itemSize.Height > 0)
			{
				return _itemSize;
			}
			_itemSize = Adaptor.MeasureItem(AllocatedSize.Width, AllocatedSize.Height);
			if (_itemSize.Width <= 0)
				_itemSize.Width = 50;
			if (_itemSize.Height <= 0)
				_itemSize.Height = 50;

			//Scroller.SetPageSize(_itemSize.Width, _itemSize.Height);

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
				System.Console.WriteLine($"!!!! Recycled!!");
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
