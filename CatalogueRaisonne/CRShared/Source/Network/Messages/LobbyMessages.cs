/*

	LobbyMessages.cs

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
using System.Text;

namespace CRShared
{
	public enum ECreateLobbyFailureReason
	{
		None,

		Invalid_State_To_Create,
		Invalid_Config_Data
	}

	public enum EJoinLobbyFailureReason
	{
		None,

		Target_Player_Does_Not_Exist,
		Invalid_State_To_Join,
		Lobby_Full,
		Password_Mismatch,
		Target_Player_Not_In_A_Lobby,
		Creator_Is_Ignoring_You,
		Banned,
		Lobby_Does_Not_Exist
	}

	public enum ELeaveLobbyFailureReason
	{
		None,

		Not_In_A_Lobby,
		Unknown_Lobby,
		Creator_Cannot_Leave,
		Lobby_Not_Ready
	}

	public enum EDestroyLobbyFailureReason
	{
		None,

		Not_In_A_Lobby,
		Not_Creator,
		Lobby_Not_Ready
	}

	public enum ELobbyDestroyedReason
	{
		Creation_Failure,
		Creator_Destroyed,
		Game_Started,
		Creator_Disconnect_Timeout,
		Chat_Channel_Creation_Failure,
		Chat_Channel_Join_Failure
	}

	public enum EKickPlayerFromLobbyError
	{
		None,

		Not_In_A_Lobby,
		Not_Lobby_Creator,
		Cannot_Kick_Self,
		Player_Not_In_Lobby,
		Lobby_Not_Ready
	}

	public enum EBanPlayerFromLobbyError
	{
		None,

		Not_In_A_Lobby,
		Not_Lobby_Creator,
		Cannot_Ban_Self,
		Already_Banned,
		Lobby_Not_Ready
	}

	public enum EUnbanPlayerFromLobbyError
	{
		None,

		Not_In_A_Lobby,
		Not_Lobby_Creator,
		Player_Not_Banned,
		Lobby_Not_Ready
	}

	public enum EMovePlayerInLobbyError
	{
		None,

		Not_In_A_Lobby,
		Not_Lobby_Creator,
		Player_Not_In_Lobby,
		Invalid_Move_Destination,
		Lobby_Not_Ready,
		No_Change
	}

	public enum EStartMatchError
	{
		None,

		Not_Everyone_Ready,
		Insufficient_Players,
		Not_Lobby_Creator,
		Not_In_A_Lobby,
		Lobby_Not_Ready
	}

	public enum ERemovedFromLobbyReason
	{
		Lobby_Destroyed_By_Creator,
		Lobby_Destroyed_Game_Started,
		Lobby_Destroyed_Creation_Failure,
		Lobby_Destroyed_Due_To_Disconnect_Timeout,

		Unable_To_Join_Chat_Channel,
		Self_Request,
		Kicked_By_Creator,
		Banned_By_Creator,
		Rejoin_Failure,
		Disconnect_Timeout,
		Unknown
	}

	public enum ELobbyChangeGameCountError
	{
		None,

		Not_In_A_Lobby,
		Not_Lobby_Creator
	}

	[NetworkMessage]
	public class CCreateLobbyRequest : CRequestMessage
	{
		// Construction
		public CCreateLobbyRequest( CLobbyConfig config ) :
			base()
		{
			Config = config;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CCreateLobbyRequest]: Config = {0} )", Config.ToString() );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Create_Lobby_Request; } }

		public CLobbyConfig Config { get; private set; }
	}

	[NetworkMessage]
	public class CCreateLobbyFailure : CResponseMessage
	{
		// Construction
		public CCreateLobbyFailure( EMessageRequestID request_id, ECreateLobbyFailureReason reason ) :
			base( request_id )
		{
			Reason = reason;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CCreateLobbyFailure]: Reason = {0} )", Reason.ToString() );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Create_Lobby_Failure; } }

		public ECreateLobbyFailureReason Reason { get; private set; }
	}

	[NetworkMessage]
	public class CCreateLobbySuccess : CResponseMessage
	{
		// Construction
		public CCreateLobbySuccess( EMessageRequestID request_id, CLobbyState lobby_state ) :
			base( request_id )
		{
			LobbyState = lobby_state;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CCreateLobbySuccess]: Lobby = {0} )", LobbyState.ToString() );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Create_Lobby_Success; } }

		public CLobbyState LobbyState { get; private set; }
	}

	[NetworkMessage]
	public class CJoinLobbyByPlayerRequest : CRequestMessage
	{
		// Construction
		public CJoinLobbyByPlayerRequest( string player_name, string password ) :
			base()
		{
			PlayerName = player_name;
			Password = password;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CJoinLobbyByPlayerRequest]: PlayerName = {0}, Password = {1} )", PlayerName, Password );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Join_Lobby_By_Player_Request; } }

		public string PlayerName { get; private set; }
		public string Password { get; private set; }
	}

	[NetworkMessage]
	public class CJoinLobbyByIDRequest : CRequestMessage
	{
		// Construction
		public CJoinLobbyByIDRequest( ELobbyID lobby_id ) :
			base()
		{
			LobbyID = lobby_id;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CJoinLobbyByIDRequest]: LobbyID = {0} )", (int)LobbyID );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Join_Lobby_By_ID_Request; } }

		public ELobbyID LobbyID { get; private set; }
	}

	[NetworkMessage]
	public class CJoinLobbyFailure : CResponseMessage
	{
		// Construction
		public CJoinLobbyFailure( EMessageRequestID request_id, EJoinLobbyFailureReason reason ) :
			base( request_id )
		{
			Reason = reason;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CJoinLobbyFailure]: Reason = {0} )", Reason.ToString() );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Join_Lobby_Failure; } }

		public EJoinLobbyFailureReason Reason { get; private set; }
	}

	[NetworkMessage]
	public class CJoinLobbySuccess : CResponseMessage
	{
		// Construction
		public CJoinLobbySuccess( EMessageRequestID request_id, CLobbyState lobby_state ) :
			base( request_id )
		{
			LobbyState = lobby_state;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CJoinLobbySuccess]: Lobby = {0} )", LobbyState.ToString() );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Join_Lobby_Success; } }
		public override bool RequiresRequest { get { return false; } }

		public CLobbyState LobbyState { get; private set; }
	}
	
	[NetworkMessage]
	public class CLeaveLobbyRequest : CRequestMessage
	{
		// Construction
		public CLeaveLobbyRequest() :
			base()
		{
		}

		// Methods
		public override string ToString()
		{
			return "( [CLeaveLobbyRequest] )";
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Leave_Lobby_Request; } }
	}

	[NetworkMessage]
	public class CLeaveLobbyResponse : CResponseMessage
	{
		// Construction
		public CLeaveLobbyResponse( EMessageRequestID request_id, ELeaveLobbyFailureReason reason ) :
			base( request_id )
		{
			Reason = reason;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CLeaveLobbyResponse]: Reason = {0} )", Reason.ToString() );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Leave_Lobby_Response; } }

		public ELeaveLobbyFailureReason Reason { get; private set; }
	}

	[NetworkMessage]
	public class CLobbyOperationMessage : CNetworkMessage
	{
		// Construction
		public CLobbyOperationMessage( CLobbyOperation operation ) :
			base()
		{
			Operation = operation;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CLobbyOperationMessage]: Operation = {0} )", Operation.ToString() );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Lobby_Operation; } }

		public CLobbyOperation Operation { get; private set; }
	}

	[NetworkMessage]
	public class CDestroyLobbyRequest : CRequestMessage
	{
		// Construction
		public CDestroyLobbyRequest() :
			base()
		{
		}

		// Methods
		public override string ToString()
		{
			return "( [CDestroyLobbyRequest] )";
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Destroy_Lobby_Request; } }
	}

	[NetworkMessage]
	public class CDestroyLobbyResponse : CResponseMessage
	{
		// Construction
		public CDestroyLobbyResponse( EMessageRequestID request_id, EDestroyLobbyFailureReason reason ) :
			base( request_id )
		{
			Reason = reason;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CDestroyLobbyResponse]: Reason = {0} )", Reason.ToString() );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Destroy_Lobby_Response; } }

		public EDestroyLobbyFailureReason Reason { get; private set; }
	}

	[NetworkMessage]
	public class CLobbyChangeMemberStateMessage : CNetworkMessage
	{
		// Construction
		public CLobbyChangeMemberStateMessage( ELobbyMemberState state ) :
			base()
		{
			State = state;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CLobbyChangeMemberStateMessage]: State = {0} )", State.ToString() );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Lobby_Change_Member_State; } }

		public ELobbyMemberState State { get; private set; }
	}

	[NetworkMessage]
	public class CKickPlayerFromLobbyRequest : CRequestMessage
	{
		// Construction
		public CKickPlayerFromLobbyRequest( string player_name ) :
			base()
		{
			PlayerName = player_name;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CKickPlayerFromLobbyRequest]: PlayerName = {0} )", PlayerName );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Kick_Player_From_Lobby_Request; } }

		public string PlayerName { get; private set; }
	}

	[NetworkMessage]
	public class CKickPlayerFromLobbyResponse : CResponseMessage
	{
		// Construction
		public CKickPlayerFromLobbyResponse( EMessageRequestID request_id, EKickPlayerFromLobbyError reason ) :
			base( request_id )
		{
			Reason = reason;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CKickPlayerFromLobbyResponse]: Reason = {0} )", Reason );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Kick_Player_From_Lobby_Response; } }

		public EKickPlayerFromLobbyError Reason { get; private set; }
	}

	[NetworkMessage]
	public class CBanPlayerFromLobbyRequest : CRequestMessage
	{
		// Construction
		public CBanPlayerFromLobbyRequest( string player_name ) :
			base()
		{
			PlayerName = player_name;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CBanPlayerFromLobbyRequest]: PlayerName = {0} )", PlayerName );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Ban_Player_From_Lobby_Request; } }

		public string PlayerName { get; private set; }
	}

	[NetworkMessage]
	public class CBanPlayerFromLobbyResponse : CResponseMessage
	{
		// Construction
		public CBanPlayerFromLobbyResponse( EMessageRequestID request_id, EBanPlayerFromLobbyError reason ) :
			base( request_id )
		{
			Reason = reason;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CBanPlayerFromLobbyResponse]: Reason = {0} )", Reason.ToString() );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Ban_Player_From_Lobby_Response; } }

		public EBanPlayerFromLobbyError Reason { get; private set; }
	}

	[NetworkMessage]
	public class CUnbanPlayerFromLobbyRequest : CRequestMessage
	{
		// Construction
		public CUnbanPlayerFromLobbyRequest( string player_name ) :
			base()
		{
			PlayerName = player_name;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CUnbanPlayerFromLobbyRequest]: PlayerName = {0} )", PlayerName );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Unban_Player_From_Lobby_Request; } }

		public string PlayerName { get; private set; }
	}

	[NetworkMessage]
	public class CUnbanPlayerFromLobbyResponse : CResponseMessage
	{
		// Construction
		public CUnbanPlayerFromLobbyResponse( EMessageRequestID request_id, EUnbanPlayerFromLobbyError reason ) :
			base( request_id )
		{
			Reason = reason;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CUnbanPlayerFromLobbyResponse]: Reason = {0} )", Reason.ToString() );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Unban_Player_From_Lobby_Response; } }

		public EUnbanPlayerFromLobbyError Reason { get; private set; }
	}

	[NetworkMessage]
	public class CUnbannedFromLobbyNotificationMessage : CNetworkMessage
	{
		// Construction
		public CUnbannedFromLobbyNotificationMessage( string player_name ) :
			base()
		{
			PlayerName = player_name;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CUnbannedFromLobbyNotificationMessage]: PlayerName = {0} )", PlayerName );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Unban_From_Lobby_Notification; } }

		public string PlayerName { get; private set; }
	}

	[NetworkMessage]
	public class CMovePlayerInLobbyRequest : CRequestMessage
	{
		// Construction
		public CMovePlayerInLobbyRequest( EPersistenceID player_id, ELobbyMemberType destination_category, uint destination_index ) :
			base()
		{
			PlayerID = player_id;
			DestinationCategory = destination_category;
			DestinationIndex = destination_index;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CMovePlayerInLobbyRequest]: PlayerID = {0}, DestinationCategory = {1}, DestinationIndex = {2} )", 
										 (long)PlayerID,
										 DestinationCategory.ToString(),
										 DestinationIndex );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Move_Player_In_Lobby_Request; } }

		public EPersistenceID PlayerID { get; private set; }
		public ELobbyMemberType DestinationCategory { get; private set; }
		public uint DestinationIndex { get; private set; }
	}

	[NetworkMessage]
	public class CMovePlayerInLobbyResponse : CResponseMessage
	{
		// Construction
		public CMovePlayerInLobbyResponse( EMessageRequestID request_id, EMovePlayerInLobbyError reason ) :
			base( request_id )
		{
			Reason = reason;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CMovePlayerInLobbyResponse]: Reason = {0} )", Reason.ToString() );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Move_Player_In_Lobby_Response; } }

		public EMovePlayerInLobbyError Reason { get; private set; }
	}

	[NetworkMessage]
	public class CLobbyStartMatchRequest : CRequestMessage
	{
		// Construction
		public CLobbyStartMatchRequest() :
			base()
		{
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CLobbyStartMatchRequest] )" );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Lobby_Start_Match_Request; } }
	}

	[NetworkMessage]
	public class CLobbyStartMatchResponse : CResponseMessage
	{
		// Construction
		public CLobbyStartMatchResponse( EMessageRequestID request_id, EStartMatchError error ) :
			base( request_id )
		{
			Error = error;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CLobbyStartMatchResponse]: Error = {0} )", Error.ToString() );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Lobby_Start_Match_Response; } }

		public EStartMatchError Error { get; private set; }
	}

	[NetworkMessage]
	public class CLobbyChangeGameCountRequest : CRequestMessage
	{
		// Construction
		public CLobbyChangeGameCountRequest( uint game_count ) :
			base()
		{
			GameCount = game_count;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CLobbyChangeGameCountRequest]: GameCount = {0} )", GameCount );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Lobby_Change_Game_Count_Request; } }

		public uint GameCount { get; private set; }
	}

	[NetworkMessage]
	public class CLobbyChangeGameCountResponse : CResponseMessage
	{
		// Construction
		public CLobbyChangeGameCountResponse( EMessageRequestID request_id, ELobbyChangeGameCountError error ) :
			base( request_id )
		{
			Error = error;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CLobbyChangeGameCountResponse]: Error = {0} )", Error );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Lobby_Change_Game_Count_Response; } }

		public ELobbyChangeGameCountError Error { get; private set; }
	}

}
