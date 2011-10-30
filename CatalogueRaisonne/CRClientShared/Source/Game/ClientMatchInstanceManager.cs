/*

	ClientMatchInstanceManager.cs

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
using System.Text;
using System.Collections.Generic;
using System.Linq;

using CRShared;

namespace CRClientShared
{
	public class CClientMatchInstanceManager
	{
		// constructors
		private CClientMatchInstanceManager() 
		{
			Match = null;
		}

		static CClientMatchInstanceManager() {}

		// Public interface

		// Private interface
		// Slash command handlers
		[GenericHandler]
		private void Handle_Leave_Match_Command( CMatchLeaveSlashCommand command )
		{
			CClientLogicalThread.Instance.Send_Message_To_Server( new CLeaveMatchRequest() );
		}

		[GenericHandler]
		private void Handle_Continue_Match_Command( CMatchContinueSlashCommand command )
		{
			CClientLogicalThread.Instance.Send_Message_To_Server( new CContinueMatchRequest( command.Continue ) );
		}

		[GenericHandler]
		private void Handle_Match_Info_Command( CMatchInfoSlashCommand command )
		{
			if ( Match == null )
			{
				CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Match_Info_No_Match );
				return;
			}

			CMatchState match_state = Match.MatchState;

			CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Match_Info_Side_Row,
																		 CClientResource.Get_Text< EClientTextID >( EClientTextID.Client_Match_Info_Side_Header_Side ), 
																		 CClientResource.Get_Text< EClientTextID >( EClientTextID.Client_Match_Info_Side_Header_Score ), 
																		 CClientResource.Get_Text< EClientTextID >( EClientTextID.Client_Match_Info_Side_Header_Won ), 
																		 CClientResource.Get_Text< EClientTextID >( EClientTextID.Client_Match_Info_Side_Header_Lost ),
																		 CClientResource.Get_Text< EClientTextID >( EClientTextID.Client_Match_Info_Side_Header_Drawn ) );
			foreach ( var side_pair in match_state.SideResults )
			{
				CSideMatchStats side_data = side_pair.Value;
				CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Match_Info_Side_Row, side_pair.Key.ToString(), side_data.Score, side_data.GamesWon, side_data.GamesLost, side_data.GamesDrawn );			
			}

			string player_list_string = CSharedUtils.Build_String_List( match_state.Players,
																							player_id => CClientPlayerInfoManager.Instance.Get_Player_Name( player_id ),
																							player_id => match_state.Is_Player_Connected( player_id ) ? null : "[D]" );
			CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Match_Info_Player_List, player_list_string );			

			string observer_list_string = CSharedUtils.Build_String_List( match_state.Observers, player_id => CClientPlayerInfoManager.Instance.Get_Player_Name( player_id ), null );
			CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Match_Info_Observer_List, observer_list_string );			

			string admin_list_string = CSharedUtils.Build_String_List( match_state.AdminObservers, player_id => CClientPlayerInfoManager.Instance.Get_Player_Name( player_id ), null );
			CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Match_Info_Admin_List, admin_list_string );			

			CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Game_Info_Side_Bindings );			
			foreach ( var side_binding_pair in match_state.SideBindings )
			{
				CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Game_Info_Side_Binding_Entry,
																			 CClientPlayerInfoManager.Instance.Get_Player_Name( side_binding_pair.Key ),
																			 side_binding_pair.Value.ToString() );							
			}

			CGameState game_state = Match.GameState;

			CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Game_Info_Game_Mode, game_state.Mode.ToString() );			
			CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Game_Info_Game_Count, match_state.CurrentGameNumber, match_state.GameCount );			
			CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Game_Info_Game_State, match_state.State.ToString() );			
			CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Game_Info_Current_Player, CClientPlayerInfoManager.Instance.Get_Player_Name( game_state.CurrentPlayer ) );			

			// Deck
			CDeck deck = game_state.Deck;
			CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Game_Info_Deck_State, deck.Count, CSharedUtils.Build_String_List( deck.Cards ) );			

			// Discard Piles
			CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Game_Info_Discard_Pile_Header );			
			foreach ( ECardColor color in CGameProperties.Get_Card_Colors() )
			{
				string pile_list_string = CSharedUtils.Build_String_List( game_state.Get_Discard_Pile( color ).Cards, CCardUtils.Get_Card_Short_Form, null );
				CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Game_Info_Discard_Pile, color.ToString(), pile_list_string );			
			}

			// Collections
			CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Game_Info_Collection_Header );
			foreach ( EGameSide side in Enum.GetValues( typeof( EGameSide ) ) )
			{
				CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Game_Info_Collection_Side_Header, side.ToString() );			
				foreach ( ECardColor color in Enum.GetValues( typeof( ECardColor ) ) )
				{
					if ( color != ECardColor.Invalid )
					{
						string collection_list_string = CSharedUtils.Build_String_List( game_state.Get_Card_Collection( side, color ).Cards, CCardUtils.Get_Card_Short_Form, null );
						CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Game_Info_Collection, color.ToString(), collection_list_string );
					}
				}
			}

			// Hands
			CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Game_Info_Player_Hand_Header );
			foreach ( var hand_pair in game_state.PlayerHands )
			{
				string hand_list_string = CSharedUtils.Build_String_List( hand_pair.Value.Cards.OrderBy( CCardUtils.Compute_Card_Sort_Key ), CCardUtils.Get_Card_Short_Form, null );
				CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Game_Info_Player_Hand,
																			 hand_pair.Value.Count, 
																			 CClientPlayerInfoManager.Instance.Get_Player_Name( hand_pair.Key ), 
																			 hand_list_string );					
			}
		}

		[GenericHandler]
		private void Handle_Play_And_Draw_Deck_Command( CMatchTurnPlayAndDrawDeckSlashCommand command )
		{
			Handle_Pending_Turn( new CMatchTakeTurnRequest( new CPlayCardGameAction( new CCard( command.Color, command.Value ) ), new CDrawFromDeckGameAction() ) );
		}

		[GenericHandler]
		private void Handle_Play_And_Draw_Discard_Command( CMatchTurnPlayAndDrawDiscardSlashCommand command )
		{
			Handle_Pending_Turn( new CMatchTakeTurnRequest( new CPlayCardGameAction( new CCard( command.Color, command.Value ) ), new CDrawFromDiscardGameAction( command.DrawColor ) ) );
		}

		[GenericHandler]
		private void Handle_Discard_And_Draw_Deck_Command( CMatchTurnDiscardAndDrawDeckSlashCommand command )
		{
			Handle_Pending_Turn( new CMatchTakeTurnRequest( new CDiscardCardGameAction( new CCard( command.Color, command.Value ) ), new CDrawFromDeckGameAction() ) );
		}

		[GenericHandler]
		private void Handle_Discard_And_Draw_Discard_Command( CMatchTurnDiscardAndDrawDiscardSlashCommand command )
		{
			Handle_Pending_Turn( new CMatchTakeTurnRequest( new CDiscardCardGameAction( new CCard( command.Color, command.Value ) ), new CDrawFromDiscardGameAction( command.DrawColor ) ) );
		}

		[GenericHandler]
		private void Handle_Pass_Cards_Command( CMatchTurnPassCardsSlashCommand command )
		{
			Handle_Pending_Turn( new CMatchTakeTurnRequest( new CPassCardsGameAction( new CCard( command.Color1, command.Value1 ), new CCard( command.Color2, command.Value2 ) ) ) );
		}

		[GenericHandler]
		private void Handle_Play_Shortform_Command( CMatchPlayShortformSlashCommand command )
		{
			ECardColor play_color;
			ECardValue play_value;
			ECardColor draw_color;

			CCardUtils.Convert_Card_From_Short_Form( command.CardShortform, out play_color, out play_value );
			if ( play_color == ECardColor.Invalid || play_value == ECardValue.Invalid )
			{
				CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Game_Action_Invalid_Card_Shortform, command.CardShortform );
				return;
			}

			CCardUtils.Extract_Color_From_Short_Form( command.DrawCode, out draw_color );

			CGameActionBase draw_action = null;
			if ( draw_color == ECardColor.Invalid )
			{
				draw_action = new CDrawFromDeckGameAction();
			}
			else
			{
				draw_action = new CDrawFromDiscardGameAction( draw_color );
			}

			Handle_Pending_Turn( new CMatchTakeTurnRequest( new CPlayCardGameAction( new CCard( play_color, play_value ) ), draw_action ) );
			if ( m_PendingTurn != null )
			{
				CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Game_Action_Turn_Submitted );

				CClientLogicalThread.Instance.Send_Message_To_Server( m_PendingTurn );
				m_PendingTurn = null;
			}
		}

		[GenericHandler]
		private void Handle_Discard_Shortform_Command( CMatchDiscardShortformSlashCommand command )
		{
			ECardColor discard_color;
			ECardValue discard_value;
			ECardColor draw_color;

			CCardUtils.Convert_Card_From_Short_Form( command.CardShortform, out discard_color, out discard_value );
			if ( discard_color == ECardColor.Invalid || discard_value == ECardValue.Invalid )
			{
				CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Game_Action_Invalid_Card_Shortform, command.CardShortform );
				return;
			}

			CCardUtils.Extract_Color_From_Short_Form( command.DrawCode, out draw_color );

			CGameActionBase draw_action = null;
			if ( draw_color == ECardColor.Invalid )
			{
				draw_action = new CDrawFromDeckGameAction();
			}
			else
			{
				draw_action = new CDrawFromDiscardGameAction( draw_color );
			}

			Handle_Pending_Turn( new CMatchTakeTurnRequest( new CDiscardCardGameAction( new CCard( discard_color, discard_value ) ), draw_action ) );
			if ( m_PendingTurn != null )
			{
				CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Game_Action_Turn_Submitted );

				CClientLogicalThread.Instance.Send_Message_To_Server( m_PendingTurn );
				m_PendingTurn = null;
			}
		}

		[GenericHandler]
		private void Handle_Pass_Shortform_Command( CMatchPassShortformSlashCommand command )
		{
			ECardColor pass_color1;
			ECardValue pass_value1;

			CCardUtils.Convert_Card_From_Short_Form( command.CardShortform1, out pass_color1, out pass_value1 );
			if ( pass_color1 == ECardColor.Invalid || pass_value1 == ECardValue.Invalid )
			{
				CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Game_Action_Invalid_Card_Shortform, command.CardShortform1 );
				return;
			}

			ECardColor pass_color2;
			ECardValue pass_value2;

			CCardUtils.Convert_Card_From_Short_Form( command.CardShortform2, out pass_color2, out pass_value2 );
			if ( pass_color2 == ECardColor.Invalid || pass_value2 == ECardValue.Invalid )
			{
				CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Game_Action_Invalid_Card_Shortform, command.CardShortform2 );
				return;
			}

			Handle_Pending_Turn( new CMatchTakeTurnRequest( new CPassCardsGameAction( new CCard( pass_color1, pass_value1 ), new CCard( pass_color2, pass_value2 ) ) ) );
			if ( m_PendingTurn != null )
			{
				CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Game_Action_Turn_Submitted );

				CClientLogicalThread.Instance.Send_Message_To_Server( m_PendingTurn );
				m_PendingTurn = null;
			}
		}

		[GenericHandler]
		private void Handle_End_Turn_Command( CMatchEndTurnSlashCommand command )
		{
			EGameActionFailure failure_message = Is_Local_Player_Turn();
			if ( failure_message != EGameActionFailure.None )
			{
				CClientResource.Output_Text< EClientTextID >( Convert_Game_Action_Failure_To_Text_ID( failure_message ) );
				return;
			}

			if ( m_PendingTurn == null )
			{
				CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Game_Action_Your_Turn_Is_Incomplete );
				return;
			}

			CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Game_Action_Turn_Submitted );

			CClientLogicalThread.Instance.Send_Message_To_Server( m_PendingTurn );
			m_PendingTurn = null;
		}

		[GenericHandler]
		private void Handle_Reset_Turn_Command( CMatchResetTurnSlashCommand command )
		{
			EGameActionFailure failure_message = Is_Local_Player_Turn();
			if ( failure_message != EGameActionFailure.None )
			{
				CClientResource.Output_Text< EClientTextID >( Convert_Game_Action_Failure_To_Text_ID( failure_message ) );
				return;
			}

			CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Game_Action_Turn_Reset );

			m_PendingTurn = null;
		}

		[GenericHandler]
		private void Handle_Score_Command( CMatchScoreSlashCommand command )
		{
			if ( Match == null )
			{
				CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Match_Score_No_Match );
				return;
			}

			Dictionary< ECardColor, int > side1_scores = new Dictionary< ECardColor, int >();
			Match.GameState.Compute_Scores( EGameSide.Side1, side1_scores );
			Display_Side_Score_Data( EGameSide.Side1, side1_scores, Match.MatchState.Get_Side_Data( EGameSide.Side1 ) );

			Dictionary< ECardColor, int > side2_scores = new Dictionary< ECardColor, int >();
			Match.GameState.Compute_Scores( EGameSide.Side2, side2_scores );
			Display_Side_Score_Data( EGameSide.Side2, side2_scores, Match.MatchState.Get_Side_Data( EGameSide.Side2 ) );
		}

		// Network message handlers
		[NetworkMessageHandler]
		private void Handle_Join_Match( CJoinMatchSuccess message )
		{
			if ( Match != null )
			{
				throw new CApplicationException( "Received duplicate match state data" );
			}

			Set_Match( new CClientMatchInstance( message.MatchState, message.GameState ) );
			CClientLogicalThread.Instance.Add_UI_Notification( new CUIScreenStateNotification( EUIScreenState.Match_Idle ) );
			CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Match_Join_Success );
		}

		[NetworkMessageHandler]
		private void Handle_New_Game_Message( CMatchNewGameMessage message )
		{
			if ( Match == null )
			{
				return;
			}

			Match.Initialize_New_Game( message.GameState );
			CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Match_New_Game_Started );
		}

		[NetworkMessageHandler]
		private void Handle_Player_Leave_Match( CMatchPlayerLeftMessage message )
		{
			if ( Match == null )
			{
				throw new CApplicationException( "Received a match player leave notice when no match exists" );
			}

			bool is_self_remove = message.PlayerID == CClientLogicalThread.Instance.ConnectedID;

			switch ( message.Reason )
			{
				case EMatchRemovalReason.Match_Shutdown:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Match_Shutdown );
					break;

				case EMatchRemovalReason.Player_Disconnect_Timeout:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Match_Player_Removed_Disconnect_Timeout, CClientPlayerInfoManager.Instance.Get_Player_Name( message.PlayerID ) );
					break;

				case EMatchRemovalReason.Player_Request:
					if ( is_self_remove )
					{
						CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Match_Player_Removed_By_Request_Self );
					}
					else
					{
						CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Match_Player_Removed_By_Request, CClientPlayerInfoManager.Instance.Get_Player_Name( message.PlayerID ) );
					}
					break;
			}

			if ( is_self_remove )
			{
				Set_Match( null );
				CClientLogicalThread.Instance.Add_UI_Notification( new CUIScreenStateNotification( EUIScreenState.Chat_Idle ) );
			}
			else
			{
				Match.Remove_Member( message.PlayerID );
			}
		}

		[NetworkMessageHandler]
		private void Handle_Player_Connection_State_Changed( CMatchPlayerConnectionStateMessage message )
		{
			if ( Match == null )
			{
				throw new CApplicationException( "Received a match connection state delta when no match exists" );
			}

			Match.On_Player_Connection_State_Change( message.PlayerID, message.IsConnected );
		}

		[NetworkMessageHandler]
		private void Handle_Leave_Response( CLeaveMatchResponse response )
		{
			switch ( response.Error )
			{
				case ELeaveMatchFailureError.Not_In_Match:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Match_Leave_Not_In_Match );
					break;

				default:
					break;
			}
		}

		[NetworkMessageHandler]
		private void Handle_Take_Turn_Response( CMatchTakeTurnResponse response )
		{
			if ( response.Error != EGameActionFailure.None )
			{
				CClientResource.Output_Text< EClientTextID >( Convert_Game_Action_Failure_To_Text_ID( response.Error ) );
			}
			else
			{
				CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Game_Action_Application_Success );
			}
		}

		[NetworkMessageHandler]
		private void Handle_Delta_Set_Message( CMatchDeltaSetMessage message )
		{
			if ( Match == null )
			{
				return;
			}

			foreach ( var delta in message.Deltas )
			{
				delta.Apply( Match.GameState );
				Output_Delta_To_Console( delta );
			}
		}

		[NetworkMessageHandler]
		private void Handle_Match_State_Delta_Message( CMatchStateDeltaMessage message )
		{
			if ( Match == null )
			{
				return;
			}

			foreach ( var delta in message.Deltas )
			{
				delta.Apply( Match.MatchState );
				Output_Delta_To_Console( delta );
			}
		}

		[NetworkMessageHandler]
		private void Handle_Continue_Match_Response( CContinueMatchResponse response )
		{
			switch ( response.Error )
			{
				case EContinueMatchFailure.Cannot_Change_Previous_Commitment:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Continue_Match_Cannot_Change_Previous_Commitment );
					break;

				case EContinueMatchFailure.Match_Not_Halted:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Continue_Match_Match_Not_Halted );
					break;

				case EContinueMatchFailure.Not_A_Player:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Continue_Match_Not_A_Player );
					break;

				case EContinueMatchFailure.Not_In_Match:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Continue_Match_Not_In_Match );
					break;
				
				case EContinueMatchFailure.Match_Is_Over:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Continue_Match_Match_Is_Over );
					break;
			}
		}

		private void Output_Delta_To_Console( IObservedClonableDelta delta )
		{
			switch ( delta.ObservableDeltaType )
			{
				case EGameStateDelta.PlayerHand:
					CPlayerHand.CPlayerHandDelta hand_delta = delta as CPlayerHand.CPlayerHandDelta;
					if ( hand_delta.PlayerID == CClientLogicalThread.Instance.ConnectedID && hand_delta.DeltaType == ECardDelta.Add )
					{
						CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Game_Action_Card_Drawn, CCardUtils.Get_Card_Short_Form( hand_delta.Color, hand_delta.CardValue ) );				
					}
					break;

				case EGameStateDelta.DiscardPile:
					CDiscardPile.CDiscardPileDelta discard_delta = delta as CDiscardPile.CDiscardPileDelta;
					if ( discard_delta.DeltaType == ECardDelta.Add )
					{
						if ( Match.GameState.CurrentPlayer == CClientLogicalThread.Instance.ConnectedID )
						{
							CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Game_Action_Card_Discarded_Self, 
																						 CCardUtils.Get_Card_Short_Form( discard_delta.Color, discard_delta.Value ) );				
						}
						else
						{
							CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Game_Action_Card_Discarded_Other, 
																						 CClientPlayerInfoManager.Instance.Get_Player_Name( Match.GameState.CurrentPlayer ), 
																						 CCardUtils.Get_Card_Short_Form( discard_delta.Color, discard_delta.Value ) );				
						}
					}
					else if ( Match.GameState.CurrentPlayer != CClientLogicalThread.Instance.ConnectedID )
					{
						CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Game_Action_Card_Drawn_Other, 
																					 CClientPlayerInfoManager.Instance.Get_Player_Name( Match.GameState.CurrentPlayer ),
																					 CCardUtils.Get_Card_Short_Form( discard_delta.Color, discard_delta.Value ) );				
					}
					break;

				case EGameStateDelta.CardCollection:
					CCardCollection.CCardCollectionDelta collection_delta = delta as CCardCollection.CCardCollectionDelta;
					if ( Match.GameState.CurrentPlayer == CClientLogicalThread.Instance.ConnectedID )
					{
						CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Game_Action_Card_Played_Self, 
																					 CCardUtils.Get_Card_Short_Form( collection_delta.Color, collection_delta.Value ) );				
					}
					else
					{
						CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Game_Action_Card_Played_Other, 
																					 CClientPlayerInfoManager.Instance.Get_Player_Name( Match.GameState.CurrentPlayer ), 
																					 CCardUtils.Get_Card_Short_Form( collection_delta.Color, collection_delta.Value ) );				
					}
					break;

				case EGameStateDelta.Deck:
					if ( Match.GameState.CurrentPlayer != CClientLogicalThread.Instance.ConnectedID )
					{
						CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Game_Action_Unknown_Card_Drawn_Other,
																					 CClientPlayerInfoManager.Instance.Get_Player_Name( Match.GameState.CurrentPlayer ) );
					}
					break;
			}
		}

		private void Output_Delta_To_Console( CAbstractMatchDelta delta )
		{
			switch ( delta.Type )
			{
				case EMatchDeltaType.State:
					On_Match_State_Delta( delta as CMatchState.CMatchStateDelta );
					break;

				case EMatchDeltaType.Continue_State:
					On_Match_Continue_State_Delta( delta as CMatchState.CMatchContinueStateDelta );
					break;

				case EMatchDeltaType.Current_Game_Number:
					break;
			}
		}

		private void On_Match_State_Delta( CMatchState.CMatchStateDelta delta )
		{
			switch ( delta.State )
			{
				case EMatchInstanceState.Halted_End_Of_Match:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Match_Halt_Because_Match_Over );
					break;

				case EMatchInstanceState.Halted_End_Of_Game:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Match_Halt_Because_Game_Over );
					break;

				case EMatchInstanceState.Halted_Player_Left:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Match_Halt_Because_Player_Left );
					break;
			}
		}

		private void On_Match_Continue_State_Delta( CMatchState.CMatchContinueStateDelta delta )
		{
			bool is_self = delta.PlayerID == CClientLogicalThread.Instance.ConnectedID;

			switch ( delta.State )
			{
				case EMatchContinueState.Accepted:
					if ( is_self )
					{
						CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Match_Continue_State_Self_Yes );
					}
					else
					{
						CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Match_Continue_State_Player_Yes,
																					 CClientPlayerInfoManager.Instance.Get_Player_Name( delta.PlayerID ) );
					}
					break;

				case EMatchContinueState.Declined:
					if ( is_self )
					{
						CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Match_Continue_State_Self_No );
					}
					else
					{
						CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Match_Continue_State_Player_No,
																					 CClientPlayerInfoManager.Instance.Get_Player_Name( delta.PlayerID ) );
					}
					break;
			}
		}

		private void Set_Match( CClientMatchInstance match )
		{
			if ( Match != null )
			{
				Match.Remove_Player_Listeners();
			}

			Match = match;

			if ( Match != null )
			{
				Match.Add_Player_Listeners();
			}
		}

		private void Handle_Pending_Turn( CMatchTakeTurnRequest pending_turn )
		{
			EGameActionFailure failure_message = Is_Local_Player_Turn();
			if ( failure_message != EGameActionFailure.None )
			{
				CClientResource.Output_Text< EClientTextID >( Convert_Game_Action_Failure_To_Text_ID( failure_message ) );
				return;
			}

			if ( m_PendingTurn != null )
			{
				CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Game_Action_Turn_Is_Already_Complete );
				return;
			}

			failure_message = Match.Validate_Turn( pending_turn, CClientLogicalThread.Instance.ConnectedID );
			if ( failure_message != EGameActionFailure.None ) 
			{
				CClientResource.Output_Text< EClientTextID >( Convert_Game_Action_Failure_To_Text_ID( failure_message ) );
				return;				
			}

			CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Game_Action_Queued, CClientResource.Get_Text< EClientTextID >( EClientTextID.Client_Command_Name_Match_End_Turn ) );

			m_PendingTurn = pending_turn;
		}

		private EGameActionFailure Is_Local_Player_Turn()
		{
			if ( Match == null )
			{
				return EGameActionFailure.Not_In_Match;
			}

			if ( Match.MatchState.State != EMatchInstanceState.Idle )
			{
				return EGameActionFailure.Match_Halted;
			}

			if ( Match.GameState.CurrentPlayer != CClientLogicalThread.Instance.ConnectedID )
			{
				return EGameActionFailure.Not_Your_Turn;
			}

			return EGameActionFailure.None;
		}

		private EGameSide Get_Local_Side()
		{
			return Match.MatchState.Get_Side_For_Player( CClientLogicalThread.Instance.ConnectedID );
		}

		private EClientTextID Convert_Game_Action_Failure_To_Text_ID( EGameActionFailure action )
		{
			switch ( action )
			{
				case EGameActionFailure.Cannot_Play_More_Than_One_Card:
					return EClientTextID.Client_Game_Action_Cannot_Play_More_Than_One_Card;

				case EGameActionFailure.Cannot_Play_A_Card_After_Passing_Cards:
					return EClientTextID.Client_Game_Action_Cannot_Play_A_Card_After_Passing_Cards;

				case EGameActionFailure.Turn_Is_Already_Complete:
					return EClientTextID.Client_Game_Action_Turn_Is_Already_Complete;

				case EGameActionFailure.You_Must_Play_A_Card_Before_Drawing:
					return EClientTextID.Client_Game_Action_You_Must_Play_A_Card_Before_Drawing;

				case EGameActionFailure.Can_Only_Pass_One_Set_Of_Cards:
					return EClientTextID.Client_Game_Action_Can_Only_Pass_One_Set_Of_Cards;

				case EGameActionFailure.Can_Only_Pass_Cards_In_A_Four_Player_Game:
					return EClientTextID.Client_Game_Action_Can_Only_Pass_Cards_In_A_Four_Player_Game;

				case EGameActionFailure.Card_Is_Not_In_Your_Hand:
					return EClientTextID.Client_Game_Action_Card_Is_Not_In_Your_Hand;

				case EGameActionFailure.A_Higher_Card_Already_Exists:
					return EClientTextID.Client_Game_Action_A_Higher_Card_Already_Exists;

				case EGameActionFailure.Deck_Is_Empty:
					return EClientTextID.Client_Game_Action_Deck_Is_Empty;

				case EGameActionFailure.Discard_Pile_Is_Empty:
					return EClientTextID.Client_Game_Action_Discard_Pile_Is_Empty;

				case EGameActionFailure.Not_Enough_Cards_In_Hand_To_Pass:
					return EClientTextID.Client_Game_Action_Not_Enough_Cards_In_Hand_To_Pass;

				case EGameActionFailure.Not_In_Match:
					return EClientTextID.Client_Game_Action_Not_In_Match;

				case EGameActionFailure.Match_Halted:
					return EClientTextID.Client_Game_Action_Match_Halted;

				case EGameActionFailure.Not_Your_Turn:
					return EClientTextID.Client_Game_Action_Not_Your_Turn;

				case EGameActionFailure.Turn_Is_Incomplete:
					return EClientTextID.Client_Game_Action_Turn_Is_Incomplete;

				case EGameActionFailure.Invalid_Turn:
					return EClientTextID.Client_Game_Action_Invalid_Turn;

				case EGameActionFailure.Play_Card_Must_Be_First_Action:
					return EClientTextID.Client_Game_Action_Play_Card_Must_Be_First_Action;

				case EGameActionFailure.Discard_Card_Must_Be_First_Action:
					return EClientTextID.Client_Game_Action_Discard_Card_Must_Be_First_Action;

				case EGameActionFailure.Passing_Cards_Must_Be_Only_Action:
					return EClientTextID.Client_Game_Action_Passing_Cards_Must_Be_Only_Action;

				case EGameActionFailure.Two_Distinct_Cards_Must_Be_Passed:
					return EClientTextID.Client_Game_Action_Two_Distinct_Cards_Must_Be_Passed;

				case EGameActionFailure.Not_A_Player:
					return EClientTextID.Client_Game_Action_Not_A_Player;

				default:
					return EClientTextID.Invalid;
			}
		}

		private void Display_Side_Score_Data( EGameSide side, Dictionary< ECardColor, int > scores, CSideMatchStats side_data )
		{
			CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Match_Score_Side_Header, side.ToString() );
			scores.Apply( p => CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Match_Score_Color, p.Key.ToString(), p.Value ) );

			CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Match_Score_Game_Total, scores.Aggregate( 0, ( sum, val ) => sum + val.Value ) );
			CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Match_Score_Game_Results, side_data.GamesWon, side_data.GamesLost, side_data.GamesDrawn );
			CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Match_Score_Match_Total, side_data.Score );
		}

		// Properties
		public static CClientMatchInstanceManager Instance { get { return m_Instance; } }

		public CClientMatchInstance Match { get; private set; }

		// Fields
		private static readonly CClientMatchInstanceManager m_Instance = new CClientMatchInstanceManager();

		private CMatchTakeTurnRequest m_PendingTurn = null;
	}
}