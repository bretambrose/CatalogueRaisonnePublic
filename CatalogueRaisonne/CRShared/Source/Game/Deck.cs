/*

	Decks.cs

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
	public enum EDeckPopulateDirective
	{
		None,
		Two_Player,
		Four_Player
	}

	public class CDeck : IObservedClonable
	{
		// Nested Types
		public class CDeckDelta : IObservedClonableDelta
		{
			public CDeckDelta()
			{
			}

			public IObservedClonableDelta Clone( EPersistenceID player_id, bool is_admin )
			{
				return new CDeckDelta();
			}

			public void Apply( CGameState game_state )
			{
				CDeck deck = game_state.Get_Deck();
				deck.Remove_Card();
			}

			public override string ToString()
			{
				return String.Format( "( [CDeckDelta] )" );
			}

			public EGameStateDelta ObservableDeltaType { get { return EGameStateDelta.Deck; } }
		}

		// Construction
		public CDeck( EDeckPopulateDirective populate_directive )
		{
			if ( populate_directive != EDeckPopulateDirective.None )
			{
				CGameProperties.Get_Card_Colors().Apply( n => Add_Color_Set( n, populate_directive ) );

				m_Count = (uint) m_Cards.Count;

				Shuffle();
			}
		}

		// Public interface
		public IObservedClonable Clone( EGameStateClonePermission permission )
		{
			CDeck clone = new CDeck( EDeckPopulateDirective.None );
			
			if ( permission == EGameStateClonePermission.Full )
			{
				m_Cards.SimpleClone( clone.m_Cards );
				clone.m_Count = (uint) clone.m_Cards.Count;
			}
			else
			{
				clone.m_Count = m_Count;
			}

			return clone; 
		}

		public override string ToString()
		{
			return String.Format( "( [CDeck] Count = {0}, Cards = ( {1} ) )", Count, CSharedUtils.Build_String_List( Cards ) );
		}

		public void Shuffle()
		{
			CSharedUtils.Shuffle_List( m_Cards );
		}

		public CCard Draw_Card()
		{
			if ( m_Cards.Count == 0 )
			{
				return null;
			}

			CCard returned_card = m_Cards[ m_Cards.Count - 1 ];
			m_Cards.RemoveAt( m_Cards.Count - 1 );
			m_Count--;

			return returned_card;
		}

		public CCard Peek_Top_Card()
		{
			if ( m_Cards.Count == 0 )
			{
				return null;
			}

			return m_Cards[ m_Cards.Count - 1 ];
		}

		// Private interface
		private void Remove_Card()
		{
			if ( m_Cards.Count != 0 )
			{
				m_Cards.RemoveAt( m_Cards.Count - 1 );
			}

			if ( m_Count > 0 )
			{
				m_Count--;
			}
		}

		private void Add_Color_Set( ECardColor color, EDeckPopulateDirective populate_directive )
		{
			m_Cards.Add( new CCard( color, ECardValue.Multiplier1 ) );
			m_Cards.Add( new CCard( color, ECardValue.Multiplier2 ) );
			m_Cards.Add( new CCard( color, ECardValue.Multiplier3 ) );

			m_Cards.Add( new CCard( color, ECardValue.Two ) );
			m_Cards.Add( new CCard( color, ECardValue.Three ) );
			m_Cards.Add( new CCard( color, ECardValue.Four ) );
			m_Cards.Add( new CCard( color, ECardValue.Five ) );
			m_Cards.Add( new CCard( color, ECardValue.Six ) );
			m_Cards.Add( new CCard( color, ECardValue.Seven ) );
			m_Cards.Add( new CCard( color, ECardValue.Eight ) );
			m_Cards.Add( new CCard( color, ECardValue.Nine ) );
			m_Cards.Add( new CCard( color, ECardValue.Ten ) );

			if ( populate_directive == EDeckPopulateDirective.Four_Player )
			{
				m_Cards.Add( new CCard( color, ECardValue.Two_v2 ) );
				m_Cards.Add( new CCard( color, ECardValue.Three_v2 ) );
				m_Cards.Add( new CCard( color, ECardValue.Four_v2 ) );
			}
		}

		// Properties
		public IEnumerable< CCard > Cards { get { return m_Cards; } }
		public bool Empty { get { return m_Count == 0; } }
		public uint Count { get { return m_Count; } }

		// Fields
		private List< CCard > m_Cards = new List< CCard >();
		private uint m_Count = 0;
	}


}