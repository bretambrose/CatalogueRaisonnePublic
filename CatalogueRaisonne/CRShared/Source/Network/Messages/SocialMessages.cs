/*

	SocialMessages.cs

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

namespace CRShared
{
	[NetworkMessage]
	public class CIgnorePlayerRequest : CRequestMessage
	{
		// Construction
		public CIgnorePlayerRequest( string player_name )
		{
			PlayerName = player_name;
		}
		
		// Methods
		public override string ToString()
		{
			return String.Format( "( [CIgnorePlayerRequest] PlayerName = {0} )", PlayerName );
		}
				
		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Ignore_Player_Request; } }

		public string PlayerName { get; private set; }
	}

	public enum EIgnorePlayerResult
	{
		Success,

		Cannot_Ignore_Self,
		Unknown_Player,
		Already_Ignored,
		Persistence_Error
	}

	[NetworkMessage]
	public class CIgnorePlayerResponse : CResponseMessage
	{
		// Construction
		public CIgnorePlayerResponse( EMessageRequestID request_id, EPersistenceID ignored_id, EIgnorePlayerResult result ) :
			base( request_id )
		{
			IgnoredID = ignored_id;
			Result = result;
		}
		
		// Methods
		public override string ToString()
		{
			return String.Format( "( [CIgnorePlayerResponse] IgnoredID = {0}, Result = {1} )", IgnoredID, Result.ToString() );
		}
				
		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Ignore_Player_Response; } }

		public EPersistenceID IgnoredID { get; private set; }
		public EIgnorePlayerResult Result { get; private set; }
	}

	[NetworkMessage]
	public class CUnignorePlayerRequest : CRequestMessage
	{
		// Construction
		public CUnignorePlayerRequest( string player_name )
		{
			PlayerName = player_name;
		}
		
		// Methods
		public override string ToString()
		{
			return String.Format( "( [CUnignorePlayerRequest] PlayerName = {0} )", PlayerName );
		}
				
		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Unignore_Player_Request; } }

		public string PlayerName { get; private set; }
	}

	public enum EUnignorePlayerResult
	{
		Success,

		Unknown_Player,
		Not_Already_Ignored,
		Persistence_Error
	}

	[NetworkMessage]
	public class CUnignorePlayerResponse : CResponseMessage
	{
		// Construction
		public CUnignorePlayerResponse( EMessageRequestID request_id, EPersistenceID ignored_id, EUnignorePlayerResult result ) :
			base( request_id )
		{
			IgnoredID = ignored_id;
			Result = result;
		}
		
		// Methods
		public override string ToString()
		{
			return String.Format( "( [CUnignorePlayerResponse] IgnoredID = {0}, Result = {1} )", IgnoredID, Result.ToString() );
		}
				
		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Unignore_Player_Response; } }

		public EPersistenceID IgnoredID { get; private set; }
		public EUnignorePlayerResult Result { get; private set; }
	}
}