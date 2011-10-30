/*

	ServerChatMessages.cs

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
using CRServer.Chat;
using CRShared.Chat;

namespace CRServer
{
	public class CAnnouncePlayerToChatServerRequest : CRequestMessage
	{
		// Construction
		public CAnnouncePlayerToChatServerRequest( ESessionID session_id, EPersistenceID persistence_id, string name, List< EPersistenceID > ignore_list ) :
			base()
		{
			SessionID = session_id;
			PersistenceID = persistence_id;
			Name = name;

			IgnoreList = new List< EPersistenceID >();
			ignore_list.ShallowCopy( IgnoreList );
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CAnnouncePlayerToChatServerRequest]: Name={0}, SessionID={1}, PersistenceID={2}, IgnoreList=({3}) )", 
										 Name, 
										 (int)SessionID, 
										 (ulong)PersistenceID, 
										 CSharedUtils.Build_String_List( IgnoreList, n => ((ulong) n).ToString() ) );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Server_Request_Announce_Player_To_Chat; } }

		public ESessionID SessionID { get; private set; }
		public EPersistenceID PersistenceID { get; private set; }
		public string Name { get; private set; }
		public List< EPersistenceID > IgnoreList { get; private set; }
	}

	public class CAnnouncePlayerToChatServerResponse : CResponseMessage
	{
		// Construction
		public CAnnouncePlayerToChatServerResponse( EMessageRequestID request_id, bool success ) :
			base( request_id )
		{
			Success = success;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CAnnouncePlayerToChatServerResponse]: Success={0} )", Success );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Server_Response_Announce_Player_To_Chat; } }

		public bool Success { get; private set; }
	}

	public class CRemovePlayerFromChatServerMessage : CNetworkMessage
	{
		// Construction
		public CRemovePlayerFromChatServerMessage( ESessionID session_id ) :
			base()
		{
			SessionID = session_id;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CRemovePlayerFromChatServerMessage]: SessionID={0} )", (int)SessionID );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Server_Message_Remove_Player_From_Chat; } }

		public ESessionID SessionID { get; private set; }
	}

	public class CCreateChatChannelRequestServerMessage : CRequestMessage
	{
		// Construction
		public CCreateChatChannelRequestServerMessage( CChatChannelConfig config ) :
			base()
		{
			Config = config;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CCreateChatChannelRequestServerMessage]: Config={0} )", Config.ToString() );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Server_Message_Create_Chat_Channel_Request; } }

		public CChatChannelConfig Config { get; private set; }
	}

	public class CCreateChatChannelResponseServerMessage : CResponseMessage
	{
		// Construction
		public CCreateChatChannelResponseServerMessage( EMessageRequestID request_id ) :
			base( request_id )
		{
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CCreateChatChannelResponseServerMessage]: ChannelName={0}, ChannelID={1}, Error={2} )", ChannelName, (int)ChannelID, Error.ToString() );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Server_Message_Create_Chat_Channel_Response; } }

		public EChannelCreationError Error { get; set; }
		public EChannelID ChannelID { get; set; }
		public string ChannelName { get; set; }
	}

	public class CDestroyChatChannelServerMessage : CNetworkMessage
	{
		// Construction
		public CDestroyChatChannelServerMessage( EChannelID channel_id ) :
			base()
		{
			ChannelID = channel_id;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CDestroyChatChannelServerMessage]: ChannelID={0} )", ChannelID );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Server_Message_Destroy_Chat_Channel; } }

		public EChannelID ChannelID { get; private set; }
	}

	public class CJoinChatChannelRequestServerMessage : CRequestMessage
	{
		// Construction
		public CJoinChatChannelRequestServerMessage( EPersistenceID player_id, EChannelID channel_id, EChannelGameProperties remove_mask ) :
			base()
		{
			PlayerID = player_id;
			ChannelID = channel_id;
			RemoveMask = remove_mask;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CJoinChatChannelRequestServerMessage]: PlayerID={0}, ChannelID={1}, RemoveMask={2} )", (long)PlayerID, (int)ChannelID, RemoveMask.ToString() );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Server_Message_Join_Chat_Channel_Request; } }

		public EPersistenceID PlayerID { get; private set; }
		public EChannelID ChannelID { get; private set; }
		public EChannelGameProperties RemoveMask { get; private set; }
	}

	public class CJoinChatChannelResponseServerMessage : CResponseMessage
	{
		// Construction
		public CJoinChatChannelResponseServerMessage( EMessageRequestID request_id, EChannelJoinError error ) :
			base( request_id )
		{
			Error = error;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CJoinChatChannelResponseServerMessage]: Error={0} )", Error );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Server_Message_Join_Chat_Channel_Response; } }

		public EChannelJoinError Error { get; private set; }
	}

	public class CRemoveFromChatChannelServerMessage : CNetworkMessage
	{
		// Construction
		public CRemoveFromChatChannelServerMessage( EPersistenceID player_id, EChannelID channel_id ) :
			base()
		{
			PlayerID = player_id;
			ChannelID = channel_id;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CRemoveFromChatChannelServerMessage]: PlayerID={0}, ChannelID={1} )", PlayerID, ChannelID );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Server_Message_Remove_From_Chat_Channel; } }

		public EPersistenceID PlayerID { get; private set; }
		public EChannelID ChannelID { get; private set; }
	}

	public class CAddPlayerToChatIgnoreListServerMessage : CNetworkMessage
	{
		// Construction
		public CAddPlayerToChatIgnoreListServerMessage( EPersistenceID player_id, EPersistenceID ignored_id ) :
			base()
		{
			PlayerID = player_id;
			IgnoredID = ignored_id;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CAddPlayerToChatIgnoreListServerMessage]: PlayerID={0}, IgnoredID={1} )", PlayerID, IgnoredID );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Server_Message_Add_Player_To_Chat_Ignore; } }

		public EPersistenceID PlayerID { get; private set; }
		public EPersistenceID IgnoredID { get; private set; }
	}

	public class CRemovePlayerFromChatIgnoreListServerMessage : CNetworkMessage
	{
		// Construction
		public CRemovePlayerFromChatIgnoreListServerMessage( EPersistenceID player_id, EPersistenceID ignored_id ) :
			base()
		{
			PlayerID = player_id;
			IgnoredID = ignored_id;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CRemovePlayerFromChatIgnoreListServerMessage]: PlayerID={0}, IgnoredID={1} )", PlayerID, IgnoredID );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Server_Message_Remove_Player_From_Chat_Ignore; } }

		public EPersistenceID PlayerID { get; private set; }
		public EPersistenceID IgnoredID { get; private set; }
	}
}
