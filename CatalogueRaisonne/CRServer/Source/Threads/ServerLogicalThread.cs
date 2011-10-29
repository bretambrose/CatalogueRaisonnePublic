using System;
using System.Collections.Generic;
using System.Reflection;

using CRShared;
using CRShared.Chat;
using CRServer.Chat;

namespace CRServer
{
	public static class CServerMessageRouter
	{
		// Construction
		static CServerMessageRouter() {}

		// Methods
		// Public static interface
		public static void Send_Message_To_Player( CNetworkMessage message, EPersistenceID player_id )
		{
			CServerLogicalThread.Instance.Send_Message( message, player_id );
		}

		public static void Send_Message_To_Session( CNetworkMessage message, ESessionID session_id )
		{
			CServerLogicalThread.Instance.Send_Message( message, session_id );
		}

		public static void Send_Message_To_Chat_Server( CNetworkMessage message )
		{
			CServerLogicalThread.Instance.Send_Message( message, ESessionID.Loopback );			
		}

		public static void Send_Message_To_Server( CNetworkMessage message )
		{
			CServerLogicalThread.Instance.Send_Message( message, ESessionID.Loopback );			
		}
	}

	public class CServerLogicalThread : CLogicalThreadBase
	{
		// Local classes
		public class CServerShutdownTask : CScheduledTask
		{	
			// Construction
			public CServerShutdownTask( long schedule_time ) :
				base( schedule_time )
			{
			}
			
			// Methods
			public override void Execute( long current_time ) 
			{
				throw new CQuitException();
			}
		}

		// Construction
		private CServerLogicalThread() :
			base()
		{
		}
		
		static CServerLogicalThread() {}
		
		// Methods
		// Public interface
		public static void Start_Main_Thread()
		{
#if DEBUG
			CSharedResource.Output_Text< ESharedTextID >( ESharedTextID.Shared_Begin_Internal_Tests );
			CTestServer.Run_Internal_Tests();
			CSharedResource.Output_Text< ESharedTextID >( ESharedTextID.Shared_End_Internal_Tests );
#endif
			Instance.Start();
		}

		public static void Find_Slash_Commands()
		{
			CSlashCommandParser.Instance.Initialize_Assembly_Commands< ESharedTextID >( "Shared", Assembly.GetAssembly( typeof( CSlashCommand ) ), ESharedTextID.Invalid );
			CSlashCommandParser.Instance.Initialize_Assembly_Commands< EServerTextID >( "Server", Assembly.GetExecutingAssembly(), EServerTextID.Invalid );
			CSlashCommandParser.Instance.Initialize_Groups();
		}

		public static void Load_Settings()
		{
			CSharedSettings.Initialize( "Data/Shared/XML/SharedSettings.xml" );
			CSharedSettings.Load_Settings();
			
			CServerSettings.Initialize( "Data/Server/XML/ServerSettings.xml" );
			CServerSettings.Load_Settings();		
		}

		public override void Send_Message( CNetworkMessage message, ESessionID destination_id ) 
		{
			Add_Message( message, destination_id );
		}

		public void Send_Message( CNetworkMessage message, EPersistenceID player_id ) 
		{
			CConnectedPlayer player = Get_Active_Player_By_Persistence_ID( player_id );
			if ( player != null )
			{
				Add_Message( message, player.SessionID );
			}
		}

		public void Submit_Persistence_Request( CPersistenceRequest request )
		{
			m_CurrentPersistenceFrame.Add_Request( request );
		}

		// Protected interface and overrides
		protected override void Initialize()
		{
			base.Initialize();
			base.Initialize_Time_Keeper( TimeKeeperExecutionMode.Internal, TimeKeeperTimeUnit.RealTime, TimeKeeperAuthority.Master, LOGICAL_SERVICE_INTERVAL, true, true );

			m_PersistenceInterface = CPersistenceFrameManager.Instance.LogicalInterface;

			CNetworkMessageHandler.Instance.Initialize( delegate( ESessionID session_id ) { return CConnectedPlayerManager.Instance.Get_Player_ID_By_Session_ID( session_id ); } );
			Register_Message_Handlers();
			Register_Generic_Handlers();

			CConnectedPlayerManager.Instance.Initialize_Chat();
		
			CServerResource.Output_Text( EServerTextID.Server_Startup_Greeting );
		}

		protected override void Flush_Data_Frames( long current_time )
		{
			base.Flush_Data_Frames( current_time );

			if ( !m_CurrentPersistenceFrame.Empty )
			{
				m_PersistenceInterface.Send( m_CurrentPersistenceFrame );
				m_CurrentPersistenceFrame = null;
			}
		}

		protected override void Build_Thread_Frames()
		{
			base.Build_Thread_Frames();

			if ( m_CurrentPersistenceFrame == null )
			{
				m_CurrentPersistenceFrame = new CToPersistenceFrame();
			}
		}

		protected override void Service( long current_time )
		{
			base.Service( current_time );
			
			CDatabaseProxy.Instance.Service_Requests();
			Service_Incoming_Persistence();
			CDatabaseProxy.Instance.Service_Responses();

			CConnectedPlayerManager.Instance.Cull_Disconnect_Timeouts();
		}

