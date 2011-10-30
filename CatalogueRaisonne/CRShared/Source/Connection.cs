/*

	Connection.cs

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
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace CRShared
{
	public enum ESessionID
	{
		Invalid = -2,
		Loopback = -1,
		First
	}
	
	public class CAppNetworkingException : CApplicationException
	{
		public CAppNetworkingException( string message ) :
			base( message )
		{}
	}
	
	public class CConnection : IDisposable
	{
		// Construction
		public CConnection( ESessionID id )
		{
			ID = id;
			m_IncomingReader = new BinaryReader( m_IncomingMessageStream );			
			Connected = false;
			PendingDisconnectReason = EDisconnectReason.Invalid;
		}
		
		~CConnection()
		{
			Dispose( false );
		}
		
		// Methods
		// Public interface
		public void Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		public void Initialize( Socket socket )
		{
			m_Socket = socket;
			Connected = true;
			LastPingTime = CurrentTime;
		}
		
		public void Service()
		{
#if _NDEBUG
			if ( LastPingTime + TIMEOUT_INTERVAL < CurrentTime )
			{
				IsFinished = true;
			}
#endif
			
			FrameSentBytes = 0;
			FrameReceivedBytes = 0;
				
			if ( IsFinished == true )
			{
				return;
			}
			
			try
			{
				// zero length send to test connected state
				m_Socket.Send( m_ConnectionBuffer, 0, SocketFlags.None );
				
				if ( m_Socket.Connected )
				{		
					while ( m_Socket.Available > 0 )
					{
						int bytes_read = m_Socket.Receive( m_ConnectionBuffer );
						FrameReceivedBytes += (uint) bytes_read;
						Process_Connection_Buffer( bytes_read );
					}
					
					Service_Outgoing_Messages();
				}
			}
			catch ( SocketException e )
			{
				if ( e.ErrorCode != 10035 )
				{
					IsFinished = true;
				}
			}
			
			if ( !m_Socket.Connected )
			{
				IsFinished = true;
			}
		}

		public void Send_Message( CNetworkMessage message )
		{
			m_OutgoingMessages.Add( message );
		}
		
		public void Service_Outgoing_Messages()
		{
			foreach ( var message in m_OutgoingMessages )
			{			
				m_OutgoingMessageStream.Position = 0;
				m_OutgoingMessageStream.SetLength( 0 );
				CNetworkMessageSerializationManager.Serialize( message, m_OutgoingMessageStream );
				
				int length = (int)m_OutgoingMessageStream.Position;
				FrameSentBytes += (uint) length;

				m_Socket.Send( m_OutgoingMessageStream.GetBuffer(), length, SocketFlags.None );		
				
				CLog.Log( ELoggingChannel.Network, ELogLevel.Medium, String.Format( "Message of type {0} pushed to socket, encode length = {1}", message.GetType().Name, length ) );		
			}
			
			m_OutgoingMessages.Clear();
		}
						
		// Protected interface
		protected virtual void Dispose( bool disposing )
		{
			CNetworkThreadBase.BaseInstance.On_Disconnection( ID, PendingDisconnectReason );			

			if ( disposing )
			{
			}
			
			if ( m_Socket != null )
			{
				m_Socket.Shutdown( SocketShutdown.Both );
				m_Socket.Close( 2 );

				Connected = false;
				m_Socket = null;
			}
		}

		protected virtual bool Handle_Message( CNetworkMessage message )
		{
			CLog.Log( ELoggingChannel.Network, ELogLevel.Medium, String.Format( "Connection handle opportunity for message of type ", message.GetType().Name ) );		

			switch ( message.MessageType )
			{
				case ENetworkMessageType.Message_Ping:
					LastPingTime = CurrentTime;
					return true;

				default:
					return false;
			}
		}
			
		// Private interface
		private void Process_Connection_Buffer( int valid_bytes )
		{
			int remaining_bytes = valid_bytes;
			int current_buffer_index = 0;
			while ( remaining_bytes > 0 )
			{
				int stream_size = (int) m_IncomingMessageStream.Length;
				
				if ( m_CurrentMessageSize == -1 )
				{
					if ( stream_size >= 4 )
					{
						throw new CAppNetworkingException( "Unexpected stream alignment error." );
					}
					
					int needed_bytes = 4 - stream_size;
					if ( needed_bytes <= remaining_bytes )
					{
						m_IncomingMessageStream.Write( m_ConnectionBuffer, current_buffer_index, needed_bytes );
						current_buffer_index += needed_bytes;
						remaining_bytes -= needed_bytes;
						
						m_IncomingMessageStream.Position = 0;
						m_CurrentMessageSize = m_IncomingReader.ReadInt32();
						m_IncomingMessageStream.Position = 4;
						continue;
					}
					else
					{
						m_IncomingMessageStream.Write( m_ConnectionBuffer, current_buffer_index, remaining_bytes );
						return;
					}
				}
				else if ( remaining_bytes + stream_size < m_CurrentMessageSize )
				{
					m_IncomingMessageStream.Write( m_ConnectionBuffer, current_buffer_index, remaining_bytes );
					return;
				}
				else
				{
					int needed_bytes = m_CurrentMessageSize - stream_size;
					m_IncomingMessageStream.Write( m_ConnectionBuffer, current_buffer_index, needed_bytes );
					
					current_buffer_index += needed_bytes;
					remaining_bytes -= needed_bytes;
					
					Handle_Message( Build_Message() );
					
					m_IncomingMessageStream.Position = 0;
					m_IncomingMessageStream.SetLength( 0 );
					m_CurrentMessageSize = -1;
				}
			}
		}
		
		private CNetworkMessage Build_Message()
		{
			m_IncomingMessageStream.Position = 4;

			CNetworkMessage message = CNetworkMessageSerializationManager.Deserialize( m_IncomingMessageStream );		
			return message;
		}
		
		// Properties
		public ESessionID ID { get; private set; }
		public bool IsFinished { get; set; }
		public bool Connected { get; private set; }
		public long LastPingTime { get; protected set; }
		public long CurrentTime { get { return CNetworkThreadBase.BaseInstance.CurrentTime; } }
		public EDisconnectReason PendingDisconnectReason { get; protected set; }
		public uint FrameSentBytes { get; private set; }
		public uint FrameReceivedBytes { get; private set; }
		
		// Fields
		protected Socket m_Socket = null;
		
		byte[] m_ConnectionBuffer = new byte[ CONNECTION_BUFFER_SIZE ];
		int m_CurrentMessageSize = -1;
		MemoryStream m_IncomingMessageStream = new MemoryStream();
		MemoryStream m_OutgoingMessageStream = new MemoryStream();
		BinaryReader m_IncomingReader = null;
		
		List< CNetworkMessage > m_OutgoingMessages = new List< CNetworkMessage >();
				
		// Constants
		const int CONNECTION_BUFFER_SIZE = 4096;
		private const long TIMEOUT_INTERVAL = 30000;				
	}
}