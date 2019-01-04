﻿using Xamarin.Forms;

namespace App2
{
	internal class ScrollToGallery : ContentPage
	{
		public ScrollToGallery()
		{
			var descriptionLabel =
				new Label { Text = "ScrollTo Galleries", Margin = new Thickness(2, 2, 2, 2) };

			Title = "ScrollTo Galleries";

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						descriptionLabel,
						GalleryBuilder.NavButton("ScrollTo Index (Code, Horizontal List)", () =>
							new ScrollToCodeGallery(ListItemsLayout.HorizontalList), Navigation),
						GalleryBuilder.NavButton("ScrollTo Index (Code, Vertical List)", () =>
							new ScrollToCodeGallery(ListItemsLayout.VerticalList), Navigation),
						GalleryBuilder.NavButton("ScrollTo Index (Code, Horizontal Grid)", () =>
								new ScrollToCodeGallery(new GridItemsLayout(3, ItemsLayoutOrientation.Horizontal)),
							Navigation),
						GalleryBuilder.NavButton("ScrollTo Index (Code, Vertical Grid)", () =>
								new ScrollToCodeGallery(new GridItemsLayout(3, ItemsLayoutOrientation.Vertical)),
							Navigation),

						GalleryBuilder.NavButton("ScrollTo Item (Code, Horizontal List)", () =>
							new ScrollToCodeGallery(ListItemsLayout.HorizontalList, ScrollToMode.Element,
								ExampleTemplates.ScrollToItemTemplate), Navigation),
						GalleryBuilder.NavButton("ScrollTo Item (Code, Vertical List)", () =>
							new ScrollToCodeGallery(ListItemsLayout.VerticalList, ScrollToMode.Element,
								ExampleTemplates.ScrollToItemTemplate), Navigation),
						GalleryBuilder.NavButton("ScrollTo Item (Code, Horizontal Grid)", () =>
							new ScrollToCodeGallery(new GridItemsLayout(3, ItemsLayoutOrientation.Horizontal),
								ScrollToMode.Element, ExampleTemplates.ScrollToItemTemplate), Navigation),
						GalleryBuilder.NavButton("ScrollTo Item (Code, Vertical Grid)", () =>
							new ScrollToCodeGallery(new GridItemsLayout(3, ItemsLayoutOrientation.Vertical),
								ScrollToMode.Element, ExampleTemplates.ScrollToItemTemplate), Navigation)
					}
				}
			};
		}
	}
}