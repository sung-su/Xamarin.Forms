using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using ElmSharp;
using ESize = ElmSharp.Size;

namespace Xamarin.Forms.Platform.Tizen.Native
{

	public abstract class ItemAdaptor : INotifyCollectionChanged
	{
		IList _itemsSource;

		public CollectionView CollectionView { get; set; }

		public ItemAdaptor(IEnumerable items)
		{
			switch (items)
			{
				case IList list:
					_itemsSource = list;
					_observableCollection = list as INotifyCollectionChanged;
					break;
				case IEnumerable<object> generic:
					_itemsSource = new List<object>(generic);
					break;
				case IEnumerable _:
					_itemsSource = new List<object>();
					foreach (var item in items)
					{
						_itemsSource.Add(item);
					}
					break;
			}
		}

		public object this[int index]
		{
			get
			{
				return _itemsSource[index];
			}
		}

		public int Count => _itemsSource.Count;

		INotifyCollectionChanged _observableCollection;
		event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
		{
			add
			{
				if (_observableCollection != null)
				{
					_observableCollection.CollectionChanged += value;
				}
			}
			remove
			{
				if (_observableCollection != null)
				{
					_observableCollection.CollectionChanged -= value;
				}
			}
		}

		public abstract EvasObject CreateNativeView(EvasObject parent);
		public abstract void RemoveNativeView(EvasObject native);
		public abstract void SetBinding(EvasObject view, int index);

		public abstract ESize MeasureItem(int widthConstraint, int heightConstraint);

	}

	public class ItemTemplateAdaptor : ItemAdaptor
	{
		Dictionary<EvasObject, View> _nativeFormsTable = new Dictionary<EvasObject, View>();
		DataTemplate _template;
		ItemsView _itemsView;

		public ItemTemplateAdaptor(ItemsView itemsView) : base(itemsView.ItemsSource)
		{
			_template = itemsView.ItemTemplate;
			_itemsView = itemsView;
		}

		public override EvasObject CreateNativeView(EvasObject parent)
		{
			System.Console.WriteLine($"CreateNativeView");
			var view = _template.CreateContent() as View;
			var renderer = Platform.GetOrCreateRenderer(view);
			var native = Platform.GetOrCreateRenderer(view).NativeView;
			view.Parent = _itemsView;
			(renderer as LayoutRenderer)?.RegisterOnLayoutUpdated();

			_nativeFormsTable[native] = view;
			return native;
		}

		public override void RemoveNativeView(EvasObject native)
		{
			System.Console.WriteLine($"RemoveNativeView");
			if (_nativeFormsTable.TryGetValue(native, out View view))
			{
				Platform.GetRenderer(view)?.Dispose();
				_nativeFormsTable.Remove(native);
			}
		}

		public override void SetBinding(EvasObject native, int index)
		{
			if (_nativeFormsTable.TryGetValue(native, out View view))
			{
				var data = this[index];
				System.Console.WriteLine($"SetBinding context= {data}");
				view.BindingContext = this[index];
			}
		}

		public override ESize MeasureItem(int widthConstraint, int heightConstraint)
		{
			var view = _template.CreateContent() as View;
			view.Parent = _itemsView;
			return view.Measure(widthConstraint, heightConstraint).Request.ToPixel();
		}

	}
}