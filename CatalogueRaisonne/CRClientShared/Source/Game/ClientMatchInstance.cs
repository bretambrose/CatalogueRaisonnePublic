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