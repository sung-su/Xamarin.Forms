using System;
using System.Collections.Specialized;
using System.Linq;

using Xamarin.Forms.Platform.Tizen.Native;

namespace Xamarin.Forms.Platform.Tizen
{
	public class CarouselViewRenderer : ViewRenderer<CarouselView, Native.CollectionView>
	{
		INotifyCollectionChanged _observableSource;

		public CarouselViewRenderer()
		{
			RegisterPropertyHandler(ItemsView.ItemsSourceProperty, UpdateItemsSource);
			RegisterPropertyHandler(ItemsView.ItemTemplateProperty, UpdateAdaptor);
			RegisterPropertyHandler(CarouselView.ItemsLayoutProperty, UpdateItemsLayout);
			RegisterPropertyHandler(ItemsView.ItemSizingStrategyProperty, UpdateSizingStrategy);
			RegisterPropertyHandler(CarouselView.IsBounceEnabledProperty, UpdateIsBounceEnabled);
			RegisterPropertyHandler(CarouselView.IsSwipeEnabledProperty, UpdateIsSwipeEnabled);
			RegisterPropertyHandler(CarouselView.PositionProperty, UpdatePosition);
		}

		void UpdateIsBounceEnabled()
		{
			Control.SetScrollBounce(Element.IsBounceEnabled);
		}

		void UpdateIsSwipeEnabled()
		{
			Control.SetScrollBlock(!Element.IsSwipeEnabled);
		}

		void OnScrollFinished(object sender, EventArgs e)
		{
			Element.SendScrolled(new ItemsViewScrolledEventArgs());
		}

		protected override void OnElementChanged(ElementChangedEventArgs<CarouselView> e)
		{
			if (Control == null)
			{
				SetNativeControl(new Native.CollectionView(Forms.NativeParent));
			}
			if (e.NewElement != null)
			{
				e.NewElement.ScrollToRequested += OnScrollToRequest;
			}

			Control.SelectionMode = CollectionViewSelectionMode.Single;
			Control.DragStart += (s, arg) => { Element.SetIsDragging(true); };
			Control.DragStop += (s, arg) => { Element.SetIsDragging(false); };

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
				}
				if (_observableSource != null)
				{
					_observableSource.CollectionChanged -= OnCollectionChanged;
				}
			}
			base.Dispose(disposing);
		}

		void UpdatePosition(bool initialize)
		{
			if (initialize)
				return;
			if (Element is CarouselView carousel)
			{
				Control?.Adaptor?.RequestItemSelected(carousel.Position);
				Element.CurrentItem = Control.Adaptor.GetItemFromIndex(Element.Position);
			}
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
			if (!initialize)
			{
				if (Element.ItemsSource == null || !Element.ItemsSource.Cast<object>().Any())
				{
					Control.Adaptor = EmptyItemAdaptor.Create(Element);
				}
				else if (Element.ItemTemplate == null)
				{
					Control.Adaptor = new ItemDefaultTemplateAdaptor(Element);
				}
				else
				{
					Control.Adaptor = new ItemTemplateAdaptor(Element);
					Control.Adaptor.ItemSelected += OnItemSelectedFromUI;
				}
			}
		}

		void OnItemSelectedFromUI(object sender, SelectedItemChangedEventArgs e)
		{
			Element.Position = e.SelectedItemIndex;
			Element.CurrentItem = e.SelectedItem;
		}

		void UpdateItemsLayout()
		{
			if (Element.ItemsLayout != null)
			{
				UpdateSizingStrategy(false);
			}
		}

		void UpdateSizingStrategy(bool initialize)
		{
			if (initialize)
			{
				return;
			}

			var isHorizontal = Element.ItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal;
			var itemSizingStrategy = Element.ItemSizingStrategy;
			Control.LayoutManager = new LinearLayoutManager(isHorizontal, itemSizingStrategy);
		}
	}
}
