using System;
using System.ComponentModel;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;

namespace Xamarin.Forms.Platform.Android
{
	public class CarouselViewRenderer : ItemsViewRenderer
	{
		// TODO hartez 2018/08/29 17:13:17 Does this need to override SelectLayout so it ignores grids?	(Yes, and so it can warn on unknown layouts)
		Context _context;
		protected CarouselView Carousel;
		bool _isSwipeEnabled;
		bool _isUpdatingPositionFromForms;

		public CarouselViewRenderer(Context context) : base(context)
		{
			_context = context;

		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				
			}
			base.Dispose(disposing);
		}

		protected override void SetUpNewElement(ItemsView newElement)
		{
			base.SetUpNewElement(newElement);
			if (newElement == null)
			{
				Carousel = null;
				return;
			}

			Carousel = newElement as CarouselView;

			UpdateIsSwipeEnabled();
			_isUpdatingPositionFromForms = true;
			//Goto to the Correct Position
			Carousel.ScrollTo(Carousel.Position);
			_isUpdatingPositionFromForms = false;
		}

		protected override void UpdateItemsSource()
		{

			// By default the CollectionViewAdapter creates the items at whatever size the template calls for
			// But for the Carousel, we want it to create the items to fit the width/height of the viewport
			// So we give it an alternate delegate for creating the views

			ItemsViewAdapter = new ItemsViewAdapter(ItemsView,
				(context) => new SizedItemContentView(context, () => Width, () => Height));

			SwapAdapter(ItemsViewAdapter, false);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == CarouselView.IsSwipeEnabledProperty.PropertyName)
				UpdateIsSwipeEnabled();
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			//TODO: this doesn't work because we need to interact with the Views
			if (!_isSwipeEnabled)
			{
				return false;
			}
			return base.OnTouchEvent(e);
		}

		// TODO: Do we want to do this through the ScrollListener?
		public override void OnScrollStateChanged(int state)
		{
			base.OnScrollStateChanged(state);

			if (_isSwipeEnabled)
			{
				if (state == ScrollStateDragging)
				{
					Carousel.SetIsDragging(true);
				}
				else
				{
					Carousel.SetIsDragging(false);
				}
			}	
		}

		void UpdateIsSwipeEnabled()
		{
			_isSwipeEnabled = Carousel.IsSwipeEnabled;
		}

		void UpdatePosition(int position)
		{
			if (position == -1 || _isUpdatingPositionFromForms)
				return;

			var context = ItemsViewAdapter?.ItemsSource[position];
			if (context == null)
				throw new InvalidOperationException("Visible item not found");

			Carousel.SetCurrentItem(context);
		}
	}
}