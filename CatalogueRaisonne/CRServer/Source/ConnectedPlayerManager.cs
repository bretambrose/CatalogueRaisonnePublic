/*

	ConnectedPlayerManager.cs

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
using System.Linq;

using CRShared;
using CRShared.Chat;
using CRServer.Chat;

namespace CRServer
{
	public class CConnectedPlayerManager
	{
		// Construction
		static CConnectedPlayerManager () {}
		private CConnectedPlayerManager()
		{
		}

		// Methods
		// Public interface
		public void Initialize_Chat() 
		{
			CChatChannelConfig config = new CChatChannelConfig( CChatChannelConfig.Make_Admin_Channel( GENERAL_CHANNEL_NAME ), 
																				 GENERAL_CHANNEL_NAME, 
																				 EPersistenceID.Invalid, 
																				 EChannelGameProperties.General );
			config.AnnounceJoinLeave = false;
			config.IsMembershipClientLocked = true;

			CCreateChatChannelRequestServerMessage create_request = new CCreateChatChannelRequestServerMessage( config );
			create_request.Handler = On_General_Chat_Creation_Response;

			CServerMessageRouter.Send_Message_To_Chat_Server( create_request );
		}

		public void Send_Message( CNetworkMessage message, ESessionID destination_id ) 
		{
			CServerLogicalThread.Instance.Send_Message( message, destination_id );
		}

		public void Send_Message( CNetworkMessage message, EPersistenceID player_id ) 
		{
			CConnectedPlayer player = Get_Active_Player_By_Persistence_ID( player_id );
			if ( player != null )
			{
				CServerLogicalThread.Instance.Send_Message( message, player.SessionID );
			}
		}

		public void Add_Connected_Player( ESessionID session_id, EPersistenceID persistence_id, CClientHelloRequest request )
		{
			if ( Get_Player_By_Session_ID( session_id ) != null )
			{
				Send_Message( new CClientHelloResponse( request.RequestID, EConnectRefusalReason.Name_Already_Connected ), session_id );
				Add_Network_Operation( new CDisconnectRequestOperation( session_id, EDisconnectReason.Server_Request ) );
				return;
			}
			
			if ( Get_Active_Player_By_Persistence_ID( persistence_id ) != null )
			{
				Send_Message( new CClientHelloResponse( request.RequestID, EConnectRefusalReason.Name_Already_Connected ), session_id );
				Add_Network_Operation( new CDisconnectRequestOperation( session_id, EDisconnectReason.Server_Request ) );
				return;
			}
			
			CPersistentPlayerData player_data = CDatabaseProxy.Instance.Get_Player_Data( persistence_id );
			if ( player_data == null )
			{
				Send_Message( new CClientHelloResponse( request.RequestID, EConnectRefusalReason.Internal_Persistence_Error ), session_id );
				Add_Network_Operation( new CDisconnectRequestOperation( session_id, EDisconnectReason.Server_Request ) );
				return;
			}

			CConnectedPlayer player = null;
			if ( m_SuspendedPlayers.TryGetValue( persistence_id, out player ) )
			{
				m_SuspendedPlayers.Remove( persistence_id );
				m_ConnectedPlayers.Add( persistence_id, player );
				player.Resume( session_id );
				CServerResource.Output_Text( EServerTextID.Server_Player_Reconnected_Notice, player.Name );
				CLog.Log( ELoggingChannel.Logic, ELogLevel.Low, CServerResource.Get_Text( EServerTextID.Server_Player_Reconnected_Notice, player.Name ) );
			}
			else
			{				
				player = new CConnectedPlayer( persistence_id, session_id, request.Name );
				m_ConnectedPlayers.Add( persistence_id, player );
				CServerResource.Output_Text( EServerTextID.Server_Player_Connected_Notice, player.Name );
				CLog.Log( ELoggingChannel.Logic, ELogLevel.Low, CServerResource.Get_Text( EServerTextID.Server_Player_Connected_Notice, player.Name ) );
			}
			
			m_ActiveSessions.Add( session_id, persistence_id );

			// add to chat
			CAsyncBackendOperations.New_Player_Announce_To_Chat( player, player_data, request.RequestID );
		}
		
		public void Suspend_Connected_Player( ESessionID session_id )
		{
			CConnectedPlayer player = Get_Player_By_Session_ID( session_id );
			if ( player != null )
			{
				m_ConnectedPlayers.Remove( player.PersistenceID );
				player.Suspend();
				CServerLobbyBrowserManager.Instance.Stop_Browsing( player.PersistenceID );
				m_SuspendedPlayers.Add( player.PersistenceID, player );
				m_ActiveSessions.Remove( session_id );

				Send_Message( new CRemovePlayerFromChatServerMessage( session_id ), ESessionID.Loopback );

				CServerResource.Output_Text( EServerTextID.Server_Player_Linkdead_Notice, player.Name );
				CLog.Log( ELoggingChannel.Logic, ELogLevel.Low, CServerResource.Get_Text( EServerTextID.Server_Player_Linkdead_Notice, player.Name ) );
			}
		}
		
		public void Remove_Connected_Player( ESessionID session_id )
		{
			CConnectedPlayer player = Get_Player_By_Session_ID( session_id );
			if ( player != null )
			{
				Detach_Player_From_Game_Systems( player.PersistenceID );

				m_ConnectedPlayers.Remove( player.PersistenceID );
				m_ActiveSessions.Remove( session_id );

				Send_Message( new CRemovePlayerFromChatServerMessage( session_id ), ESessionID.Loopback );

				CServerResource.Output_Text( EServerTextID.Server_Player_Removed_Notice, player.Name );
			}
		}

		public EPersistenceID Get_Player_ID_By_Session_ID( ESessionID session_id )
		{
			EPersistenceID persistence_id = EPersistenceID.Invalid;
			if ( !m_ActiveSessions.TryGetValue( session_id, out persistence_id ) )
			{
				return EPersistenceID.Invalid;
			}
			
			return persistence_id;
		}
				
		public CConnectedPlayer Get_Player_By_Session_ID( ESessionID session_id )
		{
			EPersistenceID persistence_id = EPersistenceID.Invalid;
			if ( !m_ActiveSessions.TryGetValue( session_id, out persistence_id ) )
			{
				return null;
			}
			
			CConnectedPlayer player = null;
			if ( !m_ConnectedPlayers.TryGetValue( persistence_id, out player ) )
			{
				return null;
			}
			
			return player;
		}

		public CConnectedPlayer Get_Active_Player_By_Persistence_ID( EPersistenceID persistence_id )
		{		
			CConnectedPlayer player = null;
			if ( !m_ConnectedPlayers.TryGetValue( persistence_id, out player ) )
			{
				return null;
			}
			
			return player;
		}

		public CConnectedPlayer Get_Player_By_Persistence_ID( EPersistenceID persistence_id )
		{
			CConnectedPlayer player = null;
			if ( m_ConnectedPlayers.TryGetValue( persistence_id, out player ) )
			{
				return player;
			}
			
			if ( m_SuspendedPlayers.TryGetValue( persistence_id, out player ) )
			{
				return player;
			}

			return null;
		}

		public static string Get_Player_Log_Name( EPersistenceID persistence_id )
		{
			CConnectedPlayer player = Instance.Get_Player_By_Persistence_ID( persistence_id );
			if ( player != null )
			{
				return player.Name;
			}
			else
			{
				return String.Format( "Player#{0}", (int)persistence_id );
			}
		}

		public bool Is_Connected( EPersistenceID player_id )
		{
			return m_ConnectedPlayers.ContainsKey( player_id );
		}

		public bool Is_Connection_Valid( ESessionID session_id )
		{
			return m_ActiveSessions.ContainsKey( session_id );
		}

		public ESessionID Get_Active_Player_Session_ID( EPersistenceID player_id )
		{
			CConnectedPlayer player = Get_Active_Player_By_Persistence_ID( player_id );
			if ( player == null )
			{
				return ESessionID.Invalid;
			}

			return player.SessionID;
		}

		public void Cull_Disconnect_Timeouts()
		{
			long timeout_cutoff = CServerLogicalThread.Instance.CurrentTime - CServerSettings.Instance.DisconnectTimeout;

			List< EPersistenceID > removed_disconnects = new List< EPersistenceID >();

			m_SuspendedPlayers.Where( sp => sp.Value.TimeOfDisconnect <= timeout_cutoff ).Apply( pp => removed_disconnects.Add( pp.Key ) );
			removed_disconnects.Apply( removed_id => Remove_Suspended_Player( removed_id ) );
		}

		public void On_Chat_Server_Announce_Response( CConnectedPlayer player, bool success, EMessageRequestID hello_request_id )
		{
			ESessionID session_id = player.SessionID;
			if ( success == false )
			{
				Send_Message( new CClientHelloResponse( hello_request_id, EConnectRefusalReason.Unable_To_Announce_To_Chat_Server ), session_id );
				Add_Network_Operation( new CDisconnectRequestOperation( session_id, EDisconnectReason.Server_Request ) );		
			}
			else
			{
				Join_Required_Channel_On_Connect( player.PersistenceID, hello_request_id );
			}
		}

		// Private interface
		private void Detach_Player_From_Game_Systems( EPersistenceID player_id )
		{
			CConnectedPlayer player = Get_Player_By_Persistence_ID( player_id );
			if ( player == null )
			{
				return;
			}

			bool is_suspended = m_SuspendedPlayers.ContainsKey( player_id );

			if ( player.LobbyID != ELobbyID.Invalid )
			{
				if ( CServerLobbyManager.Instance.Get_Lobby_By_Creator( player_id ) == null )
				{
					CServerLobbyManager.Instance.Remove_From_Lobby( player.LobbyID, player_id, is_suspended ? ERemovedFromLobbyReason.Disconnect_Timeout : ERemovedFromLobbyReason.Self_Request );
				}
				else
				{
					CServerLobbyManager.Instance.Shutdown_Lobby( player.LobbyID, is_suspended ? ELobbyDestroyedReason.Creator_Disconnect_Timeout : ELobbyDestroyedReason.Creator_Destroyed );
				}
			}

			CServerLobbyBrowserManager.Instance.Stop_Browsing( player_id );

			if ( player.MatchID != EMatchInstanceID.Invalid )
			{
				CServerMatchInstanceManager.Instance.Remove_From_Match( player.MatchID, player_id, is_suspended ? EMatchRemovalReason.Player_Disconnect_Timeout :  EMatchRemovalReason.Player_Request );
			}
		}

		private void Remove_Suspended_Player( EPersistenceID removed_id )
		{
			Detach_Player_From_Game_Systems( removed_id );

			CConnectedPlayer player = Get_Player_By_Persistence_ID( removed_id );

			CLog.Log( ELoggingChannel.Logic, ELogLevel.Low, CServerResource.Get_Text( EServerTextID.Server_Player_Linkdead_Timeout_Notice, player.Name ) );
			CServerResource.Output_Text( EServerTextID.Server_Player_Linkdead_Timeout_Notice, player.Name );

			m_SuspendedPlayers.Remove( removed_id );
		}

		private void Add_Network_Operation( CNetworkOperation operation )
		{
			CServerLogicalThread.Instance.Add_Network_Operation( operation );
		}

		private void Join_Required_Channel_On_Connect( EPersistenceID player_id, EMessageRequestID hello_request_id )
		{
			CConnectedPlayer player = Get_Player_By_Persistence_ID( player_id );
			switch ( player.State )
			{
				case EConnectedPlayerState.Chat_Idle:
					CAsyncBackendOperations.Join_General_Channel( player_id, hello_request_id );
					break;

				case EConnectedPlayerState.Lobby_Idle:
					CServerMessageRouter.Send_Message_To_Session( new CClientHelloResponse( hello_request_id, CDatabaseProxy.Instance.Get_Player_Data( player_id ) ), player.SessionID );
					player.Set_Connected();
					CServerLobbyManager.Instance.On_Player_Reconnect( player_id, player.LobbyID );
					break;

				case EConnectedPlayerState.Game_Idle:
					CServerMessageRouter.Send_Message_To_Session( new CClientHelloResponse( hello_request_id, CDatabaseProxy.Instance.Get_Player_Data( player_id ) ), player.SessionID );
					player.Set_Connected();
					CServerMatchInstanceManager.Instance.On_Player_Reconnect( player_id, player.MatchID );
					break;

				case EConnectedPlayerState.Lobby_Browsing:
				case EConnectedPlayerState.Lobby_Matching:
					throw new CApplicationException( "Player in an impossible state on reconnection" );
			}
		}

		private void On_General_Chat_Creation_Response( CResponseMessage response )
		{
			CCreateChatChannelResponseServerMessage message = response as CCreateChatChannelResponseServerMessage;
			if ( message.Error != EChannelCreationError.None )
			{
				throw new CApplicationException( "Unable to initialize global chat channel." );
			}

			GeneralChatChannel = message.ChannelID;
		}

		// Properties
		public static CConnectedPlayerManager Instance { get { return m_Instance; } }

		public EChannelID GeneralChatChannel { get; private set; }

		// Fields
		private static CConnectedPlayerManager m_Instance = new CConnectedPlayerManager();

		private Dictionary< EPersistenceID, CConnectedPlayer > m_ConnectedPlayers = new Dictionary< EPersistenceID, CConnectedPlayer >();
		private Dictionary< EPersistenceID, CConnectedPlayer > m_SuspendedPlayers = new Dictionary< EPersistenceID, CConnectedPlayer >();
		private Dictionary< ESessionID, EPersistenceID > m_ActiveSessions = new Dictionary< ESessionID, EPersistenceID >();

		private Dictionary< string, EPersistenceID > m_KnownPlayers = new Dictionary< string, EPersistenceID >();

		private const string GENERAL_CHANNEL_NAME = "General";

	}
}
