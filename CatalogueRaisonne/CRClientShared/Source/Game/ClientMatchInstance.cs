/*

	ClientMatchInstance.cs

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

namespace CRClientShared
{
	public class CClientMatchInstance : CMatchInstance
	{
		// Construction
		public CClientMatchInstance( CMatchState match_state, CGameState game_state ) :
			base( match_state, game_state )
		{
		}

		// Public interface
		public void Add_Player_Listeners()
		{
			MatchState.Players.Apply( pid => CClientPlayerInfoManager.Instance.Begin_Player_Listen( pid, EPlayerListenReason.In_Match ) );
		}

		public void Remove_Player_Listeners()
		{
			MatchState.Players.Apply( pid => CClientPlayerInfoManager.Instance.End_Player_Listen( pid, EPlayerListenReason.In_Match ) );
		}

		public void Remove_Member( EPersistenceID player_id )
		{
			MatchState.Remove_Observer( player_id );
			if ( MatchState.Remove_Player( player_id ) )
			{
				CClientPlayerInfoManager.Instance.End_Player_Listen( player_id, EPlayerListenReason.In_Match );
			}
		}

		public void On_Player_Connection_State_Change( EPersistenceID player_id, bool is_connected )
		{
			if ( is_connected )
			{
				MatchState.On_Player_Reconnect( player_id );
			}
			else
			{
				MatchState.On_Player_Disconnect( player_id );
			}
		}
	}
}