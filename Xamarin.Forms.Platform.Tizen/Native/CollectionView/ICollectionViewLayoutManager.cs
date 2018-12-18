using ElmSharp;
using ESize = ElmSharp.Size;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	public interface ICollectionViewLayoutManager
	{
		ICollectionViewController CollectionView { get; set; }
		bool IsHorizontal { get; }

		void SizeAllocated(ESize size);
		ESize GetScrollCanvasSize();

		void LayoutItems(Rect bound);
		Rect GetItemBound(int index);

		void Reset();
	}
}
