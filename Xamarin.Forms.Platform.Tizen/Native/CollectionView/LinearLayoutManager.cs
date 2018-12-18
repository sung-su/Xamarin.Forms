using System;
using System.Collections.Generic;
using System.Linq;
using ElmSharp;
using ESize = ElmSharp.Size;


namespace Xamarin.Forms.Platform.Tizen.Native
{
	public class LinearLayoutManager : ICollectionViewLayoutManager
	{
		ESize _allocatedSize;
		Dictionary<int, RealizedItem> _realizedItem = new Dictionary<int, RealizedItem>();

		public LinearLayoutManager(bool isHorizontal)
		{
			IsHorizontal = isHorizontal;
		}

		public bool IsHorizontal { get; }

		public ICollectionViewController CollectionView { get; set; }

		public void SizeAllocated(ESize size)
		{
			Reset();
			_allocatedSize = size;
			_scrollCanvasSize = new ESize(0, 0);
		}

		ESize _scrollCanvasSize;

		public ESize GetScrollCanvasSize()
		{
			if (_scrollCanvasSize.Width > 0 && _scrollCanvasSize.Height > 0)
				return _scrollCanvasSize;

			var itemCount = CollectionView.Count;
			var itemSize = CollectionView.GetItemSize();
			if (IsHorizontal)
			{
				return _scrollCanvasSize = new ESize(itemCount * itemSize.Width, _allocatedSize.Height);
			}
			else
			{
				return _scrollCanvasSize = new ESize(_allocatedSize.Width, itemCount * itemSize.Height);
			}
		}

		bool ShouldReArrange(Rect viewport)
		{
			if (_isLayouting)
				return false;
			if (_last.Size != viewport.Size)
				return true;

			var diff = IsHorizontal ? Math.Abs(_last.X - viewport.X) : Math.Abs(_last.Y - viewport.Y);
			var margin = IsHorizontal ? CollectionView.GetItemSize().Width : CollectionView.GetItemSize().Height;
			if (diff > margin)
				return true;

			return false;
		}

		bool _isLayouting;
		Rect _last;
		public void LayoutItems(Rect bound, bool force)
		{
			// TODO : need to optimization. it was frequently called with similar bound value.
			if (!ShouldReArrange(bound) && !force)
			{
				return;
			}
			_isLayouting = true;
			_last = bound;
			//System.Console.WriteLine($"------------OnLayoutItems {bound}----------------");
			var size = CollectionView.GetItemSize();
			var itemSize = IsHorizontal ? size.Width : size.Height;
			int startIndex = Math.Max(GetStartIndex(bound, itemSize) - 2, 0);
			int endIndex = Math.Min(GetEndIndex(bound, itemSize) + 2, CollectionView.Count - 1);

			System.Console.WriteLine($"--------------- OnLayoutItems s : {startIndex}, e : {endIndex} {bound}");

			foreach (var index in _realizedItem.Keys.ToList())
			{

				if (index < startIndex || index > endIndex)
				{
					System.Console.WriteLine($"Unrealized Item {index}");
					CollectionView.UnrealizeView(_realizedItem[index].View);
					_realizedItem.Remove(index);
				}
			}

			var parent = CollectionView.ParentPosition;
			for (int i = startIndex; i <= endIndex; i++)
			{
				EvasObject itemView = null;
				if (!_realizedItem.ContainsKey(i))
				{
					var view = CollectionView.RealizeView(i);
					_realizedItem[i] = new RealizedItem
					{
						View = view,
						Index = i,
					};
					itemView = view;
				}
				else
				{
					System.Console.WriteLine($"Already realized {i}");
					itemView = _realizedItem[i].View;
				}
				var itemBound = GetItemBound(i);
				itemBound.X += parent.X;
				itemBound.Y += parent.Y;
				itemView.Geometry = itemBound;
				System.Console.WriteLine($"{i} Item bound = {itemBound}");
			}
			System.Console.WriteLine($"-------------- OnLayoutItems ------- end ");
			_isLayouting = false;
		}

		public void ItemInserted(int inserted)
		{
			System.Console.WriteLine($"inserted at {inserted}");
			var items = _realizedItem.Keys.OrderByDescending(key => key);
			foreach (var index in items)
			{
				System.Console.WriteLine($"Realized Items at {index}");
				if (index >= inserted)
				{
					System.Console.WriteLine($"Realized Items move at {index + 1}");
					_realizedItem[index + 1] = _realizedItem[index];
				}
			}
			if (_realizedItem.ContainsKey(inserted))
			{
				_realizedItem.Remove(inserted);
				System.Console.WriteLine($"Removed inserted holder {inserted}");
			}
			else
			{
				var last = items.LastOrDefault();
				System.Console.WriteLine($"first Items at {last} inserted {inserted}");
				if (last >= inserted)
				{
					System.Console.WriteLine($"first Items at {last} remove");
					_realizedItem.Remove(last);
				}
			}

			_scrollCanvasSize = new ESize(0, 0);
		}
		public void ItemRemoved(int removed)
		{
			System.Console.WriteLine($"Remove at {removed}");

			if (_realizedItem.ContainsKey(removed))
			{
				CollectionView.UnrealizeView(_realizedItem[removed].View);
				_realizedItem.Remove(removed);
			}

			var items = _realizedItem.Keys.OrderBy(key => key);
			foreach (var index in items)
			{
				System.Console.WriteLine($"Realized Item : {index}");
				if (index > removed)
				{
					_realizedItem[index - 1] = _realizedItem[index];
				}
			}

			var last = items.LastOrDefault();
			if (last > removed)
			{
				_realizedItem.Remove(last);
			}

			_scrollCanvasSize = new ESize(0, 0);
		}
		public void ItemUpdated(int index)
		{
			if (_realizedItem.ContainsKey(index))
			{
				var bound = _realizedItem[index].View.Geometry;
				CollectionView.UnrealizeView(_realizedItem[index].View);
				var view = CollectionView.RealizeView(index);
				_realizedItem[index].View = view;
				view.Geometry = bound;
			}
		}

		public Rect GetItemBound(int index)
		{
			var size = CollectionView.GetItemSize();
			if (IsHorizontal)
			{
				size.Height = _allocatedSize.Height;
			}
			else
			{
				size.Width = _allocatedSize.Width;
			}
			return
				IsHorizontal ?
				new Rect(index * size.Width, 0, size.Width, size.Height) :
				new Rect(0, index * size.Height, size.Width, size.Height);
		}

		public void Reset()
		{
			foreach (var realizedItem in _realizedItem.Values)
			{
				CollectionView.UnrealizeView(realizedItem.View);
			}
			_realizedItem.Clear();
		}



		int GetStartIndex(Rect bound, int itemSize)
		{
			return ViewPortStartPoint(bound) / itemSize;
		}
		int GetEndIndex(Rect bound, int itemSize)
		{
			return (int)Math.Ceiling(ViewPortEndPoint(bound) / (double)itemSize);
		}
		int ViewPortStartPoint(Rect viewPort)
		{
			return IsHorizontal ? viewPort.X : viewPort.Y;
		}
		int ViewPortEndPoint(Rect viewPort)
		{
			return ViewPortStartPoint(viewPort) + ViewPortSize(viewPort);
		}
		int ViewPortSize(Rect viewPort)
		{
			return IsHorizontal ? viewPort.Width : viewPort.Height;
		}



		class RealizedItem
		{
			public EvasObject View { get; set; }
			public int Index { get; set; }
		}

	}

}
