using ElmSharp;
using System;
using System.ComponentModel;

namespace Xamarin.Forms.Platform.Tizen
{
	/// <summary>
	/// Renderer of a Layout.
	/// </summary>
	public class LayoutRenderer : ViewRenderer<Layout, Native.Canvas>
	{
		bool _layoutUpdatedRegistered = false;

		public void RegisterOnLayoutUpdated()
		{
			Console.WriteLine($"@@@@ LayoutRenderer.RegisterOnLayoutUpdated (-/5)");
			if (!_layoutUpdatedRegistered)
			{
				Control.LayoutUpdated += OnLayoutUpdated;
				_layoutUpdatedRegistered = true;
			}
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Layout> e)
		{
			Console.WriteLine($"@@@@ LayoutRenderer.OnElementChanged (-/5)");
			if (null == Control)
			{
				SetNativeControl(new Native.Canvas(Forms.NativeParent));
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			Console.WriteLine($"@@@@ LayoutRenderer.OnElementPropertyChanged (-/5) - [{e.PropertyName}]");
			base.OnElementPropertyChanged(sender, e);
			if (e.PropertyName == Layout.CascadeInputTransparentProperty.PropertyName)
			{
				UpdateInputTransparent(false);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_layoutUpdatedRegistered)
				{
					Control.LayoutUpdated -= OnLayoutUpdated;
					_layoutUpdatedRegistered = false;
				}
			}

			base.Dispose(disposing);
		}

		protected override void UpdateInputTransparent(bool initialize)
		{
			Console.WriteLine($"@@@@ LayoutRenderer.UpdateInputTransparent (-/5)");
			if (initialize && Element.InputTransparent == default(bool))
			{
				return;
			}

			if (Element.InputTransparent)
			{
				if (Element.CascadeInputTransparent)
				{
					//Ignore all events of both layout and it's chidren
					NativeView.PassEvents = true;
				}
				else
				{
					//Ignore Layout's event only. Children's events should be allowded.
					NativeView.PassEvents = false;
					NativeView.RepeatEvents = true;
				}
			}
			else
			{
				//Allow layout's events and children's events would be determined by CascadeInputParent.
				NativeView.PassEvents = false;
				NativeView.RepeatEvents = false;
			}
			GestureDetector.InputTransparent = Element.InputTransparent;
		}

		void OnLayoutUpdated(object sender, Native.LayoutEventArgs e)
		{
			Console.WriteLine($"@@@@ LayoutRenderer.OnLayoutUpdated (-/5)");
			Element.Layout(e.Geometry.ToDP());
		}
	}
}
