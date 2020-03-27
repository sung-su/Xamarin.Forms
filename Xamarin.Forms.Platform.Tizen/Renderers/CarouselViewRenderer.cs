using System;

namespace Xamarin.Forms.Platform.Tizen
{
	public class CarouselViewRenderer : ItemsViewRenderer<CarouselView, Native.CarouselView>
	{
		public CarouselViewRenderer()
		{
			RegisterPropertyHandler(CarouselView.ItemsLayoutProperty, UpdateItemsLayout);
			RegisterPropertyHandler(CarouselView.IsBounceEnabledProperty, UpdateIsBounceEnabled);
			RegisterPropertyHandler(CarouselView.IsSwipeEnabledProperty, UpdateIsSwipeEnabled);
		}

		protected override Native.CarouselView CreateNativeControl(ElmSharp.EvasObject parent)
		{
			return new Native.CarouselView(parent);
		}

		protected override IItemsLayout GetItemsLayout()
		{
			return Element.ItemsLayout;
		}

		ElmSharp.SmartEvent _animationStop;
		protected override void OnElementChanged(ElementChangedEventArgs<CarouselView> e)
		{
			base.OnElementChanged(e);
			if (e.NewElement != null)
			{
				Element.PlatformInitialized();
				Control.Scroll.DragStart += OnScrollStart;
				_animationStop = new ElmSharp.SmartEvent(Control.Scroll, Control.Scroll.RealHandle, "scroll,anim,stop");
				_animationStop.On += OnScrollStop;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Element != null)
				{
					Control.Scroll.DragStart -= OnScrollStart;
					_animationStop.On -= OnScrollStop;
				}
			}
			base.Dispose(disposing);
		}

		void OnScrollStart(object sender, System.EventArgs e)
		{
			if (!Element.IsDragging)
				Element.SetIsDragging(true);
			if (!Element.IsScrolling)
				Element.IsScrolling = true;
		}

		void OnScrollStop(object sender, System.EventArgs e)
		{
			if (Element.IsDragging)
				Element.SetIsDragging(false);
			if (Element.IsScrolling)
				Element.IsScrolling = false;
		}

		void UpdateIsBounceEnabled()
		{
			if (Element.IsBounceEnabled)
			{
				if (Control.LayoutManager.IsHorizontal)
				{
					Control.Scroll.HorizontalBounce = true;
					Control.Scroll.VerticalBounce = false;
				}
				else
				{
					Control.Scroll.HorizontalBounce = false;
					Control.Scroll.VerticalBounce = true;
				}
			}
			else
			{
				Control.Scroll.HorizontalBounce = false;
				Control.Scroll.VerticalBounce = false;
			}
		}

		void UpdateIsSwipeEnabled()
		{
			if (Element.IsSwipeEnabled)
			{
				Control.Scroll.ScrollBlock = ElmSharp.ScrollBlock.None;
			}
			else
			{
				if (Control.LayoutManager.IsHorizontal)
				{
					Control.Scroll.ScrollBlock = ElmSharp.ScrollBlock.Horizontal;
				}
				else
				{
					Control.Scroll.ScrollBlock = ElmSharp.ScrollBlock.Vertical;
				}
			}
		}
	}
}
