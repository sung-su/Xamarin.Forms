using System.Collections.Generic;
using ElmSharp;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	class RecyclerPool
	{
		LinkedList<EvasObject> _pool = new LinkedList<EvasObject>();

		public void Clear(ItemAdaptor adaptor)
		{
			foreach (var item in _pool)
			{
				adaptor.RemoveNativeView(item);
			}
			_pool.Clear();
		}

		public void AddRecyclerView(EvasObject view)
		{
			_pool.AddLast(view);
		}

		public EvasObject GetRecyclerView()
		{
			if (_pool.First != null)
			{
				var fisrt = _pool.First;
				_pool.RemoveFirst();
				return fisrt.Value;
			}
			return null;
		}
	}
}