		protected override bool Handle_Network_Event( CNetworkEvent network_event )
		{
			if ( base.Handle_Network_Event( network_event ) )
			{
				return true;
			}

			switch ( network_event.NetworkEvent )
			{				
				case ENetworkEvent.Disconnection:
					CConnectedPlayerManager.Instance.Suspend_Connected_Player( network_event.ID );
					return true;
			}

			return false;
		}

		// Private interface
		private void Register_Generic_Handlers()
		{
			CGenericHandlerManager.Instance.Find_Handlers< CSlashCommand >( Assembly.GetAssembly( typeof( CSlashCommand ) ) );
			CGenericHandlerManager.Instance.Find_Handlers< CSlashCommand >( Assembly.GetExecutingAssembly() );

			CGenericHandlerManager.Instance.Find_Handlers< CUIInputRequest >( Assembly.GetAssembly( typeof( CUIInputRequest ) ) );
			CGenericHandlerManager.Instance.Find_Handlers< CUIInputRequest >( Assembly.GetExecutingAssembly() );
		}
				
		private void Register_Message_Handlers()
		{
			CNetworkMessageHandler.Instance.Find_Handlers( Assembly.GetAssembly( typeof( CNetworkMessage ) ) );
			CNetworkMessageHandler.Instance.Find_Handlers( Assembly.GetExecutingAssembly() );

			// From Chat Server messages (don't do anything, any logic should be handled by the delegate embedded in the request)
			CNetworkMessageHandler.Instance.Register_Null_Handler< CCreateChatChannelResponseServerMessage >();
			CNetworkMessageHandler.Instance.Register_Null_Handler< CJoinChatChannelResponseServerMessage >();
			CNetworkMessageHandler.Instance.Register_Null_Handler< CAnnouncePlayerToChatServerResponse >();
		}
		
		private void Service_Incoming_Persistence()
		{
			var incoming_frames = new List< CFromPersistenceFrame >();
			m_PersistenceInterface.Receive( incoming_frames );
			incoming_frames.Apply( frame => Service_Incoming_Persistence_Frame( frame ) );
		}

		private void Service_Incoming_Persistence_Frame( CFromPersistenceFrame frame )
		{
			frame.Responses.Apply( response => CDatabaseProxy.Instance.Submit_Response( response ) );
		}

		private CConnectedPlayer Get_Active_Player_By_Persistence_ID( EPersistenceID player_id )
		{
			return CConnectedPlayerManager.Instance.Get_Active_Player_By_Persistence_ID( player_id ); 
		}

		// Network message handlers
		[NetworkMessageHandler]
		private void Handle_Client_Hello_Request( CClientHelloRequest message, ESessionID session_id )
		{
			CGetPlayerDataPersistenceRequest get_data_request = new CGetPlayerDataPersistenceRequest( message.Name );
			get_data_request.Handler = delegate( CPersistenceResponse response ) 
				{
					CGetPlayerDataPersistenceResponse get_data_response = response as CGetPlayerDataPersistenceResponse;

					EPersistenceID player_id = get_data_response.PlayerData.ID;
					CConnectedPlayerManager.Instance.Add_Connected_Player( session_id, player_id, message );
				};

			CDatabaseProxy.Instance.Submit_Request( get_data_request );
		}

		[NetworkMessageHandler]
		private void Handle_Disconnection_Request( CDisconnectRequestMessage message, ESessionID session_id )
		{
			CConnectedPlayerManager.Instance.Remove_Connected_Player( session_id );
			Add_Network_Operation( new CDisconnectRequestOperation( session_id, EDisconnectReason.Client_Request ) );
		}

		[NetworkMessageHandler]
		private void On_Query_Player_Info_Request( CQueryPlayerInfoRequest request, ESessionID source_session )
		{
			CQueryPlayerInfoResponse response = new CQueryPlayerInfoResponse( request.RequestID );
			foreach ( var id in request.IDs )
			{
				CConnectedPlayer player = Get_Active_Player_By_Persistence_ID( id );
				if ( player != null )
				{
					CPlayerInfo player_info = new CPlayerInfo( id );
					player_info.Name = player.Name;

					response.Add_Player_Info( player_info );
				}
			}

			Send_Message( response, source_session );
		}
		
		// Slash command handlers
		[GenericHandler]
		private void Handle_Shutdown_Command( CShutdownServerSlashCommand command )
		{
			long shutdown_time = CurrentThreadTime + Convert_Seconds_To_Internal_Time( command.Delay );
			TaskScheduler.Add_Scheduled_Task( new CServerShutdownTask( shutdown_time ) );
		}

		[GenericHandler]
		private void Handle_Crash_Command( CCrashSlashCommand command )
		{
			throw new CApplicationException( "User-ordered crash!" );
		}
				
		// Properties
		public static CServerLogicalThread Instance { get { return m_Instance; } }
		
		// Fields
		private static CServerLogicalThread m_Instance = new CServerLogicalThread();

		private ICrossThreadDataQueues< CToPersistenceFrame, CFromPersistenceFrame > m_PersistenceInterface = null;

		private CToPersistenceFrame m_CurrentPersistenceFrame = new CToPersistenceFrame();
						
		private const int LOGICAL_SERVICE_INTERVAL = 100;	
	}
}
