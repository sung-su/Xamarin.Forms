using System.Collections.Specialized;
using System.Linq;

using Xamarin.Forms.Platform.Tizen.Native;

namespace Xamarin.Forms.Platform.Tizen
{
	public class CarouselViewRenderer : ViewRenderer<CarouselView, Native.CarouselView>
	{
		INotifyCollectionChanged _observableSource;

		public CarouselViewRenderer()
		{
			System.Console.WriteLine($"@@@ CV R CarouselViewRenderer 1 in /*");
			System.Console.WriteLine($"@@@ CV R CarouselViewRenderer 2 register");
			RegisterPropertyHandler(ItemsView.ItemsSourceProperty, UpdateItemsSource);
			RegisterPropertyHandler(ItemsView.ItemTemplateProperty, UpdateAdaptor);
			RegisterPropertyHandler(CarouselView.ItemsLayoutProperty, UpdateItemsLayout);
			RegisterPropertyHandler(ItemsView.ItemSizingStrategyProperty, UpdateSizingStrategy);
			System.Console.WriteLine($"@@@ CV R CarouselViewRenderer 3 out */");
			RegisterPropertyHandler(CarouselView.PositionProperty, UpdatePosition);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<CarouselView> e)
		{
			System.Console.WriteLine($"@@@@@@@ CV R OnElementChanged 1 in /*");
			if (Control == null)
			{
				System.Console.WriteLine($"@@@@@@@ CV R OnElementChanged 2 set native control");
				SetNativeControl(new Native.CarouselView(Forms.NativeParent));
			}
			System.Console.WriteLine($"@@@@@@@ CV R OnElementChanged 3");
			if (e.NewElement != null)
			{
				System.Console.WriteLine($"@@@@@@@ CV R OnElementChanged 4 new element");
				e.NewElement.ScrollToRequested += OnScrollToRequest;
			}
			System.Console.WriteLine($"@@@@@@@ CV R OnElementChanged 5 call base on element changed");
			base.OnElementChanged(e);
			System.Console.WriteLine($"@@@@@@@ CV R OnElementChanged 6 call update adaptor");
			UpdateAdaptor(false);
			System.Console.WriteLine($"@@@@@@@ CV R OnElementChanged 7 out */");
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

		void UpdatePosition(bool initialize)
		{
			if (initialize)
				return;

			if (Element is CarouselView carousel)
			{
				Control?.Adaptor?.RequestItemSelected(carousel.Position);
			}
		}

		//void UpdateSelectionMode()
		//{
		//	if (Element is SelectableItemsView selectable)
		//	{
		//		Control.SelectionMode = selectable.SelectionMode == SelectionMode.None ? CollectionViewSelectionMode.None : CollectionViewSelectionMode.Single;
		//	}
		//}

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
			System.Console.WriteLine($"@@@@@@ CV R UpdateItemsSource 1 in /*");
			if (Element.ItemsSource is INotifyCollectionChanged collectionChanged)
			{
				System.Console.WriteLine($"@@@@@@ CV R UpdateItemsSource 2 source changing");
				if (_observableSource != null)
				{
					_observableSource.CollectionChanged -= OnCollectionChanged;
				}
				_observableSource = collectionChanged;
				_observableSource.CollectionChanged += OnCollectionChanged;
			}
			System.Console.WriteLine($"@@@@@@ CV R UpdateItemsSource 3 source changed and call update adaptor");
			UpdateAdaptor(initialize);
			System.Console.WriteLine($"@@@@@@ CV R UpdateItemsSource 4 out */");
		}

		void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			System.Console.WriteLine($"@@@@@@@@ CV R OnCollectionChanged 1 in /*");
			if (Element.ItemsSource == null || !Element.ItemsSource.Cast<object>().Any())
			{
				System.Console.WriteLine($"@@@@@@@@ CV R OnCollectionChanged 2 source empty, set empty item adaptor");
				Control.Adaptor = EmptyItemAdaptor.Create(Element);
			}
			else
			{
				System.Console.WriteLine($"@@@@@@@@ CV R OnCollectionChanged 3 source exist");
				if (Control.Adaptor is EmptyItemAdaptor)
				{
					System.Console.WriteLine($"@@@@@@@@ CV R OnCollectionChanged 4 adaptor empty item adaptor and update adaptor");
					UpdateAdaptor(false);
					System.Console.WriteLine($"@@@@@@@@ CV R OnCollectionChanged 5 adaptor updated");
				}
				System.Console.WriteLine($"@@@@@@@@ CV R OnCollectionChanged 6");
			}
			System.Console.WriteLine($"@@@@@@@@ CV R OnCollectionChanged 7 out */");
		}

