/*

	ServerLobbyManager.cs

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
	public class CServerLobbyManager
	{
		// construction
		static CServerLobbyManager() {}
		private CServerLobbyManager()
		{
			m_NextLobbyID = ( ELobbyID ) ( ELobbyID.Invalid + 1 );
		}

		// Methods
		// Public interface
		public void Join_Lobby_By_Browsing( EPersistenceID player_id, ELobbyID lobby_id )
		{
			Join_Lobby_Aux( player_id, Get_Lobby( lobby_id ), EMessageRequestID.Invalid, "" );
		}

		public void Shutdown_Lobby( ELobbyID lobby_id, ELobbyDestroyedReason reason )
		{
			CLog.Log( ELoggingChannel.Lobby, ELogLevel.Medium, String.Format( "Shutting down lobby {0} for reason {1}.", 
																									Get_Lobby_Log_Description( lobby_id ),
																									reason.ToString() ) );

			CServerLobby lobby = Get_Lobby( lobby_id );
			if ( lobby == null )
			{
				return;
			}

			if ( lobby.IsPublic )
			{
				CServerLobbyBrowserManager.Instance.Notify_Browsers_Of_Removed_Lobby( lobby_id );
			}

			List< EPersistenceID > members = new List< EPersistenceID >( lobby.MemberIDs );
			members.Apply( member => Remove_From_Lobby( lobby.ID, member, Compute_Removal_Reason( reason ) ) );

			if ( lobby.ChatChannel != EChannelID.Invalid )
			{
				CDestroyChatChannelServerMessage destroy_channel_message = new CDestroyChatChannelServerMessage( lobby.ChatChannel );
				CServerMessageRouter.Send_Message_To_Chat_Server( destroy_channel_message );
			}

			m_Lobbies.Remove( lobby_id );
			m_LobbiesByCreator.Remove( lobby.Creator );
		}

		public void Remove_From_Lobby( ELobbyID lobby_id, EPersistenceID player_id, ERemovedFromLobbyReason reason )
		{
			Remove_From_Lobby( lobby_id, player_id, reason, EMessageRequestID.Invalid );
		}

		public void Remove_From_Lobby( ELobbyID lobby_id, EPersistenceID player_id, ERemovedFromLobbyReason reason, EMessageRequestID request_id )
		{
			CServerLobby lobby = Get_Lobby( lobby_id );
			if ( lobby == null )
			{
				return;
			}

			CLog.Log( ELoggingChannel.Lobby, ELogLevel.Medium, String.Format( "Removing player {0} from lobby {1} for reason {2}.",
																									CConnectedPlayerManager.Get_Player_Log_Name( player_id ), 
																									Get_Lobby_Log_Description( lobby_id ),
																									reason.ToString() ) );
			
			if ( CConnectedPlayerManager.Instance.Is_Connected( player_id ) && reason != ERemovedFromLobbyReason.Lobby_Destroyed_Game_Started )
			{
				CAsyncBackendOperations.Join_General_Channel( player_id, EMessageRequestID.Invalid );
			}
			
			CConnectedPlayer player = CConnectedPlayerManager.Instance.Get_Player_By_Persistence_ID( player_id );
			player.On_Leave_Lobby();

			CLobbyMemberLeftOperation op = new CLobbyMemberLeftOperation( player_id, reason );
			bool is_lobby_shutdown = Is_Channel_Shutdown_Reason( reason );
			if ( is_lobby_shutdown )
			{
				CServerMessageRouter.Send_Message_To_Player( new CLobbyOperationMessage( op ), player_id );
			}
			else
			{
				Send_Message_To_Members( lobby_id, new CLobbyOperationMessage( op ) );
			}

			lobby.Remove_Member( player_id, !is_lobby_shutdown );
		}

		public void On_Player_Disconnect( EPersistenceID player_id, ELobbyID lobby_id )
		{
			Change_Player_State( player_id, lobby_id, ELobbyMemberState.Disconnected );
		}

		public void On_Player_Reconnect( EPersistenceID player_id, ELobbyID lobby_id )
		{
			CServerLobby lobby = Get_Lobby( lobby_id );
			if ( lobby == null )
			{
				throw new CApplicationException( "Player reconnected to non-existent lobby." );
			}

			CLog.Log( ELoggingChannel.Lobby, ELogLevel.Medium, String.Format( "Player {0} successfully rejoined lobby {1}.",
																									CConnectedPlayerManager.Get_Player_Log_Name( player_id ), 
																									Get_Lobby_Log_Description( lobby_id ) ) );

			Change_Player_State( player_id, lobby_id, ELobbyMemberState.Not_Ready );

			CServerMessageRouter.Send_Message_To_Player( new CJoinLobbySuccess( EMessageRequestID.Invalid, lobby.Clone_State() ), player_id );

			if ( lobby.ChatChannel != EChannelID.Invalid )
			{
				CAsyncBackendOperations.Player_Join_Lobby_Channel( player_id, lobby_id, lobby.ChatChannel );
			}
		}

		public void Change_Player_State( EPersistenceID player_id, ELobbyID lobby_id, ELobbyMemberState new_state )
		{
			CServerLobby lobby = Get_Lobby( lobby_id );
			if ( lobby == null )
			{
				return;
			}

			ELobbyMemberState old_state = lobby.Get_Member_State( player_id );
			if ( old_state == new_state )
			{
				return;
			}

			CLog.Log( ELoggingChannel.Lobby, ELogLevel.Medium, String.Format( "Player {0} changed state in lobby {1} from {2} to {3}.",
																									CConnectedPlayerManager.Get_Player_Log_Name( player_id ), 
																									Get_Lobby_Log_Description( lobby_id ),
																									old_state.ToString(),
																									new_state.ToString() ) );

			lobby.Set_Member_State( player_id, new_state );

			CLobbyMemberChangeStateOperation change_state_op = new CLobbyMemberChangeStateOperation( player_id, new_state );
			Send_Message_To_Members( lobby_id, new CLobbyOperationMessage( change_state_op ) );
		}

		public CServerLobby Get_Lobby_By_Creator( EPersistenceID player_id )
		{
			ELobbyID lobby_id = ELobbyID.Invalid;
			if ( !m_LobbiesByCreator.TryGetValue( player_id, out lobby_id ) )
			{
				return null;
			}

			return Get_Lobby( lobby_id );
		}

		public static string Get_Lobby_Log_Description( ELobbyID lobby_id )
		{
			CLobby lobby = Instance.Get_Lobby( lobby_id );
			if ( lobby != null )
			{
				return lobby.GameDescription;
			}
			else
			{
				return String.Format( "Lobby#{0}", (int)lobby_id );
			}
		}

		public void On_Lobby_Chat_Channel_Creation_Response( ELobbyID lobby_id, EPersistenceID player_id, CCreateChatChannelResponseServerMessage response_msg )
		{
			CServerLobby lobby = Get_Lobby( lobby_id );
			if ( lobby == null )
			{
				if ( response_msg.ChannelID != EChannelID.Invalid )
				{
					CDestroyChatChannelServerMessage destroy_channel_message = new CDestroyChatChannelServerMessage( response_msg.ChannelID );
					CServerMessageRouter.Send_Message_To_Chat_Server( destroy_channel_message );
				}

				return;
			}

			CLog.Log( ELoggingChannel.Lobby, ELogLevel.Medium, String.Format( "Lobby {0} chat channel {1} creation result = {2}.", 
																									Get_Lobby_Log_Description( lobby_id ), 
																									response_msg.ChannelName, 
																									response_msg.Error.ToString() ) );		

			if ( response_msg.Error != EChannelCreationError.None )
			{
				Shutdown_Lobby( lobby_id, ELobbyDestroyedReason.Chat_Channel_Creation_Failure );
				return;
			}

			lobby.Initialize_Chat_Channel( response_msg.ChannelID );
			lobby.ConnectedMemberIDs.Apply( pid => CAsyncBackendOperations.Player_Join_Lobby_Channel( pid, lobby_id, lobby.ChatChannel ) );
		}

		public void On_Lobby_Chat_Channel_Join_Response( ELobbyID lobby_id, EPersistenceID player_id, CJoinChatChannelResponseServerMessage response_msg )
		{
			CLog.Log( ELoggingChannel.Lobby, ELogLevel.Medium, String.Format( "Lobby {0} chat channel join result = {1}.", 
																									Get_Lobby_Log_Description( lobby_id ), 
																									response_msg.Error.ToString() ) );		

			CServerLobby lobby = Get_Lobby( lobby_id );
			if ( lobby == null )
			{
				return;
			}

			if ( !lobby.Is_Member( player_id ) )
			{
				return;
			}

			if ( response_msg.Error != EChannelJoinError.None )
			{
				if ( player_id == lobby.Creator )
				{
					Shutdown_Lobby( lobby_id, ELobbyDestroyedReason.Chat_Channel_Join_Failure );
				}
				else
				{
					Remove_From_Lobby( lobby_id, player_id, ERemovedFromLobbyReason.Unable_To_Join_Chat_Channel );
				}
			}
		}

		// Private interface
		// Network message handlers
		[NetworkMessageHandler]
		private void Request_Create_Lobby( CCreateLobbyRequest create_request, EPersistenceID source_player )
		{
			CConnectedPlayer player = CConnectedPlayerManager.Instance.Get_Active_Player_By_Persistence_ID( source_player );
			if ( player == null )
			{
				return;
			}

			if ( !player.State_Allows_Operation( EConnectedPlayerOperation.Create_Lobby ) )
			{
				On_Create_Lobby_Failure( ELobbyID.Invalid, source_player, create_request.RequestID, ECreateLobbyFailureReason.Invalid_State_To_Create );
				return;
			}

			if ( !Validate_Create_Request( create_request.Config ) )
			{
				On_Create_Lobby_Failure( ELobbyID.Invalid, source_player, create_request.RequestID, ECreateLobbyFailureReason.Invalid_Config_Data );
				return;
			}

			CLog.Log( ELoggingChannel.Lobby, ELogLevel.Medium, String.Format( "Player {0} attempting to create lobby with description {1}.", 
																									player.Name, 
																									create_request.Config.GameDescription ) );		

			CServerLobbyBrowserManager.Instance.Stop_Browsing( source_player );

			EMessageRequestID request_id = create_request.RequestID;
			CServerLobby lobby = Create_Lobby( source_player, create_request.Config );
		
			lobby.Add_Member( source_player );

			On_Create_Lobby_Success( source_player, request_id, lobby.ID );

			CChatChannelConfig channel_config = new CChatChannelConfig( CChatChannelConfig.Make_Admin_Channel( "Lobby_" + ( ( int ) lobby.ID ).ToString() ), 
																							CSharedResource.Get_Text< EServerTextID >( EServerTextID.Server_Lobby_Channel_Name ), 
																							EPersistenceID.Invalid,
																							EChannelGameProperties.Lobby );
			channel_config.AllowsModeration = false;
			channel_config.AnnounceJoinLeave = false;
			channel_config.DestroyWhenEmpty = false;
			channel_config.IsMembershipClientLocked = true;

			CAsyncBackendOperations.Create_Lobby_Chat_Channel( channel_config, lobby.ID, source_player );
		}

		[NetworkMessageHandler]
		private void Request_Join_Lobby_By_Player( CJoinLobbyByPlayerRequest join_by_player_request, EPersistenceID source_player )
		{
			CLog.Log( ELoggingChannel.Lobby, ELogLevel.Medium, String.Format( "Player {0} attempting to join {1}'s lobby.", 
																									CConnectedPlayerManager.Get_Player_Log_Name( source_player ), 
																									join_by_player_request.PlayerName ) );

			EPersistenceID player_id = CDatabaseProxy.Instance.Get_Persistence_ID_By_Name_Sync( join_by_player_request.PlayerName );

			if ( player_id == EPersistenceID.Invalid )
			{
				On_Join_Lobby_Failure( ELobbyID.Invalid, source_player, join_by_player_request.RequestID, EJoinLobbyFailureReason.Target_Player_Does_Not_Exist );
				return;
			}

			CConnectedPlayer target_player = CConnectedPlayerManager.Instance.Get_Player_By_Persistence_ID( player_id );
			if ( target_player == null )
			{
				On_Join_Lobby_Failure( ELobbyID.Invalid, source_player, join_by_player_request.RequestID, EJoinLobbyFailureReason.Target_Player_Does_Not_Exist );
				return;
			}

			CServerLobby lobby = Get_Lobby( target_player.LobbyID );
			if ( lobby == null )
			{
				On_Join_Lobby_Failure( ELobbyID.Invalid, source_player, join_by_player_request.RequestID, EJoinLobbyFailureReason.Target_Player_Not_In_A_Lobby );
				return;
			}

			Join_Lobby_Aux( source_player, lobby, join_by_player_request.RequestID, join_by_player_request.Password );
		}

		[NetworkMessageHandler]
		private void Request_Join_Lobby_By_ID( CJoinLobbyByIDRequest join_by_id_request, EPersistenceID source_player )
		{
			CLog.Log( ELoggingChannel.Lobby, ELogLevel.Medium, String.Format( "Player {0} attempting to lobby {1}.", 
																									CConnectedPlayerManager.Get_Player_Log_Name( source_player ), 
																									Get_Lobby_Log_Description( join_by_id_request.LobbyID ) ) );

			CServerLobby lobby = Get_Lobby( join_by_id_request.LobbyID );
			if ( lobby == null )
			{
				On_Join_Lobby_Failure( ELobbyID.Invalid, source_player, join_by_id_request.RequestID, EJoinLobbyFailureReason.Lobby_Does_Not_Exist );
				return;
			}

			Join_Lobby_Aux( source_player, lobby, join_by_id_request.RequestID, "" );
		}

		[NetworkMessageHandler]
		private void Request_Leave_Lobby( CLeaveLobbyRequest request, EPersistenceID player_id )
		{
			CConnectedPlayer player = CConnectedPlayerManager.Instance.Get_Player_By_Persistence_ID( player_id );
			if ( player.LobbyID == ELobbyID.Invalid )
			{
				CServerMessageRouter.Send_Message_To_Player( new CLeaveLobbyResponse( request.RequestID, ELeaveLobbyFailureReason.Not_In_A_Lobby ), player_id );
				return;
			}

			CLog.Log( ELoggingChannel.Lobby, ELogLevel.Medium, String.Format( "Player {0} attempting to leave lobby {1}.", 
																									CConnectedPlayerManager.Get_Player_Log_Name( player_id ), 
																									Get_Lobby_Log_Description( player.LobbyID ) ) );

			CServerLobby lobby = Get_Lobby( player.LobbyID );
			if ( lobby == null )
			{
				throw new CApplicationException( "Player thinks they're in a lobby but lobby does not exist!" );
			}

			if ( !lobby.State_Allows_Player_Operations() )
			{
				CServerMessageRouter.Send_Message_To_Player( new CLeaveLobbyResponse( request.RequestID, ELeaveLobbyFailureReason.Lobby_Not_Ready ), player_id );
				return;
			}

			if ( lobby.Creator == player_id )
			{
				CServerMessageRouter.Send_Message_To_Player( new CLeaveLobbyResponse( request.RequestID, ELeaveLobbyFailureReason.Creator_Cannot_Leave ), player_id );
				return;
			}

			Remove_From_Lobby( lobby.ID, player_id, ERemovedFromLobbyReason.Self_Request );
			CServerMessageRouter.Send_Message_To_Player( new CLeaveLobbyResponse( request.RequestID, ELeaveLobbyFailureReason.None ), player_id );
		}

		[NetworkMessageHandler]
		private void Request_Destroy_Lobby( CDestroyLobbyRequest request, EPersistenceID player_id )
		{
			CConnectedPlayer player = CConnectedPlayerManager.Instance.Get_Player_By_Persistence_ID( player_id );
			if ( player.LobbyID == ELobbyID.Invalid )
			{
				CServerMessageRouter.Send_Message_To_Player( new CDestroyLobbyResponse( request.RequestID, EDestroyLobbyFailureReason.Not_In_A_Lobby ), player_id );
				return;
			}

			CLog.Log( ELoggingChannel.Lobby, ELogLevel.Medium, String.Format( "Player {0} attempting to destroy lobby {1}.", 
																									CConnectedPlayerManager.Get_Player_Log_Name( player_id ), 
																									Get_Lobby_Log_Description( player.LobbyID ) ) );

			CServerLobby lobby = Get_Lobby( player.LobbyID );
			if ( lobby == null )
			{
				throw new CApplicationException( "Player thinks they're in a lobby but lobby does not exist!" );
			}

			if ( lobby.Creator != player_id )
			{
				CServerMessageRouter.Send_Message_To_Player( new CDestroyLobbyResponse( request.RequestID, EDestroyLobbyFailureReason.Not_Creator ), player_id );
				return;
			}

			if ( !lobby.State_Allows_Player_Operations() )
			{
				CServerMessageRouter.Send_Message_To_Player( new CDestroyLobbyResponse( request.RequestID, EDestroyLobbyFailureReason.Lobby_Not_Ready ), player_id );
				return;
			}

			Shutdown_Lobby( lobby.ID, ELobbyDestroyedReason.Creator_Destroyed );
			CServerMessageRouter.Send_Message_To_Player( new CDestroyLobbyResponse( request.RequestID, EDestroyLobbyFailureReason.None ), player_id );
		}

		[NetworkMessageHandler]
		private void Request_Change_Member_State( CLobbyChangeMemberStateMessage message, EPersistenceID source_player )
		{
			ELobbyMemberState new_state = message.State;

			CConnectedPlayer player = CConnectedPlayerManager.Instance.Get_Player_By_Persistence_ID( source_player );
			if ( player == null )
			{
				return;
			}

			CServerLobby lobby = Get_Lobby( player.LobbyID );
			if ( lobby == null )
			{
				return;
			}

			if ( !lobby.State_Allows_Player_Operations() )
			{
				return;
			}

			ELobbyMemberState current_state = lobby.Get_Member_State( source_player );
			if ( current_state == ELobbyMemberState.Disconnected || new_state == ELobbyMemberState.Disconnected )
			{
				return;
			}

			Change_Player_State( source_player, player.LobbyID, new_state );
		}

		[NetworkMessageHandler]
		private void Kick_Player_From_Lobby( CKickPlayerFromLobbyRequest request, EPersistenceID source_player )
		{
			CConnectedPlayer player = CConnectedPlayerManager.Instance.Get_Player_By_Persistence_ID( source_player );
			if ( player == null )
			{
				throw new CApplicationException( "Processing a kick request from an unknown player" );
			}

			CLog.Log( ELoggingChannel.Lobby, ELogLevel.Medium, String.Format( "Player {0} in lobby {1} attempting to kick player {2}.",
																									CConnectedPlayerManager.Get_Player_Log_Name( source_player ), 
																									Get_Lobby_Log_Description( player.LobbyID ),
																									request.PlayerName ) );
			
			if ( player.LobbyID == ELobbyID.Invalid )
			{
				CServerMessageRouter.Send_Message_To_Player( new CKickPlayerFromLobbyResponse( request.RequestID, EKickPlayerFromLobbyError.Not_In_A_Lobby ), source_player );
				return;
			}

			CServerLobby lobby = Get_Lobby_By_Creator( source_player );
			if ( lobby == null )
			{
				CServerMessageRouter.Send_Message_To_Player( new CKickPlayerFromLobbyResponse( request.RequestID, EKickPlayerFromLobbyError.Not_Lobby_Creator ), source_player );
				return;
			}

			if ( !lobby.State_Allows_Player_Operations() )
			{
				CServerMessageRouter.Send_Message_To_Player( new CKickPlayerFromLobbyResponse( request.RequestID, EKickPlayerFromLobbyError.Lobby_Not_Ready ), source_player );
				return;
			}

			EPersistenceID target_player_id = CDatabaseProxy.Instance.Get_Persistence_ID_By_Name_Sync( request.PlayerName );
			if ( target_player_id == source_player )
			{
				CServerMessageRouter.Send_Message_To_Player( new CKickPlayerFromLobbyResponse( request.RequestID, EKickPlayerFromLobbyError.Cannot_Kick_Self ), source_player );
				return;
			}

			if ( !lobby.Is_Member( target_player_id ) )
			{
				CServerMessageRouter.Send_Message_To_Player( new CKickPlayerFromLobbyResponse( request.RequestID, EKickPlayerFromLobbyError.Player_Not_In_Lobby ), source_player );
				return;
			}

			Remove_From_Lobby( lobby.ID, target_player_id, ERemovedFromLobbyReason.Kicked_By_Creator );

			CServerMessageRouter.Send_Message_To_Player( new CKickPlayerFromLobbyResponse( request.RequestID, EKickPlayerFromLobbyError.None ), source_player );
		}

		[NetworkMessageHandler]
		private void Ban_Player_From_Lobby( CBanPlayerFromLobbyRequest request, EPersistenceID source_player )
		{
			CConnectedPlayer player = CConnectedPlayerManager.Instance.Get_Player_By_Persistence_ID( source_player );
			if ( player == null )
			{
				throw new CApplicationException( "Processing a ban request from an unknown player" );
			}

			CLog.Log( ELoggingChannel.Lobby, ELogLevel.Medium, String.Format( "Player {0} in lobby {1} attempting to ban player {2}.",
																									CConnectedPlayerManager.Get_Player_Log_Name( source_player ), 
																									Get_Lobby_Log_Description( player.LobbyID ),
																									request.PlayerName ) );
			
			if ( player.LobbyID == ELobbyID.Invalid )
			{
				CServerMessageRouter.Send_Message_To_Player( new CBanPlayerFromLobbyResponse( request.RequestID, EBanPlayerFromLobbyError.Not_In_A_Lobby ), source_player );
				return;
			}

			CServerLobby lobby = Get_Lobby_By_Creator( source_player );
			if ( lobby == null )
			{
				CServerMessageRouter.Send_Message_To_Player( new CBanPlayerFromLobbyResponse( request.RequestID, EBanPlayerFromLobbyError.Not_Lobby_Creator ), source_player );
				return;
			}

			if ( !lobby.State_Allows_Player_Operations() )
			{
				CServerMessageRouter.Send_Message_To_Player( new CBanPlayerFromLobbyResponse( request.RequestID, EBanPlayerFromLobbyError.Lobby_Not_Ready ), source_player );
				return;
			}

			EPersistenceID target_player_id = CDatabaseProxy.Instance.Get_Persistence_ID_By_Name_Sync( request.PlayerName );
			if ( target_player_id == source_player )
			{
				CServerMessageRouter.Send_Message_To_Player( new CBanPlayerFromLobbyResponse( request.RequestID, EBanPlayerFromLobbyError.Cannot_Ban_Self ), source_player );
				return;
			}

			if ( lobby.Is_Banned( target_player_id ) )
			{
				CServerMessageRouter.Send_Message_To_Player( new CBanPlayerFromLobbyResponse( request.RequestID, EBanPlayerFromLobbyError.Already_Banned ), source_player );
				return;
			}

			if ( lobby.Is_Member( target_player_id ) )
			{
				Remove_From_Lobby( lobby.ID, target_player_id, ERemovedFromLobbyReason.Banned_By_Creator );
			}

			lobby.Ban_Player( target_player_id );
			Send_Message_To_Members( lobby.ID, new CLobbyOperationMessage( new CLobbyPlayerBannedOperation( target_player_id ) ) );

			CServerMessageRouter.Send_Message_To_Player( new CBanPlayerFromLobbyResponse( request.RequestID, EBanPlayerFromLobbyError.None ), source_player );

			if ( lobby.IsPublic )
			{
				CServerLobbyBrowserManager.Instance.Notify_Browser_Of_Lobby_Ban_Or_Ignore( target_player_id, lobby.ID );
			}
		}

		[NetworkMessageHandler]
		private void Unban_Player_From_Lobby( CUnbanPlayerFromLobbyRequest request, EPersistenceID source_player )
		{
			CConnectedPlayer player = CConnectedPlayerManager.Instance.Get_Player_By_Persistence_ID( source_player );
			if ( player == null )
			{
				throw new CApplicationException( "Processing an unban request from an unknown player" );
			}

			CLog.Log( ELoggingChannel.Lobby, ELogLevel.Medium, String.Format( "Player {0} in lobby {1} attempting to unban player {2}.",
																									CConnectedPlayerManager.Get_Player_Log_Name( source_player ), 
																									Get_Lobby_Log_Description( player.LobbyID ),
																									request.PlayerName ) );
			
			if ( player.LobbyID == ELobbyID.Invalid )
			{
				CServerMessageRouter.Send_Message_To_Player( new CUnbanPlayerFromLobbyResponse( request.RequestID, EUnbanPlayerFromLobbyError.Not_In_A_Lobby ), source_player );
				return;
			}

			CServerLobby lobby = Get_Lobby_By_Creator( source_player );
			if ( lobby == null )
			{
				CServerMessageRouter.Send_Message_To_Player( new CUnbanPlayerFromLobbyResponse( request.RequestID, EUnbanPlayerFromLobbyError.Not_Lobby_Creator ), source_player );
				return;
			}

			if ( !lobby.State_Allows_Player_Operations() )
			{
				CServerMessageRouter.Send_Message_To_Player( new CUnbanPlayerFromLobbyResponse( request.RequestID, EUnbanPlayerFromLobbyError.Lobby_Not_Ready ), source_player );
				return;
			}

			EPersistenceID target_player_id = CDatabaseProxy.Instance.Get_Persistence_ID_By_Name_Sync( request.PlayerName );

			if ( !lobby.Is_Banned( target_player_id ) )
			{
				CServerMessageRouter.Send_Message_To_Player( new CUnbanPlayerFromLobbyResponse( request.RequestID, EUnbanPlayerFromLobbyError.Player_Not_Banned ), source_player );
				return;
			}

			lobby.Unban_Player( target_player_id );
			Send_Message_To_Members( lobby.ID, new CLobbyOperationMessage( new CLobbyPlayerUnbannedOperation( target_player_id ) ) );

			CServerMessageRouter.Send_Message_To_Player( new CUnbanPlayerFromLobbyResponse( request.RequestID, EUnbanPlayerFromLobbyError.None ), source_player );

			if ( CConnectedPlayerManager.Instance.Is_Connected( target_player_id ) )
			{
				CServerMessageRouter.Send_Message_To_Player( new CUnbannedFromLobbyNotificationMessage( player.Name ), target_player_id );
			}

			if ( lobby.IsPublic )
			{
				CServerLobbyBrowserManager.Instance.Notify_Browser_Of_Lobby_Unban_Or_Unignore( target_player_id, lobby.ID );
			}
		}

		[NetworkMessageHandler]
		private void Move_Player_In_Lobby( CMovePlayerInLobbyRequest request, EPersistenceID source_player )
		{
			CConnectedPlayer player = CConnectedPlayerManager.Instance.Get_Player_By_Persistence_ID( source_player );
			if ( player == null )
			{
				throw new CApplicationException( "Processing a move request from an unknown player" );
			}

			CLog.Log( ELoggingChannel.Lobby, ELogLevel.Medium, String.Format( "Player {0} in lobby {1} attempting to move player {2} to role={3},slot={4}.",
																									CConnectedPlayerManager.Get_Player_Log_Name( source_player ), 
																									Get_Lobby_Log_Description( player.LobbyID ),
																									CConnectedPlayerManager.Get_Player_Log_Name( request.PlayerID ),
																									request.DestinationCategory.ToString(),
																									request.DestinationIndex ) );
			
			if ( player.LobbyID == ELobbyID.Invalid )
			{
				CServerMessageRouter.Send_Message_To_Player( new CMovePlayerInLobbyResponse( request.RequestID, EMovePlayerInLobbyError.Not_In_A_Lobby ), source_player );
				return;
			}

			CServerLobby lobby = Get_Lobby_By_Creator( source_player );
			if ( lobby == null )
			{
				CServerMessageRouter.Send_Message_To_Player( new CMovePlayerInLobbyResponse( request.RequestID, EMovePlayerInLobbyError.Not_Lobby_Creator ), source_player );
				return;
			}

			if ( !lobby.State_Allows_Player_Operations() )
			{
				CServerMessageRouter.Send_Message_To_Player( new CMovePlayerInLobbyResponse( request.RequestID, EMovePlayerInLobbyError.Lobby_Not_Ready ), source_player );
				return;
			}

			bool valid_destination = false;
			switch ( request.DestinationCategory )
			{
				case ELobbyMemberType.Player:
					valid_destination = request.DestinationIndex < CGameModeUtils.Player_Count_For_Game_Mode( lobby.GameMode );
					break;

				case ELobbyMemberType.Observer:
					valid_destination = lobby.AllowObservers;
					break;

				default:
					break;
			}

			if ( !valid_destination )
			{
				CServerMessageRouter.Send_Message_To_Player( new CMovePlayerInLobbyResponse( request.RequestID, EMovePlayerInLobbyError.Invalid_Move_Destination ), source_player );
				return;
			}

			EPersistenceID destination_player = lobby.Get_Player_At( request.DestinationCategory, request.DestinationIndex );
			if ( destination_player == request.PlayerID )
			{
				CServerMessageRouter.Send_Message_To_Player( new CMovePlayerInLobbyResponse( request.RequestID, EMovePlayerInLobbyError.No_Change ), source_player );
				return;
			}

			CServerMessageRouter.Send_Message_To_Player( new CMovePlayerInLobbyResponse( request.RequestID, EMovePlayerInLobbyError.None ), source_player );
			
			if ( destination_player == EPersistenceID.Invalid )
			{
				lobby.Move_Member( request.PlayerID, request.DestinationCategory, request.DestinationIndex );

				CLobbyMemberMovedOperation op = new CLobbyMemberMovedOperation( request.PlayerID, request.DestinationCategory, request.DestinationIndex );
				Send_Message_To_Members( lobby.ID, new CLobbyOperationMessage( op ) );
			}
			else
			{
				lobby.Swap_Members( request.PlayerID, destination_player );

				CLobbyMembersSwappedOperation op = new CLobbyMembersSwappedOperation( request.PlayerID, destination_player );
				Send_Message_To_Members( lobby.ID, new CLobbyOperationMessage( op ) );
			}
		}

		[NetworkMessageHandler]
		private void Handle_Lobby_Start_Match( CLobbyStartMatchRequest request, EPersistenceID source_player )
		{
			CConnectedPlayer player = CConnectedPlayerManager.Instance.Get_Player_By_Persistence_ID( source_player );
			if ( player == null )
			{
				throw new CApplicationException( "Processing a start match request from an unknown player" );
			}

			CLog.Log( ELoggingChannel.Lobby, ELogLevel.Medium, String.Format( "Player {0} in lobby {1} attempting to start the match.",
																									CConnectedPlayerManager.Get_Player_Log_Name( source_player ), 
																									Get_Lobby_Log_Description( player.LobbyID ) ) );

			if ( player.LobbyID == ELobbyID.Invalid )
			{
				CServerMessageRouter.Send_Message_To_Player( new CLobbyStartMatchResponse( request.RequestID, EStartMatchError.Not_In_A_Lobby ), source_player );
				return;
			}

			CServerLobby lobby = Get_Lobby_By_Creator( source_player );
			if ( lobby == null )
			{
				CServerMessageRouter.Send_Message_To_Player( new CLobbyStartMatchResponse( request.RequestID, EStartMatchError.Not_Lobby_Creator ), source_player );
				return;
			}

			if ( !lobby.Are_All_Player_Slots_Filled() )
			{
				CServerMessageRouter.Send_Message_To_Player( new CLobbyStartMatchResponse( request.RequestID, EStartMatchError.Insufficient_Players ), source_player );
				return;
			}

			if ( !lobby.Are_All_Players_Ready() )
			{
				CServerMessageRouter.Send_Message_To_Player( new CLobbyStartMatchResponse( request.RequestID, EStartMatchError.Not_Everyone_Ready ), source_player );
				return;
			}

			if ( !lobby.State_Allows_Player_Operations() )
			{
				CServerMessageRouter.Send_Message_To_Player( new CLobbyStartMatchResponse( request.RequestID, EStartMatchError.Lobby_Not_Ready ), source_player );
				return;
			}

			CServerMessageRouter.Send_Message_To_Player( new CLobbyStartMatchResponse( request.RequestID, EStartMatchError.None ), source_player );

			CServerMatchInstanceManager.Instance.Transition_To_Game_Instance_From_Lobby( lobby.LobbyState );

			Shutdown_Lobby( lobby.ID, ELobbyDestroyedReason.Game_Started );
		}

		[NetworkMessageHandler]
		private void Handle_Lobby_Change_Game_Count_Request( CLobbyChangeGameCountRequest request, EPersistenceID source_player )
		{
			CConnectedPlayer player = CConnectedPlayerManager.Instance.Get_Player_By_Persistence_ID( source_player );
			if ( player == null )
			{
				throw new CApplicationException( "Processing a start match request from an unknown player" );
			}

			CLog.Log( ELoggingChannel.Lobby, ELogLevel.Medium, String.Format( "Player {0} in lobby {1} attempting to change the game count to {2}.",
																									CConnectedPlayerManager.Get_Player_Log_Name( source_player ), 
																									Get_Lobby_Log_Description( player.LobbyID ),
																									request.GameCount ) );

			if ( player.LobbyID == ELobbyID.Invalid )
			{
				CServerMessageRouter.Send_Message_To_Player( new CLobbyChangeGameCountResponse( request.RequestID, ELobbyChangeGameCountError.Not_In_A_Lobby ), source_player );
				return;
			}

			CServerLobby lobby = Get_Lobby_By_Creator( source_player );
			if ( lobby == null )
			{
				CServerMessageRouter.Send_Message_To_Player( new CLobbyChangeGameCountResponse( request.RequestID, ELobbyChangeGameCountError.Not_Lobby_Creator ), source_player );
				return;
			}

			CServerMessageRouter.Send_Message_To_Player( new CLobbyChangeGameCountResponse( request.RequestID, ELobbyChangeGameCountError.None ), source_player );

			uint current_game_count = lobby.LobbyState.GameCount;
			if ( current_game_count == request.GameCount )
			{
				return;
			}

			lobby.LobbyState.GameCount = request.GameCount;
			
			Send_Message_To_Members( lobby.ID, new CLobbyOperationMessage( new CLobbyChangeGameCountOperation( request.GameCount ) ) );
		}

		private CServerLobby Create_Lobby( EPersistenceID creator_id, CLobbyConfig config )
		{
			ELobbyID lobby_id = Allocate_Lobby_ID();
			CServerLobby lobby = new CServerLobby( lobby_id, config, creator_id );
			m_Lobbies.Add( lobby_id, lobby );
			m_LobbiesByCreator.Add( creator_id, lobby_id );

			return lobby;
		}

		private void On_Create_Lobby_Failure( ELobbyID lobby_id, EPersistenceID player_id, EMessageRequestID request_id, ECreateLobbyFailureReason reason )
		{
			CLog.Log( ELoggingChannel.Lobby, ELogLevel.Medium, String.Format( "{0}'s lobby {1} failed to create because {2}.",
																									CConnectedPlayerManager.Get_Player_Log_Name( player_id ),
																									Get_Lobby_Log_Description( lobby_id ), 
																									reason.ToString() ) );		

			Shutdown_Lobby( lobby_id, ELobbyDestroyedReason.Creation_Failure );

			CConnectedPlayer player = CConnectedPlayerManager.Instance.Get_Player_By_Persistence_ID( player_id );
			if ( player != null )
			{
				player.Change_State( EConnectedPlayerState.Chat_Idle );
			}

			if ( !CConnectedPlayerManager.Instance.Is_Connected( player_id ) )
			{
				return;
			}

			CCreateLobbyFailure create_response = new CCreateLobbyFailure( request_id, reason );
			CServerMessageRouter.Send_Message_To_Player( create_response, player_id );
		}

		private void On_Create_Lobby_Success( EPersistenceID player_id, EMessageRequestID request_id, ELobbyID lobby_id )
		{
			CServerLobby lobby = Get_Lobby( lobby_id );

			CConnectedPlayer player = CConnectedPlayerManager.Instance.Get_Player_By_Persistence_ID( player_id );
			player.On_Join_Lobby_Success( lobby_id );

			CLog.Log( ELoggingChannel.Lobby, ELogLevel.Medium, String.Format( "Player {0}'s lobby {1} successfully created.", 
																									player.Name, 
																									lobby.GameDescription ) );		

			CCreateLobbySuccess create_response = new CCreateLobbySuccess( request_id, lobby.Clone_State() );
			CServerMessageRouter.Send_Message_To_Player( create_response, player_id );

			if ( lobby.IsPublic )
			{
				CServerLobbyBrowserManager.Instance.Notify_Browsers_Of_New_Lobby( lobby_id );
			}
		}

		private void On_Join_Lobby_Failure( ELobbyID lobby_id, EPersistenceID player_id, EMessageRequestID request_id, EJoinLobbyFailureReason reason )
		{
			CLog.Log( ELoggingChannel.Lobby, ELogLevel.Medium, String.Format( "Player {0} failed to join lobby {1} because of {2}.", 
																									CConnectedPlayerManager.Get_Player_Log_Name( player_id ), 
																									Get_Lobby_Log_Description( lobby_id ),
																									reason.ToString() ) );		

			CConnectedPlayer player = CConnectedPlayerManager.Instance.Get_Player_By_Persistence_ID( player_id );
			if ( player != null )
			{
				player.Change_State( EConnectedPlayerState.Chat_Idle );
			}

			if ( !CConnectedPlayerManager.Instance.Is_Connected( player_id ) )
			{
				return;
			}

			CJoinLobbyFailure join_response = new CJoinLobbyFailure( request_id, reason );
			CServerMessageRouter.Send_Message_To_Player( join_response, player_id );
		}

		private void On_Join_Lobby_Success( EPersistenceID player_id, EMessageRequestID request_id, ELobbyID lobby_id )
		{
			CConnectedPlayer player = CConnectedPlayerManager.Instance.Get_Player_By_Persistence_ID( player_id );
			player.On_Join_Lobby_Success( lobby_id );

			CServerLobby lobby = Get_Lobby( lobby_id );

			CLog.Log( ELoggingChannel.Lobby, ELogLevel.Medium, String.Format( "Player {0} successfully joined lobby {1}.", 
																									player.Name, 
																									lobby.GameDescription ) );

			CJoinLobbySuccess join_response = new CJoinLobbySuccess( request_id, lobby.Clone_State() );
			CServerMessageRouter.Send_Message_To_Player( join_response, player_id );

			ELobbyMemberType member_type;
			uint member_index;
			lobby.Get_Member_Info( player_id, out member_index, out member_type );

			CLobbyMemberJoinedOperation op = new CLobbyMemberJoinedOperation( player_id, player.Name, member_type, member_index );
			Send_Message_To_Members( lobby_id, new CLobbyOperationMessage( op ), player_id );
		}		

		private void Join_Lobby_Aux( EPersistenceID player_id, CServerLobby lobby, EMessageRequestID request_id, string password )
		{
			CConnectedPlayer player = CConnectedPlayerManager.Instance.Get_Active_Player_By_Persistence_ID( player_id );
			if ( player == null )
			{
				throw new CApplicationException( "Processing a join request by an inactive player" );
			}

			if ( !player.State_Allows_Operation( EConnectedPlayerOperation.Join_Lobby ) )
			{
				On_Join_Lobby_Failure( ELobbyID.Invalid, player_id, request_id, EJoinLobbyFailureReason.Invalid_State_To_Join );
				return;
			}

			EJoinLobbyFailureReason join_check_result = lobby.Check_Join( player_id );
			if ( join_check_result != EJoinLobbyFailureReason.None )
			{
				On_Join_Lobby_Failure( lobby.ID, player_id, request_id, join_check_result );
				return;
			}

			CServerLobbyBrowserManager.Instance.Stop_Browsing( player_id );

			lobby.Add_Member( player_id );
			On_Join_Lobby_Success( player_id, request_id, lobby.ID );

			if ( lobby.ChatChannel != EChannelID.Invalid )
			{
				CAsyncBackendOperations.Player_Join_Lobby_Channel( player.PersistenceID, lobby.ID, lobby.ChatChannel );
			}
		}

		private ERemovedFromLobbyReason Compute_Removal_Reason( ELobbyDestroyedReason destroy_reason )
		{
			switch ( destroy_reason )
			{
				case ELobbyDestroyedReason.Creation_Failure:
					return ERemovedFromLobbyReason.Lobby_Destroyed_Creation_Failure;

				case ELobbyDestroyedReason.Creator_Destroyed:
					return ERemovedFromLobbyReason.Lobby_Destroyed_By_Creator;

				case ELobbyDestroyedReason.Creator_Disconnect_Timeout:
					return ERemovedFromLobbyReason.Lobby_Destroyed_Due_To_Disconnect_Timeout;

				case ELobbyDestroyedReason.Game_Started:
					return ERemovedFromLobbyReason.Lobby_Destroyed_Game_Started;

				case ELobbyDestroyedReason.Chat_Channel_Creation_Failure:
				case ELobbyDestroyedReason.Chat_Channel_Join_Failure:
					return ERemovedFromLobbyReason.Unable_To_Join_Chat_Channel;
			}

			return ERemovedFromLobbyReason.Unknown;
		}

		private bool Is_Channel_Shutdown_Reason( ERemovedFromLobbyReason reason )
		{
			switch ( reason )
			{
				case ERemovedFromLobbyReason.Lobby_Destroyed_By_Creator:
				case ERemovedFromLobbyReason.Lobby_Destroyed_Creation_Failure:
				case ERemovedFromLobbyReason.Lobby_Destroyed_Game_Started:
				case ERemovedFromLobbyReason.Lobby_Destroyed_Due_To_Disconnect_Timeout:
					return true;

				default:
					return false;
			}
		}

		private bool Validate_Create_Request( CLobbyConfig config )
		{
			return true;
		}

		private ELobbyID Allocate_Lobby_ID()
		{
			ELobbyID new_lobby_id = m_NextLobbyID;
			m_NextLobbyID++;

			return new_lobby_id;
		}

		public CServerLobby Get_Lobby( ELobbyID lobby_id )
		{
			CServerLobby lobby = null;
			if ( !m_Lobbies.TryGetValue( lobby_id, out lobby ) )
			{
				return null;
			}

			return lobby;
		}

		private void Send_Message_To_Members( ELobbyID lobby_id, CNetworkMessage message )
		{
			Send_Message_To_Members( lobby_id, message, EPersistenceID.Invalid );
		}

		private void Send_Message_To_Members( ELobbyID lobby_id, CNetworkMessage message, EPersistenceID exclude_id )
		{
			CServerLobby lobby = Get_Lobby( lobby_id );
			if ( lobby == null )
			{
				return;
			}

			lobby.ConnectedMemberIDs.Where( cid => cid != exclude_id ).Apply( id => CServerMessageRouter.Send_Message_To_Player( message, id ) );
		}

		// properties
		public static CServerLobbyManager Instance { get { return m_Instance; } }

		// fields
		private static CServerLobbyManager m_Instance = new CServerLobbyManager();

		private Dictionary< ELobbyID, CServerLobby > m_Lobbies = new Dictionary< ELobbyID, CServerLobby >();
		private ELobbyID m_NextLobbyID;

		private Dictionary< EPersistenceID, ELobbyID > m_LobbiesByCreator = new Dictionary< EPersistenceID, ELobbyID >();

	}
}
