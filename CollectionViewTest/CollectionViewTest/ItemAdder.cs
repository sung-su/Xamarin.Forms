using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace CollectionViewTest
{
	internal class ItemAdder : CollectionModifier 
	{
		public ItemAdder(CollectionView cv) : base(cv, "Insert")
		{
		}

		protected override void ModifyCollection(ObservableCollection<CollectionViewGalleryTestItem> observableCollection, params int[] indexes)
		{
			var index = indexes[0];

			if (index > -1 && index < observableCollection.Count)
			{
				var item = new CollectionViewGalleryTestItem(DateTime.Now, "Inserted", "oasis.jpg", index);
				observableCollection.Insert(index, item);
			}
		}
	}
}