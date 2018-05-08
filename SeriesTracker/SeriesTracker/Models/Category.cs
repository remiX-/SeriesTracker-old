using GalaSoft.MvvmLight;
using System.ComponentModel;

namespace SeriesTracker.Models
{
	public class Category : ViewModelBase
	{
		#region Variables
		private bool isChecked;

		public int CategoryID { get; set; }
		public string Name { get; set; }

		public int UserShowCategoryID { get; set; }

		public bool IsChecked
		{
			get => isChecked;
			set => Set(ref isChecked, value);
		}
		#endregion

		public Category()
		{
			CategoryID = -1;
		}

		public Category(string name) : this()
		{
			Name = name;
		}

		public Category(int id, string name)
		{
			CategoryID = id;
			Name = name;
		}

		public override bool Equals(object obj)
		{
			return obj is Category compareTo
				? CategoryID == compareTo.CategoryID && Name == compareTo.Name
				: base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
