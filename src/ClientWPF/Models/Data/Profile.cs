﻿using Hosta.API.Data;
using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace ClientWPF.Models.Data
{
	public record Profile
	{
		public string DisplayName { get; init; }

		public string ID { get; init; }

		public string Tagline { get; init; }

		public string Bio { get; init; }

		public byte[] AvatarBytes { get; init; }

		public BitmapImage Avatar { get; init; }

		public DateTime LastUpdated { get; init; }

		public Profile()
		{
			DisplayName = "[Name]";
			ID = "[ID]";
			Tagline = "[Tagline]";
			Bio = "[Bio]";
			AvatarBytes = Array.Empty<byte>();
			Avatar = DefaultImage;
			LastUpdated = DateTime.MinValue;
		}

		public Profile(GetProfileResponse response, string id)
		{
			DisplayName = response.DisplayName;
			ID = id;
			Tagline = response.Tagline;
			Bio = response.Bio;
			LastUpdated = response.LastUpdated;
			AvatarBytes = response.Avatar;
			Avatar = TryImageFromBytes(AvatarBytes);
		}

		private static BitmapImage TryImageFromBytes(byte[] data)
		{
			try
			{
				return ImageFromBytes(data);
			}
			catch
			{
				return DefaultImage;
			}
		}

		public static BitmapImage ImageFromBytes(byte[] data)
		{
			var image = new BitmapImage();
			using (var stream = new MemoryStream(data, 0, data.Length))
			{
				image.BeginInit();
				image.DecodePixelWidth = 160;
				image.CacheOption = BitmapCacheOption.OnLoad;
				image.StreamSource = stream;
				image.EndInit();
			}
			return image;
		}

		static Profile()
		{
			var image = new BitmapImage();
			image.BeginInit();
			image.DecodePixelWidth = 160;
			image.CacheOption = BitmapCacheOption.OnLoad;
			image.UriSource = new Uri("Assets/Images/default-avatar.png", UriKind.Relative);
			image.EndInit();
			DefaultImage = image;
		}

		public static BitmapImage DefaultImage
		{
			get; private set;
		}
	}
}