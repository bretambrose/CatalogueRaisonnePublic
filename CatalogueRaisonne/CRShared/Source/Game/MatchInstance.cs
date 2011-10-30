/*

	MatchInstance.cs

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

namespace CRShared
{
	public enum EMatchInstanceID
	{
		Invalid = -1
	}

	public enum EMatchDestroyedReason
	{
		Invalid,

		Player_Left_During_Creation,
		Chat_Channel_Creation_Failure,
		Chat_Channel_Join_Failure,
		Post_Halt_Timeout,
		Match_Empty
	}

	public enum EMatchRemovalReason
	{
		Invalid,

		Player_Request,
		Player_Disconnect_Timeout,
		Match_Shutdown
	}

	public class CMatchInstance
	{
		// Construction
		public CMatchInstance( EMatchInstanceID id, EPersistenceID player1, EPersistenceID player2, uint game_count )
		{
			GameState = new CGameState( EGameModeType.Two_Players );

			List< EPersistenceID > players = new List< EPersistenceID >();
			players.Add( player1 );
			players.Add( player2 );

			GameState.Initialize_Game( players );

			MatchState = new CMatchState( id, EGameModeType.Two_Players, game_count );
			MatchState.Initialize_Match( players );
		}

		public CMatchInstance( EMatchInstanceID id, CLobbyState lobby_state )
		{
			GameState = new CGameState( lobby_state.GameMode );
			GameState.Initialize_Game( lobby_state.Players );

			MatchState = new CMatchState( id, lobby_state.GameMode, lobby_state.GameCount );
			MatchState.Initialize_Match( lobby_state.Players );
			MatchState.Add_Observers( lobby_state );
		}

		public CMatchInstance( CMatchState match_state, CGameState game_state )
		{
			GameState = game_state;
			MatchState = match_state;
		}

		// Public interface
		public CGameState Clone_Game_State( EPersistenceID observer_id )
		{
			return GameState.Clone_By_Observer( observer_id, MatchState.Is_Admin_Observer( observer_id ) );
		}

		public CMatchState Clone_Match_State( EPersistenceID observer_id )
		{
			return MatchState.Clone_By_Observer( observer_id );
		}

		public virtual void Halt_Match( EMatchInstanceState new_state )
		{	
			MatchState.Halt_Match( new_state );
		}

		public EGameActionFailure Validate_Turn( CMatchTakeTurnRequest pending_turn, EPersistenceID player_id )
		{
			if ( MatchState.State != EMatchInstanceState.Idle )
			{
				return EGameActionFailure.Match_Halted;
			}

			if ( GameState.CurrentPlayer != player_id )
			{
				return EGameActionFailure.Not_Your_Turn;
			}

			if ( pending_turn.Actions.Count > 2 )
			{
				return EGameActionFailure.Invalid_Turn;
			}

			if ( pending_turn.Actions.Count == 0 )
			{
				return EGameActionFailure.Turn_Is_Incomplete;
			}

			for ( int i = 0; i < pending_turn.Actions.Count; i++ )
			{
				CGameActionBase action = pending_turn.Actions[ i ];
				switch ( action.Action )
				{
					case EGameAction.Play_Card:
						if ( i != 0 )
						{
							return EGameActionFailure.Play_Card_Must_Be_First_Action;
						}

						if ( pending_turn.Actions.Count != 2 )
						{
							return EGameActionFailure.Turn_Is_Incomplete;
						}

						CPlayCardGameAction play_action = action as CPlayCardGameAction;
						ECardColor color = play_action.Card.Color;
						ECardValue value = play_action.Card.Value;
						if ( !GameState.Get_Player_Hand( player_id ).Contains_Card( color, value ) )
						{
							return EGameActionFailure.Card_Is_Not_In_Your_Hand;
						}

						CCardCollection local_collection = GameState.Get_Card_Collection( Get_Side_For_Player( player_id ), color );
						int local_collection_top_value = local_collection.Get_Top_Card_Score_Value();
						if ( CCardUtils.Get_Card_Score_Value( value ) <= local_collection_top_value && local_collection_top_value != 0 )
						{
							return EGameActionFailure.A_Higher_Card_Already_Exists;
						}

						break;

					case EGameAction.Discard_Card:
						if ( i != 0 )
						{
							return EGameActionFailure.Discard_Card_Must_Be_First_Action;
						}

						if ( pending_turn.Actions.Count != 2 )
						{
							return EGameActionFailure.Turn_Is_Incomplete;
						}

						CDiscardCardGameAction discard_action = action as CDiscardCardGameAction;
						if ( !GameState.Get_Player_Hand( player_id ).Contains_Card( discard_action.Card.Color, discard_action.Card.Value ) )
						{
							return EGameActionFailure.Card_Is_Not_In_Your_Hand;
						}
						break;

					case EGameAction.Draw_From_Deck:
						if ( i != 1 )
						{
							return EGameActionFailure.You_Must_Play_A_Card_Before_Drawing;
						}

						if ( GameState.Get_Deck().Count == 0 )
						{
							return EGameActionFailure.Deck_Is_Empty;
						}
						break;

					case EGameAction.Draw_From_Discard:
						if ( i != 1 )
						{
							return EGameActionFailure.You_Must_Play_A_Card_Before_Drawing;
						}

						CDrawFromDiscardGameAction draw_action = action as CDrawFromDiscardGameAction;
						if ( GameState.Get_Discard_Pile( draw_action.Color ).Count == 0 )
						{
							// handle pathological case of "spinning in place" where you discard a card and then redraw it; if the discard pile is empty when you attempt this, then
							// the simple validation approach of checking current state is insufficient
							CDiscardCardGameAction discard_to_pile_action = pending_turn.Actions[ 0 ] as CDiscardCardGameAction;
							if ( discard_to_pile_action == null || discard_to_pile_action.Card.Color != draw_action.Color )
							{
								return EGameActionFailure.Discard_Pile_Is_Empty;
							}
						}
						break;

					case EGameAction.Pass_Cards:
						if ( GameState.Mode != EGameModeType.Four_Players )
						{
							return EGameActionFailure.Can_Only_Pass_Cards_In_A_Four_Player_Game;
						}

						if ( i != 0 )
						{
							return EGameActionFailure.Passing_Cards_Must_Be_Only_Action;
						}

						if ( pending_turn.Actions.Count != 1 )
						{
							return EGameActionFailure.Passing_Cards_Must_Be_Only_Action;
						}

						CPassCardsGameAction pass_action = action as CPassCardsGameAction;
						CPlayerHand player_hand = GameState.Get_Player_Hand( player_id );
						if ( !player_hand.Contains_Card( pass_action.Card1.Color, pass_action.Card1.Value ) || !player_hand.Contains_Card( pass_action.Card2.Color, pass_action.Card2.Value ) )
						{
							return EGameActionFailure.Card_Is_Not_In_Your_Hand;
						}

						if ( pass_action.Card1.Color == pass_action.Card2.Color && pass_action.Card1.Value == pass_action.Card2.Value )
						{
							return EGameActionFailure.Two_Distinct_Cards_Must_Be_Passed;
						}

						if ( player_hand.Count < 8 )
						{
							return EGameActionFailure.Not_Enough_Cards_In_Hand_To_Pass;
						}
						break;
				}
			}

			return EGameActionFailure.None;
		}

		public void Apply_Turn_Deltas( List< IObservedClonableDelta > deltas )
		{
			deltas.Apply( n => n.Apply( GameState ) );
		}

		public void Initialize_New_Game( CGameState game_state )
		{
			GameState = game_state;
		}

		// Protected Interface
		protected virtual void Start_New_Game()
		{
			GameState = new CGameState( GameState.Mode );
			GameState.Initialize_Game( MatchState.Players );

			MatchState.Halt_Match( EMatchInstanceState.Idle );
		}

		// Private interface
		private EGameSide Get_Side_For_Player( EPersistenceID player_id )
		{
			return MatchState.Get_Side_For_Player( player_id );
		}

		// Properties
		public EMatchInstanceID ID { get { return MatchState.ID; } }
		public uint MemberCount { get { return MatchState.MemberCount; } }

		public CGameState GameState { get; private set; }
		public CMatchState MatchState { get; private set; }
	}
}