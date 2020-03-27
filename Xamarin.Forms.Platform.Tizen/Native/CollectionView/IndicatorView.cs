using System;
using System.Collections.Generic;
using ElmSharp;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	public class IndicatorView : Index
	{
		List<IndexItem> _list = new List<IndexItem>();

		public IndicatorView(EvasObject parent) : base(parent)
		{
			AutoHide = false;
			IsHorizontal = true;
			Style = "pagecontrol";
			if (Device.Idiom == TargetIdiom.Watch)
			{
				Style = "thumbnail";
			}
		}

		public event EventHandler<SelectedPositionChangedEventArgs> SelectedIndex;

		public void UpdateSelectedIndex(int index)
		{
			if (index > -1 && index < _list.Count)
			{
				_list[index].Select(true);
			}
		}

		public void AppendIndex()
		{
			var item = Append(null);
			item.Selected += OnSelected;
			_list.Add(item);
		}

		public void ClearIndex()
		{
			foreach (var item in _list)
			{
				item.Selected -= OnSelected;
			}
			_list.Clear();
			Clear();
		}

		public void ApplyIndexItemStyle(int index, string style)
		{
			if (-1 < index && index < _list.Count)
			{
				var item = _list[index];
				item.Style = style;
			}
		}

		void OnSelected(object sender, EventArgs e)
		{
			var index = _list.IndexOf((IndexItem)sender);
			SelectedIndex?.Invoke(this, new SelectedPositionChangedEventArgs(index));
			UpdateSelectedIndex(index);
		}
	}
}
