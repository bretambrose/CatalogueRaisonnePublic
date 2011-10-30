/*

	ClientLobbyManager.cs

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

using CRShared;

namespace CRClientShared
{
	public class CClientLobbyManager
	{
		// constructors
		private CClientLobbyManager() {}
		static CClientLobbyManager() {}

		// Methods
		// Public interface
		public void Reset()
		{
			Lobby = null;
		}
		 
		// Non-public interface
		// Slash command handlers
		[GenericHandler]
		private void Handle_Create_Lobby_Command( CCreateLobbySlashCommand command )
		{
			CLobbyConfig config = new CLobbyConfig();
			config.Initialize( command.GameDescription, command.GameMode, command.AllowObservers, command.Password );

			CClientLogicalThread.Instance.Send_Message_To_Server( new CCreateLobbyRequest( config ) );
		}

		[GenericHandler]
		private void Handle_Join_Lobby_By_Player_Command( CJoinLobbyByPlayerSlashCommand command )
		{
			CClientLogicalThread.Instance.Send_Message_To_Server( new CJoinLobbyByPlayerRequest( command.PlayerName, command.Password ) );
		}

		[GenericHandler]
		private void Handle_Join_Lobby_By_ID_Command( CJoinLobbyByIDSlashCommand command )
		{
			CClientLogicalThread.Instance.Send_Message_To_Server( new CJoinLobbyByIDRequest( (ELobbyID) command.LobbyID ) );
		}

		[GenericHandler]
		private void Handle_Lobby_Info_Command( CLobbyInfoSlashCommand command )
		{
			Dump_Lobby_To_Console();
		}

		[GenericHandler]
		private void Handle_Lobby_Destroy_Command( CLobbyDestroySlashCommand command )
		{
			CClientLogicalThread.Instance.Send_Message_To_Server( new CDestroyLobbyRequest() );
		}

		[GenericHandler]
		private void Handle_Lobby_Leave_Command( CLobbyLeaveSlashCommand command )
		{
			CClientLogicalThread.Instance.Send_Message_To_Server( new CLeaveLobbyRequest() );
		}

		[GenericHandler]
		private void Handle_Lobby_Kick_Command( CLobbyKickSlashCommand command )
		{
			CClientLogicalThread.Instance.Send_Message_To_Server( new CKickPlayerFromLobbyRequest( command.PlayerName ) );
		}

		[GenericHandler]
		private void Handle_Lobby_Ban_Command( CLobbyBanSlashCommand command )
		{
			CClientLogicalThread.Instance.Send_Message_To_Server( new CBanPlayerFromLobbyRequest( command.PlayerName ) );
		}

		[GenericHandler]
		private void Handle_Lobby_Unban_Command( CLobbyUnbanSlashCommand command )
		{
			CClientLogicalThread.Instance.Send_Message_To_Server( new CUnbanPlayerFromLobbyRequest( command.PlayerName ) );
		}

		[GenericHandler]
		private void Handle_Change_Lobby_Player_State_Command( CChangeLobbyPlayerStateSlashCommand command )
		{
			CClientLogicalThread.Instance.Send_Message_To_Server( new CLobbyChangeMemberStateMessage( command.State ) );
		}

		[GenericHandler]
		private void Handle_Lobby_Move_Command( CLobbyMoveSlashCommand command )
		{
			EPersistenceID player_id = CClientPlayerInfoManager.Instance.Get_Player_ID_By_Name( command.PlayerName );
			if ( player_id == EPersistenceID.Invalid )
			{
				CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Move_Unknown_Player, command.PlayerName );
				return;
			}

			CClientLogicalThread.Instance.Send_Message_To_Server( new CMovePlayerInLobbyRequest( player_id, command.DestinationCategory, command.DestinationIndex ) );
		}

		[GenericHandler]
		private void Handle_Lobby_Start_Match_Command( CLobbyStartMatchSlashCommand command )
		{
			CClientLogicalThread.Instance.Send_Message_To_Server( new CLobbyStartMatchRequest() );
		}

		[GenericHandler]
		private void Handle_Lobby_Change_Game_Count_Command( CLobbyChangeGameCountSlashCommand command )
		{
			CClientLogicalThread.Instance.Send_Message_To_Server( new CLobbyChangeGameCountRequest( command.GameCount ) );
		}

		// Network message handlers
		[NetworkMessageHandler]
		private void Handle_Create_Failure( CCreateLobbyFailure message )
		{
			switch ( message.Reason )
			{
				case ECreateLobbyFailureReason.Invalid_Config_Data:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Create_Invalid_Config_Data );
					break;

				case ECreateLobbyFailureReason.Invalid_State_To_Create:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Create_Invalid_Creator_State );
					break;
			}
		}

		[NetworkMessageHandler]
		private void Handle_Create_Success( CCreateLobbySuccess message )
		{
			CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Create_Success );
			Join_Lobby( message.LobbyState );
		}

		[NetworkMessageHandler]
		private void Handle_Join_Failure( CJoinLobbyFailure message )
		{
			switch ( message.Reason )
			{
				case EJoinLobbyFailureReason.Lobby_Full:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Join_Lobby_Full );
					break;

				case EJoinLobbyFailureReason.Password_Mismatch:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Join_Password_Mismatch );
					break;

				case EJoinLobbyFailureReason.Creator_Is_Ignoring_You:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Join_Creator_Is_Ignoring_You );
					break;

				case EJoinLobbyFailureReason.Target_Player_Does_Not_Exist:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Join_Player_Does_Not_Exist );
					break;

				case EJoinLobbyFailureReason.Target_Player_Not_In_A_Lobby:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Join_Player_Not_In_A_Lobby );
					break;

				case EJoinLobbyFailureReason.Invalid_State_To_Join:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Join_Invalid_State_To_Join );
					break;

				case EJoinLobbyFailureReason.Banned:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Join_Banned );
					break;

				case EJoinLobbyFailureReason.Lobby_Does_Not_Exist:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Join_Lobby_Does_Not_Exist );
					break;					
			}
		}
	
		[NetworkMessageHandler]
		private void Handle_Join_Success( CJoinLobbySuccess message )
		{
			CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Join_Success );
			Join_Lobby( message.LobbyState );
		}
	
		[NetworkMessageHandler]
		private void Handle_Leave_Response( CLeaveLobbyResponse message )
		{
			switch ( message.Reason )
			{
				case ELeaveLobbyFailureReason.Creator_Cannot_Leave:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Leave_Creator_Cannot_Leave );
					break;

				case ELeaveLobbyFailureReason.Lobby_Not_Ready:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Leave_Lobby_Not_Ready );
					break;

				case ELeaveLobbyFailureReason.Not_In_A_Lobby:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Leave_Not_In_A_Lobby );
					break;

				case ELeaveLobbyFailureReason.Unknown_Lobby:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Leave_Unknown_Lobby );
					break;
			}
		}

		[NetworkMessageHandler]
		private void Handle_Destroy_Response( CDestroyLobbyResponse message )
		{
			switch ( message.Reason )
			{
				case EDestroyLobbyFailureReason.Not_Creator:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Destroy_Not_Creator );
					break;

				case EDestroyLobbyFailureReason.Not_In_A_Lobby:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Destroy_Not_In_A_Lobby );
					break;

				case EDestroyLobbyFailureReason.Lobby_Not_Ready:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Destroy_Lobby_Not_Ready );
					break;
			}
		}

		[NetworkMessageHandler]
		private void Handle_Kick_Response( CKickPlayerFromLobbyResponse message )
		{
			switch ( message.Reason )
			{
				case EKickPlayerFromLobbyError.Cannot_Kick_Self:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Kick_Cannot_Kick_Self );
					break;

				case EKickPlayerFromLobbyError.Not_In_A_Lobby:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Kick_Not_In_A_Lobby );
					break;

				case EKickPlayerFromLobbyError.Not_Lobby_Creator:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Kick_Not_Lobby_Creator );
					break;

				case EKickPlayerFromLobbyError.Player_Not_In_Lobby:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Kick_Player_Not_In_Lobby );
					break;

				case EKickPlayerFromLobbyError.Lobby_Not_Ready:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Kick_Lobby_Not_Ready );
					break;
			}
		}

		[NetworkMessageHandler]
		private void Handle_Ban_Response( CBanPlayerFromLobbyResponse message )
		{
			switch ( message.Reason )
			{
				case EBanPlayerFromLobbyError.Cannot_Ban_Self:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Ban_Cannot_Ban_Self );
					break;

				case EBanPlayerFromLobbyError.Not_In_A_Lobby:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Ban_Not_In_A_Lobby );
					break;

				case EBanPlayerFromLobbyError.Not_Lobby_Creator:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Ban_Not_Lobby_Creator );
					break;

				case EBanPlayerFromLobbyError.Already_Banned:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Ban_Already_Banned );
					break;

				case EBanPlayerFromLobbyError.Lobby_Not_Ready:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Ban_Lobby_Not_Ready );
					break;

				case EBanPlayerFromLobbyError.None:
					CBanPlayerFromLobbyRequest ban_request = message.Request as CBanPlayerFromLobbyRequest;
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Ban_Successful, ban_request.PlayerName );
					break;
			}
		}
				
		[NetworkMessageHandler]
		private void Handle_Unban_Response( CUnbanPlayerFromLobbyResponse message )
		{
			switch ( message.Reason )
			{
				case EUnbanPlayerFromLobbyError.Not_In_A_Lobby:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Unban_Not_In_A_Lobby );
					break;

				case EUnbanPlayerFromLobbyError.Not_Lobby_Creator:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Unban_Not_Lobby_Creator );
					break;

				case EUnbanPlayerFromLobbyError.Player_Not_Banned:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Unban_Player_Not_Banned );
					break;

				case EUnbanPlayerFromLobbyError.Lobby_Not_Ready:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Unban_Lobby_Not_Ready );
					break;

				case EUnbanPlayerFromLobbyError.None:
					CUnbanPlayerFromLobbyRequest unban_request = message.Request as CUnbanPlayerFromLobbyRequest;
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Unban_Successful, unban_request.PlayerName );
					break;
			}
		}

		[NetworkMessageHandler]
		private void Handle_Move_Response( CMovePlayerInLobbyResponse message )
		{
			switch ( message.Reason )
			{
				case EMovePlayerInLobbyError.Not_In_A_Lobby:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Move_Not_In_A_Lobby );
					break;

				case EMovePlayerInLobbyError.Not_Lobby_Creator:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Move_Not_Lobby_Creator );
					break;

				case EMovePlayerInLobbyError.Player_Not_In_Lobby:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Move_Player_Not_In_Lobby );
					break;

				case EMovePlayerInLobbyError.Invalid_Move_Destination:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Move_Invalid_Move_Destination );
					break;

				case EMovePlayerInLobbyError.Lobby_Not_Ready:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Move_Lobby_Not_Ready );
					break;

				case EMovePlayerInLobbyError.No_Change:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Move_No_Change );
					break;
			}
		}

		[NetworkMessageHandler]
		private void Handle_Start_Match_Response( CLobbyStartMatchResponse response )
		{
			switch ( response.Error )
			{
				case EStartMatchError.Insufficient_Players:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Start_Match_Insufficient_Players );
					break;

				case EStartMatchError.Not_Everyone_Ready:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Start_Match_Not_Everyone_Ready );
					break;

				case EStartMatchError.Not_Lobby_Creator:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Start_Match_Not_Lobby_Creator );
					break;

				case EStartMatchError.Not_In_A_Lobby:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Start_Match_Not_In_A_Lobby );
					break;

				case EStartMatchError.Lobby_Not_Ready:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Start_Match_Lobby_Not_Ready );
					break;

			}
		}

		[NetworkMessageHandler]
		private void Handle_Start_Match_Response( CLobbyChangeGameCountResponse response )
		{
			switch ( response.Error )
			{
				case ELobbyChangeGameCountError.Not_In_A_Lobby:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Change_Game_Count_Not_In_A_Lobby );
					break;

				case ELobbyChangeGameCountError.Not_Lobby_Creator:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Change_Game_Count_Not_Lobby_Creator );
					break;
			}
		}

		[NetworkMessageHandler]
		private void Handle_Lobby_Operation( CLobbyOperationMessage message )
		{
			if ( Lobby == null || !CClientLogicalThread.Instance.IsConnected )
			{
				return;
			}

			CLobbyOperation operation = message.Operation;

			switch ( operation.Type )
			{
				case ELobbyOperation.Member_Joined:
					CLobbyMemberJoinedOperation join_op = operation as CLobbyMemberJoinedOperation;
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Player_Joined, join_op.PlayerName );
					CClientPlayerInfoManager.Instance.Begin_Player_Listen( join_op.PlayerID, join_op.PlayerName, EPlayerListenReason.In_Lobby );
					break;

				case ELobbyOperation.Member_Left:
					CLobbyMemberLeftOperation leave_op = operation as CLobbyMemberLeftOperation;

					string player_name = CClientPlayerInfoManager.Instance.Get_Player_Name( leave_op.PlayerID );
					string creator_name = CClientPlayerInfoManager.Instance.Get_Player_Name( Lobby.Creator );
					EClientTextID leave_lobby_id = Compute_Leave_Lobby_Text_ID( leave_op.Reason, leave_op.PlayerID, Lobby.Creator );
					if ( leave_lobby_id != EClientTextID.Invalid )
					{
						CClientResource.Output_Text< EClientTextID >( leave_lobby_id, player_name, creator_name );
					}

					if ( leave_op.PlayerID == CClientLogicalThread.Instance.ConnectedID )
					{
						Lobby = null;
						CClientLogicalThread.Instance.Add_UI_Notification( new CUIScreenStateNotification( EUIScreenState.Chat_Idle ) );
					}

					CClientPlayerInfoManager.Instance.End_Player_Listen( leave_op.PlayerID, EPlayerListenReason.In_Lobby );
					break;

				case ELobbyOperation.Player_Banned:
					CLobbyPlayerBannedOperation ban_op = operation as CLobbyPlayerBannedOperation;
					CClientPlayerInfoManager.Instance.Begin_Player_Listen( ban_op.PlayerID, EPlayerListenReason.Banned_From_Lobby );
					break;

				case ELobbyOperation.Player_Unbanned:
					CLobbyPlayerUnbannedOperation unban_op = operation as CLobbyPlayerUnbannedOperation;
					CClientPlayerInfoManager.Instance.End_Player_Listen( unban_op.PlayerID, EPlayerListenReason.Banned_From_Lobby );
					break;

				case ELobbyOperation.Change_Game_Count:
					On_Lobby_Game_Count_Change( operation as CLobbyChangeGameCountOperation );
					break;

				case ELobbyOperation.Member_Change_State:
					On_Lobby_State_Change( operation as CLobbyMemberChangeStateOperation );
					break;
			}

			if ( Lobby != null )
			{
				Lobby.Apply_Operation( operation );
			}
		}

		[NetworkMessageHandler]
		private void Handle_Unbanned_From_Lobby_Notification( CUnbannedFromLobbyNotificationMessage message )
		{
			CSharedResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Unbanned, message.PlayerName );
		}

		private void On_Lobby_Game_Count_Change( CLobbyChangeGameCountOperation operation )
		{
			if ( operation.GameCount > 0 )
			{
				if ( Lobby.Creator == CClientLogicalThread.Instance.ConnectedID )
				{
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Game_Count_Changed_Finite_Self, operation.GameCount );
				}
				else
				{
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Game_Count_Changed_Finite_Other, 
																					operation.GameCount, 
																					CClientPlayerInfoManager.Instance.Get_Player_Name( Lobby.Creator ) );
				}
			}
			else
			{
				if ( Lobby.Creator == CClientLogicalThread.Instance.ConnectedID )
				{
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Game_Count_Changed_Infinite_Self );
				}
				else
				{
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Game_Count_Changed_Infinite_Other, 
																					CClientPlayerInfoManager.Instance.Get_Player_Name( Lobby.Creator ) );
				}
			}
		}

		private void On_Lobby_State_Change( CLobbyMemberChangeStateOperation operation )
		{
			bool is_self = operation.PlayerID == CClientLogicalThread.Instance.ConnectedID;
			switch ( operation.State )
			{
				case ELobbyMemberState.Disconnected:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_State_Change_Player_Disconnected, 
																					CClientPlayerInfoManager.Instance.Get_Player_Name( operation.PlayerID ) );
					break;

				case ELobbyMemberState.Not_Ready:
					ELobbyMemberState previous_state = Lobby.Get_Member_State( operation.PlayerID );
					if ( previous_state == ELobbyMemberState.Ready )
					{
						if ( is_self )
						{
							CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_State_Change_Self_Not_Ready );
						}
						else
						{
							CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_State_Change_Player_Not_Ready, 
																						 CClientPlayerInfoManager.Instance.Get_Player_Name( operation.PlayerID ) );
						}
					}
					else
					{
						if ( !is_self )
						{
							CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_State_Change_Player_Reconnected, 
																						 CClientPlayerInfoManager.Instance.Get_Player_Name( operation.PlayerID ) );
						}
					}
					break;

				case ELobbyMemberState.Ready:
					if ( is_self )
					{
						CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_State_Change_Self_Ready );
					}
					else
					{
						CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_State_Change_Player_Ready, 
																						CClientPlayerInfoManager.Instance.Get_Player_Name( operation.PlayerID ) );
					}
					break;
			}
		}

		private void Join_Lobby( CLobbyState lobby_state )
		{
			if ( Lobby != null )
			{
				throw new CApplicationException( "Joined a second lobby while one still exists on the client." );
			}

			Lobby = new CClientLobby( lobby_state );
			Create_Player_Infos_For_Lobby_Members( Lobby );

			CClientLogicalThread.Instance.Add_UI_Notification( new CUIScreenStateNotification( EUIScreenState.Lobby ) );
		}

		private EClientTextID Compute_Leave_Lobby_Text_ID( ERemovedFromLobbyReason reason, EPersistenceID leaving_player_id, EPersistenceID creator_id )
		{
			EPersistenceID local_player_id = CClientLogicalThread.Instance.ConnectedID;
			switch ( reason )
			{
				case ERemovedFromLobbyReason.Kicked_By_Creator:
					if ( local_player_id == leaving_player_id )
					{
						return EClientTextID.Client_Lobby_Leave_You_Were_Kicked;
					}
					else if ( local_player_id == creator_id )
					{
						return EClientTextID.Client_Lobby_Leave_You_Kicked_Someone;
					}
					else
					{
						return EClientTextID.Client_Lobby_Leave_Someone_Was_Kicked;
					}

				case ERemovedFromLobbyReason.Banned_By_Creator:
					if ( local_player_id == leaving_player_id )
					{
						return EClientTextID.Client_Lobby_Leave_You_Were_Banned;
					}
					else if ( local_player_id == creator_id )
					{
						return EClientTextID.Invalid;	// in this case, we just want to print the success message from the ban result message
					}
					else
					{
						return EClientTextID.Client_Lobby_Leave_Someone_Was_Banned;
					}

				case ERemovedFromLobbyReason.Disconnect_Timeout:
					return EClientTextID.Client_Lobby_Leave_Disconnect_Timeout;
				
				case ERemovedFromLobbyReason.Lobby_Destroyed_Due_To_Disconnect_Timeout:
					return EClientTextID.Client_Lobby_Leave_Creator_Disconnect_Timeout;

				case ERemovedFromLobbyReason.Lobby_Destroyed_By_Creator:
					if ( local_player_id == creator_id )
					{
						return EClientTextID.Client_Lobby_Leave_You_Destroyed_The_Lobby;
					}
					else
					{
						return EClientTextID.Client_Lobby_Leave_Lobby_Destroyed;
					}

				case ERemovedFromLobbyReason.Lobby_Destroyed_Creation_Failure:
					return EClientTextID.Invalid;

				case ERemovedFromLobbyReason.Lobby_Destroyed_Game_Started:
					return EClientTextID.Invalid;

				case ERemovedFromLobbyReason.Self_Request:
					if ( local_player_id == leaving_player_id )
					{
						return EClientTextID.Client_Lobby_Leave_You_Left;
					}
					else
					{
						return EClientTextID.Client_Lobby_Leave_Player_Left;
					}

				case ERemovedFromLobbyReason.Unknown:
					return EClientTextID.Client_Lobby_Leave_Unknown_Reason;

				case ERemovedFromLobbyReason.Rejoin_Failure:
					return EClientTextID.Client_Lobby_Leave_Rejoin_Failure;
			}

			return EClientTextID.Invalid;
		}

		private void Create_Player_Infos_For_Lobby_Members( CClientLobby lobby )
		{
			lobby.MemberIDs.Apply( id => CClientPlayerInfoManager.Instance.Begin_Player_Listen( id, EPlayerListenReason.In_Lobby ) );
		}

		private void Dump_Lobby_To_Console()
		{
			if ( Lobby == null )
			{
				CSharedResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_No_Info_Possible );
				return;
			}

			CSharedResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Info_Header );
			CSharedResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Info_Settings,
																				(int)Lobby.ID,
																				Lobby.GameDescription,
																				Lobby.GameMode.ToString(),
																				Lobby.AllowObservers,
																				Lobby.Password,
																				Lobby.CreationTime,
																				CClientPlayerInfoManager.Instance.Get_Player_Name( Lobby.Creator ) );

			CSharedResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Info_Member_Info_Format,
																				CSharedResource.Get_Text< EClientTextID >( EClientTextID.Client_Lobby_Info_Member_Info_Header_Name ),
																				CSharedResource.Get_Text< EClientTextID >( EClientTextID.Client_Lobby_Info_Member_Info_Header_ID ),
																				CSharedResource.Get_Text< EClientTextID >( EClientTextID.Client_Lobby_Info_Member_Info_Header_Type ),
																				CSharedResource.Get_Text< EClientTextID >( EClientTextID.Client_Lobby_Info_Member_Info_Header_Index ),
																				CSharedResource.Get_Text< EClientTextID >( EClientTextID.Client_Lobby_Info_Member_Info_Header_Ready ) );
			foreach ( var member in Lobby.Members )
			{
				string member_name = CClientPlayerInfoManager.Instance.Get_Player_Name( member.ID );

				uint member_index = 0;
				ELobbyMemberType member_type = ELobbyMemberType.Invalid;
				Lobby.Get_Member_Info( member.ID, out member_index, out member_type );

				string member_state = "";
				if ( member_type == ELobbyMemberType.Player )
				{
					switch ( member.State )
					{
						case ELobbyMemberState.Ready:
							member_state = CSharedResource.Get_Text< EClientTextID >( EClientTextID.Client_Lobby_Player_Ready );
							break;

						case ELobbyMemberState.Not_Ready:
							member_state = CSharedResource.Get_Text< EClientTextID >( EClientTextID.Client_Lobby_Player_Not_Ready );
							break;

						case ELobbyMemberState.Disconnected:
							member_state = CSharedResource.Get_Text< EClientTextID >( EClientTextID.Client_Lobby_Player_Disconnected );
							break;

					}
				}

				CSharedResource.Output_Text< EClientTextID >( EClientTextID.Client_Lobby_Info_Member_Info_Format,																				
																					member_name,
																					member.ID,
																					member_type.ToString(),
																					member_index,
																					member_state );
			}
		}
		
		// Properties
		public static CClientLobbyManager Instance { get { return m_Instance; } }
		public CClientLobby Lobby { get; private set; }

		// Fields
		private static CClientLobbyManager m_Instance = new CClientLobbyManager();
	}
}

