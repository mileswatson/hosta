﻿using Hosta.API.Image;
using Hosta.Crypto;
using Hosta.RPC;
using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Node.Images
{
	internal class ImageHandler
	{
		private readonly SQLiteAsyncConnection conn;

		private readonly string self;

		private ImageHandler(SQLiteAsyncConnection conn, string self)
		{
			this.conn = conn;
			this.self = self;
		}

		public static async Task<ImageHandler> CreateAndInit(SQLiteAsyncConnection conn, string self)
		{
			await conn.CreateTableAsync<Image>();
			return new ImageHandler(conn, self);
		}

		public async Task<string> Add(AddImageRequest request, PublicIdentity client)
		{
			if (client.ID != self)
			{
				throw new RPException("Access denied.");
			}

			var resource = Image.FromAddRequest(request);
			await conn.InsertOrReplaceAsync(resource);
			return resource.Hash;
		}

		public async Task<GetImageResponse> Get(string hash, PublicIdentity _)
		{
			var p = await conn.GetAsync<Image>(hash);
			return p.ToResponse();
		}

		public async Task<List<ImageInfo>> GetList(PublicIdentity client)
		{
			if (client.ID != self)
			{
				throw new RPException("Access denied.");
			}

			var images = await conn.Table<Image>().ToListAsync();
			List<ImageInfo> info = new();
			foreach (var image in images)
			{
				info.Add(new ImageInfo
				{
					Hash = image.Hash,
					LastUpdated = image.LastUpdated
				});
			}
			return info;
		}

		public async Task Remove(string hash, PublicIdentity client)
		{
			if (client.ID != self)
			{
				throw new RPException("Access denined.");
			}
			var num = await conn.DeleteAsync<Image>(hash);
			if (num == 0) throw new RPException("Image could not be found!");
		}
	}
}