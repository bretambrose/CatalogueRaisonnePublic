/*

	ClientNetworkThread.cs

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
using System.Collections.Generic;
using System.Threading;

using CRShared;

namespace LCClient
{	
	public class CClientNetworkThread : CNetworkThreadBase
	{	
		// embedded classes
		private class CClientConnection : CConnection
		{
			// Construction
			public CClientConnection() :
				base( ESessionID.First )
			{
			}

			// Methods
			// Public interface
			public void Initialize( IPEndPoint server_address )
			{
				ServerAddress = server_address;
				
				var socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
				socket.Connect( ServerAddress );
				
				Initialize( socket );
			}
			
			// Protected interface and overrides
			protected override bool Handle_Message( CNetworkMessage message )
			{
				bool handled = base.Handle_Message( message );
				if ( handled )
				{
					return true;
				}

				switch ( message.MessageType )
				{			
					case ENetworkMessageType.Message_Disconnect_Result:
						IsFinished = true;
						return true;
						
					case ENetworkMessageType.Message_Client_Hello_Response:
						CClientHelloResponse response = message as CClientHelloResponse;
						CClientNetworkThread.Instance.On_Message_Receive( message, ID );

						if ( response.Reason != EConnectRefusalReason.None )
						{
							IsFinished = true;
						}
						return true;

					case ENetworkMessageType.Message_Connection_Dropped:
						CClientNetworkThread.Instance.On_Message_Receive( message, ID );
						IsFinished = true;
						return true;
							
					default:
						CClientNetworkThread.Instance.On_Message_Receive( message, ID );
						return true;
				}			
			}
						
			// Fields			
			private IPEndPoint ServerAddress = null;
		}	
		
		// Construction
		private CClientNetworkThread() :
			base()
		{}
		
		static CClientNetworkThread() {}

		// Methods
		// Public interface
		public static Thread Allocate_Thread()
		{
			return new Thread( Instance.Start );
		}
		
		// Protected interface and overrides		
		protected override void Initialize()
		{
			base.Initialize();
			base.Initialize_Time_Keeper( TimeKeeperExecutionMode.Internal, TimeKeeperTimeUnit.RealTime, TimeKeeperAuthority.Master, NETWORK_SERVICE_INTERVAL, true, false );
		}		
		
		protected override bool Handle_Operation( CNetworkOperation operation )
		{
			bool handled = base.Handle_Operation( operation );
			if ( handled )
			{
				return true;
			}

			switch ( operation.NetworkOp )
			{
				case ENetworkOperation.Connect:
					Handle_Connect_Request( operation as CConnectRequestOperation );
					return true;
					
				case ENetworkOperation.Disconnect:
					Handle_Disconnect_Request( operation as CDisconnectRequestOperation );
					return true;
					
				default:
					return false;				
			}
		}
		
		// Private interface
		private void Handle_Connect_Request( CConnectRequestOperation operation )
		{
			if ( ClientConnection != null )
			{
				return;
			}
										
			Add_Connection( new CClientConnection() );
					
			try
			{
				ClientConnection.Initialize( operation.Endpoint );
			}
			catch	// swallow all exceptions; the finally block will cleanup
			{
			}
			finally
			{
				if ( !ClientConnection.Connected )
				{
					Dispose_And_Remove_Connection( ClientConnection.ID );						
					Add_Event( ENetworkEvent.Connection_Failure, operation.ID );
				}
				else
				{
					Send_Message( new CWrappedNetworkMessage( new CClientHelloRequest( operation.Name ), operation.ID ) );
					Add_Event( ENetworkEvent.Connection_Success, operation.ID );
				}
			}
		}

		private void Handle_Disconnect_Request( CDisconnectRequestOperation operation )
		{
			if ( ClientConnection != null )
			{
				Send_Message( new CWrappedNetworkMessage( new CDisconnectRequestMessage(), operation.ID ) );
			}
			else
			{
				Add_Event( ENetworkEvent.Disconnection, ESessionID.First );
			}
		}

		// Properties
		public static CClientNetworkThread Instance { get { return m_Instance; } }
		protected override bool NotifyOnSendFailure { get { return true; } }
		private CClientConnection ClientConnection { get { return Get_Connection( ESessionID.First ) as CClientConnection; } }
		
		// Fields
		private static CClientNetworkThread m_Instance = new CClientNetworkThread();
				
		private const int NETWORK_SERVICE_INTERVAL = 20;
	}
	

}