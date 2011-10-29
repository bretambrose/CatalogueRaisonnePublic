using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CRShared
{
	public class CCardCollection : IObservedClonable
	{
		// Nested Types
		public class CCardCollectionDelta : IObservedClonableDelta
		{
			public CCardCollectionDelta( EGameSide side, ECardColor color, ECardValue value )
			{
				Side = side;
				Color = color;
				Value = value;
			}

			// Public interface
			public IObservedClonableDelta Clone( EPersistenceID player_id, bool admin )
			{
				return new CCardCollectionDelta( Side, Color, Value );
			}

			public void Apply( CGameState game_state )
			{
				CCardCollection collection = game_state.Get_Card_Collection( Side, Color );
				collection.Add_Card( new CCard( Color, Value ) );
			}

			public override string ToString()
			{
				return String.Format( "( [CCardCollectionDelta] Side = {0}, Color = {1}, Value = {2} )", Side.ToString(), Color.ToString(), Value.ToString() );
			}

			// Properties
			public EGameStateDelta ObservableDeltaType { get { return EGameStateDelta.CardCollection; } }

			public EGameSide Side { get; private set; }
			public ECardColor Color { get; private set; }
			public ECardValue Value { get; private set; }
		}

		// Construction
		public CCardCollection()
		{
		}

		// Public interface
		public IObservedClonable Clone( EGameStateClonePermission permission )
		{
			CCardCollection card_collection = new CCardCollection();

			m_Cards.SimpleClone( card_collection.m_Cards );

			return card_collection;
		}

		public override string ToString()
		{
			return String.Format( "( [CCardCollection] Count = {0}, Cards = ( {1} ) )", Count, CSharedUtils.Build_String_List( Cards ) );
		}

		public int Get_Top_Card_Score_Value()
		{
			if ( m_Cards.Count > 0 )
			{
				return CCardUtils.Get_Card_Score_Value( m_Cards[ m_Cards.Count - 1 ].Value );
			}

			return 0;
		}

		public int Compute_Score()
		{
			if ( m_Cards.Count == 0 )
			{
				return 0;
			}

			int multiplier = 1;
			int total = 0;

			foreach ( var card in m_Cards )
			{
				if ( CCardUtils.Is_Multiplier( card.Value ) )
				{
					multiplier++;
				}
				else
				{
					total += CCardUtils.Get_Card_Score_Value( card.Value );
				}
			}

			int score = multiplier * ( total - 20 );
			if ( m_Cards.Count >= 8 )
			{
				score += 20;
			}

			return score;
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