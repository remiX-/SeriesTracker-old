using SeriesTracker.Models;
using System.Collections;
using System.ComponentModel;

namespace SeriesTracker.Comparers
{
	public abstract class DefComparer : IComparer
	{
		public ListSortDirection Direction { get; }

		public DefComparer(ListSortDirection dir)
		{
			Direction = dir;
		}

		public abstract int Compare(object x, object y);

		public int Finish(Show x, Show y, int r)
		{
			if (r == 0)
				return string.Compare(x.DisplayName, y.DisplayName);

			if (Direction == ListSortDirection.Descending)
				return r * -1;

			return r;
		}
	}

	public sealed class StatusComparer : DefComparer
	{
		public StatusComparer(ListSortDirection dir) : base(dir)
		{

		}

		public override int Compare(object x, object y)
		{
			Show _x = (Show)x;
			Show _y = (Show)y;

			int r = 0;

			if (_x.Status == _y.Status)
				r = 0;
			else
				r = string.Compare(_x.Status, _y.Status);

			return Finish(_x, _y, r);
		}
	}
}
