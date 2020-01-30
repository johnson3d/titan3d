namespace WPG.Data
{
    public class PropertyCategory : PropertyCollection
	{
		#region Initialization

		public PropertyCategory()
		{
			this._categoryName = "Misc";
		}

		public PropertyCategory(string categoryName)
		{
			this._categoryName = categoryName;
		}

		public string Category
		{
			get { return _categoryName; }
		}

		#endregion

		#region Fields

		private readonly string _categoryName; 

		#endregion
	}
}
