using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CRShared
{
	public class CGameState
	{
		// Nested types
		public class CGameStateDelta : IObservedClonableDelta
		{
			public CGameStateDelta( uint new_turn_index )
			{
				NewTurnIndex = new_turn_index;
			}

			public IObservedClonableDelta Clone( EPersistenceID player_id, bool admin )
			{
				return new CGameStateDelta( NewTurnIndex );
			}

			public void Apply( CGameState game_state )
			{
				game_state.CurrentTurnIndex = NewTurnIndex;
			}

			public override string ToString()
			{
				return String.Format( "( [CGameStateDelta] NewTurnIndex = {0} )", NewTurnIndex );
			}

			// Properties
			public EGameStateDelta ObservableDeltaType { get { return EGameStateDelta.GameState; } }

			public uint NewTurnIndex { get; private set; }
		}

		// Construction
		private CGameState()
		{
		}

		public CGameState( EGameModeType mode )
		{
			m_Deck = new CDeck( mode == EGameModeType.Two_Players ? EDeckPopulateDirective.Two_Player : EDeckPopulateDirective.Four_Player );
			Mode = mode;

			Create_Discard_Piles();
			Create_Side_Collections();
		}

		// Public interface
		public CGameState Clone_By_Observer( EPersistenceID observer, bool is_admin )
		{
			CGameState game_state = new CGameState();

			// Unconditional cloning
			game_state.CurrentTurnIndex = CurrentTurnIndex;
			game_state.Mode = Mode;

			DiscardPiles.FullClone( game_state.m_DiscardPiles );
			m_TurnSequence.ShallowCopy( game_state.m_TurnSequence );

			foreach ( var side_pair in m_CardCollections )
			{
				Dictionary< ECardColor, CCardCollection > collection_dictionary = new Dictionary< ECardColor, CCardCollection >();
				game_state.m_CardCollections.Add( side_pair.Key, collection_dictionary );

				side_pair.Value.FullClone( collection_dictionary );
			}

			// Conditional cloning
			m_PlayerHands.VarClone( game_state.m_PlayerHands, n => ( is_admin || observer == n.Key ) ? EGameStateClonePermission.Full : EGameStateClonePermission.Hidden );

			game_state.m_Deck = m_Deck.Clone( is_admin ? EGameStateClonePermission.Full : EGameStateClonePermission.Hidden ) as CDeck;

			return game_state;
		}

		public override string ToString()
		{
			string turn_sequence_string = CSharedUtils.Build_String_List( m_TurnSequence, n => ((long) n).ToString() );
			string discard_pile_string = CSharedUtils.Build_String_List( m_DiscardPiles, n => String.Format( "{0}->{1}", n.Key.ToString(), n.Value.ToString() ) );
			string hand_string = CSharedUtils.Build_String_List( m_PlayerHands, n => String.Format( "{0}->{1}", (long)n.Key, n.Value.ToString() ) );
	
			// this isn't very readable; it was more of a can-this-be-done? situation
			string collection_side_string = CSharedUtils.Build_String_List( m_CardCollections, x => String.Format( "{0}->({1}) ", x.Key.ToString(), 
				CSharedUtils.Build_String_List( x.Value, n => String.Format( "{0}->{1}", n.Key.ToString(), n.Value.ToString() ) ) ) );

			return String.Format( "( [CGameState] TurnSequence = ( {0} ), CurrentTurnIndex = {1}, DiscardPiles = ( {2} ), Hands = ( {3} ), Collections = ( {4} ), Deck = {5} )", 
										 turn_sequence_string, 
										 CurrentTurnIndex,
										 discard_pile_string,
										 hand_string,
										 collection_side_string,
										 m_Deck.ToString() );
		}

		public void Initialize_Game( IEnumerable< EPersistenceID > player_ids )
		{
			player_ids.ShallowCopy( m_TurnSequence );

			if ( m_TurnSequence.Count != CGameModeUtils.Player_Count_For_Game_Mode( Mode ) )
			{
				throw new CApplicationException( "Game has wrong amount of players" );
			}

			CSharedUtils.Shuffle_List( m_TurnSequence );

			Create_Player_Hands();
		}

		public CDeck Get_Deck()
		{
			return m_Deck;
		}

		public CDiscardPile Get_Discard_Pile( ECardColor color )
		{
			return m_DiscardPiles[ color ];
		}

		public CPlayerHand Get_Player_Hand( EPersistenceID player_id )
		{
			return m_PlayerHands[ player_id ];
		}
		
		public CCardCollection Get_Card_Collection( EGameSide side, ECardColor color )
		{
			return ( m_CardCollections[ side ] )[ color ];
		}
		
		public bool Is_Game_Finished()
		{
			return Deck.Count == 0;
		}

		public void Compute_Scores( EGameSide side, Dictionary< ECardColor, int > side_scores )
		{
			m_CardCollections[ side ].Apply( n => side_scores[ n.Key ] = n.Value.Compute_Score() );
		}

		public int Compute_Score( EGameSide side )
		{
			return m_CardCollections[ side ].Aggregate( 0, ( acc, pair ) => acc + pair.Value.Compute_Score() );
		}

		// Private interface
		private void Create_Player_Hands()
		{
			foreach ( var player_id in m_TurnSequence )
			{
				CPlayerHand player_hand = new CPlayerHand();
				for ( uint i = 0; i < CGameProperties.StartingHandSize; i++ )
				{
					player_hand.Add_Card( m_Deck.Draw_Card() );
				}

				m_PlayerHands.Add( player_id, player_hand );
			}
		}

		private void Create_Discard_Piles()
		{
			CGameProperties.Get_Card_Colors().Apply( n => m_DiscardPiles.Add( n, new CDiscardPile() ) );
		}

		private void Create_Side_Collections()
		{
			Dictionary< ECardColor, CCardCollection > side1_collections = new Dictionary< ECardColor, CCardCollection >();
			Dictionary< ECardColor, CCardCollection > side2_collections = new Dictionary< ECardColor, CCardCollection >();

			foreach ( var card_color in CGameProperties.Get_Card_Colors() )
			{
				side1_collections.Add( card_color, new CCardCollection() );
				side2_collections.Add( card_color, new CCardCollection() );
			}

			m_CardCollections.Add( EGameSide.Side1, side1_collections );
			m_CardCollections.Add( EGameSide.Side2, side2_collections );
		}

		public IEnumerable< KeyValuePair< ECardColor, CCardCollection > > Get_Side_Collections( EGameSide side )
		{
			return m_CardCollections[ side ];
		}
		 
		// Properties
		public EPersistenceID CurrentPlayer { get { return m_TurnSequence[ (int) CurrentTurnIndex ]; } }
		public EGameModeType Mode { get; private set; }
		public uint CurrentTurnIndex { get; private set; }
		public CDeck Deck { get { return m_Deck; } }

		public IEnumerable< KeyValuePair< ECardColor, CDiscardPile > > DiscardPiles { get { return m_DiscardPiles; } }
		public IEnumerable< KeyValuePair< EPersistenceID, CPlayerHand > > PlayerHands { get { return m_PlayerHands; } }

		// Fields
		private List< EPersistenceID > m_TurnSequence = new List< EPersistenceID >();

		private Dictionary< ECardColor, CDiscardPile > m_DiscardPiles = new Dictionary< ECardColor, CDiscardPile >();
		private Dictionary< EPersistenceID, CPlayerHand > m_PlayerHands = new Dictionary< EPersistenceID, CPlayerHand >();
		private Dictionary< EGameSide, Dictionary< ECardColor, CCardCollection > > m_CardCollections = new Dictionary< EGameSide, Dictionary< ECardColor, CCardCollection > >();
		private CDeck m_Deck = null;
	}


}