﻿/*

	ClientResource.cs

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
using System.Reflection;

using CRShared;

namespace CRClientShared
{
	public enum EClientTextID
	{
		Invalid = -1,

		// Chat feedback
		Client_Chat_Unknown_Channel,
		Client_Chat_No_Tells_Received,
		Client_Chat_No_Channels,
		Client_Chat_Channel_Destroyed,
		Client_Chat_Player_Joined_Channel,
		Client_Chat_Player_Left_Channel,
		Client_Chat_Player_Kicked_From_Channel,
		Client_Chat_Player_Gagged_In_Channel,
		Client_Chat_Player_Ungagged_In_Channel,
		Client_Chat_Channel_New_Moderator,
		Client_Chat_Channel_Unmoderated,
		Client_Chat_Self_Left_Channel,
		Client_Chat_Self_Kicked_From_Channel_By_Moderator,
		Client_Chat_Self_Kicked_From_Channel_By_Server,
		Client_Chat_Player_Not_In_Channel,
		Client_Chat_Self_Not_Moderator_Of_Channel,
		Client_Chat_Player_Already_Gagged,
		Client_Chat_Player_Cannot_Become_Moderator,
		Client_Chat_Player_Not_Gagged,
		Client_Chat_Invalid_Self_Op,
		Client_Chat_Received_Tell,
		Client_Chat_Unknown_Tell_Target,
		Client_Chat_Tell_Ignored,
		Client_Chat_Sent_Tell,
		Client_Chat_Channel_Creation_Error_Unknown_Creator,
		Client_Chat_Channel_Creation_Error_Channel_Already_Exists,
		Client_Chat_Channel_Creation_Error_Invalid_Channel_Name,
		Client_Chat_Channel_Creation_Error_Autojoin_Failure,
		Client_Chat_Channel_Join_Error_Already_In_Channel,
		Client_Chat_Channel_Join_Error_Banned,
		Client_Chat_Channel_Join_Error_Channel_Does_Not_Exist,
		Client_Chat_Channel_Join_Error_Channel_Full,
		Client_Chat_Channel_Join_Error_No_Permission,
		Client_Chat_Broadcast_Gagged,
		Client_Chat_Broadcast_Not_In_Channel,
		Client_Chat_Channel_Joined,
		Client_Chat_Channel_Listing,
		Client_Chat_Channel_Listing_Gagged_Status,
		Client_Chat_Leave_Failure_Not_A_Member,
		Client_Chat_Leave_Failure_Not_Allowed,
		Client_Chat_No_Channels_Available,
		Client_Chat_Not_Connected,
		Client_Chat_Available_Channels,

		// Lobby feedback
		Client_Lobby_Create_Invalid_Config_Data,
		Client_Lobby_Create_Invalid_Creator_State,
		Client_Lobby_Create_Success,
		Client_Lobby_Join_Lobby_Full,
		Client_Lobby_Join_Password_Mismatch,
		Client_Lobby_Join_Creator_Is_Ignoring_You,
		Client_Lobby_Join_Player_Does_Not_Exist,
		Client_Lobby_Join_Player_Not_In_A_Lobby,
		Client_Lobby_Join_Invalid_State_To_Join,
		Client_Lobby_Join_Success,
		Client_Lobby_Join_Banned,
		Client_Lobby_Join_Lobby_Does_Not_Exist,
		Client_Lobby_Leave_Creator_Cannot_Leave,
		Client_Lobby_Leave_Lobby_Not_Ready,
		Client_Lobby_Leave_Not_In_A_Lobby,
		Client_Lobby_Leave_Unknown_Lobby,
		Client_Lobby_Destroy_Not_Creator,
		Client_Lobby_Destroy_Not_In_A_Lobby,
		Client_Lobby_Destroy_Lobby_Not_Ready,
		Client_Lobby_Player_Joined,
		Client_Lobby_Leave_You_Were_Kicked,
		Client_Lobby_Leave_You_Kicked_Someone,
		Client_Lobby_Leave_Someone_Was_Kicked,
		Client_Lobby_Leave_You_Were_Banned,
		Client_Lobby_Leave_Someone_Was_Banned,
		Client_Lobby_Leave_You_Destroyed_The_Lobby,
		Client_Lobby_Leave_Lobby_Destroyed,
		Client_Lobby_Leave_You_Left,
		Client_Lobby_Leave_Player_Left,
		Client_Lobby_Leave_Unknown_Reason,
		Client_Lobby_Leave_Rejoin_Failure,
		Client_Lobby_Leave_Disconnect_Timeout,
		Client_Lobby_Leave_Creator_Disconnect_Timeout,
		Client_Lobby_No_Info_Possible,
		Client_Lobby_Info_Header,
		Client_Lobby_Info_Settings,
		Client_Lobby_Info_Member_Info_Format,
		Client_Lobby_Info_Member_Info_Header_Name,
		Client_Lobby_Info_Member_Info_Header_ID,
		Client_Lobby_Info_Member_Info_Header_Type,
		Client_Lobby_Info_Member_Info_Header_Index,
		Client_Lobby_Info_Member_Info_Header_Ready,
		Client_Lobby_Player_Ready,
		Client_Lobby_Player_Not_Ready,
		Client_Lobby_Player_Disconnected,
		Client_Lobby_Kick_Cannot_Kick_Self,
		Client_Lobby_Kick_Not_In_A_Lobby,
		Client_Lobby_Kick_Not_Lobby_Creator,
		Client_Lobby_Kick_Player_Not_In_Lobby,
		Client_Lobby_Kick_Lobby_Not_Ready,
		Client_Lobby_Ban_Cannot_Ban_Self,
		Client_Lobby_Ban_Not_In_A_Lobby,
		Client_Lobby_Ban_Not_Lobby_Creator,
		Client_Lobby_Ban_Already_Banned,
		Client_Lobby_Ban_Lobby_Not_Ready,
		Client_Lobby_Ban_Successful,
		Client_Lobby_Unban_Not_In_A_Lobby,
		Client_Lobby_Unban_Not_Lobby_Creator,
		Client_Lobby_Unban_Player_Not_Banned,
		Client_Lobby_Unban_Lobby_Not_Ready,
		Client_Lobby_Unban_Successful,
		Client_Lobby_Unbanned,
		Client_Lobby_Move_Unknown_Player,
		Client_Lobby_Move_Not_In_A_Lobby,
		Client_Lobby_Move_Not_Lobby_Creator,
		Client_Lobby_Move_Player_Not_In_Lobby,
		Client_Lobby_Move_Invalid_Move_Destination,
		Client_Lobby_Move_Lobby_Not_Ready,
		Client_Lobby_Move_No_Change,
		Client_Lobby_Start_Match_Insufficient_Players,
		Client_Lobby_Start_Match_Not_Everyone_Ready,
		Client_Lobby_Start_Match_Not_Lobby_Creator,
		Client_Lobby_Start_Match_Not_In_A_Lobby,
		Client_Lobby_Start_Match_Lobby_Not_Ready,
		Client_Lobby_Game_Count_Changed_Finite_Self,
		Client_Lobby_Game_Count_Changed_Finite_Other,
		Client_Lobby_Game_Count_Changed_Infinite_Self,
		Client_Lobby_Game_Count_Changed_Infinite_Other,
		Client_Lobby_Change_Game_Count_Not_In_A_Lobby,
		Client_Lobby_Change_Game_Count_Not_Lobby_Creator,
		Client_Lobby_State_Change_Player_Disconnected,
		Client_Lobby_State_Change_Self_Not_Ready,
		Client_Lobby_State_Change_Player_Not_Ready,
		Client_Lobby_State_Change_Player_Reconnected,
		Client_Lobby_State_Change_Self_Ready,
		Client_Lobby_State_Change_Player_Ready,

		Client_Match_Continue_State_Self_Yes,
		Client_Match_Continue_State_Player_Yes,
		Client_Match_Continue_State_Self_No,
		Client_Match_Continue_State_Player_No,

		// Social commands feedback
		Client_Social_Ignore_Already_Ignored,
		Client_Social_Ignore_Persistence_Error,
		Client_Social_Ignore_Success,
		Client_Social_Ignore_Unknown_Player,
		Client_Social_Ignore_Cannot_Ignore_Self,
		Client_Social_Unignore_Not_Already_Ignored,
		Client_Social_Unignore_Persistence_Error,
		Client_Social_Unignore_Success,
		Client_Social_Unignore_Unknown_Player,
		Client_Social_Ignore_List_Header,

		// Browsing feedback
		Client_Browse_Start_Success,
		Client_Browse_Start_Invalid_State,
		Client_Browse_Cursor_Not_Browsing,
		Client_Browse_List_Not_Browsing,
		Client_Browse_List_No_Lobbies,
		Client_Browse_List_Header,
		Client_Browse_List_Summary_Info_Format,
		Client_Browse_List_Summary_Info_Header_ID,
		Client_Browse_List_Summary_Info_Header_GameMode,
		Client_Browse_List_Summary_Info_Header_Creator,
		Client_Browse_List_Summary_Info_Header_CreationTime,
		Client_Browse_List_Summary_Info_Header_PlayerCount,
		Client_Browse_List_Summary_Info_Header_ObserverCount,

		Client_Game_Mode_Two_Players,
		Client_Game_Mode_Four_Players,

		// Match feedback
		Client_Match_Halt_Because_Player_Left,
		Client_Match_Halt_Because_Game_Over,
		Client_Match_Halt_Because_Match_Over,
		Client_Match_Join_Success,
		Client_Match_Shutdown,
		Client_Match_Player_Removed_Disconnect_Timeout,
		Client_Match_Player_Removed_By_Request,
		Client_Match_Player_Removed_By_Request_Self,
		Client_Match_Leave_Not_In_Match,
		Client_Match_New_Game_Started,
		Client_Continue_Match_Cannot_Change_Previous_Commitment,
		Client_Continue_Match_Match_Not_Halted,
		Client_Continue_Match_Not_A_Player,
		Client_Continue_Match_Not_In_Match,
		Client_Continue_Match_Match_Is_Over,

		// Match state dump
		Client_Match_Info_No_Match,
		Client_Match_Info_Side_Row,
		Client_Match_Info_Side_Header_Side,
		Client_Match_Info_Side_Header_Score,
		Client_Match_Info_Side_Header_Won,
		Client_Match_Info_Side_Header_Lost,
		Client_Match_Info_Side_Header_Drawn,
		Client_Match_Info_Player_List,
		Client_Match_Info_Observer_List,
		Client_Match_Info_Admin_List,

		Client_Game_Info_Game_Mode,
		Client_Game_Info_Side_Bindings,
		Client_Game_Info_Side_Binding_Entry,
		Client_Game_Info_Game_Count,
		Client_Game_Info_Game_State,
		Client_Game_Info_Current_Player,
		Client_Game_Info_Deck_State,
		Client_Game_Info_Discard_Pile_Header,
		Client_Game_Info_Discard_Pile,
		Client_Game_Info_Collection_Header,
		Client_Game_Info_Collection_Side_Header,
		Client_Game_Info_Collection,
		Client_Game_Info_Player_Hand_Header,
		Client_Game_Info_Player_Hand,

		Client_Match_Score_No_Match,
		Client_Match_Score_Side_Header,
		Client_Match_Score_Color,
		Client_Match_Score_Game_Results,
		Client_Match_Score_Match_Total,
		Client_Match_Score_Game_Total,

		// Game action feedback
		Client_Game_Action_Queued,
		Client_Game_Action_Application_Success,
		Client_Game_Action_Card_Drawn,
		Client_Game_Action_Not_Your_Turn,
		Client_Game_Action_Cannot_Play_More_Than_One_Card,
		Client_Game_Action_Cannot_Play_A_Card_After_Passing_Cards,
		Client_Game_Action_Turn_Is_Already_Complete,
		Client_Game_Action_You_Must_Play_A_Card_Before_Drawing,
		Client_Game_Action_Can_Only_Pass_One_Set_Of_Cards,
		Client_Game_Action_Can_Only_Pass_Cards_In_A_Four_Player_Game,
		Client_Game_Action_Card_Is_Not_In_Your_Hand,
		Client_Game_Action_A_Higher_Card_Already_Exists,
		Client_Game_Action_Deck_Is_Empty,
		Client_Game_Action_Discard_Pile_Is_Empty,
		Client_Game_Action_Not_Enough_Cards_In_Hand_To_Pass,
		Client_Game_Action_Your_Turn_Is_Incomplete,
		Client_Game_Action_Turn_Submitted,
		Client_Game_Action_Turn_Reset,
		Client_Game_Action_Not_In_Match,
		Client_Game_Action_Match_Halted,
		Client_Game_Action_Turn_Is_Incomplete,
		Client_Game_Action_Invalid_Turn,
		Client_Game_Action_Play_Card_Must_Be_First_Action,
		Client_Game_Action_Discard_Card_Must_Be_First_Action,
		Client_Game_Action_Passing_Cards_Must_Be_Only_Action,
		Client_Game_Action_Two_Distinct_Cards_Must_Be_Passed,
		Client_Game_Action_Invalid_Card_Shortform,
		Client_Game_Action_Card_Discarded_Self,
		Client_Game_Action_Card_Discarded_Other,
		Client_Game_Action_Card_Drawn_Other,
		Client_Game_Action_Card_Played_Self,
		Client_Game_Action_Card_Played_Other,
		Client_Game_Action_Unknown_Card_Drawn_Other,
		Client_Game_Action_Not_A_Player,

		// Quickmatch feedback
		Client_Quickmatch_No_Invite_Request,
		Client_Quickmatch_Create_Invalid_State_To_Create,
		Client_Quickmatch_Create_Target_Invalid_State_To_Create,
		Client_Quickmatch_Create_Unknown_Target_Player,
		Client_Quickmatch_Create_Ignored_By_Target_Player,
		Client_Quickmatch_Create_Cannot_Invite_Yourself,
		Client_Quickmatch_Create_Already_Has_Pending_Quickmatch,
		Client_Quickmatch_Create_Target_Already_Has_Pending_Quickmatch,
		Client_Quickmatch_Invite_Indefinite_Game_Count,
		Client_Quickmatch_Invite_Finite_Game_Count,
		Client_Quickmatch_Cancel_Not_In_Quickmatch,

		// Misc connection/system
		Client_Startup_Greeting,
		Client_Connection_Success_Notice,
		Client_Connection_Failure_Notice,
		Client_Expected_Disconnection,
		Client_Unexpected_Disconnection,
		Client_Not_Connected_To_Server,
		Client_Connection_Refused_Unknown,
		Client_Connection_Refused_Name_Already_Connected,
		Client_Connection_Refused_Invalid_Password,
		Client_Connection_Refused_Internal_Persistence_Error,
		Client_Connection_Refused_Chat,
		Client_Connection_Refused_Default_Chat,
		Client_Error_Saving_Settings,

		Client_The_Server,

		// Help text

		// commands
		Client_Command_Name_Connect,
		Client_Command_Shortcut_Connect,
		Client_Help_Command_Connect,
		Client_Command_Name_Disconnect,
		Client_Help_Command_Disconnect,
		Client_Command_Name_Test,
		Client_Help_Command_Test,
		Client_Command_Name_Chat_Join_Channel,
		Client_Help_Command_Chat_Join_Channel,
		Client_Command_Name_Chat_Leave_Channel,
		Client_Help_Command_Chat_Leave_Channel,
		Client_Command_Name_Chat_Mod_Kick,
		Client_Help_Command_Chat_Mod_Kick,
		Client_Command_Name_Chat_Mod_Gag,
		Client_Help_Command_Chat_Mod_Gag,
		Client_Command_Name_Chat_Mod_Ungag,
		Client_Help_Command_Chat_Mod_Ungag,
		Client_Command_Name_Chat_Mod_Transfer,
		Client_Help_Command_Chat_Mod_Transfer,
		Client_Command_Name_Chat_Tell,
		Client_Help_Command_Chat_Tell,
		Client_Command_Name_Chat_Reply,
		Client_Help_Command_Chat_Reply,
		Client_Command_Name_Chat_List_Members,
		Client_Help_Command_Chat_List_Members,
		Client_Command_Name_Number,
		Client_Help_Command_Number,
		Client_Command_Name_Lobby_Create,
		Client_Help_Command_Lobby_Create,
		Client_Command_Name_Lobby_Join_By_Name,
		Client_Help_Command_Lobby_Join_By_Name,
		Client_Command_Name_Lobby_Join_By_Player,
		Client_Help_Command_Lobby_Join_By_Player,
		Client_Command_Name_Lobby_Join_By_ID,
		Client_Help_Command_Lobby_Join_By_ID,
		Client_Command_Name_Lobby_Info,
		Client_Help_Command_Lobby_Info,
		Client_Command_Name_Lobby_Leave,
		Client_Help_Command_Lobby_Leave,
		Client_Command_Name_Lobby_Destroy,
		Client_Help_Command_Lobby_Destroy,
		Client_Command_Name_Lobby_Kick,
		Client_Help_Command_Lobby_Kick,
		Client_Command_Name_Lobby_Ban,
		Client_Help_Command_Lobby_Ban,
		Client_Command_Name_Lobby_Unban,
		Client_Help_Command_Lobby_Unban,
		Client_Command_Name_Lobby_State,
		Client_Help_Command_Lobby_State,
		Client_Command_Name_Lobby_Move,
		Client_Help_Command_Lobby_Move,
		Client_Command_Name_Lobby_Start_Match,
		Client_Help_Command_Lobby_Start_Match,
		Client_Command_Name_Lobby_Change_Game_Count,
		Client_Help_Command_Lobby_Change_Game_Count,
		Client_Command_Name_Social_Ignore,
		Client_Help_Command_Social_Ignore,
		Client_Command_Name_Social_Unignore,
		Client_Help_Command_Social_Unignore,
		Client_Command_Name_Social_Ignore_List,
		Client_Help_Command_Social_Ignore_List,
		Client_Command_Name_Browse_Lobby_Start,
		Client_Help_Command_Browse_Lobby_Start,
		Client_Command_Name_Browse_Lobby_End,
		Client_Help_Command_Browse_Lobby_End,
		Client_Command_Name_Browse_Lobby_Next_Set,
		Client_Help_Command_Browse_Lobby_Next_Set,
		Client_Command_Name_Browse_Lobby_Previous_Set,
		Client_Help_Command_Browse_Lobby_Previous_Set,
		Client_Command_Name_Browse_Lobby_List,
		Client_Help_Command_Browse_Lobby_List,
		Client_Command_Name_Quit,
		Client_Help_Command_Quit,
		Client_Command_Name_Match_Leave,
		Client_Help_Command_Match_Leave,
		Client_Command_Name_Match_Info,
		Client_Help_Command_Match_Info,
		Client_Command_Name_Match_Turn_Pass_Cards,
		Client_Help_Command_Match_Turn_Pass_Cards,
		Client_Command_Name_Match_End_Turn,
		Client_Help_Command_Match_End_Turn,
		Client_Command_Name_Match_Reset_Turn,
		Client_Help_Command_Match_Reset_Turn,
		Client_Command_Name_Match_Turn_Play_And_Draw_Deck,
		Client_Help_Command_Match_Turn_Play_And_Draw_Deck,
		Client_Command_Name_Match_Turn_Play_And_Draw_Discard,
		Client_Help_Command_Match_Turn_Play_And_Draw_Discard,
		Client_Command_Name_Match_Turn_Discard_And_Draw_Deck,
		Client_Help_Command_Match_Turn_Discard_And_Draw_Deck,
		Client_Command_Name_Match_Turn_Discard_And_Draw_Discard,
		Client_Help_Command_Match_Turn_Discard_And_Draw_Discard,
		Client_Command_Name_Match_Play_Shortform,
		Client_Help_Command_Match_Play_Shortform,
		Client_Command_Name_Match_Discard_Shortform,
		Client_Help_Command_Match_Discard_Shortform,
		Client_Command_Name_Match_Pass_Shortform,
		Client_Help_Command_Match_Pass_Shortform,
		Client_Command_Name_Match_Score,
		Client_Help_Command_Match_Score,
		Client_Command_Name_Match_Continue,
		Client_Help_Command_Match_Continue,
		Client_Command_Name_Match_UI_Play_Card,
		Client_Help_Command_Match_UI_Play_Card,
		Client_Command_Name_Match_UI_Discard_Card,
		Client_Help_Command_Match_UI_Discard_Card,
		Client_Command_Name_Match_UI_Draw_From_Deck,
		Client_Help_Command_Match_UI_Draw_From_Deck,
		Client_Command_Name_Match_UI_Draw_From_Discard,
		Client_Help_Command_Match_UI_Draw_From_Discard,
		Client_Command_Name_Quickmatch_Create_Quickmatch,
		Client_Help_Command_Quickmatch_Create_Quickmatch,
		Client_Command_Name_Quickmatch_Respond_To_Quickmatch_Request,
		Client_Help_Command_Quickmatch_Respond_To_Quickmatch_Request,
		Client_Command_Name_Quickmatch_Cancel_Quickmatch,
		Client_Help_Command_Quickmatch_Cancel_Quickmatch,
		Client_Command_Name_Quickmatch_Create_AI_Quickmatch,
		Client_Help_Command_Quickmatch_Create_AI_Quickmatch,

		

		// Command param names (for building usage strings)
		Client_Command_Connect_Param_PlayerName,
		Client_Command_Connect_Param_ServerAddress,
		Client_Command_Connect_Param_ServerPort,
		Client_Command_Test_Param_Data,
		Client_Command_Chat_Join_Channel_Param_ChannelName,
		Client_Command_Chat_Leave_Channel_Param_ChannelIdentifier,
		Client_Command_Number_Param_ChatMessage,
		Client_Command_Chat_Mod_Kick_Param_ChannelIdentifier,
		Client_Command_Chat_Mod_Kick_Param_PlayerName,
		Client_Command_Chat_Mod_Gag_Param_ChannelIdentifier,
		Client_Command_Chat_Mod_Gag_Param_PlayerName,
		Client_Command_Chat_Mod_Ungag_Param_ChannelIdentifier,
		Client_Command_Chat_Mod_Ungag_Param_PlayerName,
		Client_Command_Chat_Mod_Transfer_Param_ChannelIdentifier,
		Client_Command_Chat_Mod_Transfer_Param_PlayerName,
		Client_Command_Chat_Tell_Param_PlayerName,
		Client_Command_Chat_Tell_Param_ChatMessage,
		Client_Command_Chat_Reply_Param_ChatMessage,
		Client_Command_Chat_List_Members_Param_ChannelIdentifier,
		Client_Command_Lobby_Create_Param_GameMode,
		Client_Command_Lobby_Create_Param_Password,
		Client_Command_Lobby_Create_Param_GameDescription,
		Client_Command_Lobby_Create_Param_AllowObservers,
		Client_Command_Lobby_Join_By_Player_Param_PlayerName,
		Client_Command_Lobby_Join_By_Player_Param_Password,
		Client_Command_Lobby_Join_By_ID_Param_LobbyID,
		Client_Command_Lobby_Kick_Param_PlayerName,
		Client_Command_Lobby_Ban_Param_PlayerName,
		Client_Command_Lobby_Unban_Param_PlayerName,
		Client_Command_Lobby_State_Param_State,
		Client_Command_Lobby_Move_Param_PlayerName,
		Client_Command_Lobby_Move_Param_DestinationCategory,
		Client_Command_Lobby_Move_Param_DestinationIndex,
		Client_Command_Lobby_Change_Game_Count_Param_GameCount,
		Client_Command_Social_Ignore_Param_PlayerName,
		Client_Command_Social_Unignore_Param_PlayerName,
		Client_Command_Browse_Lobby_Start_Param_GameModes,
		Client_Command_Browse_Lobby_Start_Param_MemberTypes,
		Client_Command_Browse_Lobby_Start_Param_JoinFirstAvailable,
		Client_Command_Match_Turn_Pass_Cards_Param_Color1,
		Client_Command_Match_Turn_Pass_Cards_Param_Value1,
		Client_Command_Match_Turn_Pass_Cards_Param_Color2,
		Client_Command_Match_Turn_Pass_Cards_Param_Value2,
		Client_Command_Match_Turn_Play_And_Draw_Deck_Param_Color,
		Client_Command_Match_Turn_Play_And_Draw_Deck_Param_Value,
		Client_Command_Match_Turn_Play_And_Draw_Discard_Param_Color,
		Client_Command_Match_Turn_Play_And_Draw_Discard_Param_Value,
		Client_Command_Match_Turn_Play_And_Draw_Discard_Param_DrawColor,
		Client_Command_Match_Turn_Discard_And_Draw_Deck_Param_Color,
		Client_Command_Match_Turn_Discard_And_Draw_Deck_Param_Value,
		Client_Command_Match_Turn_Discard_And_Draw_Discard_Param_Color,
		Client_Command_Match_Turn_Discard_And_Draw_Discard_Param_Value,
		Client_Command_Match_Turn_Discard_And_Draw_Discard_Param_DrawColor,
		Client_Command_Match_Play_Shortform_Param_CardShortform,
		Client_Command_Match_Play_Shortform_Param_DrawCode,
		Client_Command_Match_Discard_Shortform_Param_CardShortform,
		Client_Command_Match_Discard_Shortform_Param_DrawCode,
		Client_Command_Match_Pass_Shortform_Param_CardShortform1,
		Client_Command_Match_Pass_Shortform_Param_CardShortform2,
		Client_Command_Match_Continue_Param_Continue,
		Client_Command_Match_UI_Play_Card_Param_Color,
		Client_Command_Match_UI_Play_Card_Param_Value,
		Client_Command_Match_UI_Discard_Card_Param_Color,
		Client_Command_Match_UI_Discard_Card_Param_Value,
		Client_Command_Match_UI_Draw_From_Discard_Param_Color,
		Client_Command_Quickmatch_Create_Quickmatch_Param_PlayerName,
		Client_Command_Quickmatch_Create_Quickmatch_Param_GameCount,
		Client_Command_Quickmatch_Create_Quickmatch_Param_AllowObservers,
		Client_Command_Quickmatch_Respond_To_Quickmatch_Request_Param_Response,
		Client_Command_Quickmatch_Create_AI_Quickmatch_Param_GameCount,
		Client_Command_Quickmatch_Create_AI_Quickmatch_Param_AllowObservers,

	}

	public enum EClientImageID
	{
		Invalid = -1
	}

	public class CClientResource : CSharedResource
	{
		public static void Initialize_Resources()
		{
			CSharedResource.Initialize_Shared_Resources();

			CResourceManager.Instance.Initialize_Assembly_Resources< EClientTextID >( "CRClientShared.Source.Resources.ClientResources", Assembly.GetExecutingAssembly(), EResourceType.Text );
			CResourceManager.Instance.Initialize_Assembly_Resources< EClientImageID >( "CRClientShared.Source.Resources.ClientResources", Assembly.GetExecutingAssembly(), EResourceType.Image );
		}				
	}
}