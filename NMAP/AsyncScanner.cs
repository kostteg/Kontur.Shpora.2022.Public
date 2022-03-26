using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using log4net;

namespace NMAP
{
    public class AsyncScanner : IPScanner
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(AsyncScanner));

        public async Task Scan(IPAddress[] ipAddrs, int[] ports)
        {
	        throw new NotImplementedException();
        }
    }
}