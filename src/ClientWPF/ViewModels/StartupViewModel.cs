﻿using System;
using System.ComponentModel;
using System.Windows.Input;

namespace ClientWPF.ViewModels
{
	public class StartupViewModel : INotifyPropertyChanged
	{
		public ICommand Continue { get; private set; }
		public ICommand Quit { get; private set; }

		public StartupViewModel(Action OnContinue, Action OnQuit)
		{
			Continue = new RelayCommand((object? _) => OnContinue());
			Quit = new RelayCommand((object? _) => OnQuit());
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		public void NotifyPropertyChanged(string propertyName)
		{
			if (PropertyChanged is not null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}