/*

	ServerMatchInstanceManager.cs

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
using System.Linq;

using CRShared;
using CRServer.Chat;
using CRShared.Chat;

namespace CRServer
{
	public class CServerMatchInstanceManager
	{
		// Construction
		private CServerMatchInstanceManager() {}
		static CServerMatchInstanceManager() {}

		// Public Interface
		public void Transition_To_Game_Instance_From_Lobby( CLobbyState lobby_state )
		{
			EMatchInstanceID id = Create_Game_Instance_From_Lobby( lobby_state );
			Transition_To_Match( id );
		}

		public void Shutdown_Match( EMatchInstanceID match_id, EMatchDestroyedReason reason )
		{
			CServerMatchInstance match_instance = Get_Match_Instance( match_id );
			if ( match_instance == null )
			{
				return;
			}

			match_instance.PlayersAndObservers.Duplicate_As_List().Apply( mid => Remove_From_Match( match_id, mid, Compute_Removal_Reason( reason ) ) );

			if ( match_instance.MatchChannel != EChannelID.Invalid )
			{
				CDestroyChatChannelServerMessage destroy_channel_message = new CDestroyChatChannelServerMessage( match_instance.MatchChannel );
				CServerMessageRouter.Send_Message_To_Chat_Server( destroy_channel_message );
			}

			if ( match_instance.ObserverChannel != EChannelID.Invalid )
			{
				CDestroyChatChannelServerMessage destroy_channel_message = new CDestroyChatChannelServerMessage( match_instance.ObserverChannel );
				CServerMessageRouter.Send_Message_To_Chat_Server( destroy_channel_message );
			}

			match_instance.Shutdown();

			m_MatchInstances.Remove( match_id );
		}

		public void On_Match_Chat_Channel_Creation_Response( EMatchInstanceID match_id, CCreateChatChannelResponseServerMessage response_msg )
		{
			CServerMatchInstance match_instance = Get_Match_Instance( match_id );
			if ( match_instance == null )
			{
				if ( response_msg.ChannelID != EChannelID.Invalid )
				{
					CDestroyChatChannelServerMessage destroy_channel_message = new CDestroyChatChannelServerMessage( response_msg.ChannelID );
					CServerMessageRouter.Send_Message_To_Chat_Server( destroy_channel_message );
				}

				return;
			}

			// Chat creation and joining should never fail
			if ( response_msg.Error != EChannelCreationError.None )
			{
				Shutdown_Match( match_id, EMatchDestroyedReason.Chat_Channel_Creation_Failure );
				return;
			}

			match_instance.MatchChannel = response_msg.ChannelID;
			match_instance.Perform_Match_Channel_Joins();
		}

		public void On_Observer_Chat_Channel_Creation_Response( EMatchInstanceID match_id, CCreateChatChannelResponseServerMessage response_msg )
		{
			CServerMatchInstance match_instance = Get_Match_Instance( match_id );
			if ( match_instance == null )
			{
				if ( response_msg.ChannelID != EChannelID.Invalid )
				{
					CDestroyChatChannelServerMessage destroy_channel_message = new CDestroyChatChannelServerMessage( response_msg.ChannelID );
					CServerMessageRouter.Send_Message_To_Chat_Server( destroy_channel_message );
				}

				return;
			}

			// Chat creation and joining should never fail
			if ( response_msg.Error != EChannelCreationError.None )
			{
				Shutdown_Match( match_id, EMatchDestroyedReason.Chat_Channel_Creation_Failure );
				return;
			}

			match_instance.ObserverChannel = response_msg.ChannelID;
			match_instance.Perform_Observer_Channel_Joins();
		}

		public void On_Match_Chat_Channel_Join_Response( EMatchInstanceID match_id, EPersistenceID player_id, CJoinChatChannelResponseServerMessage response )
		{
			CServerMatchInstance match_instance = Get_Match_Instance( match_id );
			if ( match_instance == null )
			{
				return;
			}

			// Chat creation and joining should never fail
			if ( response.Error != EChannelJoinError.None )
			{
				Shutdown_Match( match_id, EMatchDestroyedReason.Chat_Channel_Join_Failure );
				return;
			}
		}

		public void On_Observer_Chat_Channel_Join_Response( EMatchInstanceID match_id, EPersistenceID player_id, CJoinChatChannelResponseServerMessage response )
		{
			CServerMatchInstance match_instance = Get_Match_Instance( match_id );
			if ( match_instance == null )
			{
				return;
			}

			// Chat creation and joining should never fail
			if ( response.Error != EChannelJoinError.None )
			{
				Shutdown_Match( match_id, EMatchDestroyedReason.Chat_Channel_Join_Failure );
				return;
			}
		}

		public CServerMatchInstance Get_Match_Instance( EMatchInstanceID id )
		{
			CServerMatchInstance match_instance = null;
			m_MatchInstances.TryGetValue( id, out match_instance );

			return match_instance;
		}

		public void On_Player_Disconnect( EPersistenceID player_id, EMatchInstanceID match_id )
		{
			CServerMatchInstance match_instance = Get_Match_Instance( match_id );
			if ( match_instance == null )
			{
				return;
			}

			match_instance.On_Player_Disconnect( player_id );
		}

		public void On_Player_Reconnect( EPersistenceID player_id, EMatchInstanceID match_id )
		{
			CServerMatchInstance match_instance = Get_Match_Instance( match_id );
			if ( match_instance == null )
			{
				return;
			}

			match_instance.On_Player_Reconnect( player_id );
		}

		public void Remove_From_Match( EMatchInstanceID match_id, EPersistenceID player_id, EMatchRemovalReason reason )
		{
			CServerMatchInstance match_instance = Get_Match_Instance( match_id );
			if ( match_instance == null )
			{
				return;
			}

			if ( CConnectedPlayerManager.Instance.Is_Connected( player_id ) )
			{
				CAsyncBackendOperations.Join_General_Channel( player_id, EMessageRequestID.Invalid );
			}
			
			CConnectedPlayer player = CConnectedPlayerManager.Instance.Get_Player_By_Persistence_ID( player_id );
			player.On_Leave_Match();

			CMatchPlayerLeftMessage leave_message = new CMatchPlayerLeftMessage( player_id, reason );
			bool is_match_shutdown = Is_Shutdown_Reason( reason );
			if ( is_match_shutdown )
			{
				CServerMessageRouter.Send_Message_To_Player( leave_message, player_id );
			}
			else
			{
				match_instance.Send_Message_To_Members( leave_message, EPersistenceID.Invalid );
			}

			match_instance.Remove_Member( player_id );

			if ( match_instance.MemberCount == 0 )
			{
				Shutdown_Match( match_id, EMatchDestroyedReason.Match_Empty );
			}
		}

		// Private Interface
		// Message handlers
		[NetworkMessageHandler]
		private void Handle_Leave_Match_Request( CLeaveMatchRequest request, EPersistenceID source_player )
		{
			CConnectedPlayer player = CConnectedPlayerManager.Instance.Get_Player_By_Persistence_ID( source_player );
			if ( player == null )
			{
				return;
			}

			CServerMatchInstance match_instance = Get_Match_Instance( player.MatchID );
			if ( match_instance == null )
			{
				CServerMessageRouter.Send_Message_To_Player( new CLeaveMatchResponse( request.RequestID, ELeaveMatchFailureError.Not_In_Match ), source_player );
				return;
			}

			CServerMessageRouter.Send_Message_To_Player( new CLeaveMatchResponse( request.RequestID, ELeaveMatchFailureError.None ), source_player );

			Remove_From_Match( player.MatchID, source_player, EMatchRemovalReason.Player_Request );
		}

		[NetworkMessageHandler]
		private void Handle_Match_Take_Turn_Message( CMatchTakeTurnRequest request, EPersistenceID source_player )
		{
			CConnectedPlayer player = CConnectedPlayerManager.Instance.Get_Player_By_Persistence_ID( source_player );
			if ( player == null )
			{
				return;
			}

			CServerMatchInstance match_instance = Get_Match_Instance( player.MatchID );
			if ( match_instance == null )
			{
				CServerMessageRouter.Send_Message_To_Player( new CMatchTakeTurnResponse( request.RequestID, EGameActionFailure.Not_In_Match ), source_player );
				return;
			}

			match_instance.Try_Take_Turn( request, source_player );
		}

		[NetworkMessageHandler]
		private void Handle_Continue_Match_Request( CContinueMatchRequest request, EPersistenceID source_player )
		{
			CConnectedPlayer player = CConnectedPlayerManager.Instance.Get_Player_By_Persistence_ID( source_player );
			if ( player == null )
			{
				return;
			}

			CServerMatchInstance match_instance = Get_Match_Instance( player.MatchID );
			if ( match_instance == null )
			{
				CServerMessageRouter.Send_Message_To_Player( new CContinueMatchResponse( request.RequestID, EContinueMatchFailure.Not_In_Match ), source_player );
				return;
			}

			match_instance.Handle_Continue_Match_Request( request, source_player );
		}

		private EMatchInstanceID Create_Game_Instance_From_Lobby( CLobbyState lobby_state )
		{
			EMatchInstanceID id = Allocate_Match_Instance_ID();

			CServerMatchInstance match_instance = new CServerMatchInstance( id, lobby_state );

			m_MatchInstances.Add( id, match_instance );

			return id;
		}

		private EMatchInstanceID Create_Game_Instance_From_Quickmatch( EPersistenceID player1, EPersistenceID player2, uint game_count )
		{
			EMatchInstanceID id = Allocate_Match_Instance_ID();

			CServerMatchInstance match_instance = new CServerMatchInstance( id, player1, player2, game_count );

			m_MatchInstances.Add( id, match_instance );

			return id;
		}

		private void Start_Match( EMatchInstanceID match_id )
		{
			CServerMatchInstance match_instance = Get_Match_Instance( match_id );
			if ( match_instance == null )
			{
				throw new CApplicationException( "Starting match but it doesn't exist" );
			}		
			
			match_instance.Start();
		}

		private EMatchInstanceID Allocate_Match_Instance_ID()
		{
			return m_NextID++;
		}

		private void Transition_To_Match( EMatchInstanceID match_id )
		{
			Create_Match_Channels( match_id );

			Get_Match_Instance( match_id ).PlayersAndObservers.Apply( pid => CConnectedPlayerManager.Instance.Get_Player_By_Persistence_ID( pid ).On_Start_Match( match_id ) );

			Start_Match( match_id );
		}

		private void Create_Match_Channels( EMatchInstanceID match_id )
		{
			CChatChannelConfig match_channel_config = new CChatChannelConfig( CChatChannelConfig.Make_Admin_Channel( "Match_" + ( ( int ) match_id ).ToString() ), 
																									CSharedResource.Get_Text< EServerTextID >( EServerTextID.Server_Match_Channel_Name ), 
																									EPersistenceID.Invalid,
																									EChannelGameProperties.MatchPlayer );
			match_channel_config.AllowsModeration = false;
			match_channel_config.AnnounceJoinLeave = false;
			match_channel_config.DestroyWhenEmpty = false;
			match_channel_config.IsMembershipClientLocked = true;

			CAsyncBackendOperations.Create_Match_Chat_Channel( match_channel_config, match_id );

			CChatChannelConfig observer_channel_config = new CChatChannelConfig( CChatChannelConfig.Make_Admin_Channel( "Observer_" + ( ( int ) match_id ).ToString() ), 
																										CSharedResource.Get_Text< EServerTextID >( EServerTextID.Server_Observer_Channel_Name ), 
																										EPersistenceID.Invalid,
																										EChannelGameProperties.MatchObserver );
			observer_channel_config.AllowsModeration = false;
			observer_channel_config.AnnounceJoinLeave = false;
			observer_channel_config.DestroyWhenEmpty = false;
			observer_channel_config.IsMembershipClientLocked = true;

			CAsyncBackendOperations.Create_Observer_Chat_Channel( observer_channel_config, match_id );
		}

		private bool Is_Shutdown_Reason( EMatchRemovalReason reason )
		{
			switch ( reason )
			{
				case EMatchRemovalReason.Match_Shutdown:
					return true;

				case EMatchRemovalReason.Player_Request:
				case EMatchRemovalReason.Player_Disconnect_Timeout:
				default:
					return false;

			}
		}

		private EMatchRemovalReason Compute_Removal_Reason( EMatchDestroyedReason destroy_reason )
		{
			switch ( destroy_reason )
			{
				case EMatchDestroyedReason.Player_Left_During_Creation:
				case EMatchDestroyedReason.Post_Halt_Timeout:
				case EMatchDestroyedReason.Match_Empty:
					return EMatchRemovalReason.Match_Shutdown;

				default:
					return EMatchRemovalReason.Invalid;
			}
		}

		// Properties
		public static CServerMatchInstanceManager Instance { get { return m_Instance; } }

		// Fields
		private static CServerMatchInstanceManager m_Instance = new CServerMatchInstanceManager();
		
		private EMatchInstanceID m_NextID = EMatchInstanceID.Invalid + 1;
		private Dictionary< EMatchInstanceID, CServerMatchInstance > m_MatchInstances = new Dictionary< EMatchInstanceID, CServerMatchInstance >();
	}
}