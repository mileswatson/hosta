﻿using ClientWPF.Models.Data;
using Hosta.RPC;
using System;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using static ClientWPF.ApplicationEnvironment;
using static ClientWPF.Models.ResourceManager;

namespace ClientWPF.ViewModels.ProfileTab
{
	public class EditViewModel : ObservableObject
	{
		public ICommand Save { get; private set; }
		public ICommand CancelEditing { get; private set; }

		private string _name = "";

		public string Name
		{
			get => _name;
			set
			{
				_name = value;
				NotifyPropertyChanged(nameof(Name));
			}
		}

		private string _tagline = "";

		public string Tagline
		{
			get => _tagline;
			set
			{
				_tagline = value;
				NotifyPropertyChanged(nameof(Tagline));
			}
		}

		private string _bio = "";

		public string Bio
		{
			get => _bio;
			set
			{
				_bio = value;
				NotifyPropertyChanged(nameof(Bio));
			}
		}

		public BitmapImage Avatar
		{
			get => DefaultImage;
		}

		public EditViewModel(Action<bool> OnDone, Profile profile)
		{
			Name = profile.Name;
			Tagline = profile.Tagline;
			Bio = profile.Bio;
			Save = new RelayCommand(async (object? _) =>
			{
				try
				{
					await Resources!.SetProfile(Name, Tagline, Bio, "");
					OnDone(true);
				}
				catch (RPException e)
				{
					Env.Alert($"Could not update profile! {e.Message}");
				}
				catch
				{
					Env.Alert("Could not update profile!");
				}
			});
			CancelEditing = new RelayCommand((object? _) => OnDone(false));
		}

		public override void Update(bool force)
		{
		}
	}
}