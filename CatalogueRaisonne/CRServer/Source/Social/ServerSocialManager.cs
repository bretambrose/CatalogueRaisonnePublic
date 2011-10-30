/*

	ServerSocialManager.cs

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

using CRShared;

namespace CRServer
{
	public class CServerSocialManager
	{
		// construction
		static CServerSocialManager() {}
		private CServerSocialManager() {}

		// Methods
		// Public interface
		public static bool Is_Ignoring( EPersistenceID source_player_id, EPersistenceID target_player_id )
		{
			CPersistentPlayerData source_data = CDatabaseProxy.Instance.Get_Player_Data( source_player_id );
			if ( source_data == null )
			{
				return false;
			}

			return source_data.IgnoreList.Contains( target_player_id );
		}

		// Private Interface
		// Network message handlers
		[NetworkMessageHandler]
		private void Handle_Ignore_Player_Request( CIgnorePlayerRequest message, EPersistenceID source_player )
		{
			CAddIgnoredPlayerPersistenceRequest ignore_request = new CAddIgnoredPlayerPersistenceRequest( source_player, message.PlayerName );
			ignore_request.Handler = delegate( CPersistenceResponse response ) 
			{
				On_Ignore_Persistence_Return( response, message.RequestID, source_player );
			};

			CDatabaseProxy.Instance.Submit_Request( ignore_request );
		}

		[NetworkMessageHandler]
		private void Handle_Unignore_Player_Request( CUnignorePlayerRequest message, EPersistenceID source_player )
		{
			CRemoveIgnoredPlayerPersistenceRequest unignore_request = new CRemoveIgnoredPlayerPersistenceRequest( source_player, message.PlayerName );
			unignore_request.Handler = delegate( CPersistenceResponse response )
			{
				On_Unignore_Persistence_Return( response, message.RequestID, source_player );
			};

			CDatabaseProxy.Instance.Submit_Request( unignore_request );
		}

		private void On_Ignore_Persistence_Return( CPersistenceResponse response, EMessageRequestID request_id, EPersistenceID source_player )
		{
			CAddIgnoredPlayerPersistenceResponse ignore_response = response as CAddIgnoredPlayerPersistenceResponse;
			if ( ignore_response.Error != EPersistenceError.None )
			{
				CServerMessageRouter.Send_Message_To_Player( new CIgnorePlayerResponse( request_id, EPersistenceID.Invalid, EIgnorePlayerResult.Persistence_Error ), source_player );
				return;
			}

			if ( ignore_response.Result != EIgnorePlayerResult.Success )
			{
				CServerMessageRouter.Send_Message_To_Player( new CIgnorePlayerResponse( request_id, EPersistenceID.Invalid, ignore_response.Result ), source_player );
				return;
			}

			CServerMessageRouter.Send_Message_To_Chat_Server( new CAddPlayerToChatIgnoreListServerMessage( source_player, ignore_response.IgnoredPlayerID ) );
			CServerMessageRouter.Send_Message_To_Player( new CIgnorePlayerResponse( request_id, ignore_response.IgnoredPlayerID, EIgnorePlayerResult.Success ), source_player );

			CServerLobby lobby = CServerLobbyManager.Instance.Get_Lobby_By_Creator( source_player );
			if ( lobby != null && lobby.IsPublic )
			{
				CServerLobbyBrowserManager.Instance.Notify_Browser_Of_Lobby_Ban_Or_Ignore( ignore_response.IgnoredPlayerID, lobby.ID );
			}
		}

		private void On_Unignore_Persistence_Return( CPersistenceResponse response, EMessageRequestID request_id, EPersistenceID source_player )
		{
			CRemoveIgnoredPlayerPersistenceResponse unignore_response = response as CRemoveIgnoredPlayerPersistenceResponse;
			if ( unignore_response.Error != EPersistenceError.None )
			{
				CServerMessageRouter.Send_Message_To_Player( new CUnignorePlayerResponse( request_id, EPersistenceID.Invalid, EUnignorePlayerResult.Persistence_Error ), source_player );
				return;
			}

			if ( unignore_response.Result != EUnignorePlayerResult.Success )
			{
				CServerMessageRouter.Send_Message_To_Player( new CUnignorePlayerResponse( request_id, EPersistenceID.Invalid, unignore_response.Result ), source_player );
				return;
			}

			CServerMessageRouter.Send_Message_To_Chat_Server( new CRemovePlayerFromChatIgnoreListServerMessage( source_player, unignore_response.IgnoredPlayerID ) );
			CServerMessageRouter.Send_Message_To_Player( new CUnignorePlayerResponse( request_id, unignore_response.IgnoredPlayerID, EUnignorePlayerResult.Success ), source_player );

			CServerLobby lobby = CServerLobbyManager.Instance.Get_Lobby_By_Creator( source_player );
			if ( lobby != null && lobby.IsPublic )
			{
				CServerLobbyBrowserManager.Instance.Notify_Browser_Of_Lobby_Unban_Or_Unignore( unignore_response.IgnoredPlayerID, lobby.ID );
			}
		}

		// Properties
		public static CServerSocialManager Instance { get { return m_Instance; } }

		// Fields
		private static CServerSocialManager m_Instance = new CServerSocialManager();
	}
}