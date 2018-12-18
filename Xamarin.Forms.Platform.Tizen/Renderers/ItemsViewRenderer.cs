using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms.Platform.Tizen.Native;
using ESize = ElmSharp.Size;

namespace Xamarin.Forms.Platform.Tizen
{
	public class ItemsViewRenderer : ViewRenderer<ItemsView, Native.CollectionView>
	{
		public ItemsViewRenderer()
		{
			System.Console.WriteLine("ItemsViewRenderer created");
			RegisterPropertyHandler(ItemsView.ItemsSourceProperty, UpdateAdaptor);
			RegisterPropertyHandler(ItemsView.ItemTemplateProperty, UpdateAdaptor);
			RegisterPropertyHandler(ItemsView.ItemsLayoutProperty, UpdateItemsLayout);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<ItemsView> e)
		{
			System.Console.WriteLine("OnElementChanged");
			if (Control == null)
			{
				SetNativeControl(new Native.CollectionView(Forms.NativeParent));
			}
			base.OnElementChanged(e);
			UpdateAdaptor(false);
		}

		protected override ESize Measure(int availableWidth, int availableHeight)
		{
			return new ESize(300, 300);
		}

		void UpdateAdaptor(bool initialize)
		{
			if (!initialize)
			{
				System.Console.WriteLine("Adaptor Update!!!");
				Control.Adaptor = new ItemTemplateAdaptor(Element);
				System.Console.WriteLine("Adaptor Update!!! - end");
			}
		}

		void UpdateItemsLayout()
		{
			Console.WriteLine($"UpdateItemslayout - start");
			System.Console.WriteLine($"Element.ItemsLayout {Element.ItemsLayout}");
			if (Element.ItemsLayout == null)
			{
				System.Console.WriteLine("Element.ItemsLayout == null is NULL!!!!");
			}else
			{
				System.Console.WriteLine($"Control == null ? {Control == null}");

				var layoutmananger = Element.ItemsLayout.ToLayoutManager();

				System.Console.WriteLine($"Created layoutmanager !! ? {layoutmananger}");

				Control.LayoutManager = layoutmananger;
			}

			Console.WriteLine($"UpdateItemslayout - end");
		}

	}

	static class ItemsLayoutExtension
	{
		public static ICollectionViewLayoutManager ToLayoutManager(this IItemsLayout layout)
		{
			System.Console.WriteLine($"ToLayoutManager {layout} null? : {layout==null}");
			switch (layout)
			{
				case ListItemsLayout listItemsLayout:
					System.Console.WriteLine($"Is ListItemsLayout listItemsLayout.Orientation = {listItemsLayout.Orientation}");
					var tmp = new LinearLayoutManager(listItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal);
					System.Console.WriteLine("LinearLayoutManager is created");
					return tmp;
				default:
					break;
			}

			System.Console.WriteLine($"ToLayoutManager 2");
			return new LinearLayoutManager(false);
		}

	}
}
