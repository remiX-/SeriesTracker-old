using System;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;

namespace SeriesTracker.ViewModels
{
	internal class ViewShowMenuItem : BindableBase
	{
		private object _icon;
		private string _text;
		private DelegateCommand _command;
		private Uri _navigationDestination;

		public object Icon
		{
			get { return this._icon; }
			set { this.SetProperty(ref this._icon, value); }
		}

		public string Text
		{
			get { return this._text; }
			set { this.SetProperty(ref this._text, value); }
		}

		public ICommand Command
		{
			get { return this._command; }
			set { this.SetProperty(ref this._command, (DelegateCommand)value); }
		}

		public Uri NavigationDestination
		{
			get { return this._navigationDestination; }
			set { this.SetProperty(ref this._navigationDestination, value); }
		}

		public bool IsNavigation => this._navigationDestination != null;
	}
}