/*

	NetworkOperation.cs

	(c) Copyright 2010-2011, Bret Ambrose (mailto:bretambrose@gmail.com).

	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program.  If not, see <http://www.gnu.org/licenses/>.
 
*/

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
