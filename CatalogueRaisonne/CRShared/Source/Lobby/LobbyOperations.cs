/*

	LobbyOperations.cs

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

namespace CRShared
{
	public enum ELobbyOperation
	{
		Member_Joined,
		Member_Left,
		Member_Moved,
		Members_Swapped,
		Member_Change_State,
		Player_Banned,
		Player_Unbanned,
		Change_Game_Count
	}

	public abstract class CLobbyOperation
	{
		public abstract ELobbyOperation Type { get; }
	}

	public class CLobbyMemberJoinedOperation : CLobbyOperation
	{
		// Construction
		public CLobbyMemberJoinedOperation( EPersistenceID player_id, string player_name, ELobbyMemberType member_type, uint member_index )
		{
			PlayerID = player_id;
			PlayerName = player_name;
			MemberType = member_type;
			MemberIndex = member_index;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CLobbyMemberJoinedOperation] PlayerID = {0}, PlayerName = {1}, MemberType = {2}, MemberIndex = {3} )",
										 (long)PlayerID,
										 PlayerName,
										 MemberType.ToString(),
										 MemberIndex );
		}

		// Properties
		public override ELobbyOperation Type { get { return ELobbyOperation.Member_Joined; } }

		public EPersistenceID PlayerID { get; private set; }
		public string PlayerName { get; private set; }
		public ELobbyMemberType MemberType { get; private set; }
		public uint MemberIndex { get; private set; }
	}

	public class CLobbyMemberLeftOperation : CLobbyOperation
	{
		// construction
		public CLobbyMemberLeftOperation( EPersistenceID player_id, ERemovedFromLobbyReason reason )
		{
			PlayerID = player_id;
			Reason = reason;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CLobbyMemberLeftOperation] PlayerID = {0}, Reason = {1} )",
										 (long)PlayerID,
										 Reason.ToString() );
		}

		// Properties
		public override ELobbyOperation Type { get { return ELobbyOperation.Member_Left; } }

		public EPersistenceID PlayerID { get; private set; }
		public ERemovedFromLobbyReason Reason { get; private set; }
	}

	public class CLobbyMemberMovedOperation : CLobbyOperation
	{
		// Construction
		public CLobbyMemberMovedOperation( EPersistenceID player_id, ELobbyMemberType member_type, uint member_index )
		{
			PlayerID = player_id;
			MemberType = member_type;
			MemberIndex = member_index;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CLobbyMemberMovedOperation] PlayerID = {0}, MemberType = {1}, MemberIndex = {2} )",
										 (long)PlayerID,
										 MemberType.ToString(),
										 MemberIndex );
		}

		// Properties
		public override ELobbyOperation Type { get { return ELobbyOperation.Member_Moved; } }

		public EPersistenceID PlayerID { get; private set; }
		public ELobbyMemberType MemberType { get; private set; }
		public uint MemberIndex { get; private set; }
	}

	public class CLobbyMembersSwappedOperation : CLobbyOperation
	{
		// Construction
		public CLobbyMembersSwappedOperation( EPersistenceID source_player_id, EPersistenceID target_player_id )
		{
			SourcePlayerID = source_player_id;
			TargetPlayerID = target_player_id;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CLobbyMembersSwappedOperation] SourcePlayerID = {0}, TargetPlayerID = {1} )",
										 (long)SourcePlayerID,
										 (long)TargetPlayerID );
		}

		// Properties
		public override ELobbyOperation Type { get { return ELobbyOperation.Members_Swapped; } }

		public EPersistenceID SourcePlayerID { get; private set; }
		public EPersistenceID TargetPlayerID { get; private set; }
	}

	public class CLobbyMemberChangeStateOperation : CLobbyOperation
	{
		// Construction
		public CLobbyMemberChangeStateOperation( EPersistenceID player_id, ELobbyMemberState state )
		{
			PlayerID = player_id;
			State = state;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CLobbyMemberChangeStateOperation] PlayerID = {0}, State = {1} )",
										 (long)PlayerID,
										 State.ToString() );
		}

		// Properties
		public override ELobbyOperation Type { get { return ELobbyOperation.Member_Change_State; } }

		public EPersistenceID PlayerID { get; private set; }
		public ELobbyMemberState State { get; private set; }
	}

	public class CLobbyPlayerBannedOperation : CLobbyOperation
	{
		// Construction
		public CLobbyPlayerBannedOperation( EPersistenceID player_id )
		{
			PlayerID = player_id;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CLobbyPlayerBannedOperation] PlayerID = {0} )",
										 (long)PlayerID );
		}

		// Properties
		public override ELobbyOperation Type { get { return ELobbyOperation.Player_Banned; } }

		public EPersistenceID PlayerID { get; private set; }
	}

	public class CLobbyPlayerUnbannedOperation : CLobbyOperation
	{
		// Construction
		public CLobbyPlayerUnbannedOperation( EPersistenceID player_id )
		{
			PlayerID = player_id;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CLobbyPlayerUnbannedOperation] PlayerID = {0} )",
										 (long)PlayerID );
		}

		// Properties
		public override ELobbyOperation Type { get { return ELobbyOperation.Player_Unbanned; } }

		public EPersistenceID PlayerID { get; private set; }
	}

	public class CLobbyChangeGameCountOperation : CLobbyOperation
	{
		// Construction
		public CLobbyChangeGameCountOperation( uint game_count )
		{
			GameCount = game_count;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CLobbyChangeGameCountOperation] GameCount = {0} )", GameCount );
		}

		// Properties
		public override ELobbyOperation Type { get { return ELobbyOperation.Change_Game_Count; } }

		public uint GameCount { get; private set; }
	}
}