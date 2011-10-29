using System;
using System.Collections.Generic;

using CRShared;

namespace CRClientShared
{
	public class CClientLobby : CLobby
	{
		// constructors
		public CClientLobby( CLobbyState lobby_state ) :
			base( lobby_state )
		{}

		// Methods
		// Public interface
		public void Apply_Operation( CLobbyOperation operation )
		{
			switch ( operation.Type )
			{
				case ELobbyOperation.Member_Joined:
					CLobbyMemberJoinedOperation join_operation = operation as CLobbyMemberJoinedOperation;
					Apply_Member_Joined_Operation( join_operation );
					break;

				case ELobbyOperation.Member_Left:
					CLobbyMemberLeftOperation left_operation = operation as CLobbyMemberLeftOperation;
					Apply_Member_Left_Operation( left_operation );
					break;

				case ELobbyOperation.Member_Change_State:
					CLobbyMemberChangeStateOperation change_state_operation = operation as CLobbyMemberChangeStateOperation;
					Apply_Member_Change_State_Operation( change_state_operation );
					break;

				case ELobbyOperation.Member_Moved:
					CLobbyMemberMovedOperation move_operation = operation as CLobbyMemberMovedOperation;
					Apply_Member_Moved_Operation( move_operation );
					break;

				case ELobbyOperation.Members_Swapped:
					CLobbyMembersSwappedOperation swap_operation = operation as CLobbyMembersSwappedOperation;
					Apply_Members_Swapped_Operation( swap_operation );
					break;
				
				case ELobbyOperation.Change_Game_Count:
					CLobbyChangeGameCountOperation change_game_count_op = operation as CLobbyChangeGameCountOperation;
					Apply_Change_Game_Count_Operation( change_game_count_op );
					break;
			}
		}

		// Non-public interface
		private void Apply_Member_Moved_Operation( CLobbyMemberMovedOperation operation )
		{
			Move_Member( operation.PlayerID, operation.MemberType, operation.MemberIndex );
		}

		private void Apply_Members_Swapped_Operation( CLobbyMembersSwappedOperation operation )
		{
			Swap_Members( operation.SourcePlayerID, operation.TargetPlayerID );
		}

		private void Apply_Change_Game_Count_Operation( CLobbyChangeGameCountOperation operation )
		{
			LobbyState.GameCount = operation.GameCount;
		}

		private void Apply_Member_Joined_Operation( CLobbyMemberJoinedOperation operation )
		{
			if ( operation.MemberType == ELobbyMemberType.Observer )
			{
				LobbyState.Observers.Add( operation.PlayerID );
			}
			else if ( operation.MemberType == ELobbyMemberType.Player )
			{
				Dictionary< uint, EPersistenceID > players = LobbyState.PlayersBySlot;
				if ( players.ContainsKey( operation.MemberIndex ) )
				{
					throw new CApplicationException( "Lobby member collision" );
				}

				players.Add( operation.MemberIndex, operation.PlayerID );

				Mark_Players_Not_Ready();
			}
			else
			{
				throw new CApplicationException( "Invalid member type in lobby join operation." );
			}


			LobbyState.Members.Add( operation.PlayerID, new CLobbyMember( operation.PlayerID ) );
		}

		private void Apply_Member_Left_Operation( CLobbyMemberLeftOperation operation )
		{
			Remove_Member( operation.PlayerID, false );
		}

		private void Apply_Member_Change_State_Operation( CLobbyMemberChangeStateOperation operation )
		{
			LobbyState.Members[ operation.PlayerID ].State = operation.State;
		}
	}
}
