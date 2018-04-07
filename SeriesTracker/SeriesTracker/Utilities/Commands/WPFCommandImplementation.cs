using System;
using System.Windows.Input;

namespace SeriesTracker.Utilities.Commands
{
	public class WPFCommandImplementation : ICommand
	{
		private readonly Action<object> _execute;
		private readonly Func<object, bool> _canExecute;

		public WPFCommandImplementation(Action<object> execute) : this(execute, null)
		{

		}

		public WPFCommandImplementation(Action<object> execute, Func<object, bool> canExecute)
		{
			_execute = execute ?? throw new ArgumentNullException(nameof(execute));
			_canExecute = canExecute ?? (x => true);
		}

		public bool CanExecute(object parameter)
		{
			return _canExecute(parameter);
		}

		public void Execute(object parameter)
		{
			_execute(parameter);
		}

		public event EventHandler CanExecuteChanged
		{
			add
			{
				CommandManager.RequerySuggested += value;
			}
			remove
			{
				CommandManager.RequerySuggested -= value;
			}
		}

		public void Refresh()
		{
			CommandManager.InvalidateRequerySuggested();
		}
	}
}
