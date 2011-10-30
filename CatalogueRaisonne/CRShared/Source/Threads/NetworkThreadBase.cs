/*

	NetworkThreadBase.cs

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
using System.Collections.Generic;

namespace CRShared
{
	public class CNetworkThreadBase : CTimeKeeperThread
	{
		// Local classes
		public class CPingConnectionsTask : CRecurrentTask
		{	
			// Construction
			public CPingConnectionsTask( long recurrent_time ) :
				base( 0, recurrent_time )
			{
			}
			
			// Methods
			public override void Execute( long current_time ) 
			{
				CNetworkThreadBase.BaseInstance.Ping_Connections();
			}
		}

		// Construction
		protected CNetworkThreadBase() :
			base()
		{
			m_BaseInstance = this;
		}

		// Methods
		// Public interface
		public void On_Disconnection( ESessionID id, EDisconnectReason reason )
		{
			Add_Event( ENetworkEvent.Disconnection, id );
		}

		public void On_Message_Receive( CNetworkMessage message, ESessionID id )
		{
			CLog.Log( ELoggingChannel.Network, ELogLevel.Medium, String.Format( "Received network message {0}", message.GetType().Name ) );
			CLog.Log( ELoggingChannel.Network, ELogLevel.High, String.Format( "		Internal state: {0}", message.ToString() ) );

			m_RequestResponseManager.Process_Incoming_Message( message );
			m_CurrentFrame.Add_Message( message, id );
		}

		public override void Send_Message( CNetworkMessage message, ESessionID destination_id ) 
		{
			Send_Message( new CWrappedNetworkMessage( message, destination_id ) );
		}

		// Protected interface and overrides
		protected override void Initialize()
		{
			base.Initialize();

			m_DataInterface = CNetworkFrameManager.Instance.NetworkInterface;	
			TaskScheduler.Add_Scheduled_Task( new CPingConnectionsTask( PING_INTERVAL ) );
		}
		
		protected override void Shutdown()
		{
			base.Shutdown();

			m_Connections.Apply( connection_pair => connection_pair.Value.Dispose() );	
			m_Connections.Clear();
		}
				
		protected override void Flush_Data_Frames( long current_time )
		{
			base.Flush_Data_Frames( current_time );

			if ( !m_CurrentFrame.Empty )
			{
				m_DataInterface.Send( m_CurrentFrame );
				m_CurrentFrame = null;
			}			
		}
		
		protected override void Build_Thread_Frames()
		{
			base.Build_Thread_Frames();

			if ( m_CurrentFrame == null )
			{
				m_CurrentFrame = new CInboundNetworkFrame();
			}
		}

		protected override void Service( long current_time )
		{
			base.Service( current_time );
			
			// build inbound frame
			Service_Outbound_Network_Frames();
			Service_Connections();
			
			Update_Network_Statistics();		
		}

		protected virtual void Service_Connections()
		{		
			var finished_connections = new List< ESessionID >();
			foreach ( var connection in Connections )
			{
				connection.Service();
				
				if ( connection.IsFinished || connection.PendingDisconnectReason != EDisconnectReason.Invalid )
				{
					connection.Dispose();
					finished_connections.Add( connection.ID );
				}
			}
			
			foreach ( var conn_id in finished_connections )
			{
				CLog.Log( ELoggingChannel.Network, ELogLevel.Medium, String.Format( "Removing connection on session id {0}", (int) conn_id ) );

				m_Connections.Remove( conn_id );
			}
		}

		protected virtual bool Handle_Operation( CNetworkOperation operation )
		{
			CLog.Log( ELoggingChannel.Network, ELogLevel.Medium, String.Format( "Handling network operation {0} on session id {1}", operation.NetworkOp.ToString(), (int) operation.ID ) );

			return false;
		}

		protected void Send_Message( CWrappedNetworkMessage message )
		{
			CLog.Log( ELoggingChannel.Network, ELogLevel.Medium, String.Format( "Sending network message {0}", message.Message.GetType().Name ) );
			CLog.Log( ELoggingChannel.Network, ELogLevel.High, String.Format( "		Internal state: {0}", message.Message.ToString() ) );

			m_RequestResponseManager.Process_Outgoing_Message( message.Message );
			if ( message.ID == ESessionID.Loopback )
			{
				On_Message_Receive( message.Message, message.ID );
			}
			else
			{	
				CConnection connection = Get_Connection( message.ID );
				if ( connection != null )
				{
					connection.Send_Message( message.Message );
				}
				else if ( NotifyOnSendFailure )
				{
					Add_Event( ENetworkEvent.Unable_To_Send_Message, ESessionID.Invalid );
				}
			}
		}

		protected void Add_Event( ENetworkEvent event_type, ESessionID session_id )
		{
			CLog.Log( ELoggingChannel.Network, ELogLevel.Medium, String.Format( "Adding network event {0} on session id {1}", event_type.ToString(), (int) session_id ) );

			m_CurrentFrame.Add_Event( event_type, session_id );
		}

		protected void Add_Connection( CConnection connection )
		{
			m_Connections.Add( connection.ID, connection );
		}

		protected void Dispose_And_Remove_Connection( ESessionID session_id )
		{
			CConnection connection = Get_Connection( session_id );
			if ( connection != null )
			{
				CLog.Log( ELoggingChannel.Network, ELogLevel.Medium, String.Format( "Removing connection on session id {0}", (int) session_id ) );

				connection.Dispose();
				m_Connections.Remove( session_id );
			}
		}

		protected CConnection Get_Connection( ESessionID id )
		{
			CConnection connection = null;
			if ( m_Connections.TryGetValue( id, out connection ) )
			{
				return connection;
			}
			
			return null;
		}

		// Private interface
		private void Update_Network_Statistics()
		{
			uint frame_sent_bytes = 0;
			uint frame_received_bytes = 0;
			foreach ( var connection in Connections )
			{
				frame_sent_bytes += connection.FrameSentBytes;
				frame_received_bytes += connection.FrameReceivedBytes;
			}

			bool updated_max = false;
			TotalSentBytes += frame_sent_bytes;
			if ( frame_sent_bytes > MaxFrameSentBytes )
			{
				MaxFrameSentBytes = frame_sent_bytes;
				updated_max = true;
			}

			TotalReceivedBytes += frame_received_bytes;
			if ( frame_received_bytes > MaxFrameReceivedBytes )
			{
				MaxFrameReceivedBytes = frame_received_bytes;
				updated_max = true;
			}

			if ( updated_max )
			{
				CLog.Log( ELoggingChannel.Network, ELogLevel.Medium, String.Format( "Netstats -- Max bytes sent on a frame: {0}; Max bytes received on a frame: {1}", MaxFrameSentBytes, MaxFrameReceivedBytes ) );
			}

			if ( frame_sent_bytes > 0 || frame_received_bytes > 0 )
			{
				CLog.Log( ELoggingChannel.Network, ELogLevel.Medium, String.Format( "Netstats -- Total bytes sent: {0}; Total bytes received: {1}", TotalSentBytes, TotalReceivedBytes ) );
			}
		}

		private void Service_Outbound_Network_Frames()
		{
			// service all outbound frames
			var outbound_frames = new List< COutboundNetworkFrame >();
			m_DataInterface.Receive( outbound_frames );
			
			foreach ( var frame in outbound_frames )
			{
				frame.Operations.Apply( operation => Handle_Operation( operation ) );
				frame.Messages.Apply( message => Send_Message( message ) );	
			}		
		}

		private void Ping_Connections()
		{
			foreach ( var connection in Connections )
			{				
				if ( connection.Connected && !connection.IsFinished )
				{
					CPingMessage ping_message = new CPingMessage();
					connection.Send_Message( ping_message );
				}
			}
		}

		// Properties
		public static CNetworkThreadBase BaseInstance { get { return m_BaseInstance; } }
		public IEnumerable< CConnection > Connections { get { return m_Connections.Values; } }
		protected virtual bool NotifyOnSendFailure { get { return false; } }

		// Fields
		private static CNetworkThreadBase m_BaseInstance = null;

		private ICrossThreadDataQueues< CInboundNetworkFrame, COutboundNetworkFrame > m_DataInterface = null;
		private CInboundNetworkFrame m_CurrentFrame = null;
		private Dictionary< ESessionID, CConnection > m_Connections = new Dictionary< ESessionID, CConnection >();

		private CRequestResponseManager m_RequestResponseManager = new CRequestResponseManager();

		private ulong MaxFrameSentBytes = 0;
		private ulong TotalSentBytes = 0;
		private ulong MaxFrameReceivedBytes = 0;
		private ulong TotalReceivedBytes = 0;

		private const long PING_INTERVAL = 10000;
	}
}