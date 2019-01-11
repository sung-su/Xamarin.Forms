using System;
using System.Collections.Specialized;
using System.Linq;

using Xamarin.Forms.Platform.Tizen.Native;

namespace Xamarin.Forms.Platform.Tizen
{
	public class ItemsViewRenderer : ViewRenderer<ItemsView, Native.CollectionView>
	{
		INotifyCollectionChanged _observableSource;

		public ItemsViewRenderer()
		{
			Console.WriteLine($"@@@@ ItemsViewRenderer.ItemsViewRenderer (1/6)");
			RegisterPropertyHandler(ItemsView.ItemsSourceProperty, UpdateItemsSource);
			RegisterPropertyHandler(ItemsView.ItemTemplateProperty, UpdateAdaptor);
			RegisterPropertyHandler(ItemsView.ItemsLayoutProperty, UpdateItemsLayout);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<ItemsView> e)
		{
			Console.WriteLine($"@@@@ ItemsViewRenderer.OnElementChanged (2/6)");
			if (Control == null)
			{
				SetNativeControl(new Native.CollectionView(Forms.NativeParent));
			}
			
			if (e.NewElement != null)
			{
				Console.WriteLine($"@@@@ ItemsViewRenderer.OnElementChanged (2/6) - e.NewElement[{e.NewElement}]");
				e.NewElement.ScrollToRequested += OnScrollToRequest;
			}

			base.OnElementChanged(e);
			UpdateAdaptor(false);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Element != null)
				{
					Element.ScrollToRequested -= OnScrollToRequest;
					Element.ItemsLayout.PropertyChanged -= OnLayoutPropertyChanged;
				}
				if (_observableSource != null)
				{
					_observableSource.CollectionChanged -= OnCollectionChanged;
				}
			}
			base.Dispose(disposing);
		}

		void OnScrollToRequest(object sender, ScrollToRequestEventArgs e)
		{
			if (e.Mode == ScrollToMode.Position)
			{
				Control.ScrollTo(e.Index, e.ScrollToPosition, e.IsAnimated);
			}
			else
			{
				Control.ScrollTo(e.Item, e.ScrollToPosition, e.IsAnimated);
			}
		}

		void UpdateItemsSource(bool initialize)
		{
			Console.WriteLine($"@@@@ ItemsViewRenderer.UpdateItemsSource (3/6)");
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
			Console.WriteLine($"@@@@ ItemsViewRenderer.OnCollectionChanged (-)");
			if (Element.ItemsSource == null || !Element.ItemsSource.Cast<object>().Any())
			{
				Control.Adaptor = EmptyItemAdaptor.Create(Element);
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
			Console.WriteLine($"@@@@ ItemsViewRenderer.UpdateAdaptor (4/6) - initialize [{initialize}]");
			if (!initialize && Element != null)
			{
				if (Element.ItemsSource == null || !Element.ItemsSource.Cast<object>().Any())
				{
					Control.Adaptor = EmptyItemAdaptor.Create(Element);
				}
				else
				{
					Console.WriteLine($"@@@@ ItemsViewRenderer.UpdateAdaptor (4/6) - Control.Adaptor = new ItemTemplateAdaptor");
					Control.Adaptor = new ItemTemplateAdaptor(Element);
				}
			}
		}

		void UpdateItemsLayout()
		{
			Console.WriteLine($"@@@@ ItemsViewRenderer.UpdateItemsLayout (5/6)");
			if (Element.ItemsLayout != null)
			{
				Console.WriteLine($"@@@@ ItemsViewRenderer.UpdateItemsLayout (5/6) - set Control.LayoutManager");
				Control.LayoutManager = Element.ItemsLayout.ToLayoutManager();
				Control.SnapPointsType = (Element.ItemsLayout as ItemsLayout)?.SnapPointsType ?? SnapPointsType.None;
				if ((Element.ItemsLayout as GridItemsLayout) != null)
				{
					Console.WriteLine($"@@@@ ItemsViewRenderer.UpdateItemsLayout (5/6) - set Span [{Control.Span}] <- [{((GridItemsLayout)Element.ItemsLayout).Span}]");
					Control.Span = ((GridItemsLayout)Element.ItemsLayout).Span;
				}
				Element.ItemsLayout.PropertyChanged += OnLayoutPropertyChanged;
			}
		}

		void OnLayoutPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			Console.WriteLine($"@@@@ ItemsViewRenderer.OnLayoutPropertyChanged (7) - [{e.PropertyName}]");
			if (e.PropertyName == nameof(ItemsLayout.SnapPointsType))
			{
				Control.SnapPointsType = (Element.ItemsLayout as ItemsLayout)?.SnapPointsType ?? SnapPointsType.None;
			}
			if (e.PropertyName == nameof(GridItemsLayout.Span))
			{
				Console.WriteLine($"@@@@ ItemsViewRenderer.OnLayoutPropertyChanged (7) - set Span [{Control.Span}] <- [{((GridItemsLayout)Element.ItemsLayout).Span}]");
				Control.Span = ((GridItemsLayout)Element.ItemsLayout).Span;
			}
		}
	}

	static class ItemsLayoutExtension
	{
		public static ICollectionViewLayoutManager ToLayoutManager(this IItemsLayout layout)
		{
			Console.WriteLine($"@@@@ ItemsViewRenderer.ToLayoutManager (6/6) ");
			switch (layout)
			{
				case ListItemsLayout listItemsLayout:
					Console.WriteLine($"@@@@ ItemsViewRenderer.ToLayoutManager (6/6) - LIST");
					return new LinearLayoutManager(listItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal);
				case GridItemsLayout gridItemsLayout:
					Console.WriteLine($"@@@@ ItemsViewRenderer.ToLayoutManager (6/6) - GRID, Span[{gridItemsLayout.Span}]");
					return new GridLayoutManager(gridItemsLayout.Span, gridItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal);
				default:
					break;
			}

			return new LinearLayoutManager(false);
		}
	}
}
