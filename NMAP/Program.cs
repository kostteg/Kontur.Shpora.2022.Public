using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using log4net;
using log4net.Config;

namespace NMAP
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            XmlConfigurator.Configure(LogManager.GetRepository(Assembly.GetCallingAssembly()), new FileInfo("log4net.config"));


            var ipAddrs = GenIpAddrs();
            var ports = new[] {21, 25, 80, 443, 3389};

            var scanner = new SequentialScanner();
            scanner.Scan(ipAddrs, ports).Wait();
        }

        private static IPAddress[] GenIpAddrs()
        {
            var urguAddrs = new List<IPAddress>();
            uint urguOrgIp = 0xFE44C1D4;
            for(int b = 0; b <= byte.MaxValue; b++)
                urguAddrs.Add(new IPAddress((urguOrgIp & 0x00FFFFFF) | (uint)b << 24));
            return urguAddrs.ToArray();
        }
    }
}