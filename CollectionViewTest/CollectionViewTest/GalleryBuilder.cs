﻿using System;
using Xamarin.Forms;

namespace CollectionViewTest
{
	public static class GalleryBuilder
	{
		public static Button NavButton(string galleryName, Func<ContentPage> gallery, INavigation nav)
		{
			var button = new Button { Text = $"{galleryName}" };
			button.Clicked += (sender, args) => { nav.PushAsync(gallery()); };
			return button;
		}
	}
}