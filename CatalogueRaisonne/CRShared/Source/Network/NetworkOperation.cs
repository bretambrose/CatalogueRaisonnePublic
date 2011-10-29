using System;
using System.Net;
using System.Net.Sockets;

using CRShared;

namespace CRShared
{
	public enum ENetworkOperation
	{
		Connect,
		Disconnect
	}

	public class CNetworkOperation
	{
		// Construction
		public CNetworkOperation( ENetworkOperation op, ESessionID id ) 
		{
			NetworkOp = op;
			ID = id;
		}
		
		// Properties
		public ENetworkOperation NetworkOp { get; private set; }
		public ESessionID ID { get; private set; } 
	}
	
	public class CConnectRequestOperation : CNetworkOperation
	{
		// Construction
		public CConnectRequestOperation( IPEndPoint connection_endpoint, ESessionID id, string name ) :
			base( ENetworkOperation.Connect, id )
		{
			Endpoint = connection_endpoint;
			Name = name;
		}
		
		// Properties
		public IPEndPoint Endpoint { get; private set; }
		public string Name { get; private set; }
	}
	
	public enum EDisconnectReason
	{
		Invalid = 0,

		Client_Request,
		Client_Request_Quit,
		Server_Request,
		Timeout,
		Unknown
	}
	
	public class CDisconnectRequestOperation : CNetworkOperation
	{
		// Construction
		public CDisconnectRequestOperation( ESessionID id, EDisconnectReason reason ) :
			base( ENetworkOperation.Disconnect, id )
		{
			Reason = reason;
		}
		
		// Properties
		public EDisconnectReason Reason { get; private set; }
	}
}
