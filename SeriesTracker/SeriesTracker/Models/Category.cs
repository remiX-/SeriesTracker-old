using System.ComponentModel;

namespace SeriesTracker.Models
{
	public class Category : INotifyPropertyChanged
	{
		public int CategoryID { get; set; }
		public string Name { get; set; }

		public int UserShowCategoryID { get; set; }

		private bool isChecked;
		public bool IsChecked
		{
			get { return isChecked; }
			set
			{
				isChecked = value;

				//RaisePropertyChanged("IsChecked");
			}
		}

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

		public event PropertyChangedEventHandler PropertyChanged;
		protected void RaisePropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public override bool Equals(object obj)
		{
			return obj is Category ? CategoryID == (obj as Category).CategoryID : base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
