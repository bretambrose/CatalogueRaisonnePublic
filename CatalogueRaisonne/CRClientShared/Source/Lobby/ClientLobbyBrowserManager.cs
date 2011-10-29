using System;
using System.Collections.Generic;

using CRShared;

namespace CRClientShared
{
	public enum EClientBrowseState
	{
		None,
		Starting,
		Browse_Idle,
		Browse_Moving
	}

	public class CClientLobbyBrowserManager
	{
		// Construction
		static CClientLobbyBrowserManager() {}
		private CClientLobbyBrowserManager() 
		{
		}

		// Methods
		// Non-public interface
		// Slash command handlers
		[GenericHandler]
		private void Handle_Start_Browse_Lobby_Command( CStartBrowseLobbySlashCommand command )
		{
			CClientLogicalThread.Instance.Send_Message_To_Server( new CStartBrowseLobbyRequest( command.GameModes, command.MemberTypes, command.JoinFirstAvailable ) );
			State = EClientBrowseState.Starting;
		}

		[GenericHandler]
		private void Handle_End_Browse_Lobby_Command( CEndBrowseLobbySlashCommand command )
		{
			if ( State != EClientBrowseState.None )
			{
				CClientLogicalThread.Instance.Send_Message_To_Server( new CEndBrowseLobbyMessage() );
			}
			State = EClientBrowseState.None;
		}

		[GenericHandler]
		private void Handle_Browse_Previous_Lobby_Command( CBrowsePreviousLobbySetSlashCommand command )
		{
			if ( State == EClientBrowseState.Browse_Idle )
			{
				State = EClientBrowseState.Browse_Moving;
				CClientLogicalThread.Instance.Send_Message_To_Server( new CBrowsePreviousLobbySetRequest() );
			}
		}

		[GenericHandler]
		private void Handle_Browse_Next_Lobby_Command( CBrowseNextLobbySetSlashCommand command )
		{
			if ( State == EClientBrowseState.Browse_Idle )
			{
				State = EClientBrowseState.Browse_Moving;
				CClientLogicalThread.Instance.Send_Message_To_Server( new CBrowseNextLobbySetRequest() );
			}
		}

		[GenericHandler]
		private void Handle_Browse_Lobby_List_Command( CBrowseLobbyListSlashCommand command )
		{
			Handle_List_Lobbies();
		}

		// Network message handlers
		[NetworkMessageHandler]
		private void Handle_Start_Browse_Lobby_Response( CStartBrowseLobbyResponse response )
		{
			Clear_Lobbies();

			switch ( response.Result )
			{
				case EStartBrowseResult.AutoJoined:
					State = EClientBrowseState.None;
					break;

				case EStartBrowseResult.Success:
					State = EClientBrowseState.Browse_Idle;
					response.LobbySummaries.Apply( summary => Add_Summary( summary ) );

					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Browse_Start_Success );
					break;

				case EStartBrowseResult.Invalid_State:
					State = EClientBrowseState.None;
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Browse_Start_Invalid_State );
					break;

