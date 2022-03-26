using System;
using System.Threading.Tasks;
using ClusterClient.Clients;
using log4net;

namespace ClusterTests;

public class RoundRobinClusterClient : ClusterClientBase
{
	public RoundRobinClusterClient(string[] replicaAddresses)
		: base(replicaAddresses)
	{
		throw new NotImplementedException();
	}

	public override Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
	{
		throw new NotImplementedException();
	}

	protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
}