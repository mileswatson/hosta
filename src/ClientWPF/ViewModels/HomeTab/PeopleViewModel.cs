﻿using Hosta.API;
using Hosta.API.Friend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static ClientWPF.ApplicationEnvironment;
using static ClientWPF.Models.ResourceManager;

namespace ClientWPF.ViewModels.HomeTab
{
	public class PeopleViewModel : ObservableObject
	{
		public FriendViewModel Self { get; init; }

		private List<FriendViewModel> _bestfriends = new();

		public List<FriendViewModel> Favorites
		{
			get => _bestfriends;
			private set
			{
				_bestfriends = value;
				NotifyPropertyChanged(nameof(Favorites));
			}
		}

		private List<FriendViewModel> _friends = new();

		public List<FriendViewModel> Friends
		{
			get => _friends;
			private set
			{
				_friends = value;
				NotifyPropertyChanged(nameof(Friends));
			}
		}

		public PeopleViewModel()
		{
			Self = new FriendViewModel(new FriendInfo
			{
				ID = Resources!.Self,
				Name = "You",
				IsFavorite = true
			}, new());
		}

		public void MakeFavorite(FriendViewModel? friend) => SetFavoriteStatus(friend, true);

		public void Unfavorite(FriendViewModel? friend) => SetFavoriteStatus(friend, false);

		public async void SetFavoriteStatus(FriendViewModel? friend, bool isFavorite)
		{
			if (friend is null) throw new NullReferenceException();
			try
			{
				await Resources!.SetFriend(friend.ID, friend.Name, isFavorite);
				Env.Alert("Changed favorite status.");
			}
			catch (APIException e)
			{
				Env.Alert($"Could not change favorite status! {e.Message}");
			}
			catch
			{
				Env.Alert("Could not change favorite status!");
			}
			Update(false);
		}

		public async void RemoveFriend(FriendViewModel? friend)
		{
			if (friend is null) throw new NullReferenceException();
			try
			{
				await Resources!.RemoveFriend(friend.ID);
				Env.Alert("Removed friend.");
			}
			catch (APIException e)
			{
				Env.Alert($"Could not remove friend! {e.Message}");
			}
			catch
			{
				Env.Alert("Could not remove friend!");
			}
			Update(false);
		}

		public override async Task UpdateAsync(bool force)
		{
			Self.Update(force);

			var response = await Resources!.GetFriendList();

			response.Sort(Compare);

			var friendMenuItems = new List<ContextMenuItem<FriendViewModel>>
			{
				new("Make favorite", MakeFavorite),
				new("Remove friend", RemoveFriend)
			};

			var friends = response.Select(info => new FriendViewModel(
				info,
				friendMenuItems
			)).ToList();

			var favorites = friends.Where(friend =>
			{
				if (!friend.IsFavorite) return false;
				friend.MenuItems = new() { new("Remove favorite", Unfavorite) };
				return true;
			}).ToList();

			Favorites = favorites;
			Friends = friends;

			foreach (var friend in Friends)
			{
				friend.Update(force);
			}
			foreach (var friend in Favorites)
			{
				friend.Update(force);
			}
		}

		private int Compare(FriendInfo x, FriendInfo y)
		{
			return StringComparer.InvariantCulture.Compare(x.Name, y.Name);
		}
	}
}