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
		}

		public ESize GetScrollCanvasSize()
		{
			var itemCount = CollectionView.Count;
			var itemSize = CollectionView.GetItemSize();
			if (IsHorizontal)
			{
				return new ESize(itemCount * itemSize.Width, _allocatedSize.Height);
			}
			else
			{
				return new ESize(_allocatedSize.Width, itemCount * itemSize.Height);
			}
		}

		Rect _last;
		public void LayoutItems(Rect bound)
		{
			// TODO : need to optimization. it was frequently called with similar bound value.
			if (_last == bound)
				return;
			_last = bound;
			//System.Console.WriteLine($"------------OnLayoutItems {bound}----------------");
			var size = CollectionView.GetItemSize();
			var itemSize = IsHorizontal ? size.Width : size.Height;
			int startIndex = Math.Max(GetStartIndex(bound, itemSize) - 1, 0);
			int endIndex = Math.Min(GetEndIndex(bound, itemSize) + 1, CollectionView.Count - 1);

			//System.Console.WriteLine($"OnLayoutItems s : {startIndex}, e : {endIndex} {bound}");

			foreach (var index in _realizedItem.Keys.ToList())
			{

				if (index < startIndex || index > endIndex)
				{
					CollectionView.UnrealizeView(_realizedItem[index].View);
					_realizedItem.Remove(index);
				}
			}

			var parent = CollectionView.ParentPosition;
			for (int i = startIndex; i <= endIndex; i++)
			{
				if (!_realizedItem.ContainsKey(i))
				{
					var view = CollectionView.RealizeView(i);
					_realizedItem[i] = new RealizedItem
					{
						View = view,
						Index = i,
					};
					var itemBound = GetItemBound(i);
					itemBound.X += parent.X;
					itemBound.Y += parent.Y;
					view.Geometry = itemBound;
					//System.Console.WriteLine($"{i} Item bound = {itemBound}");
				}
			}
			//System.Console.WriteLine($"-------------- OnLayoutItems ------- end ");
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

		class RealizedItem
		{
			public EvasObject View { get; set; }
			public int Index { get; set; }
		}

	}

}
