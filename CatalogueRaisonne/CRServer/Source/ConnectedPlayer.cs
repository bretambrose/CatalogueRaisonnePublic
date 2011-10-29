// merge conflict test
using System;

using CRShared;
using CRShared.Chat;

namespace CRServer
{

	public enum EConnectedPlayerState
	{
		Invalid = -1,

		Chat_Idle,

		Lobby_Idle,
		Lobby_Browsing,
		Lobby_Matching,

		Game_Idle
	}

	public enum EConnectedPlayerOperation
	{
		Create_Lobby,
		Join_Lobby,
		Browse_Lobbies,
		Match_Lobby,
		Create_Quickmatch,
		Join_Quickmatch,
		Start_Game
	}

	public enum EConnectedPlayerConnectionState
	{
		Disconnected,
		Connecting,
		Connected
	}

	public class CConnectedPlayer
	{
		// Construction
		public CConnectedPlayer( EPersistenceID persistence_id, ESessionID session_id, string name )
		{
			PersistenceID = persistence_id;
			SessionID = session_id;
			Name = name;
			LobbyID = ELobbyID.Invalid;
			MatchID = EMatchInstanceID.Invalid;
			State = EConnectedPlayerState.Chat_Idle;
			ConnectionState = EConnectedPlayerConnectionState.Connecting;
		}
		
		// Methods
		// Public interface
		public void Suspend()
		{
			SessionID = ESessionID.Invalid;
			ConnectionState = EConnectedPlayerConnectionState.Disconnected;
			TimeOfDisconnect = CServerLogicalThread.Instance.CurrentTime;

			switch ( State )
			{
				case EConnectedPlayerState.Lobby_Idle:
					CServerLobbyManager.Instance.On_Player_Disconnect( PersistenceID, LobbyID );
					break;

				case EConnectedPlayerState.Game_Idle:
					CServerMatchInstanceManager.Instance.On_Player_Disconnect( PersistenceID, MatchID );
					break;

			}
		}
		
		public void Resume( ESessionID new_id )
		{
			SessionID = new_id;
			ConnectionState = EConnectedPlayerConnectionState.Connecting;
		}

		public void Set_Connected()
		{
			ConnectionState = EConnectedPlayerConnectionState.Connected;
		}

		public bool State_Allows_Operation( EConnectedPlayerOperation operation )
		{
			if ( ConnectionState != EConnectedPlayerConnectionState.Connected )
			{
				return false;
			}

			switch ( operation )
			{
				case EConnectedPlayerOperation.Create_Lobby:
					return State == EConnectedPlayerState.Chat_Idle;

				case EConnectedPlayerOperation.Join_Lobby:
					return State == EConnectedPlayerState.Chat_Idle || State == EConnectedPlayerState.Lobby_Browsing;

				case EConnectedPlayerOperation.Browse_Lobbies:
					return State == EConnectedPlayerState.Chat_Idle || State == EConnectedPlayerState.Lobby_Browsing;

				case EConnectedPlayerOperation.Match_Lobby:
					return State == EConnectedPlayerState.Chat_Idle;

				case EConnectedPlayerOperation.Start_Game:
					return State == EConnectedPlayerState.Lobby_Idle;

				case EConnectedPlayerOperation.Create_Quickmatch:
					return State != EConnectedPlayerState.Game_Idle;

				case EConnectedPlayerOperation.Join_Quickmatch:
					return State != EConnectedPlayerState.Game_Idle;
			}

			return false;
		}

		public void Change_State( EConnectedPlayerState new_state )
		{
			State = new_state;
		}

		public void On_Join_Lobby_Success( ELobbyID lobby_id )
		{
			State = EConnectedPlayerState.Lobby_Idle;
			LobbyID = lobby_id;
		}

		public void On_Leave_Lobby()
		{
			LobbyID = ELobbyID.Invalid;
		}

		public void On_Start_Match( EMatchInstanceID match_id )
		{
			State = EConnectedPlayerState.Game_Idle;
			MatchID = match_id;
			LobbyID = ELobbyID.Invalid;
		}

		public void On_Leave_Match()
		{
			State = EConnectedPlayerState.Chat_Idle;
			MatchID = EMatchInstanceID.Invalid;
		}

		public void On_Join_General_Channel_Response( EChannelJoinError error, EMessageRequestID client_request_id )
		{
			if ( error != EChannelJoinError.None && error != EChannelJoinError.AlreadyJoined )
			{
				CServerMessageRouter.Send_Message_To_Session( new CConnectionDroppedMessage( EConnectRefusalReason.Unable_To_Join_Required_Chat_Channel ), SessionID );
				CServerLogicalThread.Instance.Add_Network_Operation( new CDisconnectRequestOperation( SessionID, EDisconnectReason.Server_Request ) );		
			}
			else 
			{
				if ( ConnectionState == EConnectedPlayerConnectionState.Connecting )
				{
					// send client hello and set state to connected
					ConnectionState = EConnectedPlayerConnectionState.Connected;
					CServerMessageRouter.Send_Message_To_Session( new CClientHelloResponse( client_request_id, CDatabaseProxy.Instance.Get_Player_Data( PersistenceID ) ), SessionID );
				}

				State = EConnectedPlayerState.Chat_Idle;
			}
		}

		// Properties
		public ESessionID SessionID { get; private set; }
		public EPersistenceID PersistenceID { get; private set; }
		public string Name { get; private set; }
		public ELobbyID LobbyID { get; private set; }
		public EMatchInstanceID MatchID { get; private set; }
		public EConnectedPlayerState State { get; private set; }
		public EConnectedPlayerConnectionState ConnectionState { get; set; }
		public long TimeOfDisconnect { get; set; }
		
		// Fields
	}
}