				default:
					State = EClientBrowseState.None;
					break;
			}
		}

		[NetworkMessageHandler]
		private void Handle_Cursor_Browse_Lobby_Response( CCursorBrowseLobbyResponse response )
		{
			switch ( response.Result )
			{
				case ECursorBrowseResult.Not_Browsing:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Browse_Cursor_Not_Browsing );
					break;

				case ECursorBrowseResult.End_Of_Cursor:
					break;

				case ECursorBrowseResult.Success:
					Clear_Lobbies();
					response.LobbySummaries.Apply( summary => Add_Summary( summary ) );
					break;
			}

			State = EClientBrowseState.Browse_Idle;
		}

		[NetworkMessageHandler]
		private void Handle_Browse_Lobby_Delta_Message( CBrowseLobbyDeltaMessage message )
		{
			CLobbySummary summary = Get_Summary( message.Delta.ID );
			if ( summary != null )
			{
				summary.Apply_Delta( message.Delta );
			}
		}

		[NetworkMessageHandler]
		private void Handle_Browse_Lobby_Add_Remove_Message( CBrowseLobbyAddRemoveMessage message )
		{
			if ( message.RemovedLobby != ELobbyID.Invalid )
			{
				Remove_Summary( message.RemovedLobby );
			}

			foreach ( var lobby_summary in message.Lobbies )
			{
				Add_Summary( lobby_summary );
			}
		}

		private void Handle_List_Lobbies()
		{
			switch ( State )
			{
				case EClientBrowseState.None:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Browse_List_Not_Browsing );
					break;

				default:
					Dump_Lobbies_To_Console();
					break;
			}
		}

		private void Dump_Lobbies_To_Console()
		{
			if ( m_WatchedLobbies.Count == 0 )
			{
				CSharedResource.Output_Text< EClientTextID >( EClientTextID.Client_Browse_List_No_Lobbies );
				return;
			}

			CSharedResource.Output_Text< EClientTextID >( EClientTextID.Client_Browse_List_Header );

			CSharedResource.Output_Text< EClientTextID >( EClientTextID.Client_Browse_List_Summary_Info_Format,
																				CSharedResource.Get_Text< EClientTextID >( EClientTextID.Client_Browse_List_Summary_Info_Header_ID ),
																				CSharedResource.Get_Text< EClientTextID >( EClientTextID.Client_Browse_List_Summary_Info_Header_GameMode ),
																				CSharedResource.Get_Text< EClientTextID >( EClientTextID.Client_Browse_List_Summary_Info_Header_Creator ),
																				CSharedResource.Get_Text< EClientTextID >( EClientTextID.Client_Browse_List_Summary_Info_Header_CreationTime ),
																				CSharedResource.Get_Text< EClientTextID >( EClientTextID.Client_Browse_List_Summary_Info_Header_PlayerCount ),
																				CSharedResource.Get_Text< EClientTextID >( EClientTextID.Client_Browse_List_Summary_Info_Header_ObserverCount ) );
			
			List< ELobbyID > summary_ids = new List< ELobbyID >();
			m_WatchedLobbies.Apply( n => summary_ids.Add( n.Key ) );

			summary_ids.Sort();

			foreach ( var summary_id in summary_ids )
			{
				CLobbySummary summary = Get_Summary( summary_id );
				string game_mode_string = Get_Game_Mode_String( summary.GameMode );
				string creator_string = CClientPlayerInfoManager.Instance.Get_Player_Name( summary.Creator );

				CSharedResource.Output_Text< EClientTextID >( EClientTextID.Client_Browse_List_Summary_Info_Format,																				
																					summary.ID,
																					game_mode_string,
																					creator_string,
																					summary.CreationTime.ToShortDateString(),
																					summary.PlayerCount,
																					summary.ObserverCount );
			}
		}

		private string Get_Game_Mode_String( EGameModeType game_mode )
		{
			switch ( game_mode )
			{
				case EGameModeType.Two_Players:
					return CSharedResource.Get_Text< EClientTextID >( EClientTextID.Client_Game_Mode_Two_Players );

				case EGameModeType.Four_Players:
					return CSharedResource.Get_Text< EClientTextID >( EClientTextID.Client_Game_Mode_Four_Players );
			}

			return "";
		}

		private void On_State_Changed()
		{	
			// Notify UI
		}

		private void Add_Summary( CLobbySummary summary )
		{
			m_WatchedLobbies.Add( summary.ID, summary );

			CClientPlayerInfoManager.Instance.Begin_Player_Listen( summary.Creator, EPlayerListenReason.Browsed_Lobby_Creator );
		}

		private void Remove_Summary( ELobbyID lobby_id )
		{
			CLobbySummary summary = Get_Summary( lobby_id );
			if ( summary != null )
			{
				CClientPlayerInfoManager.Instance.End_Player_Listen( summary.Creator, EPlayerListenReason.Browsed_Lobby_Creator );
			}

			m_WatchedLobbies.Remove( lobby_id );
		}

		private CLobbySummary Get_Summary( ELobbyID lobby_id )
		{
			CLobbySummary summary = null;
			m_WatchedLobbies.TryGetValue( lobby_id, out summary );
			return summary;
		}

		private void Clear_Lobbies()
		{
			m_WatchedLobbies.Clear();
		}

		// Properties
		public static CClientLobbyBrowserManager Instance { get { return m_Instance; } }
		
		public EClientBrowseState State { get { return m_State; } private set { m_State = value; On_State_Changed(); } }

		// Fields
		private static CClientLobbyBrowserManager m_Instance = new CClientLobbyBrowserManager();
		
		private EClientBrowseState m_State = EClientBrowseState.None;
		private Dictionary< ELobbyID, CLobbySummary > m_WatchedLobbies = new Dictionary< ELobbyID, CLobbySummary >();
	}
}