		void UpdateAdaptor(bool initialize)
		{
			System.Console.WriteLine($"@@@@@ CV R UpdateAdaptor 1 in /*");
			System.Console.WriteLine($"@@@@ CV R UpdateAdaptor 2 init=[{initialize}]");
			if (!initialize)
			{
				System.Console.WriteLine($"@@@@@ CV R UpdateAdaptor 3 init=[{initialize}]");
				if (Element.ItemsSource == null || !Element.ItemsSource.Cast<object>().Any())
				{
					System.Console.WriteLine($"@@@@@ CV R UpdateAdaptor 4-1 source empty , set empty item adaptor");
					Control.Adaptor = EmptyItemAdaptor.Create(Element);
				}
				else if (Element.ItemTemplate == null)
				{
					System.Console.WriteLine($"@@@@@ CV R UpdateAdaptor 4-2 empty template , set item default template adaptor");
					Control.Adaptor = new ItemDefaultTemplateAdaptor(Element);
				}
				else
				{
					System.Console.WriteLine($"@@@@@ CV R UpdateAdaptor 4-3 set item template adaptor");
					Control.Adaptor = new ItemTemplateAdaptor(Element);
					Control.Adaptor.ItemSelected += OnItemSelectedFromUI;
				}
				System.Console.WriteLine($"@@@@@ CV R UpdateAdaptor 5 adaptor set done");
			}
			System.Console.WriteLine($"@@@@@ CV R UpdateAdaptor 6 out */");
		}

		void OnItemSelectedFromUI(object sender, SelectedItemChangedEventArgs e)
		{
			//if (Element is SelectableItemsView selectableItemsView)
			//{
			//	selectableItemsView.SelectedItem = e.SelectedItem;
			//}
		}

		void UpdateItemsLayout()
		{
			System.Console.WriteLine($"@@@@ CV R UpdateItemsLayout 1 in /*");
			if (Element.ItemsLayout != null)
			{
				System.Console.WriteLine($"@@@@ CV R UpdateItemsLayout 2 items layout exist");

				//origin
				//Control.LayoutManager = Element.ItemsLayout.ToLayoutManager(Element.ItemSizingStrategy);

				//second
				//var isHorizontal = Element.ItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal;
				//var itemSizingStrategy = Element.ItemSizingStrategy;
				//Control.LayoutManager = new LinearLayoutManager(isHorizontal, itemSizingStrategy);

				//third
				UpdateSizingStrategy(false);

				Control.SnapPointsType = (Element.ItemsLayout as ItemsLayout)?.SnapPointsType ?? SnapPointsType.None;
				Element.ItemsLayout.PropertyChanged += OnLayoutPropertyChanged;
				System.Console.WriteLine($"@@@@ CV R UpdateItemsLayout 3 set layout manager");
			}
			System.Console.WriteLine($"@@@@ CV R UpdateItemsLayout 4 out */");
		}

		void UpdateSizingStrategy(bool initialize)
		{
			System.Console.WriteLine($"@@@@ CV R UpdateSizingStrategy 1 in /*");
			if (initialize)
			{
				System.Console.WriteLine($"@@@@ CV R UpdateSizingStrategy 2 already initialized, reuse layout manager");
				return;
			}
			System.Console.WriteLine($"@@@@ CV R UpdateSizingStrategy 3 new layout manager");

			//origin
			//Control.LayoutManager = Element.ItemsLayout.ToLayoutManager(Element.ItemSizingStrategy);

			//second
			var isHorizontal = Element.ItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal;
			var itemSizingStrategy = Element.ItemSizingStrategy;
			Control.LayoutManager = new LinearLayoutManager(isHorizontal, itemSizingStrategy);
			System.Console.WriteLine($"@@@@ CV R UpdateSizingStrategy 4 out */");
		}

		void OnLayoutPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(ItemsLayout.SnapPointsType))
			{
				Control.SnapPointsType = (Element.ItemsLayout as ItemsLayout)?.SnapPointsType ?? SnapPointsType.None;
			}
			//else if (e.PropertyName == nameof(GridItemsLayout.Span))
			//{
			//	((GridLayoutManager)(Control.LayoutManager)).UpdateSpan(((GridItemsLayout)Element.ItemsLayout).Span);
			//}
		}
	}
}
