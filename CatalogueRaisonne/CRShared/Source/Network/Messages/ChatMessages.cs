/*

	ChatMessages.cs

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
using System.Runtime.Serialization;
using System.Text;

using CRShared.Chat;

namespace CRShared
{

	[NetworkMessage]
	public class CBroadcastToChatChannelMessage : CNetworkMessage
	{
		// Construction
		public CBroadcastToChatChannelMessage( EChannelID channel_id, string chat_message ) :
			base()
		{
			ChannelID = channel_id;
			ChatMessage = chat_message;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CBroadcastToChatChannelMessage]: ChannelID = {0}, ChatMessage = {1} )", (int) ChannelID, ChatMessage );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Broadcast_To_Chat_Channel; } }

		public EChannelID ChannelID { get; private set; }
		public string ChatMessage { get; private set; }
	}

	[NetworkMessage]
	public class CPlayerChatMessage : CNetworkMessage
	{
		// Construction
		public CPlayerChatMessage( EChannelID channel_id, string player_name, string chat_message ) :
			base()
		{
			ChannelID = channel_id;
			PlayerName = player_name;
			ChatMessage = chat_message;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CPlayerChatMessage]: ChannelID = {0}, PlayerName = {1}, ChatMessage = {2} )", (int) ChannelID, PlayerName, ChatMessage );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Player_Chat; } }

		public EChannelID ChannelID { get; private set; }
		public string PlayerName { get; private set; }
		public string ChatMessage { get; private set; }
	}

	[NetworkMessage]
	public class CBroadcastFailureMessage : CNetworkMessage
	{
		// Construction
		public CBroadcastFailureMessage( EChannelBroadcastError error ) :
			base()
		{
			Error = error;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CBroadcastFailureMessage]: Error = {0} )", Error.ToString() );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Broadcast_Failure; } }

		public EChannelBroadcastError Error { get; private set; }
	}

	[NetworkMessage]
	public class CCreateOrJoinChatChannelMessage : CNetworkMessage
	{
		// Construction
		public CCreateOrJoinChatChannelMessage( string channel_name ) :
			base()
		{
			ChannelName = channel_name;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CCreateOrJoinChatChannelMessage]: ChannelName = {0} )", ChannelName );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Create_Or_Join_Chat_Channel; } }

		public string ChannelName { get; private set; }
	}

	[NetworkMessage]
	public class CJoinChatChannelResultMessage : CNetworkMessage
	{
		// Construction
		public CJoinChatChannelResultMessage() :
			base()
		{
			Members = new HashSet< EPersistenceID >();
			Gagged = new HashSet< EPersistenceID >();
		}

		// Methods
		// Public interface
		public override string ToString()
		{
			return String.Format( "( [CJoinChatChannelResultMessage]: ChannelID = {0}, CreateError = {1}, JoinError = {2}, ChannelName = {3}, Moderator = {4}, AnnounceJoinLeave = {5}, Members = {6}, Gagged = {7} )", 
										 (int)ChannelID, 
										 CreateError.ToString(), 
										 JoinError.ToString(),
										 ChannelName,
										 (long)Moderator,
										 AnnounceJoinLeave,
										 CSharedUtils.Build_String_List( Members, n => ((long) n).ToString() ),
										 CSharedUtils.Build_String_List( Gagged, n => ((long) n).ToString() ) );
		}

		public void Add_Member( EPersistenceID id ) { Members.Add( id ); }
		public void Add_Gagged( EPersistenceID id ) { Gagged.Add( id ); }

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Join_Chat_Channel_Result; } }

		public EChannelCreationError CreateError { get; set; }
		public EChannelJoinError JoinError { get; set; }
		public EChannelID ChannelID { get; set; }
		public string ChannelName { get; set; }
		public EPersistenceID Moderator { get; set; }
		public bool AnnounceJoinLeave { get; set; }

		public HashSet< EPersistenceID > Members { get; private set; }
		public HashSet< EPersistenceID > Gagged { get; private set; }
	}

	[NetworkMessage]
	public class CLeaveChatChannelMessage : CNetworkMessage
	{
		// Construction
		public CLeaveChatChannelMessage( EChannelID channel_id ) :
			base()
		{
			ChannelID = channel_id;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CLeaveChatChannelMessage]: ChannelID = {0} )" , (int)ChannelID );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Leave_Chat_Channel; } }

		public EChannelID ChannelID { get; private set; }
	}	

	[NetworkMessage]
	public class CLeaveChatChannelFailureMessage : CNetworkMessage
	{
		// Construction
		public CLeaveChatChannelFailureMessage( ELeaveChatChannelFailureReason reason ) :
			base()
		{
			Reason = reason;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CLeaveChatChannelFailureMessage]: Reason = {0} )", Reason.ToString() );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Leave_Chat_Channel_Failure; } }

		public ELeaveChatChannelFailureReason Reason { get; private set; }
	}

	[NetworkMessage]
	public class CChatClientNotificationMessage : CNetworkMessage
	{
		// Construction
		public CChatClientNotificationMessage( EChannelID channel_id, string target_name, EPersistenceID target_id, EChatNotification chat_event, string source_name ) :
			base()
		{
			ChannelID = channel_id;
			TargetName = target_name;
			TargetID = target_id;
			SourceName = source_name;
			ChatEvent = chat_event;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CChatClientNotificationMessage]: ChannelID = {0}, Source = {1}, Target = {2}, Event = {3} )",
										 (int)ChannelID,
										 SourceName,
										 TargetName,
										 ChatEvent.ToString() );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Chat_Client_Notification; } }

		public EChannelID ChannelID { get; private set; }
		public string TargetName { get; private set; }
		public string SourceName { get; private set; }
		public EChatNotification ChatEvent { get; private set; }
		public EPersistenceID TargetID { get; private set; }
	}	
	
	[NetworkMessage]
	public class CLeftChatChannelMessage : CNetworkMessage
	{
		// Construction
		public CLeftChatChannelMessage( EChannelID channel_id, ELeftChannelReason reason, string source_name ) :
			base()
		{
			ChannelID = channel_id;
			Reason = reason;
			SourceName = source_name;
		}

		// Methods
		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();

			builder.Append( "( [CLeftChatChannelMessage]: ChannelID = " );
			builder.Append( (int)ChannelID );
			builder.Append( ", Reason = " );
			builder.Append( Reason.ToString() );
			if ( SourceName != null )
			{
				builder.Append( ", SourceName = " );
				builder.Append( SourceName );
			}
			builder.Append( ")" );

			return builder.ToString();
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Left_Chat_Channel; } }

		public EChannelID ChannelID { get; private set; }
		public ELeftChannelReason Reason { get; private set; }
		public string SourceName { get; private set; }
	}	
	
	[NetworkMessage]
	public class CChatModOperationMessage : CNetworkMessage
	{
		// Construction
		public CChatModOperationMessage( EChannelID channel_id, EChatModOperation operation, string player_name ) :
			base()
		{
			ChannelID = channel_id;
			Operation = operation;
			PlayerName = player_name;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CChatModOperationMessage]: ChannelID = {0}, Operation = {1}, PlayerName = {2} )", (int)ChannelID, Operation.ToString(), PlayerName );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Chat_Mod_Operation; } }

		public EChannelID ChannelID { get; private set; }
		public EChatModOperation Operation { get; private set; }
		public string PlayerName { get; private set; }
	}

	[NetworkMessage]
	public class CChatModOperationErrorMessage : CNetworkMessage
	{
		// Construction
		public CChatModOperationErrorMessage( EChatModOperationError error ) :
			base()
		{
			Error = error;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CChatModOperationErrorMessage]: Error = {0} )", Error.ToString() );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Chat_Mod_Operation_Error; } }

		public EChatModOperationError Error { get; private set; }
	}
		
	[NetworkMessage]
	public class CPlayerTellMessage : CNetworkMessage
	{
		// Construction
		public CPlayerTellMessage( string player_name, string chat_message ) :
			base()
		{
			PlayerName = player_name;
			ChatMessage = chat_message;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CPlayerTellMessage]: PlayerName = {0}, ChatMessage = {1} )", PlayerName, ChatMessage );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Player_Tell; } }

		public string PlayerName { get; private set; }
		public string ChatMessage { get; private set; }
	}

	[NetworkMessage]
	public class CPlayerTellRequest : CRequestMessage
	{
		// Construction
		public CPlayerTellRequest( string player_name, string chat_message ) :
			base()
		{
			PlayerName = player_name;
			ChatMessage = chat_message;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CPlayerTellRequest]: PlayerName = {0}, ChatMessage = {1} )", PlayerName, ChatMessage );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Player_Tell_Request; } }

		public string PlayerName { get; private set; }
		public string ChatMessage { get; private set; }
	}	
	
	[NetworkMessage]
	public class CPlayerTellResponse : CResponseMessage
	{
		// Construction
		public CPlayerTellResponse( EMessageRequestID request_id ) :
			base( request_id )
		{
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CPlayerTellRequest]: Error = {0} )", Error.ToString() );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Player_Tell_Response; } }

		public ETellError Error { get; set; }
	}						
}
