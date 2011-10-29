using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CRShared
{
	public enum EMessageRequestID
	{
		Invalid = 0,
		Start
	}

	public enum ENetworkMessageType
	{
		Message_Invalid = 0,

		Message_Serialization_Test,
		Message_Serialization_Test2,

		Message_Client_Hello_Request,
		Message_Client_Hello_Response,
		Message_Disconnect_Request,
		Message_Disconnect_Result,
		Message_Connection_Dropped,
		Message_Ping,

		// Server <-> Chat Server
		Server_Request_Announce_Player_To_Chat,
		Server_Response_Announce_Player_To_Chat,
		Server_Message_Remove_Player_From_Chat,
		Server_Message_Create_Chat_Channel_Request,
		Server_Message_Create_Chat_Channel_Response,
		Server_Message_Destroy_Chat_Channel,
		Server_Message_Join_Chat_Channel_Request,
		Server_Message_Join_Chat_Channel_Response,
		Server_Message_Remove_From_Chat_Channel,
		Server_Message_Add_Player_To_Chat_Ignore,
		Server_Message_Remove_Player_From_Chat_Ignore,

		// Client <-> chat server
		Message_Create_Or_Join_Chat_Channel,
		Message_Join_Chat_Channel_Result,
		Message_Leave_Chat_Channel,
		Message_Leave_Chat_Channel_Failure,
		Message_Broadcast_To_Chat_Channel,
		Message_Broadcast_Failure,
		Message_Player_Chat,
		Message_Chat_Client_Notification,
		Message_Left_Chat_Channel,
		Message_Chat_Mod_Operation,
		Message_Chat_Mod_Operation_Error,
		Message_Player_Tell,
		Message_Player_Tell_Request,
		Message_Player_Tell_Response,

		// Lobby
		Message_Create_Lobby_Request,
		Message_Create_Lobby_Failure,
		Message_Create_Lobby_Success,
		Message_Join_Lobby_By_Player_Request,
		Message_Join_Lobby_By_ID_Request,
		Message_Join_Lobby_Failure,
		Message_Join_Lobby_Success,
		Message_Leave_Lobby_Request,
		Message_Leave_Lobby_Response,
		Message_Lobby_Operation,
		Message_Destroy_Lobby_Request,
		Message_Destroy_Lobby_Response,
		Message_Lobby_Operation_Notification,
		Message_Lobby_Change_Member_State,
		Message_Kick_Player_From_Lobby_Request,
		Message_Kick_Player_From_Lobby_Response,
		Message_Ban_Player_From_Lobby_Request,
		Message_Ban_Player_From_Lobby_Response,
		Message_Unban_Player_From_Lobby_Request,
		Message_Unban_Player_From_Lobby_Response,
		Message_Unban_From_Lobby_Notification,
		Message_Move_Player_In_Lobby_Request,
		Message_Move_Player_In_Lobby_Response,
		Message_Lobby_Start_Match_Request,
		Message_Lobby_Start_Match_Response,
		Message_Lobby_Change_Game_Count_Request,
		Message_Lobby_Change_Game_Count_Response,

		// Social
		Message_Ignore_Player_Request,
		Message_Ignore_Player_Response,
		Message_Unignore_Player_Request,
		Message_Unignore_Player_Response,

		// Browse
		Message_Start_Browse_Lobby_Request,
		Message_Start_Browse_Lobby_Response,
		Message_End_Browse_Lobby,
		Message_Browse_Next_Lobby_Set_Request,
		Message_Browse_Previous_Lobby_Set_Request,
		Message_Cursor_Browse_Lobby_Response,
		Message_Browse_Lobby_Delta_Message,
		Message_Browse_Lobby_Add_Remove_Message,

		// Match
		Message_Join_Match_Success,
		Message_Match_New_Game,
		Message_Match_Player_Left,
		Message_Match_Player_Connection_State,
		Message_Leave_Match_Request,
		Message_Leave_Match_Response,
		Message_Match_Take_Turn_Request,
		Message_Match_Take_Turn_Response,
		Message_Match_Delta_Set,
		Message_Match_State_Delta,
		Message_Continue_Match_Request,
		Message_Continue_Match_Response,

		// Quickmatch
		Message_Create_Quickmatch_Request,
		Message_Create_Quickmatch_Response,
		Message_Invite_To_Quickmatch_Request,
		Message_Invite_To_Quickmatch_Response,
		Message_Cancel_Quickmatch_Request,
		Message_Cancel_Quickmatch_Response,
		Message_Quickmatch_Notification,

		// Misc
		Message_Query_Player_Info_Request,
		Message_Query_Player_Info_Response

	}
	
	public class CNetworkMessage
	{
		public virtual ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Invalid; } }
	}

	public class CRequestMessage : CNetworkMessage
	{
		// Properties
		public EMessageRequestID RequestID { get; set; }
		public DRequestResponseHandler Handler { get { return m_Handler; } set { m_Handler = value; } }

		// Fields
		[DontSerialize]
		private DRequestResponseHandler m_Handler = null;
	}

	public class CResponseMessage : CNetworkMessage
	{
		// Construction
		public CResponseMessage( EMessageRequestID request_id )
		{
			RequestID = request_id;
		}

		// Properties
		public EMessageRequestID RequestID { get; private set; }
		public CRequestMessage Request { get { return m_Request; } set { m_Request = value; } }

		public virtual bool RequiresRequest { get { return true; } }

		// Fields
		[DontSerialize]
		private CRequestMessage m_Request = null;
	}
	
					
}
