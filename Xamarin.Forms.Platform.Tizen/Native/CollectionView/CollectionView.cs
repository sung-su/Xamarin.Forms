using System;
using ElmSharp;
using EBox = ElmSharp.Box;
using EScroller = ElmSharp.Scroller;
using ESize = ElmSharp.Size;
using EPoint = ElmSharp.Point;

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
			}
		}
		void OnAdaptorChanged()
		{
			if (_adaptor == null)
				return;

			RequestLayoutItems();
		}


		Rect _lastGeometry;

		void OnLayout()
		{
			System.Console.WriteLine("CollectionView : OnLayout");
			if (_lastGeometry == Geometry)
			{
				return;
			}

			_lastGeometry = Geometry;
			Scroller.Geometry = Geometry;
			Scroller.ScrollBlock = ScrollBlock.None;
			AllocatedSize = Geometry.Size;

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
						_layoutManager?.LayoutItems(ViewPort);
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
			Device.BeginInvokeOnMainThread(() =>
			{
				if (Scroller != null)
				{
					OnScrolled(Scroller, EventArgs.Empty);
				}
			});
		}

		void OnScrolled(object sender, EventArgs e)
		{
			_layoutManager.LayoutItems(Scroller.CurrentRegion);
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
			}

			Adaptor.SetBinding(view, index);
			_innerLayout.PackEnd(view);
			return view;
		}

		ESize _itemSize = new ESize(-1, -1);
		ESize ICollectionViewController.GetItemSize()
		{
			if (_itemSize.Width > 0 && _itemSize.Height > 0)
			{
				return _itemSize;
			}
			return _itemSize = Adaptor.MeasureItem(AllocatedSize.Width, AllocatedSize.Height);
		}

		void ICollectionViewController.UnrealizeView(EvasObject view)
		{
			System.Console.WriteLine($"UnrealizeView {view}");
			_innerLayout.UnPack(view);
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
