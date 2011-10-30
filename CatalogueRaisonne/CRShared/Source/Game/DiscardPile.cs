/*

	DiscardPile.cs

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

namespace CRShared
{
	public class CDiscardPile : IObservedClonable
	{
		// Nested Types
		public class CDiscardPileDelta : IObservedClonableDelta
		{
			public CDiscardPileDelta( ECardDelta delta_type, ECardColor color, ECardValue value )
			{
				DeltaType = delta_type;
				Color = color;
				Value = value;
			}

			// Public interface
			public IObservedClonableDelta Clone( EPersistenceID player_id, bool admin )
			{
				return new CDiscardPileDelta( DeltaType, Color, Value );
			}

			public void Apply( CGameState game_state )
			{
				CDiscardPile pile = game_state.Get_Discard_Pile( Color );
				switch ( DeltaType )
				{
					case ECardDelta.Add:
						pile.Add_Card( new CCard( Color, Value ) );
						break;

					case ECardDelta.Remove:
						pile.Remove_Card();
						break;
				}
			}

			public override string ToString()
			{
				return String.Format( "( [CDiscardPileDelta] DeltaType = {0}, Color = {1}, Value = {2} )", DeltaType.ToString(), Color.ToString(), Value.ToString() );
			}

			// Properties
			public EGameStateDelta ObservableDeltaType { get { return EGameStateDelta.DiscardPile; } }

			public ECardColor Color { get; private set; }
			public ECardDelta DeltaType { get; private set; }
			public ECardValue Value { get; private set; }
		}

		// Construction
		public CDiscardPile()
		{
		}

		// Public interface
		public IObservedClonable Clone( EGameStateClonePermission permission )
		{
			CDiscardPile discard_pile = new CDiscardPile();

			Cards.SimpleClone( discard_pile.m_Cards );

			return discard_pile;
		}

		public override string ToString()
		{
			return String.Format( "( [CDiscardPile] Cards = ( {0} ) )", CSharedUtils.Build_String_List( Cards ) );
		}

		public CCard Get_Top_Card()
		{
			if ( m_Cards.Count == 0 )
			{
				return null;
			}

			return m_Cards[ m_Cards.Count - 1 ];
		}

		// Private Interface
		private void Add_Card( CCard card )
		{
			m_Cards.Add( card );
		}

		private void Remove_Card()
		{
			m_Cards.RemoveAt( m_Cards.Count - 1 );
		}

		// Properties
		public IEnumerable< CCard > Cards { get { return m_Cards; } }
		public uint Count { get { return (uint) m_Cards.Count; } }
		public CCard this[ uint index ] { get { return m_Cards[ (int) index ]; } }

		// Fields
		private List< CCard > m_Cards = new List< CCard >();
	}


}