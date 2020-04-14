using System.Collections.Generic;
using System.Diagnostics;
using ElmSharp;
using EColor = ElmSharp.Color;

namespace Xamarin.Forms.Platform.Tizen
{
	public class IndicatorViewRenderer : LayoutRenderer
	{
		int _itemSize = -1;
		int _itemCount = -1;
		EColor _selectedColor;
		EColor _filledColor;

		IndicatorView IndicatorView => Element as IndicatorView;

		public IndicatorViewRenderer() : base()
		{
			Debug.WriteLine($"@@@ @@@ (R) IndicatorViewRenderer");
			_itemSize = Device.Idiom == TargetIdiom.Watch ? 18 : 40;
			_selectedColor = EColor.Red;
			_filledColor = EColor.Green;

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
			Control.BackgroundColor = new EColor(100, 100, 100, 100);
			Control.AlignmentX = -1;
			Control.AlignmentY = -1;
			Control.WeightX = 1;
			Control.WeightY = 1;

			//TODO: why added one more control.child ??
			Control.LayoutUpdated += OnLayoutUpdated;
			//Control.SetLayoutCallback(OnLayoutUpdated);

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

		void OnLayoutUpdated()
		{
			Debug.WriteLine($"@@@ @@@ (R) OnLayoutUpdated [{Control.Children.Count}][{_itemCount}]");
			UpdateGeometry();
		}

		void OnLayoutUpdated(object sender, Native.LayoutEventArgs e)
		{
			Debug.WriteLine($"@@@ @@@ (R) OnLayoutUpdated [{Control.Children.Count}][{_itemCount}]");
			UpdateGeometry();
		}

		void UpdateGeometry()
		{
			Debug.WriteLine($"@@@ @@@ (R) UpdateGeometry [{Control.Children.Count}][{_itemCount}]");
			SetLinearLayout(NativeView.Geometry);
		}

		void UpdateItemsSource(bool isInitializing)
		{
			if (isInitializing)
				return;

			Control.Children.Clear();

			if (IndicatorView.ItemsSource == null)
				return;

			_itemCount = 0;
			foreach (var item in IndicatorView.ItemsSource)
			{
				var box = new ElmSharp.Box(NativeView);

				EvasObject rect = new ElmSharp.Rectangle(NativeView);
				if (IndicatorView.IndicatorTemplate != null)
				{
					View view = null;
					if (IndicatorView.IndicatorTemplate is DataTemplateSelector selector)
						view = selector.SelectTemplate(item, IndicatorView).CreateContent() as View;
					else
						view = IndicatorView.IndicatorTemplate.CreateContent() as View;

					var renderer = Platform.GetOrCreateRenderer(view);
					view.Parent = IndicatorView;
					(renderer as LayoutRenderer)?.RegisterOnLayoutUpdated();

					if (renderer != null)
						rect = renderer.NativeView;
				}
				rect.MinimumWidth = IndicatorView.IndicatorSize < _itemSize ? _itemSize : (int)IndicatorView.IndicatorSize;
				rect.MinimumHeight = IndicatorView.IndicatorSize < _itemSize ? _itemSize : (int)IndicatorView.IndicatorSize;

				box.PackStart(rect);
				box.Show();

				Control.Children.Add(box);
				_itemCount++;
			}
			Debug.WriteLine($"@@@ @@@ (R) UpdateItemsSource [{Control.Children.Count}][{_itemCount}]");
		}

		void UpdatePosition(bool isInitializing)
		{
			if (isInitializing)
				return;

			Debug.WriteLine($"@@@ @@@ (R) UpdatePosition [{Control.Children.Count}][{_itemCount}]");

			int index = 0;
			foreach (var child in Control.Children)
			{
				var item = child as ElmSharp.Box;
				if (item != null)
				{
					if (index == IndicatorView.Position)
						item.BackgroundColor = IndicatorView.SelectedIndicatorColor == Color.Default ? _selectedColor : IndicatorView.SelectedIndicatorColor.ToNative();
					else
						item.BackgroundColor = IndicatorView.IndicatorColor == Color.Default ? _filledColor : IndicatorView.IndicatorColor.ToNative();
				}
				index++;
			}
		}

		void SetLinearLayout(Rect geometry)
		{
			int padding = 0;
			foreach (var item in Control.Children)
			{
				var rect = new Rect();
				rect.Width = IndicatorView.IndicatorSize < _itemSize ? _itemSize : (int)IndicatorView.IndicatorSize;
				rect.Height = IndicatorView.IndicatorSize < _itemSize ? _itemSize : (int)IndicatorView.IndicatorSize;
				rect.X = geometry.X + padding;
				rect.Y = geometry.Y;

				item.Geometry = rect;
				item.Show();

				padding += rect.Width;
			}
		}

		List<(int, int)> _even = new List<(int, int)>
		{
			(37, 63), (49, 49), (63, 37), (77, 27), (93, 18), (109, 10), (127, 5), (144, 1), (162, -1),
			(180, -1), (198, 1), (216, 5), (233, 10), (249, 18), (265, 27), (279, 37), (293, 49), (305, 63),
		};
		List<(int, int)> _odd = new List<(int, int)>
		{
			(32, 70), (43, 56), (56, 43), (70, 32), (85, 22), (101, 14), (118, 7), (135, 3), (153, 0),
			(171, -1), (189, 0), (207, 3), (224, 7), (241, 14), (257, 22), (272, 32), (286, 43), (277, 56), (310, 70),
		};

		void SetCurveLayout(Rect geometry)
		{
			var count = Control.Children.Count;
			int center = 9;
			int i = 0;
			foreach (var item in Control.Children)
			{
				int start = center - (count / 2);
				int offset = i++;
				int position = start + offset;
				(int X, int Y) coordinate = count % 2 == 0 ? _even[position] : _odd[position];
				item.Geometry = new Rect
				{
					Width = IndicatorView.IndicatorSize < _itemSize ? _itemSize : (int)IndicatorView.IndicatorSize,
					Height = IndicatorView.IndicatorSize < _itemSize ? _itemSize : (int)IndicatorView.IndicatorSize,
					X = geometry.X + coordinate.X,
					Y = geometry.Y + coordinate.Y,
				};
				item.Show();
			}
		}
	}
}
