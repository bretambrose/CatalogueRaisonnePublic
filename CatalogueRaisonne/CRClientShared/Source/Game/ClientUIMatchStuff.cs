/*

	ClientUIMatchStuff.cs

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

// A temp file archiving rewritten functionality that will eventually be needed by the UI thread in its separate game state managenment logic
using System;

using CRShared;

namespace CRClientShared
{
	class CMatchUIArchive
	{
		CMatchUIArchive() {}

		private EGameActionFailure Pending_Turn_Allows_Action( EGameAction action )
		{
			switch ( action )
			{
				case EGameAction.Play_Card:
				case EGameAction.Discard_Card:
					if ( m_PendingTurn.Actions.Count == 1 )
					{
						if ( m_PendingTurn.Actions[ 0 ].Action == EGameAction.Pass_Cards )
						{						
							return EGameActionFailure.Cannot_Play_A_Card_After_Passing_Cards;
						}
						else
						{
							return EGameActionFailure.Cannot_Play_More_Than_One_Card;
						}
					}
					else if ( m_PendingTurn.Actions.Count > 1 )
					{
						return EGameActionFailure.Turn_Is_Already_Complete;
					}
					break;

				case EGameAction.Draw_From_Discard:
				case EGameAction.Draw_From_Deck:
					if ( m_PendingTurn.Actions.Count == 0 )
					{
						return EGameActionFailure.You_Must_Play_A_Card_Before_Drawing;
					}
					else if ( m_PendingTurn.Actions.Count > 1 )
					{
						return EGameActionFailure.Turn_Is_Already_Complete;
					}
					else if ( m_PendingTurn.Actions[ 0 ].Action == EGameAction.Pass_Cards )
					{
						return EGameActionFailure.Cannot_Play_A_Card_After_Passing_Cards;
					}
					break;


				case EGameAction.Pass_Cards:
					if ( Match.GameState.Mode != EGameModeType.Four_Players )
					{
						return EGameActionFailure.Can_Only_Pass_Cards_In_A_Four_Player_Game;
					}

					if ( m_PendingTurn.Actions.Count == 1 )
					{
						if ( m_PendingTurn.Actions[ 0 ].Action == EGameAction.Pass_Cards )
						{
							return EGameActionFailure.Can_Only_Pass_One_Set_Of_Cards;
						}
						else
						{
							return EGameActionFailure.Turn_Is_Already_Complete;
						}
					}
					else if ( m_PendingTurn.Actions.Count > 1 )
					{
						return EGameActionFailure.Turn_Is_Already_Complete;
					}
					break;
			}

			return EGameActionFailure.None;
		}

		private EGameActionFailure Is_Card_Playable( ECardColor color, ECardValue value )
		{
			CPlayerHand local_hand = Match.GameState.Get_Player_Hand( CClientLogicalThread.Instance.ConnectedID );
			if ( !local_hand.Contains_Card( color, value ) )
			{
				return EGameActionFailure.Card_Is_Not_In_Your_Hand;
			}

			CCardCollection local_collection = Match.GameState.Get_Card_Collection( Get_Local_Side(), color );
			int local_collection_top_value = local_collection.Get_Top_Card_Score_Value();
			if ( CCardUtils.Get_Card_Score_Value( value ) <= local_collection_top_value && local_collection_top_value != 0 )
			{
				return EGameActionFailure.A_Higher_Card_Already_Exists;
			}

			return EGameActionFailure.None;
		}

		private EGameActionFailure Is_Card_Discardable( ECardColor color, ECardValue value )
		{
			CPlayerHand local_hand = Match.GameState.Get_Player_Hand( CClientLogicalThread.Instance.ConnectedID );
			if ( !local_hand.Contains_Card( color, value ) )
			{
				return EGameActionFailure.Card_Is_Not_In_Your_Hand;
			}

			return EGameActionFailure.None;
		}

		private EGameActionFailure Is_Deck_Drawable()
		{
			CDeck deck = Match.GameState.Get_Deck();
			if ( deck.Count == 0 )
			{
				return EGameActionFailure.Deck_Is_Empty;
			}

			return EGameActionFailure.None;
		}

		private EGameActionFailure Is_Discard_Pile_Drawable( ECardColor color )
		{
			CDiscardPile pile = Match.GameState.Get_Discard_Pile( color );
			if ( pile.Count == 0 )
			{
				return EGameActionFailure.Discard_Pile_Is_Empty;
			}

			return EGameActionFailure.None;
		}

		private EGameActionFailure Are_Cards_Passable( ECardColor color1, ECardValue value1, ECardColor color2, ECardValue value2 )
		{
			CPlayerHand local_hand = Match.GameState.Get_Player_Hand( CClientLogicalThread.Instance.ConnectedID );
			if ( !local_hand.Contains_Card( color1, value1 ) || !local_hand.Contains_Card( color2, value2 ) )
			{
				return EGameActionFailure.Card_Is_Not_In_Your_Hand;
			}

			if ( local_hand.Count < 8 )
			{
				return EGameActionFailure.Not_Enough_Cards_In_Hand_To_Pass;
			}

			return EGameActionFailure.None;
		}

		private EGameSide Get_Local_Side()
		{
			return Match.MatchState.Get_Side_For_Player( CClientLogicalThread.Instance.ConnectedID );
		}

		public CClientMatchInstance Match { get; private set; }
		CMatchTakeTurnRequest m_PendingTurn = null;
	}
}