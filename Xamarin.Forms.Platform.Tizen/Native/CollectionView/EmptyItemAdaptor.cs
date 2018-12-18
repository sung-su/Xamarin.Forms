using System.Collections;

using ESize = ElmSharp.Size;
namespace Xamarin.Forms.Platform.Tizen.Native
{
	public class EmptyItemAdaptor : ItemTemplateAdaptor
	{
		public EmptyItemAdaptor(ItemsView itemsView, IEnumerable items, DataTemplate template) : base(itemsView, items, template)
		{
		}

		public override ElmSharp.Size MeasureItem(int widthConstraint, int heightConstraint)
		{
			return new ESize(widthConstraint, heightConstraint);
		}

	}
}