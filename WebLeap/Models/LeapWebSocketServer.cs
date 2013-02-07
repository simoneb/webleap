using System;
using System.Collections.Concurrent;
using System.Linq;
using Fleck;

namespace WebLeap.Models
{
	class LeapWebSocketServer
	{
		private static readonly ConcurrentDictionary<Guid, IWebSocketConnection> Clients = new ConcurrentDictionary<Guid, IWebSocketConnection>();

		public static WebSocketServer Start(string host)
		{
			var server = new WebSocketServer(new UriBuilder("ws", host, 32165, "webleap").Uri.ToString());

			server.Start(AcceptClient);

			return server;
		}

		private static void AcceptClient(IWebSocketConnection connection)
		{
			Console.WriteLine("Client connected {0}", connection.ConnectionInfo.Id);
			Clients.TryAdd(connection.ConnectionInfo.Id, connection);
			connection.OnMessage = HandleMessage;
			connection.OnClose = RemoveClient(connection);
		}

		private static Action RemoveClient(IWebSocketConnection connection)
		{
			return () =>
			{
				Console.WriteLine("Client disconnected {0}", connection.ConnectionInfo.Id);
				IWebSocketConnection _;
				Clients.TryRemove(connection.ConnectionInfo.Id, out _);
			};
		}

		private static void HandleMessage(string message)
		{
			var clientsCopy = Clients.Values.ToList();

			foreach (var client in clientsCopy)
				client.Send(message);
		}
	}
}