namespace Xamarin.Forms.Platform.Tizen
{
	public class IndicatorViewRenderer : ViewRenderer<IndicatorView, Native.IndicatorView>
	{
		public IndicatorViewRenderer()
		{
			RegisterPropertyHandler(IndicatorView.CountProperty, UpdateItemsSource);
			RegisterPropertyHandler(IndicatorView.HideSingleProperty, UpdateHideSingle);
			RegisterPropertyHandler(IndicatorView.PositionProperty, UpdatePosition);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<IndicatorView> e)
		{
			if (Control == null)
			{
				SetNativeControl(new Native.IndicatorView(Forms.NativeParent));

			}
			if (e.NewElement != null)
			{
				Control.SelectedIndex += OnSelectedIndex;
			}
			if (e.OldElement != null)
			{
				Control.SelectedIndex -= OnSelectedIndex;
			}
			base.OnElementChanged(e);
		}

		void OnSelectedIndex(object sender, SelectedPositionChangedEventArgs e)
		{
			Element.Position = (int)(e.SelectedPosition);
		}

		void UpdateHideSingle()
		{
			Element.IsVisible = (!Element.HideSingle || Element.Count > 1);
		}

		void UpdateItemsSource()
		{
			Control.ClearIndex();
			for (int i = 0 ; i < Element.Count; i++)
			{
				Control.AppendIndex();
			}
			UpdateHideSingle();
		}

		void UpdatePosition()
		{
			Control.UpdateSelectedIndex(Element.Position);
		}
	}
}
