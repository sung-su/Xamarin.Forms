using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Xamarin.Forms.Platform.Tizen.Native;
using ESize = ElmSharp.Size;

namespace Xamarin.Forms.Platform.Tizen
{
	public class ItemsViewRenderer : ViewRenderer<ItemsView, Native.CollectionView>
	{
		static DataTemplate s_defaultEmptyTemplate = new DataTemplate(typeof(EmptyView));
		INotifyCollectionChanged _observableSource;

		public ItemsViewRenderer()
		{
			System.Console.WriteLine("ItemsViewRenderer created");
			RegisterPropertyHandler(ItemsView.ItemsSourceProperty, UpdateItemsSource);
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

		void UpdateItemsSource(bool initialize)
		{
			if (Element.ItemsSource is INotifyCollectionChanged collectionChanged)
			{
				if (_observableSource != null)
				{
					_observableSource.CollectionChanged -= OnCollectionChanged;
				}
				_observableSource = collectionChanged;
				_observableSource.CollectionChanged += OnCollectionChanged;
			}

			UpdateAdaptor(initialize);
		}

		void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (Element.ItemsSource == null || !Element.ItemsSource.Cast<object>().Any())
			{
				System.Console.WriteLine($"Empty adaptor setup");
				DataTemplate template = Element.EmptyViewTemplate ?? s_defaultEmptyTemplate;
				var empty = new List<object>();
				empty.Add(Element.EmptyView ?? new object());
				Control.Adaptor = new EmptyItemAdaptor(Element, empty, template);
			}
			else
			{
				if (Control.Adaptor is EmptyItemAdaptor)
				{
					UpdateAdaptor(false);
				}
			}
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
	class EmptyView : StackLayout
	{
		public EmptyView()
		{
			HorizontalOptions = LayoutOptions.FillAndExpand;
			VerticalOptions = LayoutOptions.FillAndExpand;
			Children.Add(
				new Label
				{
					Text = "No items found",
					VerticalOptions = LayoutOptions.CenterAndExpand,
					HorizontalOptions = LayoutOptions.CenterAndExpand,
					HorizontalTextAlignment = TextAlignment.Center,
					VerticalTextAlignment = TextAlignment.Center,
				}
			);
		}
	}
}
