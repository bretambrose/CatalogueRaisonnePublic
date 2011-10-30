/*

	ChatClient.cs

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
using System.Text;

using CRShared;
using CRShared.Chat;
using CRClientShared;

namespace CRClientShared.Chat
{
	public enum EClientChannelNumber
	{
		Invalid
	}

	public class CChatClient
	{
		// Construction
		private CChatClient() 
		{
			LastChannelUsed = EChannelID.Invalid;
			LastReceivedTellPlayerName = null;
		}

		static CChatClient() {}

		// Methods
		// Public Interface
		public void Reset()
		{
			m_ChannelsByName.Clear();
			m_ChannelsByNumber.Clear();
			m_Channels.Clear();

			LastChannelUsed = EChannelID.Invalid;
			LastReceivedTellPlayerName = null;
		}

		// Non-public interface
		// Slash Command handlers
		[GenericHandler]
		private void Handle_Join_Chat_Channel_Command( CJoinChatChannelSlashCommand command )
		{
			Send_Message_To_Chat_Server( new CCreateOrJoinChatChannelMessage( command.ChannelName ) );
		}

		[GenericHandler]
		private void Handle_Leave_Chat_Channel_Command( CLeaveChatChannelSlashCommand command )
		{
			CClientChatChannel channel = null;
			int channel_number;
			if ( Int32.TryParse( command.ChannelIdentifier, out channel_number ) )
			{
				channel = Instance.Get_Channel_By_Channel_Number( ( EClientChannelNumber ) channel_number );
			}
			else
			{
				channel = Instance.Get_Channel_By_Channel_Name( command.ChannelIdentifier );
			}

			if ( channel != null )
			{
				CLeaveChatChannelMessage leave_message = new CLeaveChatChannelMessage( channel.ChannelID );
				Send_Message_To_Chat_Server( leave_message );
			}
			else
			{
				CClientResource.Output_Text( EClientTextID.Client_Chat_Unknown_Channel );
			}
		}

		[GenericHandler]
		private void Handle_Broadcast_On_Chat_Channel_Command( CBroadcastOnChatChannelSlashCommand command )
		{
			CClientChatChannel broadcast_channel = Instance.Get_Channel_By_Channel_Number( command.ChannelNumber );
			if ( broadcast_channel != null )
			{
				CBroadcastToChatChannelMessage broadcast_msg = new CBroadcastToChatChannelMessage( broadcast_channel.ChannelID, command.ChatMessage );
				Send_Message_To_Chat_Server( broadcast_msg );
				Instance.LastChannelUsed = broadcast_channel.ChannelID;
			}
			else
			{
				CClientResource.Output_Text( EClientTextID.Client_Chat_Unknown_Channel );
			}
		}

		[GenericHandler]
		private void Handle_Chat_List_Command( CChatListSlashCommand command )
		{
			if ( command.ChannelIdentifier == null || command.ChannelIdentifier.Length == 0 )
			{
				CSharedResource.Output_Text( Instance.Build_Channel_Listing() );
				return;
			}

			CClientChatChannel list_channel = Instance.Infer_Channel_From_String( command.ChannelIdentifier );
			if ( list_channel != null )
			{
				string channel_listing = list_channel.Build_Channel_Listing( Instance.Build_Channel_Name_Display_String( list_channel.ChannelName, list_channel.ChannelNumber ) );
				CSharedResource.Output_Text( channel_listing );
			}
			else
			{
				CClientResource.Output_Text( EClientTextID.Client_Chat_Unknown_Channel );
			}
		}

		[GenericHandler]
		private void Handle_Chat_Gag_Command( CChatGagSlashCommand command )
		{
			CClientChatChannel gag_channel = Instance.Infer_Channel_From_String( command.ChannelIdentifier );
			if ( gag_channel != null )
			{
				CChatModOperationMessage mod_op_msg = new CChatModOperationMessage( gag_channel.ChannelID, EChatModOperation.Gag, command.PlayerName );
				Send_Message_To_Chat_Server( mod_op_msg );
			}
			else
			{
				CClientResource.Output_Text( EClientTextID.Client_Chat_Unknown_Channel );
			}
		}

		[GenericHandler]
		private void Handle_Chat_Ungag_Command( CChatUngagSlashCommand command )
		{
			CClientChatChannel ungag_channel = Instance.Infer_Channel_From_String( command.ChannelIdentifier );
			if ( ungag_channel != null )
			{
				CChatModOperationMessage mod_op_msg = new CChatModOperationMessage( ungag_channel.ChannelID, EChatModOperation.Ungag, command.PlayerName );
				Send_Message_To_Chat_Server( mod_op_msg );
			}
			else
			{
				CClientResource.Output_Text( EClientTextID.Client_Chat_Unknown_Channel );
			}
		}

		[GenericHandler]
		private void Handle_Chat_Kick_Command( CChatKickSlashCommand command )
		{
			CClientChatChannel kick_channel = Instance.Infer_Channel_From_String( command.ChannelIdentifier );
			if ( kick_channel != null )
			{
				CChatModOperationMessage mod_op_msg = new CChatModOperationMessage( kick_channel.ChannelID, EChatModOperation.Kick, command.PlayerName );
				Send_Message_To_Chat_Server( mod_op_msg );
			}
			else
			{
				CClientResource.Output_Text( EClientTextID.Client_Chat_Unknown_Channel );
			}
		}

		[GenericHandler]
		private void Handle_Chat_Transfer_Command( CChatTransferSlashCommand command )
		{
			CClientChatChannel transfer_channel = Instance.Infer_Channel_From_String( command.ChannelIdentifier );
			if ( transfer_channel != null )
			{
				CChatModOperationMessage mod_op_msg = new CChatModOperationMessage( transfer_channel.ChannelID, EChatModOperation.Transfer_Moderator, command.PlayerName );
				Send_Message_To_Chat_Server( mod_op_msg );
			}
			else
			{
				CClientResource.Output_Text( EClientTextID.Client_Chat_Unknown_Channel );
			}
		}

		[GenericHandler]
		private void Handle_Chat_Tell_Command( CChatTellSlashCommand command )
		{
			Send_Message_To_Chat_Server( new CPlayerTellRequest( command.PlayerName, command.ChatMessage ) );
		}

		[GenericHandler]
		private void Handle_Chat_Reply_Command( CChatReplySlashCommand command )
		{
			string reply_target = Instance.LastReceivedTellPlayerName;
			if ( reply_target == null )
			{
				CClientResource.Output_Text( EClientTextID.Client_Chat_No_Tells_Received );
			}
			else
			{
				CPlayerTellRequest reply_request = new CPlayerTellRequest( reply_target, command.ChatMessage );
				Send_Message_To_Chat_Server( reply_request );
			}
		}

		[GenericHandler]
		static public void Handle_Chat_Request( CUIInputChatRequest request )
		{
			CClientChatChannel broadcast_channel = Instance.Get_Channel_By_Channel_ID( Instance.LastChannelUsed );
			if ( broadcast_channel != null )
			{
				CBroadcastToChatChannelMessage broadcast_msg = new CBroadcastToChatChannelMessage( broadcast_channel.ChannelID, request.Text );
				Send_Message_To_Chat_Server( broadcast_msg );
			}
			else
			{
				CClientResource.Output_Text( EClientTextID.Client_Chat_No_Channels );
			}
		}

		// Network message handlers
		[NetworkMessageHandler]
		private void On_Join_Chat_Channel_Result( CJoinChatChannelResultMessage message )
		{
			if ( message.ChannelID != EChannelID.Invalid )
			{
				Add_Client_Channel( message.ChannelID, message.ChannelName, message.AnnounceJoinLeave );

				CClientChatChannel channel = Get_Channel_By_Channel_ID( message.ChannelID );
				channel.Moderator = message.Moderator;

				message.Members.Apply( n => channel.Add_Player( n ) );
				message.Gagged.Apply( n => channel.Set_Gag_Status( n, true ) );
			}
			else
			{
				if ( message.CreateError != EChannelCreationError.None )
				{
					Handle_Create_Channel_Error( message.CreateError );
				}
				else if ( message.JoinError != EChannelJoinError.None )
				{
					Handle_Join_Channel_Error( message.JoinError );
				}
			}
		}

		[NetworkMessageHandler]
		private void On_Broadcast_Failure( CBroadcastFailureMessage message )
		{
			Handle_Broadcast_Error( message.Error );
		}

		[NetworkMessageHandler]
		private void On_Player_Chat( CPlayerChatMessage message )
		{
			CClientChatChannel channel = Get_Channel_By_Channel_ID( message.ChannelID );
			string channel_name = null;
			if ( channel == null )
			{
				channel_name = "???";
			}
			else
			{
				channel_name = channel.ChannelName;
			}

			string chat_line = Build_Chat_Line( channel_name, message.PlayerName, message.ChatMessage, channel.ChannelNumber );

			CSharedResource.Output_Text( chat_line );
		}

		[NetworkMessageHandler]
		private void On_Chat_Client_Notification( CChatClientNotificationMessage message )
		{
			CClientChatChannel channel = Get_Channel_By_Channel_ID( message.ChannelID );
			if ( channel == null )
			{
				return;
			}

			switch ( message.ChatEvent )
			{
				case EChatNotification.Channel_Destroyed:
					CClientResource.Output_Text( EClientTextID.Client_Chat_Channel_Destroyed, Build_Channel_Name_Display_String( channel.ChannelName, channel.ChannelNumber ) );
					break;

				case EChatNotification.Player_Joined_Channel:
					if ( channel.AnnounceJoinLeave )
					{
						CClientResource.Output_Text( EClientTextID.Client_Chat_Player_Joined_Channel, message.TargetName, Build_Channel_Name_Display_String( channel.ChannelName, channel.ChannelNumber ) );
					}
					channel.Add_Player( message.TargetID );
					break;

				case EChatNotification.Player_Left_Channel:
					if ( channel.AnnounceJoinLeave )
					{
						CClientResource.Output_Text( EClientTextID.Client_Chat_Player_Left_Channel, message.TargetName, Build_Channel_Name_Display_String( channel.ChannelName, channel.ChannelNumber ) );
					}
					channel.Remove_Player( message.TargetID );
					break;

				case EChatNotification.Player_Kicked_From_Channel:
					CClientResource.Output_Text( EClientTextID.Client_Chat_Player_Kicked_From_Channel, 
																 message.TargetName, 
																 Build_Channel_Name_Display_String( channel.ChannelName, channel.ChannelNumber ),
																 ( message.SourceName.Length > 0 ) ? message.SourceName : CClientResource.Get_Text( EClientTextID.Client_The_Server ) );

					channel.Remove_Player( message.TargetID );
					break;

				case EChatNotification.Player_Gagged:
					CClientResource.Output_Text( EClientTextID.Client_Chat_Player_Gagged_In_Channel, 
																 message.TargetName, 
																 Build_Channel_Name_Display_String( channel.ChannelName, channel.ChannelNumber ),
																 message.SourceName );

					channel.Set_Gag_Status( message.TargetID, true );
					break;

				case EChatNotification.Player_Ungagged:
					CClientResource.Output_Text( EClientTextID.Client_Chat_Player_Ungagged_In_Channel, 
																 message.TargetName, 
																 Build_Channel_Name_Display_String( channel.ChannelName, channel.ChannelNumber ),
																 message.SourceName );

					channel.Set_Gag_Status( message.TargetID, false );
					break;

				case EChatNotification.New_Moderator:
					if ( message.TargetName != null )
					{
						CClientResource.Output_Text( EClientTextID.Client_Chat_Channel_New_Moderator, 
																	 message.TargetName, 
																	 Build_Channel_Name_Display_String( channel.ChannelName, channel.ChannelNumber ) );
					}
					else
					{
						CClientResource.Output_Text( EClientTextID.Client_Chat_Channel_Unmoderated, 
																	 Build_Channel_Name_Display_String( channel.ChannelName, channel.ChannelNumber ) );
					}

					channel.Set_Moderator( message.TargetID );
					break;
			}
		}

		[NetworkMessageHandler]
		private void On_Left_Chat_Channel( CLeftChatChannelMessage message )
		{
			CClientChatChannel channel = Get_Channel_By_Channel_ID( message.ChannelID );
			if ( channel == null )
			{
				return;
			}

			Remove_Channel( channel.ChannelID );

			switch ( message.Reason )
			{
				case ELeftChannelReason.Swapped_By_Server:
				case ELeftChannelReason.Self_Request:
				case ELeftChannelReason.Channel_Shutdown:
					CClientResource.Output_Text( EClientTextID.Client_Chat_Self_Left_Channel, 
																 Build_Channel_Name_Display_String( channel.ChannelName, channel.ChannelNumber ) );
					break;

				case ELeftChannelReason.Kicked_By_Mod:
					CClientResource.Output_Text( EClientTextID.Client_Chat_Self_Kicked_From_Channel_By_Moderator,
																 message.SourceName,
																 Build_Channel_Name_Display_String( channel.ChannelName, channel.ChannelNumber ) );
					break;

				case ELeftChannelReason.Kicked_By_Server:
					CClientResource.Output_Text( EClientTextID.Client_Chat_Self_Kicked_From_Channel_By_Server, 
																 Build_Channel_Name_Display_String( channel.ChannelName, channel.ChannelNumber ) );
					break;

				// Print nothing in this case
				case ELeftChannelReason.Player_Logoff:
					return;
			}
		}

		[NetworkMessageHandler]
		private void On_Leave_Chat_Channel_Failure( CLeaveChatChannelFailureMessage message )
		{
			switch ( message.Reason )
			{
				case ELeaveChatChannelFailureReason.Not_A_Member:
					CClientResource.Output_Text( EClientTextID.Client_Chat_Leave_Failure_Not_A_Member );
					break;

				case ELeaveChatChannelFailureReason.Not_Allowed:
					CClientResource.Output_Text( EClientTextID.Client_Chat_Leave_Failure_Not_Allowed );
					break;

				case ELeaveChatChannelFailureReason.Unknown_Channel:
					CClientResource.Output_Text( EClientTextID.Client_Chat_Unknown_Channel );
					break;
			}
		}

		[NetworkMessageHandler]
		private void On_Chat_Channel_Mod_Operator_Error( CChatModOperationErrorMessage message )
		{
			switch ( message.Error )
			{
				case EChatModOperationError.No_Such_Player_In_Channel:
					CClientResource.Output_Text( EClientTextID.Client_Chat_Player_Not_In_Channel );
					break;

				case EChatModOperationError.Not_Moderator:
					CClientResource.Output_Text( EClientTextID.Client_Chat_Self_Not_Moderator_Of_Channel );
					break;

				case EChatModOperationError.Player_Already_Gagged:
					CClientResource.Output_Text( EClientTextID.Client_Chat_Player_Already_Gagged );
					break;

				case EChatModOperationError.Unable_To_Transfer_Moderator:
					CClientResource.Output_Text( EClientTextID.Client_Chat_Player_Cannot_Become_Moderator );
					break;

				case EChatModOperationError.Unknown_Channel:
					CClientResource.Output_Text( EClientTextID.Client_Chat_Unknown_Channel );
					break;

				case EChatModOperationError.Player_Not_Gagged:
					CClientResource.Output_Text( EClientTextID.Client_Chat_Player_Not_Gagged );
					break;

				case EChatModOperationError.Cannot_Do_That_To_Self:
					CClientResource.Output_Text( EClientTextID.Client_Chat_Invalid_Self_Op );
					break;
			}
		}

		[NetworkMessageHandler]
		private void On_Player_Tell_Receive( CPlayerTellMessage message )
		{
			CClientResource.Output_Text( EClientTextID.Client_Chat_Received_Tell, message.PlayerName, message.ChatMessage );
			LastReceivedTellPlayerName = message.PlayerName;
		}

		[NetworkMessageHandler]
		private void On_Player_Tell_Response( CPlayerTellResponse message )
		{
			switch ( message.Error )
			{
				case ETellError.None:
					CPlayerTellRequest tell_request = message.Request as CPlayerTellRequest;
					CClientResource.Output_Text( EClientTextID.Client_Chat_Sent_Tell, tell_request.PlayerName, tell_request.ChatMessage );
					break;

				case ETellError.Unknown_Player:
					CClientResource.Output_Text( EClientTextID.Client_Chat_Unknown_Tell_Target );
					break;

				case ETellError.Ignored:
					CClientResource.Output_Text( EClientTextID.Client_Chat_Tell_Ignored );
					break;
			}

		}

		static private void Send_Message_To_Chat_Server( CNetworkMessage message )
		{
			CClientLogicalThread.Instance.Send_Message( message, ESessionID.First );
		}

		private void Handle_Create_Channel_Error( EChannelCreationError error )
		{
			switch ( error )
			{
				case EChannelCreationError.UnknownPlayer:
					CClientResource.Output_Text( EClientTextID.Client_Chat_Channel_Creation_Error_Unknown_Creator );
					break;

				case EChannelCreationError.DuplicateInternalName:
					CClientResource.Output_Text( EClientTextID.Client_Chat_Channel_Creation_Error_Channel_Already_Exists );
					break;

				case EChannelCreationError.InvalidExternalName:
				case EChannelCreationError.InvalidInternalName:
				case EChannelCreationError.InvalidInternalNamePrefix:
					CClientResource.Output_Text( EClientTextID.Client_Chat_Channel_Creation_Error_Invalid_Channel_Name );
					break;

				case EChannelCreationError.UnableToAutoJoin:
					CClientResource.Output_Text( EClientTextID.Client_Chat_Channel_Creation_Error_Autojoin_Failure );
					break;
			}
		}

		private void Handle_Join_Channel_Error( EChannelJoinError error )
		{
			switch ( error )
			{
				case EChannelJoinError.AlreadyJoined:
					CClientResource.Output_Text( EClientTextID.Client_Chat_Channel_Join_Error_Already_In_Channel );
					break;

				case EChannelJoinError.Banned:
					CClientResource.Output_Text( EClientTextID.Client_Chat_Channel_Join_Error_Banned );
					break;

				case EChannelJoinError.ChannelDoesNotExist:
					CClientResource.Output_Text( EClientTextID.Client_Chat_Channel_Join_Error_Channel_Does_Not_Exist );
					break;

				case EChannelJoinError.ChannelFull:
					CClientResource.Output_Text( EClientTextID.Client_Chat_Channel_Join_Error_Channel_Full );
					break;

				case EChannelJoinError.NoPermission:
					CClientResource.Output_Text( EClientTextID.Client_Chat_Channel_Join_Error_No_Permission );
					break;
			}
		}

		private void Handle_Broadcast_Error( EChannelBroadcastError error )
		{
			switch ( error )
			{
				case EChannelBroadcastError.Gagged:
					CClientResource.Output_Text( EClientTextID.Client_Chat_Broadcast_Gagged );
					break;

				case EChannelBroadcastError.NotInChannel:
					CClientResource.Output_Text( EClientTextID.Client_Chat_Broadcast_Not_In_Channel );
					break;

				case EChannelBroadcastError.UnknownChannel:
					CClientResource.Output_Text( EClientTextID.Client_Chat_Unknown_Channel );
					break;
			}
		}

		private string Build_Chat_Line( string channel_name, string player_name, string chat_message, EClientChannelNumber channel_number )
		{
			StringBuilder builder = new StringBuilder();
			builder.Append( Build_Channel_Name_Display_String( channel_name, channel_number ) );
			builder.Append( " " );
			builder.Append( player_name );
			builder.Append( ": " );
			builder.Append( chat_message );
			
			return builder.ToString(); 
		}

		private EClientChannelNumber Allocate_Channel_Number()
		{
			EClientChannelNumber channel_number = EClientChannelNumber.Invalid + 1;
			while ( Get_Channel_By_Channel_Number( channel_number ) != null )
			{
				channel_number++;
			}

			return channel_number;
		}

		private CClientChatChannel Get_Channel_By_Channel_Number( EClientChannelNumber channel_number )
		{
			EChannelID channel_id = EChannelID.Invalid;
			if ( !m_ChannelsByNumber.TryGetValue( channel_number, out channel_id ) )
			{
				return null;
			}

			return Get_Channel_By_Channel_ID( channel_id );
		}

		private CClientChatChannel Get_Channel_By_Channel_ID( EChannelID channel_id )
		{
			CClientChatChannel channel = null;
			m_Channels.TryGetValue( channel_id, out channel );
			return channel;
		}

		private CClientChatChannel Get_Channel_By_Channel_Name( string channel_name )
		{
			EChannelID channel_id = EChannelID.Invalid;
			if ( !m_ChannelsByName.TryGetValue( channel_name.ToUpper(), out channel_id ) )
			{
				return null;
			}

			return Get_Channel_By_Channel_ID( channel_id );
		}

		private CClientChatChannel Infer_Channel_From_String( string channel_identifier )
		{
			int channel_number;
			if ( Int32.TryParse( channel_identifier, out channel_number ) )
			{
				return Get_Channel_By_Channel_Number( ( EClientChannelNumber ) channel_number );
			}
			else
			{
				return Get_Channel_By_Channel_Name( channel_identifier );
			}
		}

		private void Add_Client_Channel( EChannelID channel_id, string channel_name, bool announce_join_leave )
		{
			EClientChannelNumber channel_number = Allocate_Channel_Number();
			CClientChatChannel channel = new CClientChatChannel( channel_id, channel_name, channel_number, announce_join_leave );
			Add_Channel( channel );

			CClientResource.Output_Text( EClientTextID.Client_Chat_Channel_Joined, Build_Channel_Name_Display_String( channel_name, channel_number ) );
		}

		private string Build_Channel_Name_Display_String( string channel_name, EClientChannelNumber channel_number )
		{
			StringBuilder builder = new StringBuilder();
			builder.Append( "[" );
			if ( channel_number != EClientChannelNumber.Invalid )
			{
				builder.Append( channel_number.ToString() );
				builder.Append( ". " );
			}
			builder.Append( channel_name );
			builder.Append( "]" );

			return builder.ToString();
		}

		private void Add_Channel( CClientChatChannel channel )
		{
			m_Channels.Add( channel.ChannelID, channel );
			m_ChannelsByName.Add( channel.ChannelName.ToUpper(), channel.ChannelID );
			if ( channel.ChannelNumber != EClientChannelNumber.Invalid )
			{
				m_ChannelsByNumber.Add( channel.ChannelNumber, channel.ChannelID );
			}

			if ( LastChannelUsed == EChannelID.Invalid )
			{
				LastChannelUsed = channel.ChannelID;
			}
		}

		private void Remove_Channel( EChannelID channel_id )
		{
			CClientChatChannel channel = Get_Channel_By_Channel_ID( channel_id );
			if ( channel == null )
			{
				return;
			}

			if ( channel.ChannelNumber != EClientChannelNumber.Invalid )
			{
				m_ChannelsByNumber.Remove( channel.ChannelNumber );
			}

			m_ChannelsByName.Remove( channel.ChannelName.ToUpper() );
			m_Channels.Remove( channel_id );
		}

		private string Build_Channel_Listing()
		{
			if ( CClientLogicalThread.Instance.ConnectedID == EPersistenceID.Invalid )
			{
				return CSharedResource.Get_Text< EClientTextID >( EClientTextID.Client_Chat_Not_Connected );
			}

			if ( m_Channels.Count == 0 )
			{
				return CSharedResource.Get_Text< EClientTextID >( EClientTextID.Client_Chat_No_Channels_Available );
			}

			return String.Format( "{0}{1}",
										 CSharedResource.Get_Text< EClientTextID >( EClientTextID.Client_Chat_Available_Channels ),
										 CSharedUtils.Build_String_List( m_Channels, n => Build_Channel_Name_Display_String( n.Value.ChannelName, n.Value.ChannelNumber ) ) );
		}

		// Properties
		public static CChatClient Instance { get { return m_Instance; } }
		public EChannelID LastChannelUsed { get; private set; }
		public string LastReceivedTellPlayerName { get; private set; }
		
		// Fields
		private static CChatClient m_Instance = new CChatClient();
		
		// Note: This map is not complete; not all channels will have a client channel number in the general case.  Think of WoW, where party, guild, raid and battleground
		// channels do not have a corresponding numerical index, input-wise.  
		private Dictionary< EClientChannelNumber, EChannelID > m_ChannelsByNumber = new Dictionary< EClientChannelNumber, EChannelID >();
		private Dictionary< string, EChannelID > m_ChannelsByName = new Dictionary< string, EChannelID >();

		private Dictionary< EChannelID, CClientChatChannel > m_Channels = new Dictionary< EChannelID, CClientChatChannel >();


	}
}