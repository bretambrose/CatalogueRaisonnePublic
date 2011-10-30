/*

	ServerQuickmatchManager.cs

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

namespace CRServer
{
	public class CServerQuickmatchManager
	{
		// Construction
		private CServerQuickmatchManager() {}
		static CServerQuickmatchManager() {}

		// Methods
		// Public interface

		// Private interface
		[NetworkMessageHandler]
		private void Handle_Create_Quickmatch_Request( CCreateQuickmatchRequest request, EPersistenceID source_player_id )
		{
			CConnectedPlayer source_player = CConnectedPlayerManager.Instance.Get_Active_Player_By_Persistence_ID( source_player_id );
			if ( source_player == null )
			{
				return;
			}

			if ( !source_player.State_Allows_Operation( EConnectedPlayerOperation.Create_Quickmatch ) )
			{
				CServerMessageRouter.Send_Message_To_Player( new CCreateQuickmatchResponse( request.RequestID, ECreateQuickmatchResult.Invalid_State_To_Create ), source_player_id );
				return;
			}

			if ( Has_Pending_Quickmatch_Invite( source_player_id ) )
			{
				CServerMessageRouter.Send_Message_To_Player( new CCreateQuickmatchResponse( request.RequestID, ECreateQuickmatchResult.Already_Has_Pending_Quickmatch ), source_player_id );
				return;
			}

			EPersistenceID target_player_id = CDatabaseProxy.Instance.Get_Persistence_ID_By_Name_Sync( request.PlayerName );
			if ( target_player_id == EPersistenceID.Invalid )
			{
				CServerMessageRouter.Send_Message_To_Player( new CCreateQuickmatchResponse( request.RequestID, ECreateQuickmatchResult.Unknown_Target_Player ), source_player_id );
				return;
			}

			if ( target_player_id == source_player_id )
			{
				CServerMessageRouter.Send_Message_To_Player( new CCreateQuickmatchResponse( request.RequestID, ECreateQuickmatchResult.Cannot_Invite_Yourself ), source_player_id );
				return;
			}

			if ( Has_Pending_Quickmatch_Invite( target_player_id ) )
			{
				CServerMessageRouter.Send_Message_To_Player( new CCreateQuickmatchResponse( request.RequestID, ECreateQuickmatchResult.Target_Already_Has_Pending_Quickmatch ), source_player_id );
				return;
			}

			CConnectedPlayer target_player = CConnectedPlayerManager.Instance.Get_Active_Player_By_Persistence_ID( target_player_id );
			if ( target_player == null )
			{
				CServerMessageRouter.Send_Message_To_Player( new CCreateQuickmatchResponse( request.RequestID, ECreateQuickmatchResult.Unknown_Target_Player ), source_player_id );
				return;
			}

			if ( !target_player.State_Allows_Operation( EConnectedPlayerOperation.Join_Quickmatch ) )
			{
				CServerMessageRouter.Send_Message_To_Player( new CCreateQuickmatchResponse( request.RequestID, ECreateQuickmatchResult.Target_Invalid_State_To_Create ), source_player_id );
				return;
			}

			if ( CServerSocialManager.Is_Ignoring( target_player_id, source_player_id ) )
			{
				CServerMessageRouter.Send_Message_To_Player( new CCreateQuickmatchResponse( request.RequestID, ECreateQuickmatchResult.Ignored_By_Target_Player ), source_player_id );
				return;
			}

			CServerMessageRouter.Send_Message_To_Player( new CCreateQuickmatchResponse( request.RequestID, ECreateQuickmatchResult.Success ), source_player_id );

			m_QuickmatchInvitesForward[ source_player_id ] = target_player_id;
			m_QuickmatchInvitesBackward[ target_player_id ] = source_player_id;

			CServerMessageRouter.Send_Message_To_Player( new CInviteToQuickmatchRequest( source_player.Name, request.GameCount ), target_player_id );
		}

		[NetworkMessageHandler]
		private void Handle_Invite_To_Quickmatch_Response( CInviteToQuickmatchResponse response, EPersistenceID source_player_id )
		{
			if ( !m_QuickmatchInvitesBackward.ContainsKey( source_player_id ) )
			{
				return;
			}

			CConnectedPlayer source_player = CConnectedPlayerManager.Instance.Get_Active_Player_By_Persistence_ID( source_player_id );

			EPersistenceID inviting_player_id = m_QuickmatchInvitesBackward[ source_player_id ];
			CConnectedPlayer inviting_player = CConnectedPlayerManager.Instance.Get_Active_Player_By_Persistence_ID( inviting_player_id );

			if ( inviting_player == null || source_player == null )
			{
				Cancel_Pending_Quickmatch( source_player_id );
				return;
			}

			if ( response.Accepted == false )
			{
				CServerMessageRouter.Send_Message_To_Player( new CQuickmatchNotificationMessage( EQuickmatchNotification.Declined_Invite, null ), source_player_id );
				CServerMessageRouter.Send_Message_To_Player( new CQuickmatchNotificationMessage( EQuickmatchNotification.Declined_Invite, source_player.Name ), inviting_player_id );
			}
			else
			{
				CServerMessageRouter.Send_Message_To_Player( new CQuickmatchNotificationMessage( EQuickmatchNotification.Accepted_Invite, null ), source_player_id );
				CServerMessageRouter.Send_Message_To_Player( new CQuickmatchNotificationMessage( EQuickmatchNotification.Accepted_Invite, source_player.Name ), inviting_player_id );
			}
			
		}

		[NetworkMessageHandler]
		private void Handle_Cancel_Quickmatch_Request( CCancelQuickmatchRequest request, EPersistenceID source_player_id )
		{
			if ( !m_QuickmatchInvitesForward.ContainsKey( source_player_id ) )
			{
				CServerMessageRouter.Send_Message_To_Player( new CCancelQuickmatchResponse( request.RequestID, ECancelQuickmatchResult.Not_In_Quickmatch ), source_player_id );
				return;
			}

			CServerMessageRouter.Send_Message_To_Player( new CCancelQuickmatchResponse( request.RequestID, ECancelQuickmatchResult.Success ), source_player_id );

			EPersistenceID target_player_id = m_QuickmatchInvitesForward[ source_player_id ];
			CConnectedPlayer source_player = CConnectedPlayerManager.Instance.Get_Active_Player_By_Persistence_ID( source_player_id );

			CServerMessageRouter.Send_Message_To_Player( new CQuickmatchNotificationMessage( EQuickmatchNotification.Canceled_Invite, source_player.Name ), target_player_id );

			Cancel_Pending_Quickmatch( source_player_id );
		}

		private bool Has_Pending_Quickmatch_Invite( EPersistenceID player_id )
		{
			return m_QuickmatchInvitesForward.ContainsKey( player_id ) || m_QuickmatchInvitesBackward.ContainsKey( player_id );
		}

		private void Cancel_Pending_Quickmatch( EPersistenceID player_id )
		{
			EPersistenceID other_player_id = EPersistenceID.Invalid;
			if ( m_QuickmatchInvitesForward.TryGetValue( player_id, out other_player_id ) )
			{
				m_QuickmatchInvitesForward.Remove( player_id );
				m_QuickmatchInvitesBackward.Remove( other_player_id );
				return;
			}

			if ( m_QuickmatchInvitesBackward.TryGetValue( player_id, out other_player_id ) )
			{
				m_QuickmatchInvitesBackward.Remove( player_id );
				m_QuickmatchInvitesForward.Remove( other_player_id );
				return;
			}

			throw new CApplicationException( "Attempt to cancel a quickmatch with no record" );
		}

		// Properties
		public static CServerQuickmatchManager Instance { get { return m_Instance; } }

		// Fields
		private static readonly CServerQuickmatchManager m_Instance = new CServerQuickmatchManager();

		private Dictionary< EPersistenceID, EPersistenceID > m_QuickmatchInvitesForward = new Dictionary< EPersistenceID, EPersistenceID >();
		private Dictionary< EPersistenceID, EPersistenceID > m_QuickmatchInvitesBackward = new Dictionary< EPersistenceID, EPersistenceID >();
	}
}