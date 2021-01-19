﻿using Hosta.API.Post;
using Hosta.Crypto;
using Hosta.API;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Node.Posts
{
	internal class PostHandler
	{
		private readonly SQLiteAsyncConnection conn;

		private readonly string self;

		private PostHandler(SQLiteAsyncConnection conn, string self)
		{
			this.conn = conn;
			this.self = self;
		}

		public static async Task<PostHandler> Create(SQLiteAsyncConnection conn, string self)
		{
			await conn.CreateTableAsync<Post>();
			return new PostHandler(conn, self);
		}

		public async Task<string> Add(AddPostRequest request, PublicIdentity client)
		{
			if (client.ID != self)
			{
				throw new APIException("Access denied.");
			}
			var post = Post.FromAddRequest(request);
			var num = await conn.InsertAsync(post);
			if (num == 0) throw new APIException("Image could not be found!");
			return post.ID;
		}

		public async Task<GetPostResponse> Get(string id, PublicIdentity client)
		{
			var post = await conn.GetAsync<Post>(id);
			return post.ToResponse();
		}

		public async Task<List<PostInfo>> GetList(DateTime start, PublicIdentity _)
		{
			var posts = await conn.Table<Post>().Where(x => start < x.TimePosted).ToListAsync();
			return posts.Select(x => new PostInfo { ID = x.ID, TimePosted = x.TimePosted }).ToList();
		}

		public async Task RemovePost(string id, PublicIdentity client)
		{
			if (client.ID != self)
			{
				throw new APIException("Access denied.");
			}
			var num = await conn.DeleteAsync<Post>(id);
			if (num == 0) throw new APIException("Image could not be found!");
		}
	}
}