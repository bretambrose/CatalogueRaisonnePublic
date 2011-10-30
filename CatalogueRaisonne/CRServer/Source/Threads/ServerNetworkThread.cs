/*

	ServerNetworkThread.cs

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

namespace CRServer
{	

	public class CServerNetworkThread : CNetworkThreadBase
	{	
		// embedded classes
		private class CServerConnection : CConnection
		{
			// Construction
			public CServerConnection( ESessionID id ) :
				base( id )
			{
			}
			
			// Methods
			// Public interface
			public void Set_Pending_Disconnect( EDisconnectReason reason )
			{
				if ( PendingDisconnectReason != EDisconnectReason.Invalid )
				{
					return;
				}
				
				PendingDisconnectReason = reason;
				Send_Message( new CDisconnectResultMessage( reason ) );
			}

			// Protected interface and overrides
			protected override void Dispose( bool disposing )
			{
				base.Dispose( disposing );
			}
			
			protected override bool Handle_Message( CNetworkMessage message )
			{
				bool handled = base.Handle_Message( message );
				if ( handled )
				{
					return true;
				}
						
				CServerNetworkThread.Instance.On_Message_Receive( message, ID );
				return true;								
			}
						
			// Fields
		}	
					
		// Construction
		private CServerNetworkThread() :
			base()
		{}
		
		static CServerNetworkThread() {}

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
			
			m_ConnectionListener = new TcpListener( IPAddress.Any, CSharedSettings.Instance.ListenerPort );
			m_ConnectionListener.Start();				
		}
	
		protected override void Shutdown()
		{
			base.Shutdown();
			
			if ( m_ConnectionListener != null )
			{
				m_ConnectionListener.Stop();
				m_ConnectionListener = null;
			}
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
				case ENetworkOperation.Disconnect:
					CDisconnectRequestOperation disconnect_op = operation as CDisconnectRequestOperation;
					CServerConnection connection = Get_Connection( operation.ID ) as CServerConnection;
					if ( connection != null )
					{
						connection.Set_Pending_Disconnect( disconnect_op.Reason );
					}
					return true;
				
				default:
					return false;				
			}
		}
				
		protected override void Service_Connections()
		{
			while ( m_ConnectionListener.Pending() )
			{
				Socket socket = m_ConnectionListener.AcceptSocket();
				ESessionID id = Allocate_Connection_ID();
				
				CServerConnection connection = new CServerConnection( id );
				connection.Initialize( socket );
				
				Add_Connection( connection );				
				Add_Event( ENetworkEvent.Connection_Success, id );
			}

			base.Service_Connections();
		}
		
		// Private interface
		private ESessionID Allocate_Connection_ID()
		{
			ESessionID id = m_NextSessionID;
			m_NextSessionID++;
			
			return id;
		}
				
		// Properties
		public static CServerNetworkThread Instance { get { return m_Instance; } }
		
		// Fields
		private static CServerNetworkThread m_Instance = new CServerNetworkThread();
		private static ESessionID m_NextSessionID = ESessionID.First;
		
		private TcpListener m_ConnectionListener = null;	
		
		private const long PING_INTERVAL = 10000;
		private const int NETWORK_SERVICE_INTERVAL = 20;
	}
}