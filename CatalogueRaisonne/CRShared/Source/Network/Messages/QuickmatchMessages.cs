/*

	QuickmatchMessages.cs

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
	[NetworkMessage]
	public class CCreateQuickmatchRequest : CRequestMessage
	{
		// Construction
		public CCreateQuickmatchRequest( string player_name, uint game_count, bool allow_observers ) :
			base()
		{
			PlayerName = player_name;
			GameCount = game_count;
			AllowObservers = allow_observers;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CCreateQuickmatchRequest] PlayerName={0}, GameCount={1}, AllowObservers={2} )", PlayerName.ToString(), GameCount, AllowObservers );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Create_Quickmatch_Request; } }

		public string PlayerName { get; private set; }
		public uint GameCount { get; private set; }
		public bool AllowObservers { get; private set; } 
	}

	public enum ECreateQuickmatchResult
	{
		Success,

		Invalid_State_To_Create,
		Target_Invalid_State_To_Create,
		Cannot_Invite_Yourself,
		Unknown_Target_Player,
		Ignored_By_Target_Player,
		Already_Has_Pending_Quickmatch,
		Target_Already_Has_Pending_Quickmatch
	}

	[NetworkMessage]
	public class CCreateQuickmatchResponse : CResponseMessage
	{
		// Construction
		public CCreateQuickmatchResponse( EMessageRequestID request_id, ECreateQuickmatchResult result ) :
			base( request_id )
		{
			Result = result;
		}

		// Methods
		// Public interface
		public override string ToString()
		{
			return String.Format( "( [CCreateQuickmatchResponse] Result={0} )", Result.ToString() );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Create_Quickmatch_Response; } }

		public ECreateQuickmatchResult Result { get; private set; }

	}

	[NetworkMessage]
	public class CInviteToQuickmatchRequest : CRequestMessage
	{
		// Construction
		public CInviteToQuickmatchRequest( string player_name, uint game_count ) :
			base()
		{
			PlayerName = player_name;
			GameCount = game_count;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CInviteToQuickmatchRequest] PlayerName={0}, GameCount={1} )", PlayerName, GameCount );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Invite_To_Quickmatch_Request; } }

		public string PlayerName { get; private set; }
		public uint GameCount { get; private set; }
	}

	[NetworkMessage]
	public class CInviteToQuickmatchResponse : CResponseMessage
	{
		// Construction
		public CInviteToQuickmatchResponse( EMessageRequestID request_id, bool accepted ) :
			base( request_id )
		{
			Accepted = accepted;
		}

		// Methods
		// Public interface
		public override string ToString()
		{
			return String.Format( "( [CInviteToQuickmatchResponse] Accepted={0} )", Accepted.ToString() );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Invite_To_Quickmatch_Response; } }

		public bool Accepted { get; private set; }

	}

	[NetworkMessage]
	public class CCancelQuickmatchRequest : CRequestMessage
	{
		// Construction
		public CCancelQuickmatchRequest() :
			base()
		{
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CCancelQuickmatchRequest] )" );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Cancel_Quickmatch_Request; } }
	}

	public enum ECancelQuickmatchResult
	{
		Success,

		Not_In_Quickmatch
	}

	[NetworkMessage]
	public class CCancelQuickmatchResponse : CResponseMessage
	{
		// Construction
		public CCancelQuickmatchResponse( EMessageRequestID request_id, ECancelQuickmatchResult result ) :
			base( request_id )
		{
			Result = result;
		}

		// Methods
		// Public interface
		public override string ToString()
		{
			return String.Format( "( [CCancelQuickmatchResponse] Result={0} )", Result.ToString() );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Cancel_Quickmatch_Response; } }

		public ECancelQuickmatchResult Result { get; private set; }

	}

	public enum EQuickmatchNotification
	{
		Invalid,

		Declined_Invite,
		Accepted_Invite,
		Canceled_Invite
	}

	[NetworkMessage]
	public class CQuickmatchNotificationMessage : CNetworkMessage
	{
		// Construction
		public CQuickmatchNotificationMessage( EQuickmatchNotification notice, string player_name ) 
		{
			Notice = notice;
			PlayerName = player_name;
		}

		// Methods
		// Public interface
		public override string ToString()
		{
			return String.Format( "( [CQuickmatchNotificationMessage] Notice={0}, PlayerName={1} )", Notice.ToString(), PlayerName );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Quickmatch_Notification; } }

		public string PlayerName { get; private set; }
		public EQuickmatchNotification Notice { get; private set; }

	}
}
