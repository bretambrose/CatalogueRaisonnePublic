/*

	MatchState.cs

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
using System.Linq;

namespace CRShared
{
	public enum EMatchContinueState
	{
		None,

		Accepted,
		Declined
	}

	public enum EContinueMatchFailure
	{
		None,

		Not_In_Match,
		Not_A_Player,
		Match_Not_Halted,
		Cannot_Change_Previous_Commitment,
		Match_Is_Over
	}

	public enum EMatchInstanceState
	{
		Idle,

		Halted_End_Of_Game,
		Halted_End_Of_Match,
		Halted_Player_Left
	}

	public enum EMatchDeltaType
	{
		Continue_State,
		State,
		Current_Game_Number,
		Side_Stats
	}

	public abstract class CAbstractMatchDelta
	{
		public abstract void Apply( CMatchState match_state );
		public abstract EMatchDeltaType Type { get; }
	}

	public class CMatchState
	{
		// Nested Types
		public class CMatchContinueStateDelta : CAbstractMatchDelta
		{
			public CMatchContinueStateDelta( EPersistenceID player_id, EMatchContinueState state )
			{
				PlayerID = player_id;
				State = state;
			}

			// Public interface
			public override void Apply( CMatchState match_state )
			{
				match_state.Set_Continue_State_For_Player( PlayerID, State );
			}

			// Properties
			public override EMatchDeltaType Type { get { return EMatchDeltaType.Continue_State; } }

			public EPersistenceID PlayerID { get; private set; }
			public EMatchContinueState State { get; private set; }
		}

		public class CMatchStateDelta : CAbstractMatchDelta
		{
			public CMatchStateDelta( EMatchInstanceState state )
			{
				State = state;
			}

			// Public interface
			public override void Apply( CMatchState match_state )
			{
				match_state.Halt_Match( State );
			}

			// Properties
			public override EMatchDeltaType Type { get { return EMatchDeltaType.State; } }

			public EMatchInstanceState State { get; private set; }
		}

		public class CMatchCurrentGameNumberDelta : CAbstractMatchDelta
		{
			public CMatchCurrentGameNumberDelta( uint current_game_number )
			{
				CurrentGameNumber = current_game_number;
			}

			// Public interface
			public override void Apply( CMatchState match_state )
			{
				match_state.CurrentGameNumber = CurrentGameNumber;
			}

			// Properties
			public override EMatchDeltaType Type { get { return EMatchDeltaType.Current_Game_Number; } }

			public uint CurrentGameNumber { get; private set; }
		}

		// Construction
		public CMatchState( EMatchInstanceID match_id, EGameModeType mode, uint game_count )
		{
			ID = match_id;
			Mode = mode;
			GameCount = game_count;
			CurrentGameNumber = 1;
		}

		// Public Interface
		public CMatchState Clone_By_Observer( EPersistenceID observer_id )
		{
			CMatchState clone = new CMatchState( ID, Mode, GameCount );
			clone.State = State;
			clone.CurrentGameNumber = CurrentGameNumber;

			// Unconditional cloning
			SideResults.SimpleClone( clone.m_SideResults );
			m_Players.ShallowCopy( clone.m_Players );
			SideBindings.ShallowCopy( clone.m_SideBindings );
			m_ContinueStates.ShallowCopy( clone.m_ContinueStates );

			// admin-only info
			if ( Is_Admin_Observer( observer_id ) )
			{
				m_AdminObservers.ShallowCopy( clone.m_AdminObservers );
				m_Observers.ShallowCopy( clone.m_Observers );
			}

			return clone;
		}

		public override string ToString()
		{
			string side_string = CSharedUtils.Build_String_List( SideBindings, n => String.Format( "{0}->{1}", (long)n.Key, n.Value.ToString() ) );
			string observer_list_string = CSharedUtils.Build_String_List( Observers, n => ((long) n).ToString() );
			string admin_list_string = CSharedUtils.Build_String_List( AdminObservers, n => ((long) n).ToString() );
			string side_data_string = CSharedUtils.Build_String_List( SideResults, n => String.Format( "{0}->{1}", n.Key.ToString(), n.Value.ToString() ) );

			return String.Format( "( [CMatchState] ID = {0}, Sides=( {1} ), Observers=( {2} ), AdminObservers=( {3} ), SideData=( {4} ) )",
										 (int)ID,
										 side_string,
										 observer_list_string,
										 admin_list_string,
										 side_data_string );
		}

		public void Initialize_Match( IEnumerable< EPersistenceID > players )
		{
			m_SideResults.Add( EGameSide.Side1, new CSideMatchStats() );
			m_SideResults.Add( EGameSide.Side2, new CSideMatchStats() );

			List< EPersistenceID > player_list = new List< EPersistenceID >( players );
			int player_count = player_list.Count;
			for ( int i = 0; i < player_count; i++ )
			{
				m_SideBindings.Add( player_list[ i ], ( i / ( player_count / 2 ) == 0 ) ? EGameSide.Side1 : EGameSide.Side2 );
				m_Players.Add( player_list[ i ], true );
			}

			Reset_Continue_States();

			State = EMatchInstanceState.Idle;
		}

		public void Add_Observers( CLobbyState lobby_state )
		{
			lobby_state.Observers.Apply( n => m_Observers.Add( n ) );
		}

		public void Remove_Observer( EPersistenceID player_id )
		{
			m_Observers.Remove( player_id );
		}

		public bool Remove_Player( EPersistenceID player_id )
		{
			return m_Players.Remove( player_id );
		}

		public bool Is_Admin_Observer( EPersistenceID id )
		{
			return m_AdminObservers.Contains( id );
		}

		public bool Is_Observer( EPersistenceID id )
		{
			return m_AdminObservers.Contains( id ) || m_Observers.Contains( id );
		}

		public bool On_Player_Disconnect( EPersistenceID player_id )
		{
			if ( !m_Players.ContainsKey( player_id ) )
			{
				return false;
			}

			bool current_state = m_Players[ player_id ];
			if ( current_state == false )
			{
				return false;
			}

			m_Players[ player_id ] = false;
			return true;
		}

		public bool On_Player_Reconnect( EPersistenceID player_id )
		{
			if ( !m_Players.ContainsKey( player_id ) )
			{
				return false;
			}

			bool current_state = m_Players[ player_id ];
			if ( current_state == true )
			{
				return false;
			}

			m_Players[ player_id ] = true;
			return true;
		}

		public bool Is_Player_Connected( EPersistenceID player_id )
		{
			return m_Players[ player_id ];
		}

		public CSideMatchStats Get_Side_Data( EGameSide side )
		{
			return m_SideResults[ side ];
		}

		public EGameSide Get_Side_For_Player( EPersistenceID player_id )
		{
			return m_SideBindings[ player_id ];
		}

		public EPersistenceID Get_Partner_For( EPersistenceID player_id )
		{
			if ( m_Players.Count != 4 )
			{
				throw new CApplicationException( "Only a four player game has partners" );
			}

			EGameSide side = Get_Side_For_Player( player_id );
			return SideBindings.First( pair => pair.Value == side && pair.Key != player_id ).Key;
		}

		public bool Is_Player( EPersistenceID player_id )
		{
			return m_Players.ContainsKey( player_id );
		}

		public void Reset_Continue_States()
		{
			Players.Apply( n => m_ContinueStates[ n ] = EMatchContinueState.None );
		}

		public EMatchContinueState Get_Continue_State_For_Player( EPersistenceID player_id )
		{
			return m_ContinueStates[ player_id ];
		}

		public void Set_Continue_State_For_Player( EPersistenceID player_id, EMatchContinueState state )
		{
			m_ContinueStates[ player_id ] = state;
		}

		public EMatchContinueState Get_Collective_Ready_State()
		{
			EMatchContinueState worst_state = EMatchContinueState.Accepted;

			foreach ( var state_pair in m_ContinueStates )
			{
				EMatchContinueState state = state_pair.Value;
				if ( state == EMatchContinueState.Declined && worst_state == EMatchContinueState.Accepted )
				{
					worst_state = EMatchContinueState.Declined;
				}
				else if ( state == EMatchContinueState.None && worst_state != EMatchContinueState.None )
				{
					worst_state = EMatchContinueState.None;
				}
			}

			return worst_state;
		}

		public virtual void Halt_Match( EMatchInstanceState new_state )
		{	
			State = new_state;
		}

		// Properties
		public EMatchInstanceID ID { get; private set; }
		public EGameModeType Mode { get; private set; }
		public EMatchInstanceState State { get; private set; }
		public uint GameCount { get; private set; }
		public uint CurrentGameNumber { get; private set; }
		public uint MemberCount { get { return (uint)( m_Players.Count + m_Observers.Count + m_AdminObservers.Count ); } }

		public IEnumerable< EPersistenceID > Observers { get { return m_Observers; } }
		public IEnumerable< EPersistenceID > AdminObservers { get { return m_AdminObservers; } }
		public IEnumerable< EPersistenceID > Players { get { return m_Players.Select( n => n.Key ); } }
		public IEnumerable< KeyValuePair< EGameSide, CSideMatchStats > > SideResults { get { return m_SideResults; } }
		public IEnumerable< KeyValuePair< EPersistenceID, EGameSide > > SideBindings { get { return m_SideBindings; } }

		// Fields
		private Dictionary< EPersistenceID, bool > m_Players = new Dictionary< EPersistenceID, bool >();
		private HashSet< EPersistenceID > m_Observers = new HashSet< EPersistenceID >();
		private HashSet< EPersistenceID > m_AdminObservers = new HashSet< EPersistenceID >();

		private Dictionary< EGameSide, CSideMatchStats > m_SideResults = new Dictionary< EGameSide, CSideMatchStats >();
		private Dictionary< EPersistenceID, EGameSide > m_SideBindings = new Dictionary< EPersistenceID, EGameSide >();

		private Dictionary< EPersistenceID, EMatchContinueState > m_ContinueStates = new Dictionary< EPersistenceID, EMatchContinueState >();
	}
}