using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CRShared
{
	public class CPlayerHand : IObservedClonable
	{
		// Nested Types
		public class CPlayerHandDelta : IObservedClonableDelta
		{
			public CPlayerHandDelta( EPersistenceID player_id, ECardDelta delta_type, ECardColor color, ECardValue card_value )
			{
				PlayerID = player_id;
				DeltaType = delta_type;
				Color = color;
				CardValue = card_value;
			}

			// Public interface
			public IObservedClonableDelta Clone( EPersistenceID player_id, bool admin )
			{
				bool visible = admin || player_id == PlayerID;
				return new CPlayerHandDelta( PlayerID, DeltaType, visible ? Color : ECardColor.Invalid, visible ? CardValue : ECardValue.Invalid );
			}

			public void Apply( CGameState game_state )
			{
				CPlayerHand hand = game_state.Get_Player_Hand( PlayerID );
				switch ( DeltaType )
				{
					case ECardDelta.Add:
						hand.Add_Card( new CCard( Color, CardValue ) );
						break;

					case ECardDelta.Remove:
						hand.Remove_Card( Color, CardValue );
						break;
				}
			}

			public override string ToString()
			{
				return String.Format( "( [CPlayerHandDelta] PlayerID = {0}, DeltaType = {1}, Color = {2}, CardValue = {3} )", (int) PlayerID, DeltaType.ToString(), Color.ToString(), CardValue.ToString() );
			}

			// Properties
			public EGameStateDelta ObservableDeltaType { get { return EGameStateDelta.PlayerHand; } }

			public EPersistenceID PlayerID { get; private set; }
			public ECardColor Color { get; private set; }
			public ECardDelta DeltaType { get; private set; }
			public ECardValue CardValue { get; private set; }
		}

		// Construction
		public CPlayerHand()
		{
		}

		// Public interface
		public IObservedClonable Clone( EGameStateClonePermission permission )
		{
			CPlayerHand player_hand = new CPlayerHand();
			player_hand.m_Count = m_Count;

			if ( permission == EGameStateClonePermission.Full )
			{
				Cards.SimpleClone( player_hand.m_Cards );
			}

			return player_hand;
		}

		public override string ToString()
		{
			return String.Format( "( [CPlayerHand] Count = {0}, Cards = ( {1} ) )", Count, CSharedUtils.Build_String_List( Cards ) );
		}

		public void Add_Card( CCard card )
		{
			if ( card.Color != ECardColor.Invalid && card.Value != ECardValue.Invalid )
			{
				m_Cards.Add( card );
			}

			m_Count++;
		}

		public bool Contains_Card( ECardColor color, ECardValue value )
		{
			return Cards.FirstOrDefault( card => card.Color == color && card.Value == value ) != null;
		}

		// Private Interface
		private void Remove_Card( ECardColor color, ECardValue card_value )
		{
			if ( color != ECardColor.Invalid && card_value != ECardValue.Invalid )
			{
				m_Cards.RemoveAt( m_Cards.FindIndex( n => n.Color == color && n.Value == card_value ) );
			}

			m_Count--;
		}

		// Properties
		public IEnumerable< CCard > Cards { get { return m_Cards; } }
		public uint Count { get { return m_Count; } }
		public bool HasCards { get { return m_Cards.Count > 0; } }
		public CCard this[ uint index ] { get { return m_Cards[ (int) index ]; } }

		// Fields
		private List< CCard > m_Cards = new List< CCard >();
		private uint m_Count = 0;
	}


}