using System;
using System.ComponentModel;

namespace WPG.Data
{
    public abstract class Item : INotifyPropertyChanged, IDisposable
	{
		#region Notify Property Changed Members

		protected void NotifyPropertyChanged(string property)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(property));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#region IDisposable Members

		private bool _disposed = false;

		protected bool Disposed
		{
			get { return _disposed; }
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!Disposed)
			{
				_disposed = true;				
			}			
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~Item() { Dispose(false); }

		#endregion
	}
}
