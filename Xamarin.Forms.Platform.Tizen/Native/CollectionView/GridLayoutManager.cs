using System;
using System.Collections.Generic;
using System.Linq;
using ElmSharp;
using ESize = ElmSharp.Size;


namespace Xamarin.Forms.Platform.Tizen.Native
{
	public class GridLayoutManager : ICollectionViewLayoutManager
	{
		ESize _allocatedSize;
		Dictionary<int, RealizedItem> _realizedItem = new Dictionary<int, RealizedItem>();

		public GridLayoutManager(bool isHorizontal)
		{
			System.Console.WriteLine($"@@@@ GridLayoutManager.GridLayoutManager (1/6) - [{(isHorizontal ? "horizontal" : "vertical")}]");
			IsHorizontal = isHorizontal;
			Span = 1;
		}

		public GridLayoutManager(int span, bool isHorizontal)
		{
			System.Console.WriteLine($"@@@@ GridLayoutManager.GridLayoutManager (1/6) - [{(isHorizontal?"horizontal":"vertical")}], Span[{span}]");
			IsHorizontal = isHorizontal;
			Span = span;
		}

		public int Span { get; set;  }

		public bool IsHorizontal { get; }

		public ICollectionViewController CollectionView { get; set; }

		public void SizeAllocated(ESize size)
		{
			System.Console.WriteLine($"@@@@ GridLayoutManager.SizeAllocated (2/6) - input_size[{size.Width}][{size.Height}]");
			Reset();
			System.Console.WriteLine($"@@@@ GridLayoutManager.SizeAllocated (2/6) - _allocatedSize[{_allocatedSize.Width}][{_allocatedSize.Height}]");
			System.Console.WriteLine($"@@@@ GridLayoutManager.SizeAllocated (2/6) - _scrollCanvasSize[{_scrollCanvasSize.Width}][{_scrollCanvasSize.Height}]");
			_allocatedSize = size;
			_scrollCanvasSize = new ESize(0, 0);
		}

		ESize _scrollCanvasSize;

		public ESize GetScrollCanvasSize()
		{
			System.Console.WriteLine($"@@@@ GridLayoutManager.GetScrollCanvasSize (5/6)");
			System.Console.WriteLine($"@@@@ GridLayoutManager.GetScrollCanvasSize (5/6) -1 before CanvasSize [{_scrollCanvasSize.Width}][{_scrollCanvasSize.Height}]");
			if (_scrollCanvasSize.Width > 0 && _scrollCanvasSize.Height > 0)
				return _scrollCanvasSize;
			System.Console.WriteLine($"@@@@ GridLayoutManager.GetScrollCanvasSize (5/6) -2 itemCount [{CollectionView.Count}]");
			System.Console.WriteLine($"@@@@ GridLayoutManager.GetScrollCanvasSize (5/6) -3 itemSize [{CollectionView.GetItemSize().Width}][{CollectionView.GetItemSize().Height}]");
			var itemCount = CollectionView.Count;
			var itemSize = CollectionView.GetItemSize();
			if (IsHorizontal)
			{
				System.Console.WriteLine($"@@@@ GridLayoutManager.GetScrollCanvasSize (5/6) -4 Horizontal, set canvas [{itemCount * itemSize.Width}][{_allocatedSize.Height}]");
				return _scrollCanvasSize = new ESize(itemCount * itemSize.Width, _allocatedSize.Height);
			}
			else
			{
				System.Console.WriteLine($"@@@@ GridLayoutManager.GetScrollCanvasSize (5/6) -4 Vertical, set canvas [{_allocatedSize.Width}][{itemCount * itemSize.Height}]");
				return _scrollCanvasSize = new ESize(_allocatedSize.Width, itemCount * itemSize.Height);
			}
		}

