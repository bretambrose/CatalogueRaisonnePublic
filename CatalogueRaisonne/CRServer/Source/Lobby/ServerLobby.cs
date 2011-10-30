/*

	ServerLobby.cs

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

using CRShared;
using CRShared.Chat;

namespace CRServer
{
	public class CServerLobby : CLobby
	{
		// constructors
		public CServerLobby( ELobbyID id, CLobbyConfig config, EPersistenceID creator ) :
			base( id, config, creator )
		{}

		// Methods
		// Public interface
		public void Initialize_Chat_Channel( EChannelID channel_id )
		{
			ChatChannel = channel_id;
		}

		public bool Can_Be_Joined()
		{
			return AllowObservers || LobbyState.PlayerCount < Get_Max_Player_Count();
		}

		public void Add_Member( EPersistenceID player_id )
		{
			LobbyState.Members.Add( player_id, new CLobbyMember( player_id ) );
			if ( LobbyState.PlayerCount < CGameModeUtils.Player_Count_For_Game_Mode( GameMode ) )
			{
				LobbyState.PlayersBySlot.Add( Find_Open_Player_Slot(), player_id );
				Mark_Players_Not_Ready();

				if ( IsPublic && LobbyState.PlayerCount == CGameModeUtils.Player_Count_For_Game_Mode( GameMode ) )
				{
					CServerLobbyBrowserManager.Instance.Notify_Browsers_Of_Full_Lobby( ID );
				}
			}
			else
			{
				LobbyState.Observers.Add( player_id );
			}

			On_Lobby_Delta_Change();
		}

		public bool Passwords_Match( string password )
		{
			return LobbyState.Password == null || LobbyState.Password.Length == 0 || String.Equals( password, LobbyState.Password );
		}

		public CLobbySummary Create_Summary()
		{
			CLobbySummary summary = new CLobbySummary();
			summary.Initialize( ID, LobbyState.Config, CreationTime, Creator, (uint) LobbyState.PlayerCount, (uint) LobbyState.ObserverCount );

			return summary;
		}

		public CLobbySummaryDelta Create_Summary_Delta()
		{
			CLobbySummaryDelta summary_delta = new CLobbySummaryDelta();
			summary_delta.Initialize( ID, (uint) LobbyState.PlayerCount, (uint) LobbyState.ObserverCount );

			return summary_delta;
		}

		public bool Matches_Browse_Criteria( CBrowseLobbyMatchCriteria browse_criteria )
		{
			if ( ( browse_criteria.GameModeMask & LobbyState.GameMode ) == 0 )
			{
				return false;
			}

			ELobbyMemberType current_mask = 0;
			if ( LobbyState.PlayerCount < LobbyState.MaxPlayers )
			{
				current_mask |= ELobbyMemberType.Player;
			}

			if ( LobbyState.AllowObservers )
			{
				current_mask |= ELobbyMemberType.Observer;
			}

			if ( ( browse_criteria.LobbyMemberTypeMask & current_mask ) == 0 )
			{
				return false;
			}

			return true;
		}

		public EJoinLobbyFailureReason Check_Join( EPersistenceID player_id )
		{
			CPersistentPlayerData creator_player_data = CDatabaseProxy.Instance.Get_Player_Data( Creator );
			if ( creator_player_data.IgnoreList.Contains( player_id ) )
			{
				return EJoinLobbyFailureReason.Creator_Is_Ignoring_You;
			}

			if ( !Can_Be_Joined() )
			{
				return EJoinLobbyFailureReason.Lobby_Full;
			}

			if ( Is_Banned( player_id ) )
			{
				return EJoinLobbyFailureReason.Banned;
			}

			if ( Password != null && Password.Length > 0 )
			{
				return EJoinLobbyFailureReason.Password_Mismatch;
			}

			return EJoinLobbyFailureReason.None;
		}

		public bool State_Allows_Player_Operations()
		{
			return true;
		}

		// Protected interface and overrides
		protected override void On_Lobby_Full_To_Not_Full()
		{
			if ( IsPublic )
			{
				CServerLobbyBrowserManager.Instance.Notify_Browsers_Of_Non_Full_Lobby( ID );
			}
		}

		protected override void On_Lobby_Delta_Change()
		{
			if ( IsPublic )
			{
				CServerLobbyBrowserManager.Instance.Broadcast_Delta_To_Watchers( Create_Summary_Delta() );
			}
		}

		// Private interface
		private uint Find_Open_Player_Slot()
		{
			uint max_slots = CGameModeUtils.Player_Count_For_Game_Mode( LobbyState.GameMode );
			for ( uint i = 0; i < max_slots; i++ )
			{
				if ( !LobbyState.PlayersBySlot.ContainsKey( i ) )
				{
					return i;
				}
			}

			throw new CApplicationException( "Could not find open player slot in lobby" );
		}

		// Properties
		public EChannelID ChatChannel { get; set; }

	}
}
