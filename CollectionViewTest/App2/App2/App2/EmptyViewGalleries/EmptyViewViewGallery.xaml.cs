﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App2
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class EmptyViewViewGallery : ContentPage
	{
		readonly DemoFilteredItemSource _demoFilteredItemSource = new DemoFilteredItemSource();

		public EmptyViewViewGallery ()
		{
			InitializeComponent ();

			CollectionView.ItemTemplate = ExampleTemplates.PhotoTemplate();

			CollectionView.ItemsSource = _demoFilteredItemSource.Items;

			SearchBar.TextChanged += SearchBarOnTextChanged;
		}

		void SearchBarOnTextChanged(object sender, TextChangedEventArgs e)
		{
			_demoFilteredItemSource.FilterItems(e.NewTextValue);
		}
	}
}