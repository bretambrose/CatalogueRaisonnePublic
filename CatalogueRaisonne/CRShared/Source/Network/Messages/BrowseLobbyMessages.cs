/*

	BrowseLobbyMessages.cs

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

namespace CRShared
{
	[NetworkMessage]
	public class CStartBrowseLobbyRequest : CRequestMessage
	{
		// Construction
		public CStartBrowseLobbyRequest( EGameModeType game_mode_mask, ELobbyMemberType member_type_mask, bool join_first_available ) :
			base()
		{
			BrowseCriteria = new CBrowseLobbyMatchCriteria( game_mode_mask, member_type_mask );
			JoinFirstAvailable = join_first_available;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CStartBrowseLobbyRequest] BrowseCriteria={0}, JoinFirstAvailable={1} )", BrowseCriteria.ToString(), JoinFirstAvailable.ToString() );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Start_Browse_Lobby_Request; } }

		public CBrowseLobbyMatchCriteria BrowseCriteria { get; private set; }
		public bool JoinFirstAvailable { get; private set; }
	}

	public enum EStartBrowseResult
	{
		Invalid = 0,
		Success,
		AutoJoined,
		Invalid_State
	}

	[NetworkMessage]
	public class CStartBrowseLobbyResponse : CResponseMessage
	{
		// Construction
		public CStartBrowseLobbyResponse( EMessageRequestID request_id, EStartBrowseResult result ) :
			base( request_id )
		{
			Result = result;
			LobbySummaries = new List< CLobbySummary >();
		}

		// Methods
		// Public interface
		public override string ToString()
		{
			return String.Format( "( [CStartBrowseLobbyResponse] Result={0}, LobbySummaries=({1}) )", Result.ToString(), CSharedUtils.Build_String_List( LobbySummaries ) );
		}

		public void Add_Summary( CLobbySummary summary )
		{
			LobbySummaries.Add( summary );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Start_Browse_Lobby_Response; } }

		public List< CLobbySummary > LobbySummaries { get; private set; }
		public EStartBrowseResult Result { get; private set; }

	}

	[NetworkMessage]
	public class CEndBrowseLobbyMessage : CNetworkMessage
	{
		// Construction
		public CEndBrowseLobbyMessage() :
			base()
		{}

		// Methods
		public override string ToString()
		{
			return "( [CEndBrowseLobbyMessage] )";
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_End_Browse_Lobby; } }
	}

	[NetworkMessage, Serializable]
	public class CBrowseNextLobbySetRequest : CRequestMessage
	{
		// Construction
		public CBrowseNextLobbySetRequest() :
			base()
		{}

		// Methods
		public override string ToString()
		{
			return "( [CBrowseNextLobbySetRequest] )";
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Browse_Next_Lobby_Set_Request; } }
	}

	[NetworkMessage]
	public class CBrowsePreviousLobbySetRequest : CRequestMessage
	{
		// Construction
		public CBrowsePreviousLobbySetRequest() :
			base()
		{}

		// Methods
		public override string ToString()
		{
			return "( [CBrowsePreviousLobbySetRequest] )";
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Browse_Previous_Lobby_Set_Request; } }
	}
	
	public enum ECursorBrowseResult
	{
		Success = 0,

		Not_Browsing,
		End_Of_Cursor
	}

	[NetworkMessage]
	public class CCursorBrowseLobbyResponse : CResponseMessage
	{
		// Construction
		public CCursorBrowseLobbyResponse( EMessageRequestID request_id, ECursorBrowseResult result ) :
			base( request_id )
		{
			Result = result;
			LobbySummaries = new List< CLobbySummary >();
		}

		// Methods
		// Public interface
		public override string ToString()
		{
			return String.Format( "( [CCursorBrowseLobbyResponse] Result={0}, LobbySummaries=({1}) )", Result.ToString(), CSharedUtils.Build_String_List( LobbySummaries ) );
		}

		public void Add_Summary( CLobbySummary summary )
		{
			LobbySummaries.Add( summary );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Cursor_Browse_Lobby_Response; } }

		public List< CLobbySummary > LobbySummaries { get; private set; }
		public ECursorBrowseResult Result { get; private set; }
	}

	[NetworkMessage]
	public class CBrowseLobbyAddRemoveMessage : CNetworkMessage
	{
		// Construction
		public CBrowseLobbyAddRemoveMessage( CLobbySummary new_lobby, ELobbyID removed_lobby ) :
			base()
		{
			Lobbies = new List< CLobbySummary >();
			if ( new_lobby != null )
			{
				Lobbies.Add( new_lobby );
			}

			RemovedLobby = removed_lobby;
		}

		// Methods
		// Public interface
		public void Add_Summary( CLobbySummary summary )
		{
			Lobbies.Add( summary );
		}

		public override string ToString()
		{
			return String.Format( "( [CBrowseLobbyAddRemoveMessage] NewLobbies=({0}), RemovedLobby={1} )", CSharedUtils.Build_String_List( Lobbies ), (int)RemovedLobby );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Browse_Lobby_Add_Remove_Message; } }

		public List< CLobbySummary > Lobbies { get; private set; }
		public ELobbyID RemovedLobby { get; private set; }
	}

	[NetworkMessage]
	public class CBrowseLobbyDeltaMessage : CNetworkMessage
	{
		// Construction
		public CBrowseLobbyDeltaMessage( CLobbySummaryDelta delta ) :
			base()
		{
			Delta = delta;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CBrowseLobbyDeltaMessage] Delta={0} )", Delta.ToString() );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Browse_Lobby_Delta_Message; } }

		public CLobbySummaryDelta Delta { get; private set; }
	}
}
