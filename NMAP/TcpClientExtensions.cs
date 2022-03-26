using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NMAP
{
    public static class TcpClientExtensions
    {
        public static TaskStatus ConnectWithTimeout(this TcpClient tcpClient, IPAddress ipAddr, int port, int timeout = 3000)
        {
            var connectTask = tcpClient.ConnectAsync(ipAddr, port);
            Task.WaitAny(connectTask, Task.Delay(timeout));
            return connectTask.Status;
        }

		public static async Task<TaskStatus> ConnectWithTimeoutAsync(this TcpClient tcpClient, IPAddress ipAddr, int port, int timeout = 3000)
        {
            var connectTask = tcpClient.ConnectAsync(ipAddr, port);
            await Task.WhenAny(connectTask, Task.Delay(timeout));
            return connectTask.Status;
        }
    }
}