using System.Collections.ObjectModel;

namespace WPG.Data
{
    public abstract class CompositeItem : Item
	{
		#region Fields

		private readonly ObservableCollection<Item> _items = new ObservableCollection<Item>();

		#endregion

		#region Properties

		public ObservableCollection<Item> Items
		{
			get { return _items; }
		} 

		#endregion

		#region IDisposable Members

		protected override void Dispose(bool disposing)
		{
			if (Disposed)
			{
				return;
			}
			if (disposing)
			{
				foreach (Item item in Items)
				{
					item.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
