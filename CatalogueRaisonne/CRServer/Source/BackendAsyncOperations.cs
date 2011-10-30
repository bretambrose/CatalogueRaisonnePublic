/*

	BackendAsyncOperations.cs

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
using CRShared.Chat;
using CRServer.Chat;

namespace CRServer
{
	public static class CAsyncBackendOperations
	{
		// Public interface
		// Operations

		// Channel Creation
		public static void Create_Lobby_Chat_Channel( CChatChannelConfig channel_config, ELobbyID lobby_id, EPersistenceID source_player )
		{
			CCreateChatChannelRequestServerMessage create_lobby_channel_message = new CCreateChatChannelRequestServerMessage( channel_config );
			create_lobby_channel_message.Handler = 
				delegate( CResponseMessage response ) 
				{
					CCreateChatChannelResponseServerMessage response_msg = response as CCreateChatChannelResponseServerMessage;
					On_Lobby_Chat_Channel_Creation_Response( lobby_id, source_player, response_msg );
				};

			CServerMessageRouter.Send_Message_To_Chat_Server( create_lobby_channel_message );
		}

		public static void Create_Match_Chat_Channel( CChatChannelConfig match_channel_config, EMatchInstanceID match_id )
		{
			CCreateChatChannelRequestServerMessage create_match_channel_message = new CCreateChatChannelRequestServerMessage( match_channel_config );
			create_match_channel_message.Handler = 
				delegate( CResponseMessage response ) 
				{
					CCreateChatChannelResponseServerMessage response_msg = response as CCreateChatChannelResponseServerMessage;
					On_Match_Chat_Channel_Creation_Response( match_id, response_msg );
				};

			CServerMessageRouter.Send_Message_To_Chat_Server( create_match_channel_message );
		}

		public static void Create_Observer_Chat_Channel( CChatChannelConfig observer_channel_config, EMatchInstanceID match_id )
		{
			CCreateChatChannelRequestServerMessage create_observer_channel_message = new CCreateChatChannelRequestServerMessage( observer_channel_config );
			create_observer_channel_message.Handler = 
				delegate( CResponseMessage response ) 
				{
					CCreateChatChannelResponseServerMessage response_msg = response as CCreateChatChannelResponseServerMessage;
					On_Observer_Chat_Channel_Creation_Response( match_id, response_msg );
				};

			CServerMessageRouter.Send_Message_To_Chat_Server( create_observer_channel_message );
		}

		public static void New_Player_Announce_To_Chat( CConnectedPlayer player, CPersistentPlayerData player_data, EMessageRequestID client_request_id )
		{
			ESessionID session_id = player.SessionID;
			CAnnouncePlayerToChatServerRequest announce_request = new CAnnouncePlayerToChatServerRequest( session_id, player.PersistenceID, player.Name, player_data.IgnoreList );
			announce_request.Handler = delegate( CResponseMessage response ) { On_Chat_Server_Announce_Response( response, session_id, client_request_id ); };

			CServerMessageRouter.Send_Message_To_Chat_Server( announce_request );
		}

		// Player ops
		public static void Join_General_Channel( EPersistenceID player_id, EMessageRequestID client_request_id )
		{
			ESessionID session_id = CConnectedPlayerManager.Instance.Get_Active_Player_Session_ID( player_id );

			Change_System_Channel( player_id, 
										  CConnectedPlayerManager.Instance.GeneralChatChannel, 
										  EChannelGameProperties.OrdinarySingletonMask, 
										  delegate( CResponseMessage response ) { On_Join_General_Channel_Response( response, session_id, client_request_id ); } );
		}	

		public static void Player_Join_Lobby_Channel( EPersistenceID player_id, ELobbyID lobby_id, EChannelID channel_id )
		{
			Change_System_Channel( player_id, 
										  channel_id, 
										  EChannelGameProperties.OrdinarySingletonMask,
										  delegate( CResponseMessage response ) {
												On_Lobby_Chat_Channel_Join_Response( player_id, lobby_id, response as CJoinChatChannelResponseServerMessage );
										  } );
		}

		public static void Join_Match_Channel( EMatchInstanceID match_id, EPersistenceID player_id, EChannelID match_channel_id )
		{
			Change_System_Channel( player_id, 
										  match_channel_id, 
										  EChannelGameProperties.OrdinarySingletonMask,
										  delegate( CResponseMessage response ) {
												On_Match_Chat_Channel_Join_Response( match_id, player_id, response as CJoinChatChannelResponseServerMessage );
										  } );
		}

		public static void Join_Observer_Channel( EMatchInstanceID match_id, EPersistenceID player_id, EChannelID observer_channel_id )
		{
			Change_System_Channel( player_id, 
										  observer_channel_id, 
										  EChannelGameProperties.OrdinarySingletonMask,
										  delegate( CResponseMessage response ) {
												On_Observer_Chat_Channel_Join_Response( match_id, player_id, response as CJoinChatChannelResponseServerMessage );
										  } );
		}

		// Private interface
		// Callbacks
		// Channel Creation
		private static void On_Lobby_Chat_Channel_Creation_Response( ELobbyID lobby_id, EPersistenceID player_id, CCreateChatChannelResponseServerMessage response_msg )
		{
			CServerLobbyManager.Instance.On_Lobby_Chat_Channel_Creation_Response( lobby_id, player_id, response_msg );
		}

		private static void On_Match_Chat_Channel_Creation_Response( EMatchInstanceID match_id, CCreateChatChannelResponseServerMessage response_msg )
		{
			CServerMatchInstanceManager.Instance.On_Match_Chat_Channel_Creation_Response( match_id, response_msg );
		}

		private static void On_Observer_Chat_Channel_Creation_Response( EMatchInstanceID match_id, CCreateChatChannelResponseServerMessage response_msg )
		{
			CServerMatchInstanceManager.Instance.On_Observer_Chat_Channel_Creation_Response( match_id, response_msg );
		}

		// Player Ops
		private static void On_Chat_Server_Announce_Response( CResponseMessage response, ESessionID session_id, EMessageRequestID client_request_id )
		{
			if ( !CConnectedPlayerManager.Instance.Is_Connection_Valid( session_id ) )
			{
				return;
			}

			CAnnouncePlayerToChatServerResponse message = response as CAnnouncePlayerToChatServerResponse;
			CAnnouncePlayerToChatServerRequest request = message.Request as CAnnouncePlayerToChatServerRequest;
			CConnectedPlayer player = CConnectedPlayerManager.Instance.Get_Active_Player_By_Persistence_ID( request.PersistenceID );
			if ( player != null )
			{
				CConnectedPlayerManager.Instance.On_Chat_Server_Announce_Response( player, message.Success, client_request_id );
			}
		}

		private static void On_Join_General_Channel_Response( CResponseMessage response, ESessionID session_id, EMessageRequestID client_request_id )
		{
			if ( !CConnectedPlayerManager.Instance.Is_Connection_Valid( session_id ) )
			{
				return;
			}

			CJoinChatChannelResponseServerMessage message = response as CJoinChatChannelResponseServerMessage;
			CJoinChatChannelRequestServerMessage request = message.Request as CJoinChatChannelRequestServerMessage;
			EPersistenceID player_id = request.PlayerID;

			CConnectedPlayer player = CConnectedPlayerManager.Instance.Get_Active_Player_By_Persistence_ID( player_id );
			if ( player != null )
			{
				player.On_Join_General_Channel_Response( message.Error, client_request_id );
			}
		}

		private static void On_Lobby_Chat_Channel_Join_Response( EPersistenceID player_id, ELobbyID lobby_id, CJoinChatChannelResponseServerMessage response )
		{
			CServerLobbyManager.Instance.On_Lobby_Chat_Channel_Join_Response( lobby_id, player_id, response );
		}

		private static void On_Match_Chat_Channel_Join_Response( EMatchInstanceID match_id, EPersistenceID player_id, CJoinChatChannelResponseServerMessage response )
		{
			CServerMatchInstanceManager.Instance.On_Match_Chat_Channel_Join_Response( match_id, player_id, response );
		}

		private static void On_Observer_Chat_Channel_Join_Response( EMatchInstanceID match_id, EPersistenceID player_id, CJoinChatChannelResponseServerMessage response )
		{
			CServerMatchInstanceManager.Instance.On_Observer_Chat_Channel_Join_Response( match_id, player_id, response );
		}

		// Helper
		private static void Change_System_Channel( EPersistenceID player_id, EChannelID new_channel_id, EChannelGameProperties remove_mask, DRequestResponseHandler response_handler )
		{
			CJoinChatChannelRequestServerMessage change_request = new CJoinChatChannelRequestServerMessage( player_id, new_channel_id, remove_mask );
			change_request.Handler = response_handler;
		
			CServerMessageRouter.Send_Message_To_Chat_Server( change_request );
		}
	}
}