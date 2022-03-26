using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using log4net;

namespace NMAP
{
	public class TPLScanner : SequentialScanner
	{
		protected override ILog log => LogManager.GetLogger(typeof(TPLScanner));

		public override Task Scan(IPAddress[] ipAddrs, int[] ports)
		{
			throw new NotImplementedException();
		}
	}
}