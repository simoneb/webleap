using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Fleck;

namespace WebLeap
{
	class Program
	{
		private const int HttpPort = 32165;
		private const int WebSocketPort = HttpPort + 1;
		private const string WebSocketHost = "localhost";
		private static readonly ConcurrentDictionary<Guid, IWebSocketConnection> Clients = new ConcurrentDictionary<Guid, IWebSocketConnection>();

		static void Main(string[] args)
		{
			using(StartHttpServer())
			using (StartFleck())
			{
				Console.WriteLine("Press Enter to quit");
				Console.ReadLine();
			}
		}

		private static IDisposable StartFleck()
		{
			var server = new WebSocketServer(WebSocketPort, string.Format("http://{0}:{1}/webleap", WebSocketHost, WebSocketPort));

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

		private static IDisposable StartHttpServer()
		{
			var listener = new HttpListener();
			listener.Prefixes.Add(string.Format("http://+:{0}/", HttpPort));

			listener.Start();

			Console.WriteLine("Started HTTP server on port {0}", HttpPort);

			listener.BeginGetContext(HandleHttpRequest, listener);

			return listener;
		}

		private static void HandleHttpRequest(IAsyncResult ar)
		{
			var listener = (HttpListener)ar.AsyncState;

			try
			{
				listener.BeginGetContext(HandleHttpRequest, listener);
			}
			catch (ObjectDisposedException e)
			{
				// this handler might have executed because the listener was closed
			}

			var client = listener.EndGetContext(ar);

			client.Response.ContentType = "text/html";

			using (var writer = new StreamWriter(client.Response.OutputStream, Encoding.UTF8))
				writer.Write(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "index.html")));

			client.Response.Close();
		}
	}
}