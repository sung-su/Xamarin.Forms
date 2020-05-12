using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using ElmSharp;

namespace Xamarin.Forms.Platform.Tizen
{
	public class IndicatorViewRenderer : LayoutRenderer
	{
		const string _defaultIndicator = "Xamarin.Forms.Platform.Tizen.Resource.b_home_indicator_horizontal_dot.png";
		const string _focusedIndicator = "Xamarin.Forms.Platform.Tizen.Resource.b_home_indicator_horizontal_focus_dot.png";
		int _itemSize = -1;

		IndicatorView IndicatorView => Element as IndicatorView;

		public IndicatorViewRenderer() : base()
		{
			Debug.WriteLine($"@@@ @@@ (R) IndicatorViewRenderer");
			_itemSize = Device.Idiom == TargetIdiom.Watch ? 18 : 40;

			RegisterPropertyHandler(IndicatorView.ItemsSourceProperty, UpdateItemsSource);
			//RegisterPropertyHandler(IndicatorView.CountProperty, UpdateItemsSource);
			//RegisterPropertyHandler(IndicatorView.IndicatorSizeProperty, UpdateItemsSource);
			RegisterPropertyHandler(IndicatorView.PositionProperty, UpdatePosition);
			//RegisterPropertyHandler(IndicatorView.IndicatorColorProperty, UpdatePosition);
			//RegisterPropertyHandler(IndicatorView.SelectedIndicatorColorProperty, UpdatePosition);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Layout> e)
		{
			base.OnElementChanged(e);

			//TODO: need to align to center
			Control.BackgroundColor = new ElmSharp.Color(100, 100, 100, 100);
			Control.AlignmentX = -1;
			Control.AlignmentY = -1;
			Control.WeightX = 1;
			Control.WeightY = 1;
			Control.LayoutUpdated += OnLayoutUpdated;

			UpdateItemsSource(false);
			UpdatePosition(false);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Control != null)
				{
					Control.LayoutUpdated -= OnLayoutUpdated;
				}
			}
			base.Dispose(disposing);
		}

		void OnLayoutUpdated(object sender, Native.LayoutEventArgs e)
		{
			UpdateGeometry();
		}

		void UpdateGeometry()
		{
			int padding = 0;
			int itemSize = IndicatorView.IndicatorSize < 1 ? _itemSize : (int)IndicatorView.IndicatorSize;
			for (int index = 0; index < IndicatorView.Count; index++)
			{
				var item = Control.Children[index];
				//item.MinimumWidth = itemSize;
				//item.MinimumHeight = itemSize;
				//item.Move(NativeView.Geometry.X + padding, NativeView.Geometry.Y);
				item.Geometry = new Rect()
				{
					Width = itemSize,
					Height = itemSize,
					X = NativeView.Geometry.X + padding,
					Y = NativeView.Geometry.Y,
				};
				Debug.WriteLine($"@@@ @@@ (R) UpdateGeometry 1 [{padding}]");
				Debug.WriteLine($"@@@ @@@ (R) UpdateGeometry 2 [{item.MinimumWidth}] [{item.MinimumHeight}]");
				Debug.WriteLine($"@@@ @@@ (R) UpdateGeometry 3 [{item.Geometry}]");
				//item.Show();
				padding += itemSize;
			}
		}

		void UpdateItemsSource(bool isInitializing)
		{
			if (isInitializing)
				return;

			foreach (var child in Control.Children)
			{
				Debug.WriteLine($"@@@ @@@ (R) UpdateItemsSource Control children need to GCGCGGCGCGCGCG");
				child.Hide();
			}
			if (Control.Children.Count > 0)
			{
				Debug.WriteLine($"@@@ @@@ (R) UpdateItemsSource Control children CLEAR CLEAR CLEAR");
				Control.Children.Clear();
			}

			if (IndicatorView.ItemsSource == null)
				return;

			int itemSize = IndicatorView.IndicatorSize < 1 ? _itemSize : (int)IndicatorView.IndicatorSize;
			foreach (var item in IndicatorView.ItemsSource)
			{
				var box = new Box(NativeView);
				box.Geometry = new Rect(180, 180, itemSize, itemSize);

				EvasObject native = null;
				if (IndicatorView.IndicatorTemplate != null)
				{
					native = GetNativeView(item);
					//View view = null;
					//if (IndicatorView.IndicatorTemplate is DataTemplateSelector selector)
					//	view = selector.SelectTemplate(item, IndicatorView).CreateContent() as View;
					//else
					//	view = IndicatorView.IndicatorTemplate.CreateContent() as View;

					//view.Parent = IndicatorView;
					//var renderer = Platform.GetOrCreateRenderer(view);
					//(renderer as LayoutRenderer)?.RegisterOnLayoutUpdated();
					//native = renderer.NativeView;
					Debug.WriteLine($"@@@ @@@ (R) UpdateItemsSource case1");
				}
				if (native == null)
				{
					Debug.WriteLine($"@@@ @@@ (R) UpdateItemsSource case2");
					native = CreateNativeView();
					//native = new ElmSharp.Rectangle(NativeView);
				}
				native.MinimumWidth = itemSize;
				native.MinimumHeight = itemSize;
				native.Show();
				box.PackStart(native);

				Debug.WriteLine($"@@@ @@@ (R) UpdateItemsSource box.geo=[{box.Geometry}]");

				Control.Children.Add(box);
			}
			Debug.WriteLine($"@@@ @@@ (R) UpdateItemsSource ChildCount=[{Control.Children.Count}]");
			Debug.WriteLine($"@@@ @@@ (R) UpdateItemsSource ItemsSourceCount=[{IndicatorView.Count}]");
		}

		EvasObject GetNativeView(object item)
		{
			View view = null;
			if (IndicatorView.IndicatorTemplate is DataTemplateSelector selector)
				view = selector.SelectTemplate(item, IndicatorView).CreateContent() as View;
			else
				view = IndicatorView.IndicatorTemplate.CreateContent() as View;

			//view.Parent = IndicatorView;
			var renderer = Platform.GetOrCreateRenderer(view);
			(renderer as LayoutRenderer)?.RegisterOnLayoutUpdated();
			return renderer.NativeView;
		}

		EvasObject CreateNativeView()
		{
			var img = new ElmSharp.Image(NativeView);
			img.Load(ResourcePath.GetPath(_defaultIndicator));
			return img;
			//return new ElmSharp.Rectangle(NativeView);
		}

		void UpdatePosition(bool isInitializing)
		{
			if (isInitializing)
				return;

			for (int index = 0; index < IndicatorView.Count; index++)
			{
				var item = Control.Children[index] as Box;
				if (item != null)
				{
					if (index == IndicatorView.Position)
					{
						Debug.WriteLine($"@@@ @@@ (R) UpdatePosition [{index}] colored");
						item.BackgroundColor = IndicatorView.SelectedIndicatorColor.ToNative();
						if (IndicatorView.IndicatorTemplate == null)
						{
							Debug.WriteLine($"@@@ @@@ (R) UpdatePosition [{index}] changed");
							var img = new ElmSharp.Image(NativeView);
							img.Load(ResourcePath.GetPath(_focusedIndicator));
							Control.Children[index] = img;
						}
					}
					else
					{
						item.BackgroundColor = IndicatorView.IndicatorColor.ToNative();
						if (IndicatorView.IndicatorTemplate == null)
						{
							var img = new ElmSharp.Image(NativeView);
							img.Load(ResourcePath.GetPath(_defaultIndicator));
							Control.Children[index] = img;
						}
					}
				}
			}
		}
	}
}
