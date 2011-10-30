/*

	ClientLogicalThread.cs

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
using System.Net;
using System.Text;
using System.Reflection;

using CRShared;
using CRShared.Chat;
using CRClientShared.Chat;
using CRClientShared;

namespace CRClientShared
{
	public class CClientLogicalThread : CLogicalThreadBase
	{
		// Construction
		private CClientLogicalThread() :
			base()
		{
		}
		
		static CClientLogicalThread() {}
		
		// Methods
		// Public interface
		public static void Start_Main_Thread()
		{
#if DEBUG
			CSharedResource.Output_Text< ESharedTextID >( ESharedTextID.Shared_Begin_Internal_Tests );
			CTestClientShared.Run_Internal_Tests();
			CSharedResource.Output_Text< ESharedTextID >( ESharedTextID.Shared_End_Internal_Tests );
#endif

			Instance.Start();
		}

		public static void Load_Settings()
		{			
			CSharedSettings.Initialize( "Data/Shared/XML/SharedSettings.xml" );
			CSharedSettings.Load_Settings();
						
			CClientSettings.Initialize( "Data/Client/XML/ClientSettings.xml" );
			CClientSettings.Load_Settings();			
		}

		public static void Find_Slash_Commands()
		{
			CSlashCommandParser.Instance.Initialize_Assembly_Commands< ESharedTextID >( "Shared", Assembly.GetAssembly( typeof( CSlashCommand ) ), ESharedTextID.Invalid );
			CSlashCommandParser.Instance.Initialize_Assembly_Commands< EClientTextID >( "Client", Assembly.GetExecutingAssembly(), EClientTextID.Invalid );
			CSlashCommandParser.Instance.Initialize_Groups();
		}

		public void Send_Message_To_Server( CNetworkMessage message ) 
		{
			Send_Message( message, ESessionID.First );
		}

		// Protected interface and overrides
		protected override void Initialize()
		{
			base.Initialize();
			base.Initialize_Time_Keeper( TimeKeeperExecutionMode.Internal, TimeKeeperTimeUnit.RealTime, TimeKeeperAuthority.Master, LOGICAL_SERVICE_INTERVAL, true, true );
			PendingDisconnectReason = EDisconnectReason.Invalid;

			CNetworkMessageHandler.Instance.Initialize( delegate( ESessionID session_id ) { return ConnectedID; } );
			Register_Message_Handlers();
			Register_Generic_Handlers();

			CClientPlayerInfoManager.Instance.Initialize();

			CClientResource.Output_Text( EClientTextID.Client_Startup_Greeting );
		}

		protected override bool Handle_Network_Event( CNetworkEvent network_event )
		{
			if ( base.Handle_Network_Event( network_event ) )
			{
				return true;
			}

			switch ( network_event.NetworkEvent )
			{
				case ENetworkEvent.Connection_Success:
					CClientResource.Output_Text( EClientTextID.Client_Connection_Success_Notice );
					return true;
					
				case ENetworkEvent.Connection_Failure:
					CClientResource.Output_Text( EClientTextID.Client_Connection_Failure_Notice );
					return true;
					
				case ENetworkEvent.Disconnection:
					Handle_Disconnection_Event();
					return true;

				case ENetworkEvent.Unable_To_Send_Message:
					CClientResource.Output_Text( EClientTextID.Client_Not_Connected_To_Server );
					return true;
			}

			return false;
		}

		// Slash Command handlers
		[GenericHandler]
		private void Handle_Connect_Command( CConnectSlashCommand command )
		{
			Add_Network_Operation( new CConnectRequestOperation( new IPEndPoint( CClientSettings.Instance.ServerAddress, command.ServerPort ), ESessionID.First, command.PlayerName ) );
		}

		[GenericHandler]
		private void Handle_Disconnect_Command( CDisconnectSlashCommand command )
		{
			Add_Network_Operation( new CDisconnectRequestOperation( ESessionID.First, EDisconnectReason.Client_Request ) );
			PendingDisconnectReason = EDisconnectReason.Client_Request;
		}

		[GenericHandler]
		private void Handle_Quit_Command( CQuitClientSlashCommand command )
		{
			Add_Network_Operation( new CDisconnectRequestOperation( ESessionID.First, EDisconnectReason.Client_Request ) );
			PendingDisconnectReason = EDisconnectReason.Client_Request_Quit;
		}

		[GenericHandler]
		private void Handle_Crash_Command( CCrashSlashCommand command )
		{
			throw new CApplicationException( "User-ordered crash!" );
		}

		// Network message handlers
		[NetworkMessageHandler]
		private void Handle_Hello_Response( CClientHelloResponse message )
		{
			if ( message.Reason == EConnectRefusalReason.None )
			{
				On_Successful_Hello( message.PlayerData );
			}
			else
			{
				Handle_Connection_Dropped( message.Reason );
			}
		}

		// Private interface
		private void Register_Generic_Handlers()
		{
			CGenericHandlerManager.Instance.Find_Handlers< CSlashCommand >( Assembly.GetAssembly( typeof( CSlashCommand ) ) );
			CGenericHandlerManager.Instance.Find_Handlers< CSlashCommand >( Assembly.GetExecutingAssembly() );

			CGenericHandlerManager.Instance.Find_Handlers< CUIInputRequest >( Assembly.GetAssembly( typeof( CSlashCommand ) ) );
			CGenericHandlerManager.Instance.Find_Handlers< CUIInputRequest >( Assembly.GetExecutingAssembly() );
		}

		private void Register_Message_Handlers()
		{
			CNetworkMessageHandler.Instance.Find_Handlers( Assembly.GetAssembly( typeof( CNetworkMessage ) ) );
			CNetworkMessageHandler.Instance.Find_Handlers( Assembly.GetExecutingAssembly() );

			CNetworkMessageHandler.Instance.Register_Handler< CConnectionDroppedMessage >( delegate( CConnectionDroppedMessage message ) { Handle_Connection_Dropped( message.Reason ); } );
			CNetworkMessageHandler.Instance.Register_Null_Handler< CPingMessage >();
		}

		private void Handle_Network_Message( CWrappedNetworkMessage message )
		{
			if ( !CNetworkMessageHandler.Instance.Try_Handle_Message( message ) )
			{
				throw new CApplicationException( String.Format( "Encountered an unhandlable message: {0}", message.Message.MessageType.ToString() ) );
			}
		}		

		private void Handle_Disconnection_Event()
		{
			switch ( PendingDisconnectReason )
			{
				case EDisconnectReason.Client_Request_Quit:
					throw new CQuitException();

				case EDisconnectReason.Client_Request:
					CClientResource.Output_Text( EClientTextID.Client_Expected_Disconnection );
					break;

				default:
					CClientResource.Output_Text( EClientTextID.Client_Unexpected_Disconnection );
					break;
			}

			PendingDisconnectReason = EDisconnectReason.Invalid;
			Reset_Complete_Logical_State();
		}

		private void On_Successful_Hello( CPersistentPlayerData player_data )
		{
			CClientPersistentDataManager.Instance.On_Receive_Player_Data( player_data );
			CClientPlayerInfoManager.Instance.Begin_Player_Listen( player_data.ID, EPlayerListenReason.Self );
			Add_UI_Notification( new CUIScreenStateNotification( EUIScreenState.Chat_Idle ) );
		}

		private void Handle_Connection_Dropped( EConnectRefusalReason reason )
		{
			Reset_Complete_Logical_State();

			switch ( reason )
			{
				case EConnectRefusalReason.Unknown:
					CClientResource.Output_Text( EClientTextID.Client_Connection_Refused_Unknown );
					break;

				case EConnectRefusalReason.Name_Already_Connected:
					CClientResource.Output_Text( EClientTextID.Client_Connection_Refused_Name_Already_Connected );
					break;

				case EConnectRefusalReason.Invalid_Password:
					CClientResource.Output_Text( EClientTextID.Client_Connection_Refused_Invalid_Password );
					break;

				case EConnectRefusalReason.Unable_To_Announce_To_Chat_Server:
					CClientResource.Output_Text( EClientTextID.Client_Connection_Refused_Chat );
					break;

				case EConnectRefusalReason.Unable_To_Join_Required_Chat_Channel:
					CClientResource.Output_Text( EClientTextID.Client_Connection_Refused_Default_Chat );
					break;

				case EConnectRefusalReason.Internal_Persistence_Error:
					CClientResource.Output_Text( EClientTextID.Client_Connection_Refused_Internal_Persistence_Error );
					break;
			}
		}

		private void Reset_Complete_Logical_State()
		{
			CChatClient.Instance.Reset();
			CClientPersistentDataManager.Instance.Reset_Player_Data();
			CClientPlayerInfoManager.Instance.Reset();
			CClientLobbyManager.Instance.Reset();

			Add_UI_Notification( new CUIScreenStateNotification( EUIScreenState.Main_Menu ) );
		}
					
		// Properties
		public static CClientLogicalThread Instance { get { return m_Instance; } }
		public EDisconnectReason PendingDisconnectReason { get; private set; }
		public EPersistenceID ConnectedID { get { return ( ( CClientPersistentDataManager.Instance.PlayerData != null ) ? CClientPersistentDataManager.Instance.PlayerData.ID : EPersistenceID.Invalid ); } }
		public bool IsConnected { get { return ConnectedID != EPersistenceID.Invalid; } }

		// Fields
		private static CClientLogicalThread m_Instance = new CClientLogicalThread();
		
		private const int LOGICAL_SERVICE_INTERVAL = 50;
	
	}
}
