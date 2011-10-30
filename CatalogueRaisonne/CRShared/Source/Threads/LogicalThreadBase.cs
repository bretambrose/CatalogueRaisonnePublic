/*

	LogicalThreadBase.cs

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
	public class CLogicalThreadBase : CTimeKeeperThread
	{
		// Construction
		protected CLogicalThreadBase() :
			base()
		{
			m_BaseInstance = this;
		}

		// Methods
		// Public interface
		public void Add_Network_Operation( CNetworkOperation operation )
		{
			m_CurrentNetworkFrame.Add_Operation( operation );	
		}

		public void Add_Message( CNetworkMessage message, ESessionID destination_id )
		{
			m_CurrentNetworkFrame.Add_Message( message, destination_id );
		}

		public override void Send_Message( CNetworkMessage message, ESessionID destination_id ) 
		{
			Add_Message( message, destination_id );
		}

		public void Add_UI_Notification( CUILogicNotification notification )
		{
			m_CurrentUIFrame.Add_Notification( notification );
		}

		// Protected interface and overrides
		protected override void Initialize()
		{
			base.Initialize();

			m_NetworkDataInterface = CNetworkFrameManager.Instance.LogicalInterface;
			m_UIDataInterface = CUIFrameManager.Instance.LogicalInterface;	
		}

		protected override void Shutdown()
		{
			base.Shutdown();

			m_CurrentNetworkFrame = null;
			m_CurrentUIFrame = null;
		}

		protected override void Service( long current_time )
		{
			base.Service( current_time );

			Service_Incoming_Network( current_time );
			Service_Incoming_UI( current_time );
		}

		protected override void Build_Thread_Frames()
		{
			base.Build_Thread_Frames();

			if ( m_CurrentNetworkFrame == null )
			{
				m_CurrentNetworkFrame = new COutboundNetworkFrame();
			}
			
			if ( m_CurrentUIFrame == null )
			{
				m_CurrentUIFrame = new CUIOutputFrame();
			}
		}

		protected override void Flush_Data_Frames( long current_time )
		{
			base.Flush_Data_Frames( current_time );

			if ( !m_CurrentNetworkFrame.Empty )
			{
				m_NetworkDataInterface.Send( m_CurrentNetworkFrame );
				m_CurrentNetworkFrame = null;
			}

			if ( !m_CurrentUIFrame.Empty )
			{
				m_UIDataInterface.Send( m_CurrentUIFrame );
				m_CurrentUIFrame = null;
			}
		}

		protected virtual bool Handle_Network_Event( CNetworkEvent network_event )
		{
			return false;
		}

		// Private interface
		private void Service_Incoming_UI( long current_time )
		{
			var incoming_frames = new List< CUIInputFrame >();
			m_UIDataInterface.Receive( incoming_frames );
			incoming_frames.Apply( frame => Service_Incoming_UI_Frame( frame ) );
		}

		private void Service_Incoming_UI_Frame( CUIInputFrame frame )
		{
			frame.Requests.Apply( request => CGenericHandlerManager.Instance.Try_Handle( request ) );
		}

		private void Service_Incoming_Network( long current_time )
		{
			var incoming_frames = new List< CInboundNetworkFrame >();
			m_NetworkDataInterface.Receive( incoming_frames );
			incoming_frames.Apply( frame => Service_Incoming_Network_Frame( frame ) );
		}
		
		private void Service_Incoming_Network_Frame( CInboundNetworkFrame frame )
		{
			frame.Events.Apply( network_event => Handle_Network_Event( network_event ) );
			frame.Messages.Apply( message => Handle_Network_Message( message ) );
		}

		private void Handle_Network_Message( CWrappedNetworkMessage message )
		{
			CLog.Log( ELoggingChannel.Logic, ELogLevel.Medium, "Logic processing network message " + message.GetType().Name );

			if ( !CNetworkMessageHandler.Instance.Try_Handle_Message( message ) )
			{
				throw new CApplicationException( String.Format( "Encountered an unhandlable message: {0}", message.Message.ToString() ) );
			}
		}		

		// Slash command handlers
		[GenericHandler]
		private void Handle_Slash_Command_Request( CUIInputSlashCommandRequest request )
		{
			CSlashCommand command = null;
			string error_string = null;
			
			if ( !CSlashCommandParser.Instance.Try_Parse( request, out command, out error_string ) )
			{
				CSharedResource.Output_Text_By_Category( ETextOutputCategory.Error, error_string );
				return;
			}
			
			CLog.Log( ELoggingChannel.Logic, ELogLevel.Low, String.Format( "Processing slash command {0}", command.GetType().Name ) );

			if ( !CGenericHandlerManager.Instance.Try_Handle( command ) )
			{
				throw new CApplicationException( String.Format( "Encountered an unhandlable slash command: {0}", command.GetType().Name ) );
			}
		}

		// Properties
		public static CLogicalThreadBase BaseInstance { get { return m_BaseInstance; } }

		// Fields
		private ICrossThreadDataQueues< COutboundNetworkFrame, CInboundNetworkFrame > m_NetworkDataInterface = null;
		private ICrossThreadDataQueues< CUIOutputFrame, CUIInputFrame > m_UIDataInterface = null;

		private COutboundNetworkFrame m_CurrentNetworkFrame = new COutboundNetworkFrame();
		private CUIOutputFrame m_CurrentUIFrame = new CUIOutputFrame();

		protected static CLogicalThreadBase m_BaseInstance = null;
	}
}
