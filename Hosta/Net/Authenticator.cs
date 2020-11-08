﻿using Hosta.Crypto;
using Hosta.Tools;
using System;
using System.Threading.Tasks;

namespace Hosta.Net
{
	public class Authenticator : IDisposable
	{
		private PrivateIdentity self;

		public Authenticator(PrivateIdentity self)
		{
			this.self = self;
		}

		public async Task<AuthenticatedMessenger> AuthenticateClient(ProtectedMessenger protectedMessenger)
		{
			ThrowIfDisposed();

			// Throw if client has the wrong address.
			var wantedID = Transcoder.TextFromBytes(await protectedMessenger.Receive());
			if (wantedID != self.ID) throw new Exception("Wrong address!");

			// Send the public key and the signature.
			await Task.WhenAll(
				protectedMessenger.Send(self.PublicIdentityInfo),
				protectedMessenger.Send(self.Sign(protectedMessenger.ID))
			);

			// Gets the client public key
			var pkInfo = await protectedMessenger.Receive();
			var clientIdentity = new PublicIdentity(pkInfo);

			// Verifies the client's signature.
			if (!clientIdentity.Verify(protectedMessenger.ID, await protectedMessenger.Receive()))
				throw new Exception("Could not authenticate session!");

			// Returns client ID and secureMessenger
			return new AuthenticatedMessenger(protectedMessenger, clientIdentity);
		}

		public async Task<AuthenticatedMessenger> AuthenticateServer(ProtectedMessenger protectedMessenger, string serverID)
		{
			ThrowIfDisposed();

			// Initialise connection attempt.
			await protectedMessenger.Send(Transcoder.BytesFromText(serverID));

			// Receive the server's public key.
			var pkInfo = await protectedMessenger.Receive();
			PublicIdentity serverIdentity;
			serverIdentity = new PublicIdentity(pkInfo);

			// Checks that the server has the correct ID.
			if (serverIdentity.ID != serverID) throw new Exception("Wrong address!");

			// Verifies the server's signature.
			var signature = await protectedMessenger.Receive();
			if (!serverIdentity.Verify(protectedMessenger.ID, signature)) throw new Exception("Could not authenticate session!");

			// Send the public key and the signature.
			await Task.WhenAll(
				protectedMessenger.Send(self.PublicIdentityInfo),
				protectedMessenger.Send(self.Sign(protectedMessenger.ID))
			);

			return new AuthenticatedMessenger(protectedMessenger, serverIdentity);
		}

		//// Implements IDisposable

		private bool disposed = false;

		private void ThrowIfDisposed()
		{
			if (disposed) throw new ObjectDisposedException("Attempted post-disposal use!");
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed) return;

			if (disposing)
			{
				self = null;
				disposed = true;
			}
		}
	}
}