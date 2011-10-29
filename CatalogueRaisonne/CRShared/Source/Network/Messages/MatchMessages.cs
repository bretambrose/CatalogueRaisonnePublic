using System;
using System.Collections.Generic;
using System.Text;

namespace CRShared
{
	public enum ELeaveMatchFailureError
	{
		None,

		Not_In_Match
	}

	[NetworkMessage]
	public class CJoinMatchSuccess : CNetworkMessage
	{
		// Construction
		public CJoinMatchSuccess( CMatchState match_state, CGameState game_state ) :
			base()
		{
			MatchState = match_state;
			GameState = game_state;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CJoinMatchSuccess]: Match = {0}, Game = {1} )", MatchState.ToString(), GameState.ToString() );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Join_Match_Success; } }

		public CMatchState MatchState { get; private set; }
		public CGameState GameState { get; private set; }
	}

	[NetworkMessage]
	public class CMatchNewGameMessage : CNetworkMessage
	{
		// Construction
		public CMatchNewGameMessage( CGameState game_state ) :
			base()
		{
			GameState = game_state;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CMatchNewGameMessage]: Game = {0} )", GameState.ToString() );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Match_New_Game; } }

		public CGameState GameState { get; private set; }
	}

	[NetworkMessage]
	public class CMatchPlayerLeftMessage : CNetworkMessage
	{
		// Construction
		public CMatchPlayerLeftMessage( EPersistenceID player_id, EMatchRemovalReason reason ) :
			base()
		{
			PlayerID = player_id;
			Reason = reason;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CMatchPlayerLeftMessage]: PlayerID={0}, Reason={1} )", (long)PlayerID, Reason.ToString() );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Match_Player_Left; } }

		public EPersistenceID PlayerID { get; private set; }
		public EMatchRemovalReason Reason { get; private set; }
	}
	
	[NetworkMessage]
	public class CMatchPlayerConnectionStateMessage : CNetworkMessage
	{
		// Construction
		public CMatchPlayerConnectionStateMessage( EPersistenceID player_id, bool is_connected ) :
			base()
		{
			PlayerID = player_id;
			IsConnected = is_connected;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CMatchPlayerConnectionStateMessage]: PlayerID={0}, IsConnected={1} )", (long)PlayerID, IsConnected.ToString() );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Match_Player_Connection_State; } }

		public EPersistenceID PlayerID { get; private set; }
		public bool IsConnected { get; private set; }
	}

	[NetworkMessage]
	public class CLeaveMatchRequest : CRequestMessage
	{
		// Construction
		public CLeaveMatchRequest() :
			base()
		{
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CLeaveMatchRequest] )" );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Leave_Match_Request; } }
	}

	[NetworkMessage]
	public class CLeaveMatchResponse : CResponseMessage
	{
		// Construction
		public CLeaveMatchResponse( EMessageRequestID request_id, ELeaveMatchFailureError error ) :
			base( request_id )
		{
			Error = error;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CLeaveMatchResponse]: Error = {0} )", Error.ToString() );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Leave_Match_Response; } }

		public ELeaveMatchFailureError Error { get; private set; }
	}

	[NetworkMessage]
	public class CMatchTakeTurnRequest : CRequestMessage
	{
		// Construction
		public CMatchTakeTurnRequest( CGameActionBase action ) :
			base()
		{
			Actions = new List< CGameActionBase >();

			Actions.Add( action );
		}

		public CMatchTakeTurnRequest( CGameActionBase action1, CGameActionBase action2 ) :
			base()
		{
			Actions = new List< CGameActionBase >();

			Actions.Add( action1 );
			Actions.Add( action2 );
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CMatchTakeTurnRequest]: Actions = ( {0} ) )", CSharedUtils.Build_String_List( Actions ) );
		}

		public void Add_Action( CGameActionBase action ) { Actions.Add( action ); }

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Match_Take_Turn_Request; } }

		public List< CGameActionBase > Actions { get; private set; }
	}

	[NetworkMessage]
	public class CMatchTakeTurnResponse : CResponseMessage
	{
		// Construction
		public CMatchTakeTurnResponse( EMessageRequestID request_id, EGameActionFailure error ) :
			base( request_id )
		{
			Error = error;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CMatchTakeTurnResponse]: Error = {0} )", Error.ToString() );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Match_Take_Turn_Response; } }

		public EGameActionFailure Error { get; private set; }
	}

	[NetworkMessage]
	public class CMatchDeltaSetMessage : CNetworkMessage
	{
		// Construction
		public CMatchDeltaSetMessage( List< IObservedClonableDelta > deltas ) :
			base()
		{
			Deltas = deltas;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CMatchDeltaSetMessage]: Deltas = ( {0} ) )", CSharedUtils.Build_String_List( Deltas ) );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Match_Delta_Set; } }

		public List< IObservedClonableDelta > Deltas { get; private set; }
	}

	[NetworkMessage]
	public class CMatchStateDeltaMessage : CNetworkMessage
	{
		// Construction
		public CMatchStateDeltaMessage( List< CAbstractMatchDelta > deltas ) :
			base()
		{
			Deltas = deltas;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CMatchStateDeltaMessage]: Deltas = ( {0} ) )", CSharedUtils.Build_String_List( Deltas ) );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Match_State_Delta; } }

		public List< CAbstractMatchDelta > Deltas { get; private set; }
	}

	[NetworkMessage]
	public class CContinueMatchRequest : CRequestMessage
	{
		// Construction
		public CContinueMatchRequest( bool should_continue_match ) :
			base()
		{
			ShouldContinueMatch = should_continue_match;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CContinueMatchRequest] ShouldContinueMatch = {0} )", ShouldContinueMatch.ToString() );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Continue_Match_Request; } }

		public bool ShouldContinueMatch { get; private set; }
	}

	[NetworkMessage]
	public class CContinueMatchResponse : CResponseMessage
	{
		// Construction
		public CContinueMatchResponse( EMessageRequestID request_id, EContinueMatchFailure error ) :
			base( request_id )
		{
			Error = error;
		}

		// Methods
		public override string ToString()
		{
			return String.Format( "( [CContinueMatchResponse]: Error = {0} )", Error.ToString() );
		}

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Continue_Match_Response; } }

		public EContinueMatchFailure Error { get; private set; }
	}

}
