/*

	GameAction.cs

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

namespace CRShared
{
	public enum EGameAction
	{
		Invalid,

		Play_Card,
		Discard_Card,
		Draw_From_Deck,
		Draw_From_Discard,
		Pass_Cards
	}

	public enum EGameActionFailure
	{
		None,

		Cannot_Play_More_Than_One_Card,
		Cannot_Play_A_Card_After_Passing_Cards,
		Turn_Is_Already_Complete,
		You_Must_Play_A_Card_Before_Drawing,
		Can_Only_Pass_One_Set_Of_Cards,
		Can_Only_Pass_Cards_In_A_Four_Player_Game,
		Card_Is_Not_In_Your_Hand,
		A_Higher_Card_Already_Exists,
		Deck_Is_Empty,
		Discard_Pile_Is_Empty,
		Not_Enough_Cards_In_Hand_To_Pass,
		Not_In_Match,
		Match_Halted,
		Not_Your_Turn,
		Turn_Is_Incomplete,
		Invalid_Turn,
		Play_Card_Must_Be_First_Action,
		Discard_Card_Must_Be_First_Action,
		Passing_Cards_Must_Be_Only_Action,
		Two_Distinct_Cards_Must_Be_Passed,
		Not_A_Player,
	}

	public class CGameActionBase
	{
		// Construction
		public CGameActionBase() {}

		// Public Interface
		public virtual EGameAction Action { get { return EGameAction.Invalid; } }
	}

	public class CPlayCardGameAction : CGameActionBase
	{
		// Construction
		public CPlayCardGameAction( CCard card )
		{
			Card = card.Clone();
		}

		// Public Interface
		public override string ToString()
		{
			return String.Format( "( [CPlayCardGameAction]: {0} )", Card.ToString() );
		}

		public override EGameAction Action { get { return EGameAction.Play_Card; } }

		// Properties
		public CCard Card { get; private set; } 
	}

	public class CDiscardCardGameAction : CGameActionBase
	{
		// Construction
		public CDiscardCardGameAction( CCard card )
		{
			Card = card.Clone();
		}

		// Public Interface
		public override string ToString()
		{
			return String.Format( "( [CDiscardCardGameAction]: {0} )", Card.ToString() );
		}

		public override EGameAction Action { get { return EGameAction.Discard_Card; } }

		// Properties
		public CCard Card { get; private set; } 
	}

	public class CDrawFromDeckGameAction : CGameActionBase
	{
		// Construction
		public CDrawFromDeckGameAction()
		{
		}

		// Public Interface
		public override string ToString()
		{
			return String.Format( "( [CDrawFromDeckGameAction] )" );
		}

		public override EGameAction Action { get { return EGameAction.Draw_From_Deck; } }
	}

	public class CDrawFromDiscardGameAction : CGameActionBase
	{
		// Construction
		public CDrawFromDiscardGameAction( ECardColor color )
		{
			Color = color;
		}

		// Public Interface
		public override string ToString()
		{
			return String.Format( "( [CDrawFromDiscardGameAction]: {0} )", Color.ToString() );
		}

		public override EGameAction Action { get { return EGameAction.Draw_From_Discard; } }

		// Properties
		public ECardColor Color { get; private set; } 
	}

	public class CPassCardsGameAction : CGameActionBase
	{
		// Construction
		public CPassCardsGameAction( CCard card1, CCard card2 )
		{
			Card1 = card1.Clone();
			Card2 = card2.Clone();
		}

		// Public Interface
		public override string ToString()
		{
			return String.Format( "( [CPassCardsGameAction]: {0}, {1} )", Card1.ToString(), Card2.ToString() );
		}

		public override EGameAction Action { get { return EGameAction.Pass_Cards; } }

		// Properties
		public CCard Card1 { get; private set; } 
		public CCard Card2 { get; private set; } 
	}
}