		bool ShouldReArrange(Rect viewport)
		{
			System.Console.WriteLine($"@@@@ GridLayoutManager.ShouldReArrange (4/6)");
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
			System.Console.WriteLine($"@@@@ GridLayoutManager.LayoutItems (3/6) -0 viewport[{bound}] bool[{force}]");
			// TODO : need to optimization. it was frequently called with similar bound value.
			if (!ShouldReArrange(bound) && !force)
			{
				return;
			}
			_isLayouting = true;
			_last = bound;
			//System.Console.WriteLine($"------------OnLayoutItems {bound}----------------");
			var size = CollectionView.GetItemSize();
			System.Console.WriteLine($"@@@@ GridLayoutManager.LayoutItems (3/6) -1 size[{size.Width}][{size.Height}]");
			var itemSize = IsHorizontal ? size.Width : size.Height;
			System.Console.WriteLine($"@@@@ GridLayoutManager.LayoutItems (3/6) -2 itemSize[{itemSize}]");
			int startIndex = Math.Max(GetStartIndex(bound, itemSize) - 2, 0);
			System.Console.WriteLine($"@@@@ GridLayoutManager.LayoutItems (3/6) -3 startIndex[{startIndex}]");
			int endIndex = Math.Min(GetEndIndex(bound, itemSize) + 2, CollectionView.Count - 1);
			System.Console.WriteLine($"@@@@ GridLayoutManager.LayoutItems (3/6) -4 endIndex[{endIndex}]");
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
			System.Console.WriteLine($"@@@@ GridLayoutManager.ItemInserted 6/6");
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
			System.Console.WriteLine($"@@@@ GridLayoutManager.GetItemBound({index}) -0");
			var size = CollectionView.GetItemSize();
			System.Console.WriteLine($"@@@@ GridLayoutManager.GetItemBound -1 size[{size.Width}][{size.Height}]");
			if (IsHorizontal)
			{
				size.Height = _allocatedSize.Height;
				System.Console.WriteLine($"@@@@ GridLayoutManager.GetItemBound -2 Horizontal, size[{size.Height}]");
			}
			else
			{
				size.Width = _allocatedSize.Width;
				System.Console.WriteLine($"@@@@ GridLayoutManager.GetItemBound -2 Vertical, size[{size.Width}]");
			}

			if (IsHorizontal)
			{
				System.Console.WriteLine($"@@@@ GridLayoutManager.GetItemBound -3 Horizontal, [{index}] * [{size.Width}]");
				System.Console.WriteLine($"@@@@ GridLayoutManager.GetItemBound -3 Horizontal, rect x[{index * size.Width}], y[0], w[{size.Width}], h[{size.Height}]");
			}
			else
			{
				System.Console.WriteLine($"@@@@ GridLayoutManager.GetItemBound -3 Vertical, rect x[0], y[{index * size.Height}], w[{size.Width}], h[{size.Height}]");
			}

			int rowIndex = index / Span;
			int colIndex = index % Span;
			var colSize = ColumnSize();
			System.Console.WriteLine($"@@@@ GridLayoutManager.Calculate -0 [{(rowIndex)}][{colIndex}]//[{colSize}]//[{_allocatedSize.Width}][{_allocatedSize.Height}]");

			if (IsHorizontal)
			{
				System.Console.WriteLine($"@@@@ GridLayoutManager.Calculate -00 [{(rowIndex * size.Width)}][{colIndex * colSize}][{size.Width}][{colSize}]");
				return new Rect(rowIndex * size.Width, colIndex * colSize, size.Width, colSize);
			}
			else
			{
				System.Console.WriteLine($"@@@@ GridLayoutManager.Calculate -000 [{(colIndex * size.Width)}][{rowIndex * colSize}][{size.Width}][{colSize}]");
				return new Rect(colIndex * colSize, rowIndex * size.Height, colSize, size.Height);
			}

			int ColumnSize()
			{
				if (IsHorizontal)
				{
					if (size.Height > 0)
					{
						System.Console.WriteLine($"@@@@ GridLayoutManager.Calculate -1");
						return size.Height;
					}
					else if (_allocatedSize.Height > 0)
					{
						System.Console.WriteLine($"@@@@ GridLayoutManager.Calculate -2");
						return _allocatedSize.Height / Span;
					}
					else
					{
						System.Console.WriteLine($"@@@@ GridLayoutManager.Calculate -3");
						return 100;
					}
				}
				else
				{
					if (size.Width > 0)
					{
						System.Console.WriteLine($"@@@@ GridLayoutManager.Calculate -4");
						return size.Width;
					}
					else if (_allocatedSize.Width > 0)
					{
						System.Console.WriteLine($"@@@@ GridLayoutManager.Calculate -5");
						return _allocatedSize.Width / Span;
					}
					else
					{
						System.Console.WriteLine($"@@@@ GridLayoutManager.Calculate -6");
						return 100;
					}
				}
			}
			/*

			Rect Calculate(int index)
			var size = CollectionView.GetItemSize();

			int rowIndex = index / Span;
			int colIndex = index % Span;
			System.Console.WriteLine($"@@@@ GridLayoutManager.Calculate -1 index [{rowIndex}][{colIndex}]");
			System.Console.WriteLine($"@@@@ GridLayoutManager.Calculate -1 size [{size.Width}][{size.Height}]");

			if (IsHorizontal)
			{
				System.Console.WriteLine($"@@@@ GridLayoutManager.Calculate -2 rect x[{rowIndex * size.Width}], y[{colIndex * size.Height}], w[{size.Width}], h[{size.Height}]");
				return new Rect(rowIndex * size.Width, colIndex * size.Height, size.Width, size.Height);
			}
			else
			{
				System.Console.WriteLine($"@@@@ GridLayoutManager.Calculate -2 rect x[{colIndex * size.Width}], y[{rowIndex * size.Height}], w[{size.Width}], h[{size.Height}]");
				return new Rect(colIndex * size.Width, rowIndex * size.Height, size.Width, size.Height);
			}

			//
			return new Rectangle( rowIndex * ItemSize, colIndex * ColumnSize, ItemWidth, ColumnSize);
			return new Rectangle(colIndex * ColumnSize, rowIndex * ItemSize, ColumnSize, ItemHeight);

			return new Rectangle(HeaderSizeWithSpacing + rowIndex * ItemSize, colIndex * ColumnSize, ItemWidth, ColumnSize - Spacing);
			return new Rectangle(colIndex * ColumnSize, HeaderSizeWithSpacing + rowIndex * ItemSize, ColumnSize - Spacing, ItemHeight);

			//

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

			*/
		}

		public void Reset()
		{
			foreach (var realizedItem in _realizedItem.Values)
			{
				CollectionView.UnrealizeView(realizedItem.View);
			}
			_realizedItem.Clear();
			_scrollCanvasSize = new ESize(0, 0);
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
