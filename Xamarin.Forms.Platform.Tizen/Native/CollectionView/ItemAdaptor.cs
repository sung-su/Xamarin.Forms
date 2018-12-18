﻿using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using ElmSharp;
using ESize = ElmSharp.Size;

namespace Xamarin.Forms.Platform.Tizen.Native
{

	public abstract class ItemAdaptor : INotifyCollectionChanged
	{
		IList _itemsSource;

		public CollectionView CollectionView { get; set; }

		protected ItemAdaptor(IEnumerable items)
		{
			SetItemsSource(items);
		}

		protected void SetItemsSource(IEnumerable items)
		{
			switch (items)
			{
				case IList list:
					_itemsSource = list;
					_observableCollection = list as INotifyCollectionChanged;
					break;
				case IEnumerable<object> generic:
					_itemsSource = new List<object>(generic);
					break;
				case IEnumerable _:
					_itemsSource = new List<object>();
					foreach (var item in items)
					{
						_itemsSource.Add(item);
					}
					break;
			}
		}

		public object this[int index]
		{
			get
			{
				return _itemsSource[index];
			}
		}

		public int Count => _itemsSource.Count;

		INotifyCollectionChanged _observableCollection;
		event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
		{
			add
			{
				if (_observableCollection != null)
				{
					_observableCollection.CollectionChanged += value;
				}
			}
			remove
			{
				if (_observableCollection != null)
				{
					_observableCollection.CollectionChanged -= value;
				}
			}
		}

		public abstract EvasObject CreateNativeView(EvasObject parent);
		public abstract void RemoveNativeView(EvasObject native);
		public abstract void SetBinding(EvasObject view, int index);

		public abstract ESize MeasureItem(int widthConstraint, int heightConstraint);

	}
}