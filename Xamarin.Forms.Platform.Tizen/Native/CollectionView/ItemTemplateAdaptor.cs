using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ElmSharp;
using ESize = ElmSharp.Size;

namespace Xamarin.Forms.Platform.Tizen.Native
{
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

		protected ItemTemplateAdaptor(ItemsView itemsView, IEnumerable items, DataTemplate template) : base(items)
		{
			_template = template;
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
				System.Console.WriteLine($"SetBinding context = {this[index]}");
				view.BindingContext = this[index];
			}
		}

		public override ESize MeasureItem(int widthConstraint, int heightConstraint)
		{
			System.Console.WriteLine($"MeasureItem {widthConstraint} , {heightConstraint}");
			var view = _template.CreateContent() as View;
			var renderer = Platform.GetOrCreateRenderer(view);
			view.Parent = _itemsView;
			var request = view.Measure(Forms.ConvertToScaledDP(widthConstraint), Forms.ConvertToScaledDP(heightConstraint), MeasureFlags.IncludeMargins).Request;
			System.Console.WriteLine($"Request = {request}");
			renderer.Dispose();

			return request.ToPixel();
		}

	}
}
