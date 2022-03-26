using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using log4net;

namespace NMAP
{
	public class SequentialScanner : IPScanner
	{
		protected virtual ILog log => LogManager.GetLogger(typeof(SequentialScanner));

		public virtual Task Scan(IPAddress[] ipAddrs, int[] ports)
		{
			return Task.Run(() =>
			{
				foreach(var ipAddr in ipAddrs)
                {
                    ProcessIpAddr(ports, ipAddr);
                }
			});
		}

        private void ProcessIpAddr(int[] ports, IPAddress ipAddr)
        {
            if(PingAddr(ipAddr) != IPStatus.Success)
                return;

            foreach(var port in ports)
                CheckPort(ipAddr, port);
        }

        protected IPStatus PingAddr(IPAddress ipAddr, int timeout = 3000)
		{
			log.Info($"Pinging {ipAddr}");
			using(var ping = new Ping())
			{
				var status = ping.Send(ipAddr, timeout).Status;
				log.Info($"Pinged {ipAddr}: {status}");
				return status;
			}
		}

		protected void CheckPort(IPAddress ipAddr, int port, int timeout = 3000)
		{
			using(var tcpClient = new TcpClient())
			{
				log.Info($"Checking {ipAddr}:{port}");

				var connectStatus = tcpClient.ConnectWithTimeout(ipAddr, port, timeout);
				PortStatus portStatus;
				switch(connectStatus)
				{
					case TaskStatus.RanToCompletion:
						portStatus = PortStatus.OPEN;
						break;
					case TaskStatus.Faulted:
						portStatus = PortStatus.CLOSED;
						break;
					default:
						portStatus = PortStatus.FILTERED;
						break;
				}
				log.Info($"Checked {ipAddr}:{port} - {portStatus}");
			}
		}
	}
}