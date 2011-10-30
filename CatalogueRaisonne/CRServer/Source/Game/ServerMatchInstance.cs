/*

	ServerMatchInstance.cs

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

using CRShared;
using CRShared.Chat;

namespace CRServer
{

	public class CServerMatchInstance : CMatchInstance
	{
		// Local classes
		public class CMatchShutdownTask : CScheduledTask
		{	
			// Construction
			public CMatchShutdownTask( EMatchInstanceID match_id, long schedule_time ) :
				base( schedule_time )
			{
				MatchID = match_id;
			}
			
			// Methods
			public override void Execute( long current_time ) 
			{
				CServerMatchInstanceManager.Instance.Shutdown_Match( MatchID, EMatchDestroyedReason.Post_Halt_Timeout );
			}

			// Properties
			public EMatchInstanceID MatchID { get; private set; }
		}

		// Construction
		public CServerMatchInstance( EMatchInstanceID match_id, CLobbyState lobby_state ) :
			base( match_id, lobby_state )
		{
			MatchChannel = EChannelID.Invalid;
			ObserverChannel = EChannelID.Invalid;
		}

		public CServerMatchInstance( EMatchInstanceID match_id, EPersistenceID player1, EPersistenceID player2, uint game_count ) :
			base( match_id, player1, player2, game_count )
		{
			MatchChannel = EChannelID.Invalid;
			ObserverChannel = EChannelID.Invalid;
		}

		// Public Interface
		public void Perform_Match_Channel_Joins()
		{
			ConnectedPlayers.Apply( pid => CAsyncBackendOperations.Join_Match_Channel( ID, pid, MatchChannel ) );
		}

		public void Perform_Observer_Channel_Joins()
		{
			ConnectedObservers.Apply( oid => CAsyncBackendOperations.Join_Observer_Channel( ID, oid, ObserverChannel ) );
		}

		public void Start()
		{
			ConnectedPlayersAndObservers.Apply( pid => CServerMessageRouter.Send_Message_To_Player( new CJoinMatchSuccess( Clone_Match_State( pid ), Clone_Game_State( pid ) ), pid ) );
		}

		public void Shutdown()
		{
		}

		public void Send_Message_To_Members( CNetworkMessage message, EPersistenceID exclude_id )
		{
			ConnectedPlayersAndObservers.Where( id => id != exclude_id ).Apply( pid => CServerMessageRouter.Send_Message_To_Player( message, pid ) );
		}

		public void Remove_Member( EPersistenceID player_id )
		{
			MatchState.Remove_Observer( player_id );
			if ( MatchState.Remove_Player( player_id ) )
			{
				Halt_Match( EMatchInstanceState.Halted_Player_Left );
			}
		}

		public void On_Player_Disconnect( EPersistenceID player_id )
		{
			if ( MatchState.On_Player_Disconnect( player_id ) )
			{
				Send_Message_To_Members( new CMatchPlayerConnectionStateMessage( player_id, false ), EPersistenceID.Invalid );
			}
		}

		public void On_Player_Reconnect( EPersistenceID player_id )
		{
			if ( MatchState.On_Player_Reconnect( player_id ) )
			{
				Send_Message_To_Members( new CMatchPlayerConnectionStateMessage( player_id, true ), player_id );
			}

			CMatchState match_state = Clone_Match_State( player_id );
			CGameState game_state = Clone_Game_State( player_id );
			CJoinMatchSuccess join_success = new CJoinMatchSuccess( match_state, game_state );
			CServerMessageRouter.Send_Message_To_Player( join_success, player_id );

			if ( Players.Contains( player_id ) )
			{
				CAsyncBackendOperations.Join_Match_Channel( ID, player_id, MatchChannel );
			}

			if ( Observers.Contains( player_id ) )
			{
				CAsyncBackendOperations.Join_Observer_Channel( ID, player_id, ObserverChannel );
			}
		}

		public override void Halt_Match( EMatchInstanceState new_state )
		{
			if ( MatchState.State != new_state )
			{
				base.Halt_Match( new_state );

				List< CAbstractMatchDelta > deltas = new List< CAbstractMatchDelta >();
				deltas.Add( new CMatchState.CMatchStateDelta( new_state ) );

				Send_Message_To_Members( new CMatchStateDeltaMessage( deltas ), EPersistenceID.Invalid );	
				
				if ( new_state == EMatchInstanceState.Halted_End_Of_Match || new_state == EMatchInstanceState.Halted_Player_Left )
				{
					long match_shutdown_time = CServerLogicalThread.CurrentThreadTime + CTimeKeeperThread.Convert_Seconds_To_Internal_Time( MATCH_SHUTDOWN_GRACE_PERIOD );
					CServerLogicalThread.Instance.TaskScheduler.Add_Scheduled_Task( new CMatchShutdownTask( ID, match_shutdown_time ) );
				}		
			}
		}

		public void Build_Turn_Deltas( CMatchTakeTurnRequest request, EPersistenceID source_player, List< IObservedClonableDelta > deltas )
		{
			foreach ( var action in request.Actions )
			{
				switch ( action.Action )
				{
					case EGameAction.Play_Card:
						CPlayCardGameAction play_action = action as CPlayCardGameAction;
						ECardColor play_color = play_action.Card.Color;
						ECardValue play_value = play_action.Card.Value;
						deltas.Add( new CPlayerHand.CPlayerHandDelta( source_player, ECardDelta.Remove, play_color, play_value ) );
						deltas.Add( new CCardCollection.CCardCollectionDelta( MatchState.Get_Side_For_Player( source_player ), play_color, play_value ) );
						break;

					case EGameAction.Discard_Card:
						CDiscardCardGameAction discard_action = action as CDiscardCardGameAction;
						ECardColor discard_color = discard_action.Card.Color;
						ECardValue discard_value = discard_action.Card.Value;
						deltas.Add( new CPlayerHand.CPlayerHandDelta( source_player, ECardDelta.Remove, discard_color, discard_value ) );
						deltas.Add( new CDiscardPile.CDiscardPileDelta( ECardDelta.Add, discard_color, discard_value ) );
						break;

					case EGameAction.Draw_From_Deck:
						CDeck deck = GameState.Get_Deck();
						CCard top_card = deck.Peek_Top_Card();
						deltas.Add( new CDeck.CDeckDelta() );
						deltas.Add( new CPlayerHand.CPlayerHandDelta( source_player, ECardDelta.Add, top_card.Color, top_card.Value ) );
						break;

					case EGameAction.Draw_From_Discard:
						CDrawFromDiscardGameAction draw_from_discard_action = action as CDrawFromDiscardGameAction;
						ECardColor draw_color = draw_from_discard_action.Color;
						CDiscardPile discard_pile = GameState.Get_Discard_Pile( draw_color );
						ECardValue draw_value = discard_pile.Get_Top_Card().Value;
						deltas.Add( new CPlayerHand.CPlayerHandDelta( source_player, ECardDelta.Add, draw_color, draw_value ) );
						deltas.Add( new CDiscardPile.CDiscardPileDelta( ECardDelta.Remove, draw_color, draw_value ) );
						break;

					case EGameAction.Pass_Cards:
						CPassCardsGameAction pass_action = action as CPassCardsGameAction;
						deltas.Add( new CPlayerHand.CPlayerHandDelta( source_player, ECardDelta.Remove, pass_action.Card1.Color, pass_action.Card1.Value ) );
						deltas.Add( new CPlayerHand.CPlayerHandDelta( source_player, ECardDelta.Remove, pass_action.Card2.Color, pass_action.Card2.Value ) );

						EPersistenceID partner_id = MatchState.Get_Partner_For( source_player );
						deltas.Add( new CPlayerHand.CPlayerHandDelta( partner_id, ECardDelta.Add, pass_action.Card1.Color, pass_action.Card1.Value ) );
						deltas.Add( new CPlayerHand.CPlayerHandDelta( partner_id, ECardDelta.Add, pass_action.Card2.Color, pass_action.Card2.Value ) );
						break;
				}
			}

			deltas.Add( new CGameState.CGameStateDelta( ( GameState.CurrentTurnIndex + 1 ) % CGameModeUtils.Player_Count_For_Game_Mode( GameState.Mode ) ) );
		}

		public void Try_Take_Turn( CMatchTakeTurnRequest request, EPersistenceID source_player )
		{
			if ( !MatchState.Is_Player( source_player ) )
			{
				CServerMessageRouter.Send_Message_To_Player( new CMatchTakeTurnResponse( request.RequestID, EGameActionFailure.Not_A_Player ), source_player );
				return;
			}

			EGameActionFailure failure_reason = Validate_Turn( request, source_player );
			if ( failure_reason != EGameActionFailure.None )
			{
				CServerMessageRouter.Send_Message_To_Player( new CMatchTakeTurnResponse( request.RequestID, failure_reason ), source_player );
				return;
			}

			CServerMessageRouter.Send_Message_To_Player( new CMatchTakeTurnResponse( request.RequestID, EGameActionFailure.None ), source_player );

			// Build, Apply, Broadcast deltas
			List< IObservedClonableDelta > deltas = new List< IObservedClonableDelta >();
			Build_Turn_Deltas( request, source_player, deltas );
			Apply_Turn_Deltas( deltas );

			foreach ( var pid in ConnectedPlayersAndObservers )
			{
				List< IObservedClonableDelta > cloned_deltas = new List< IObservedClonableDelta >( deltas.Select( delta => delta.Clone( pid, MatchState.Is_Admin_Observer( pid ) ) ) );
				CServerMessageRouter.Send_Message_To_Player( new CMatchDeltaSetMessage( cloned_deltas ), pid );			
			}

			if ( GameState.Is_Game_Finished() )
			{
				On_Game_End_Naturally();
			}
		}

		public void Handle_Continue_Match_Request( CContinueMatchRequest request, EPersistenceID source_player )
		{
			if ( !MatchState.Is_Player( source_player ) )
			{
				CServerMessageRouter.Send_Message_To_Player( new CContinueMatchResponse( request.RequestID, EContinueMatchFailure.Not_A_Player ), source_player );
				return;
			}

			if ( MatchState.State == EMatchInstanceState.Idle )
			{
				CServerMessageRouter.Send_Message_To_Player( new CContinueMatchResponse( request.RequestID, EContinueMatchFailure.Match_Not_Halted ), source_player );
				return;
			}

			if ( MatchState.State == EMatchInstanceState.Halted_End_Of_Match )
			{
				CServerMessageRouter.Send_Message_To_Player( new CContinueMatchResponse( request.RequestID, EContinueMatchFailure.Match_Is_Over ), source_player );
				return;
			}

			if ( MatchState.Get_Continue_State_For_Player( source_player ) != EMatchContinueState.None )
			{
				CServerMessageRouter.Send_Message_To_Player( new CContinueMatchResponse( request.RequestID, EContinueMatchFailure.Cannot_Change_Previous_Commitment ), source_player );
				return;
			}

			CServerMessageRouter.Send_Message_To_Player( new CContinueMatchResponse( request.RequestID, EContinueMatchFailure.None ), source_player );

			EMatchContinueState continue_state = request.ShouldContinueMatch ? EMatchContinueState.Accepted : EMatchContinueState.Declined;
			MatchState.Set_Continue_State_For_Player( source_player, continue_state );

			List< CAbstractMatchDelta > deltas = new List< CAbstractMatchDelta >();
			deltas.Add( new CMatchState.CMatchContinueStateDelta( source_player, continue_state ) );

			Send_Message_To_Members( new CMatchStateDeltaMessage( deltas ), EPersistenceID.Invalid );

			EMatchContinueState collective_state = MatchState.Get_Collective_Ready_State();
			switch ( collective_state )
			{
				case EMatchContinueState.Declined:
					Halt_Match( EMatchInstanceState.Halted_End_Of_Match );
					break;

				case EMatchContinueState.Accepted:
					Start_New_Game();
					break;
			}
		}

		// Protected Interface
		protected override void Start_New_Game()
		{
			base.Start_New_Game();

			List< CAbstractMatchDelta > deltas = new List< CAbstractMatchDelta >();
			deltas.Add( new CMatchState.CMatchCurrentGameNumberDelta( MatchState.CurrentGameNumber + 1 ) );

			Send_Message_To_Members( new CMatchStateDeltaMessage( deltas ), EPersistenceID.Invalid );	

			ConnectedPlayersAndObservers.Apply( pid => CServerMessageRouter.Send_Message_To_Player( new CMatchNewGameMessage( Clone_Game_State( pid ) ), pid ) );
		}

		// Private Interface
		private void On_Game_End_Naturally()
		{
			if ( MatchState.CurrentGameNumber >= MatchState.GameCount )
			{
				Halt_Match( EMatchInstanceState.Halted_End_Of_Match );
			}
			else
			{
				Halt_Match( EMatchInstanceState.Halted_End_Of_Game );
			}

			Dictionary< EGameSide, int > scores = new Dictionary< EGameSide, int >();
			scores[ EGameSide.Side1 ] = GameState.Compute_Score( EGameSide.Side1 );
			scores[ EGameSide.Side2 ] = GameState.Compute_Score( EGameSide.Side2 );

			CSideMatchStats data1 = MatchState.Get_Side_Data( EGameSide.Side1 );
			CSideMatchStats data2 = MatchState.Get_Side_Data( EGameSide.Side2 );

			CSideMatchStats.CSideMatchStatsDelta delta1 = null, delta2 = null;
			if ( scores[ EGameSide.Side1 ] > scores [ EGameSide.Side2 ] )
			{
				delta1 = new CSideMatchStats.CSideMatchStatsDelta( EGameSide.Side1, data1.Score + scores[ EGameSide.Side1 ], data1.GamesLost, 1 + data1.GamesWon, data1.GamesDrawn );
				delta2 = new CSideMatchStats.CSideMatchStatsDelta( EGameSide.Side2, data2.Score + scores[ EGameSide.Side2 ], data2.GamesLost + 1, data2.GamesWon, data2.GamesDrawn );
			}
			else if ( scores[ EGameSide.Side1 ] < scores [ EGameSide.Side2 ] )
			{
				delta1 = new CSideMatchStats.CSideMatchStatsDelta( EGameSide.Side1, data1.Score + scores[ EGameSide.Side1 ], data1.GamesLost + 1, data1.GamesWon, data1.GamesDrawn );
				delta2 = new CSideMatchStats.CSideMatchStatsDelta( EGameSide.Side2, data2.Score + scores[ EGameSide.Side2 ], data2.GamesLost, data2.GamesWon + 1, data2.GamesDrawn );
			}
			else
			{
				delta1 = new CSideMatchStats.CSideMatchStatsDelta( EGameSide.Side1, data1.Score + scores[ EGameSide.Side1 ], data1.GamesLost, data1.GamesWon, data1.GamesDrawn + 1 );
				delta2 = new CSideMatchStats.CSideMatchStatsDelta( EGameSide.Side2, data2.Score + scores[ EGameSide.Side2 ], data2.GamesLost, data2.GamesWon, data2.GamesDrawn + 1 );
			}

			List< CAbstractMatchDelta > deltas = new List< CAbstractMatchDelta >();
			deltas.Add( delta1 );
			deltas.Add( delta2 );

			deltas.Apply( n => n.Apply( MatchState ) );
			Send_Message_To_Members( new CMatchStateDeltaMessage( deltas ), EPersistenceID.Invalid );
		}

		// Properties
		public EChannelID MatchChannel { get; set; }
		public EChannelID ObserverChannel { get; set; }

		public IEnumerable< EPersistenceID > Players { get { return MatchState.Players; } }
		public IEnumerable< EPersistenceID > ConnectedPlayers { get { return Players.Where( pid => CConnectedPlayerManager.Instance.Is_Connected( pid ) ); } }

		public IEnumerable< EPersistenceID > Observers { get { return MatchState.Observers.Union( MatchState.AdminObservers ); } }
		public IEnumerable< EPersistenceID > ConnectedObservers { get { return Observers.Where( pid => CConnectedPlayerManager.Instance.Is_Connected( pid ) ); } }

		public IEnumerable< EPersistenceID > PlayersAndObservers { get { return Players.Union( Observers ); } }
		public IEnumerable< EPersistenceID > ConnectedPlayersAndObservers { get { return PlayersAndObservers.Where( pid => CConnectedPlayerManager.Instance.Is_Connected( pid ) ); } }

		// Fields

		private const uint MATCH_SHUTDOWN_GRACE_PERIOD = 60;
	}
}