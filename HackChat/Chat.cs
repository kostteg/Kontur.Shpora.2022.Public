using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace HackChat
{
	public class Chat
	{
		public const int DefaultPort = 31337;

		private readonly byte[] PingMsg = new byte[1];
		private readonly ConcurrentDictionary<IPEndPoint, (TcpClient Client, NetworkStream Stream)> OutboundConnections = new();

		private readonly int port;
		private readonly TcpListener tcpListener;

		public Chat(int port) => tcpListener = new TcpListener(IPAddress.Any, this.port = port);

		public void Start()
		{
            tcpListener.Start(100500);
			Task.Factory.StartNew(() =>
			{
				while(true)
				{
					var tcpClient = tcpListener.AcceptTcpClient();
					ConsoleWriteLineAsync($"[{tcpClient.Client.RemoteEndPoint}] connected", ConsoleColor.Yellow);
					Task.Run(() => ProcessInboundConnectionsAsync(tcpClient));
				}
			}, TaskCreationOptions.LongRunning);

			Task.Factory.StartNew(DiscoverLoop, TaskCreationOptions.LongRunning);

			Task.Factory.StartNew(() =>
			{
				string line;
				while ((line = Console.ReadLine()) != null)
					Task.Run(() => BroadcastAsync(line));
			}, TaskCreationOptions.LongRunning);
		}

        private async Task ProcessInboundConnectionsAsync(TcpClient tcpClient)
        {
            EndPoint endpoint = null;
            try { endpoint = tcpClient.Client.RemoteEndPoint; } catch { /* ignored */ }

            try
            {
                using (tcpClient)
                {
                    var stream = tcpClient.GetStream();
                    await ReadLinesToConsoleAsync(stream);
                }
            }
            catch { /* ignored */ }
            await ConsoleWriteLineAsync($"[{endpoint}] disconnected", ConsoleColor.DarkRed);
        }

        private async Task ReadLinesToConsoleAsync(Stream stream)
        {
            string line;
            using var sr = new StreamReader(stream);
            while ((line = await sr.ReadLineAsync()) != null)
                await ConsoleWriteLineAsync($"[{((NetworkStream)stream).Socket.RemoteEndPoint}] {line}");
        }


        private async void DiscoverLoop()
		{
			while(true)
			{
				try { await Discover(); } catch { /* ignored */ }
				await Task.Delay(1000);
			}
		}

        private async Task Discover()
		{
			OutboundConnections.Where(pair => !pair.Value.Client.Client.Connected).ForEach(pair =>
			{
				try { pair.Value.Client.Dispose(); } catch { /* ignored */ }
				ConsoleWriteLineAsync($"[ME] disconnected from {pair.Key}", ConsoleColor.DarkRed).Wait();
				OutboundConnections.TryRemove(pair);
			});

            var myAddresses = await GetMyAddresses();
			var nearbyAddresses = await GetNearbyAddresses(myAddresses);

			throw new NotImplementedException();
        }

        private async Task<IEnumerable<IPAddress>> GetNearbyAddresses(IPAddress[] myAddresses)
        {
            return myAddresses.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                .SelectMany(CalcNearbyIPAddresses);
        }

        private static async Task<IPAddress[]> GetMyAddresses()
        {
            return (await Dns.GetHostEntryAsync(Dns.GetHostName())).AddressList;
        }

        private IEnumerable<IPAddress> CalcNearbyIPAddresses(IPAddress ip)
        {
            yield return IPAddress.Parse("127.0.0.1");
            yield break;
            var bytes = ip.GetAddressBytes();
            for (int i = 0; i < 256; i++)
            {
                bytes[3] = (byte)i;
                yield return new IPAddress(bytes);
            }
        }

        

        private async Task BroadcastAsync(string message)
        {
	        throw new NotImplementedException();
        }


        private SemaphoreSlim consoleSemaphore = new SemaphoreSlim(1, 1);
        private async Task ConsoleWriteLineAsync(string str, ConsoleColor color = ConsoleColor.Gray)
        {
            await consoleSemaphore.WaitAsync();
            try
            {
                Console.ForegroundColor = color;
                await Console.Out.WriteLineAsync(str);
                Console.ResetColor();
            }
            finally
            {
                consoleSemaphore.Release();
            }
        }
	}
}