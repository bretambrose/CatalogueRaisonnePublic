/*

	ServerLobbyBrowserManager.cs

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

namespace CRServer
{
	public enum ELobbyBrowseNotification
	{
		Invalid = -1,

		New_Lobby,
		Removed_Lobby,
		Full_Lobby,
		Non_Full_Lobby
	}

	public class CServerLobbyBrowserManager
	{

		private class CLobbyBrowseNotificationTask : IOrderedTask
		{
			// Construction
			public CLobbyBrowseNotificationTask( ELobbyID lobby_id, ELobbyBrowseNotification notice )
			{
				LobbyID = lobby_id;
				Notice = notice;
			}

			// Methods
			public void Execute()
			{
				switch ( Notice )
				{
					case ELobbyBrowseNotification.New_Lobby:
						CServerLobbyBrowserManager.Instance.Notify_Browsers_Of_New_Lobby_Internal( LobbyID );
						break;

					case ELobbyBrowseNotification.Removed_Lobby:
						CServerLobbyBrowserManager.Instance.Notify_Browsers_Of_Removed_Lobby_Internal( LobbyID );
						break;

					case ELobbyBrowseNotification.Full_Lobby:
						CServerLobbyBrowserManager.Instance.Notify_Browsers_Of_Full_Lobby_Internal( LobbyID );
						break;

					case ELobbyBrowseNotification.Non_Full_Lobby:
						CServerLobbyBrowserManager.Instance.Notify_Browsers_Of_Non_Full_Lobby_Internal( LobbyID );
						break;

				}
			}

			// Properties
			public ELobbyID LobbyID { get; private set; }
			public ELobbyBrowseNotification Notice { get; private set; }
		}

		public enum ELobbyPlayerBrowseNotification
		{
			Banned_Or_Ignored,
			Unbanned_Or_Unignored
		}

		private class CLobbyPlayerBrowseNotificationTask : IOrderedTask
		{
			// Construction
			public CLobbyPlayerBrowseNotificationTask( ELobbyID lobby_id, EPersistenceID player_id, ELobbyPlayerBrowseNotification notice )
			{
				LobbyID = lobby_id;
				PlayerID = player_id;
				Notice = notice;
			}

			// Methods
			public void Execute()
			{
				switch ( Notice )
				{
					case ELobbyPlayerBrowseNotification.Banned_Or_Ignored:
						CServerLobbyBrowserManager.Instance.Notify_Browser_Of_Lobby_Ban_Or_Ignore_Internal( PlayerID, LobbyID );
						break;

					case ELobbyPlayerBrowseNotification.Unbanned_Or_Unignored:
						CServerLobbyBrowserManager.Instance.Notify_Browser_Of_Lobby_Unban_Or_Unignore_Internal( PlayerID, LobbyID );
						break;

				}
			}

			// Properties
			private ELobbyID LobbyID { get; set; }
			private EPersistenceID PlayerID { get; set; }
			private ELobbyPlayerBrowseNotification Notice { get; set; }
		}

		// Construction
		static CServerLobbyBrowserManager() {}
		private CServerLobbyBrowserManager() {}

		// Methods
		// Public interface
		public void Stop_Browsing( EPersistenceID player_id )
		{
			if ( Is_Browsing( player_id ) )
			{
				CLog.Log( ELoggingChannel.Lobby, ELogLevel.Medium, String.Format( "Player {0} has stopped browsing lobbies.",
																										CConnectedPlayerManager.Get_Player_Log_Name( player_id ) ) );

				CConnectedPlayer player = CConnectedPlayerManager.Instance.Get_Player_By_Persistence_ID( player_id );
				player.Change_State( EConnectedPlayerState.Chat_Idle );

				Clear_Watched_Lobbies( player_id );

				m_Browsers.Remove( player_id );
			}
		}

		public void Notify_Browsers_Of_New_Lobby( ELobbyID lobby_id )
		{
			CServerLogicalThread.Instance.TaskScheduler.Add_Ordered_Task( new CLobbyBrowseNotificationTask( lobby_id, ELobbyBrowseNotification.New_Lobby ) );
		}

		public void Notify_Browsers_Of_Removed_Lobby( ELobbyID lobby_id )
		{
			CServerLogicalThread.Instance.TaskScheduler.Add_Ordered_Task( new CLobbyBrowseNotificationTask( lobby_id, ELobbyBrowseNotification.Removed_Lobby ) );
		}

		public void Notify_Browsers_Of_Full_Lobby( ELobbyID lobby_id )
		{
			CServerLogicalThread.Instance.TaskScheduler.Add_Ordered_Task( new CLobbyBrowseNotificationTask( lobby_id, ELobbyBrowseNotification.Full_Lobby ) );
		}

		public void Notify_Browsers_Of_Non_Full_Lobby( ELobbyID lobby_id )
		{
			CServerLogicalThread.Instance.TaskScheduler.Add_Ordered_Task( new CLobbyBrowseNotificationTask( lobby_id, ELobbyBrowseNotification.Non_Full_Lobby ) );
		}

		public void Notify_Browser_Of_Lobby_Ban_Or_Ignore( EPersistenceID player_id, ELobbyID lobby_id )
		{
			CServerLogicalThread.Instance.TaskScheduler.Add_Ordered_Task( new CLobbyPlayerBrowseNotificationTask( lobby_id, player_id, ELobbyPlayerBrowseNotification.Banned_Or_Ignored ) );
		}

		public void Notify_Browser_Of_Lobby_Unban_Or_Unignore( EPersistenceID player_id, ELobbyID lobby_id )
		{
			CServerLogicalThread.Instance.TaskScheduler.Add_Ordered_Task( new CLobbyPlayerBrowseNotificationTask( lobby_id, player_id, ELobbyPlayerBrowseNotification.Unbanned_Or_Unignored ) );
		}

		public bool Is_Browsing( EPersistenceID player_id )
		{
			return m_Browsers.ContainsKey( player_id );
		}

		public void Broadcast_Delta_To_Watchers( CLobbySummaryDelta delta )
		{
			HashSet< EPersistenceID > watchers = Get_Watchers( delta.ID );
			if ( watchers != null )
			{
				watchers.Apply( wid => CServerMessageRouter.Send_Message_To_Player( new CBrowseLobbyDeltaMessage( delta ), wid ) );
			}
		}

		// Non-public interface
		// Network message handlers
		[NetworkMessageHandler]
		private void Handle_Start_Browse_Lobby_Request( CStartBrowseLobbyRequest request, EPersistenceID player_id )
		{
			CConnectedPlayer player = CConnectedPlayerManager.Instance.Get_Active_Player_By_Persistence_ID( player_id );
			if ( player == null )
			{
				return;
			}

			if ( !player.State_Allows_Operation( EConnectedPlayerOperation.Browse_Lobbies ) )
			{
				CServerMessageRouter.Send_Message_To_Player( new CStartBrowseLobbyResponse( request.RequestID, EStartBrowseResult.Invalid_State ), player_id );
				return;
			}			

			CLog.Log( ELoggingChannel.Lobby, ELogLevel.Medium, String.Format( "Player {0} requesting to start browsing lobbies.",
																									CConnectedPlayerManager.Get_Player_Log_Name( player_id ) ) );

			Stop_Browsing( player_id );

			CServerLobbyBrowser browser = new CServerLobbyBrowser( player_id, request.BrowseCriteria, request.JoinFirstAvailable );
			m_Browsers.Add( player_id, browser );

			Browse_Forwards( browser, 0 );

			if ( browser.Has_Matches() && request.JoinFirstAvailable )
			{
				ELobbyID lobby_id = browser.WatchedLobbies.First();
				Stop_Browsing( player_id );
				CServerMessageRouter.Send_Message_To_Player( new CStartBrowseLobbyResponse( request.RequestID, EStartBrowseResult.AutoJoined ), player_id );
				CServerLobbyManager.Instance.Join_Lobby_By_Browsing( player_id, lobby_id );
				return;
			}

			// send initial lobby set
			CStartBrowseLobbyResponse response = new CStartBrowseLobbyResponse( request.RequestID, EStartBrowseResult.Success );
			browser.WatchedLobbies.Apply( lobby_id => response.Add_Summary( Build_Summary( lobby_id ) ) );

			CServerMessageRouter.Send_Message_To_Player( response, player_id );
		}

		[NetworkMessageHandler]
		private void Handle_End_Browse_Lobby_Request( CEndBrowseLobbyMessage message, EPersistenceID player_id )
		{
			Stop_Browsing( player_id );
		}

		[NetworkMessageHandler]
		private void Handle_Browse_Next_Lobby_Set_Request( CBrowseNextLobbySetRequest request, EPersistenceID player_id )
		{
			if ( !Is_Browsing( player_id ) )
			{
				CServerMessageRouter.Send_Message_To_Player( new CCursorBrowseLobbyResponse( request.RequestID, ECursorBrowseResult.Not_Browsing ), player_id );
				return;
			}

			CServerLobbyBrowser browser = Get_Browser( player_id );
			if ( browser.WatchedLobbyCount < MAX_WATCHED_LOBBIES )
			{
				CServerMessageRouter.Send_Message_To_Player( new CCursorBrowseLobbyResponse( request.RequestID, ECursorBrowseResult.End_Of_Cursor ), player_id );
				return;
			}

			ELobbyID last_watched_lobby_id = browser.LastLobby;
			int last_index = Get_Sorted_Lobby_Index( last_watched_lobby_id );

			bool new_lobbies = Browse_Forwards( browser, last_index + 1 );
			if ( !new_lobbies )
			{
				CServerMessageRouter.Send_Message_To_Player( new CCursorBrowseLobbyResponse( request.RequestID, ECursorBrowseResult.End_Of_Cursor ), player_id );
				return;
			}

			CCursorBrowseLobbyResponse response = new CCursorBrowseLobbyResponse( request.RequestID, ECursorBrowseResult.Success );
			browser.WatchedLobbies.Apply( lobby_id => response.Add_Summary( Build_Summary( lobby_id ) ) );

			CServerMessageRouter.Send_Message_To_Player( response, player_id );
		}

		[NetworkMessageHandler]
		private void Handle_Browse_Previous_Lobby_Set_Request( CBrowsePreviousLobbySetRequest request, EPersistenceID player_id )
		{
			if ( !Is_Browsing( player_id ) )
			{
				CServerMessageRouter.Send_Message_To_Player( new CCursorBrowseLobbyResponse( request.RequestID, ECursorBrowseResult.Not_Browsing ), player_id );
				return;
			}

			CServerLobbyBrowser browser = Get_Browser( player_id );
			if ( browser.WatchedLobbyCount == 0 )
			{
				CServerMessageRouter.Send_Message_To_Player( new CCursorBrowseLobbyResponse( request.RequestID, ECursorBrowseResult.End_Of_Cursor ), player_id );
				return;
			}

			ELobbyID first_watched_lobby_id = browser.FirstLobby;
			int first_index = Get_Sorted_Lobby_Index( first_watched_lobby_id );

			bool new_lobbies = Browse_Backwards( browser, first_index );
			if ( !new_lobbies )
			{
				CServerMessageRouter.Send_Message_To_Player( new CCursorBrowseLobbyResponse( request.RequestID, ECursorBrowseResult.End_Of_Cursor ), player_id );
				return;
			}

			CCursorBrowseLobbyResponse response = new CCursorBrowseLobbyResponse( request.RequestID, ECursorBrowseResult.Success );
			browser.WatchedLobbies.Apply( lobby_id => response.Add_Summary( Build_Summary( lobby_id ) ) );

			CServerMessageRouter.Send_Message_To_Player( response, player_id );
		}

		private void Notify_Browsers_Of_New_Lobby_Internal( ELobbyID lobby_id )
		{
			CServerLobby lobby = CServerLobbyManager.Instance.Get_Lobby( lobby_id );
			if ( lobby == null )
			{
				return;
			}

			CLobbySummary summary = null;

			m_SortedLobbies.Add( lobby_id );
			m_LobbyWatchers.Add( lobby_id, new HashSet< EPersistenceID >() );

			List< EPersistenceID > browser_set = Clone_Browser_Set();

			// Eval non-full browsers
			foreach ( var browser_id in browser_set )
			{
				CServerLobbyBrowser browser = Get_Browser( browser_id );

				if ( browser.WatchedLobbyCount >= MAX_WATCHED_LOBBIES )
				{
					continue;
				}

				if ( !lobby.Matches_Browse_Criteria( browser.BrowseCriteria ) )
				{
					continue;
				}

				if ( lobby.Check_Join( browser.PlayerID ) != EJoinLobbyFailureReason.None )
				{
					continue;
				}

				if ( browser.JoinFirstAvailable )
				{
					CServerLobbyManager.Instance.Join_Lobby_By_Browsing( browser_id, lobby_id );
					continue;
				}

				Add_As_Watcher( lobby_id, browser.PlayerID );

				if ( summary == null )
				{
					summary = lobby.Create_Summary();
				}

				CServerMessageRouter.Send_Message_To_Player( new CBrowseLobbyAddRemoveMessage( summary, ELobbyID.Invalid ), browser_id );
			}
		}

		private void Notify_Browsers_Of_Removed_Lobby_Internal( ELobbyID lobby_id )
		{
			int previous_start_index = Get_Sorted_Lobby_Index( lobby_id );

			m_SortedLobbies.Remove( lobby_id );

			HashSet< EPersistenceID > watchers = Get_Watchers( lobby_id );
			foreach ( var watcher_id in watchers )
			{
				CServerLobbyBrowser browser = Get_Browser( watcher_id );
				browser.Remove_Watched_Lobby( lobby_id );

				Find_Next_Lobby_For( browser, lobby_id, previous_start_index );
			}

			m_LobbyWatchers.Remove( lobby_id );
		}

		private void Notify_Browsers_Of_Full_Lobby_Internal( ELobbyID lobby_id )
		{
			CServerLobby lobby = CServerLobbyManager.Instance.Get_Lobby( lobby_id );
			if ( lobby == null )
			{
				return;
			}

			HashSet< EPersistenceID > watcher_set = Get_Watchers( lobby_id );
			List< EPersistenceID > watcher_list = new List< EPersistenceID >();
			watcher_set.ShallowCopy( watcher_list );

			int previous_start_index = Get_Sorted_Lobby_Index( lobby_id );

			foreach ( var watcher_id in watcher_list )
			{
				CServerLobbyBrowser browser = Get_Browser( watcher_id );
				if ( !lobby.Matches_Browse_Criteria( browser.BrowseCriteria ) )
				{
					Remove_As_Watcher( lobby_id, watcher_id );
					Find_Next_Lobby_For( browser, lobby_id, previous_start_index );
				}
			}
		}

		private void Notify_Browsers_Of_Non_Full_Lobby_Internal( ELobbyID lobby_id )
		{
			CServerLobby lobby = CServerLobbyManager.Instance.Get_Lobby( lobby_id );
			if ( lobby == null )
			{
				return;
			}

			List< EPersistenceID > browser_set = Clone_Browser_Set();
			browser_set.Apply( bid => Notify_Browser_Of_Potential_Lobby( Get_Browser( bid ), lobby ) );
		}

		private void Notify_Browser_Of_Lobby_Ban_Or_Ignore_Internal( EPersistenceID player_id, ELobbyID lobby_id )
		{
			CServerLobbyBrowser browser = Get_Browser( player_id );
			if ( browser == null )
			{
				return;
			}

			if ( !browser.Is_Watching( lobby_id ) )
			{
				return;
			}

			Remove_As_Watcher( lobby_id, player_id );
			Find_Next_Lobby_For( browser, lobby_id, Get_Sorted_Lobby_Index( lobby_id ) );
		}

		private void Notify_Browser_Of_Lobby_Unban_Or_Unignore_Internal( EPersistenceID player_id, ELobbyID lobby_id )
		{
			CServerLobbyBrowser browser = Get_Browser( player_id );
			if ( browser == null )
			{
				return;
			}

			CServerLobby lobby = CServerLobbyManager.Instance.Get_Lobby( lobby_id );
			if ( lobby == null )
			{
				return;
			}

			Notify_Browser_Of_Potential_Lobby( browser, lobby );
		}

		private List< EPersistenceID > Clone_Browser_Set()
		{
			List< EPersistenceID > browser_set = new List< EPersistenceID >();
			m_Browsers.Apply( browser_pair => browser_set.Add( browser_pair.Key ) );

			return browser_set;
		}

		private CLobbySummary Build_Summary( ELobbyID lobby_id )
		{
			CServerLobby lobby = CServerLobbyManager.Instance.Get_Lobby( lobby_id );
			return lobby.Create_Summary();
		}

		private void Browse_Lobby_List( CServerLobbyBrowser browser, List< ELobbyID > lobby_list )
		{
			Clear_Watched_Lobbies( browser.PlayerID );
			lobby_list.Apply( lobby_id => Add_As_Watcher( lobby_id, browser.PlayerID ) );
		}

		private bool Browse_Forwards( CServerLobbyBrowser browser, int starting_index )
		{
			List< ELobbyID > lobby_list = new List< ELobbyID >();
			Build_Forwards_Set_Aux( browser, starting_index, lobby_list );

			if ( lobby_list.Count > 0 )
			{
				Browse_Lobby_List( browser, lobby_list );
			}

			return lobby_list.Count > 0;
		}

		private void Build_Forwards_Set_Aux( CServerLobbyBrowser browser, int starting_index, List< ELobbyID > lobby_list )
		{
			for ( int i = starting_index; i < m_SortedLobbies.Count; i++ )
			{
				ELobbyID lobby_id = m_SortedLobbies[ i ];
				CServerLobby lobby = CServerLobbyManager.Instance.Get_Lobby( lobby_id );

				if ( !lobby.Matches_Browse_Criteria( browser.BrowseCriteria ) )
				{
					continue;
				}

				if ( lobby.Check_Join( browser.PlayerID ) != EJoinLobbyFailureReason.None )
				{
					continue;
				}

				lobby_list.Add( lobby_id );
				if ( lobby_list.Count >= MAX_WATCHED_LOBBIES )
				{
					break;
				}
			}
		}

		private bool Browse_Backwards( CServerLobbyBrowser browser, int previous_starting_index )
		{
			List< ELobbyID > lobby_list = new List< ELobbyID >();
			Build_Backwards_Set_Aux( browser, previous_starting_index, lobby_list );

			if ( lobby_list.Count > 0 )
			{
				Browse_Lobby_List( browser, lobby_list );
			}

			return lobby_list.Count > 0;
		}

		private void Build_Backwards_Set_Aux( CServerLobbyBrowser browser, int previous_starting_index, List< ELobbyID > lobby_list )
		{
			for ( int i = previous_starting_index - 1; i >= 0; i-- )
			{
				ELobbyID lobby_id = m_SortedLobbies[ i ];
				CServerLobby lobby = CServerLobbyManager.Instance.Get_Lobby( lobby_id );

				if ( !lobby.Matches_Browse_Criteria( browser.BrowseCriteria ) )
				{
					continue;
				}

				if ( lobby.Check_Join( browser.PlayerID ) != EJoinLobbyFailureReason.None )
				{
					continue;
				}

				lobby_list.Add( lobby_id );
				if ( lobby_list.Count >= MAX_WATCHED_LOBBIES )
				{
					break;
				}
			}

			lobby_list.Reverse();

			if ( lobby_list.Count < MAX_WATCHED_LOBBIES )
			{
				Build_Forwards_Set_Aux( browser, previous_starting_index, lobby_list );
			}
		}

		private CServerLobbyBrowser Get_Browser( EPersistenceID player_id )
		{
			CServerLobbyBrowser browser = null;
			m_Browsers.TryGetValue( player_id, out browser );

			return browser;
		}

		private int Get_Sorted_Lobby_Index( ELobbyID lobby_id )
		{
			int lobby_count = m_SortedLobbies.Count;
			if ( lobby_count == 0 )
			{
				return -1;
			}

			int low_index = 0;
			int high_index = lobby_count - 1;
			int middle_index = ( low_index + high_index ) / 2;

			while ( high_index >= 0 && low_index < lobby_count )
			{
				ELobbyID checked_id = m_SortedLobbies[ middle_index ];
				if ( checked_id == lobby_id )
				{
					return middle_index;
				}
				else if ( checked_id < lobby_id )
				{
					low_index = middle_index + 1;
				}
				else
				{
					high_index = middle_index - 1;
				}

				int new_middle_index = ( low_index + high_index ) / 2;
				if ( new_middle_index == middle_index )
				{
					return -1;
				}

				middle_index = new_middle_index;
			}

			return -1;
		}

		private ELobbyID Find_New_Lobby( CServerLobbyBrowser browser, int starting_index )
		{
			for ( int i = starting_index; i < m_SortedLobbies.Count; i++ )
			{
				ELobbyID lobby_id = m_SortedLobbies[ i ];
				CServerLobby lobby = CServerLobbyManager.Instance.Get_Lobby( lobby_id );

				if ( !lobby.Matches_Browse_Criteria( browser.BrowseCriteria ) )
				{
					continue;
				}

				if ( lobby.Check_Join( browser.PlayerID ) != EJoinLobbyFailureReason.None )
				{
					continue;
				}

				return lobby_id;
			}

			return ELobbyID.Invalid;
		}

		private void Notify_Browser_Of_Potential_Lobby( CServerLobbyBrowser browser, CServerLobby lobby )
		{
			EPersistenceID browser_id = browser.PlayerID;
			ELobbyID lobby_id = lobby.ID;

			if ( browser.WatchedLobbyCount == MAX_WATCHED_LOBBIES && browser.FirstLobby > lobby_id )
			{
				return;
			}

			if ( browser.Is_Watching ( lobby_id ) )
			{
				return;
			}

			if ( browser.WatchedLobbyCount == MAX_WATCHED_LOBBIES && browser.LastLobby < lobby_id )
			{
				return;
			}

			if ( lobby.Matches_Browse_Criteria( browser.BrowseCriteria ) )
			{

				if ( browser.JoinFirstAvailable )
				{
					CServerLobbyManager.Instance.Join_Lobby_By_Browsing( browser_id, lobby_id );
					return;
				}

				ELobbyID removed_lobby = ELobbyID.Invalid;
				if ( browser.WatchedLobbyCount == MAX_WATCHED_LOBBIES )
				{
					removed_lobby = browser.LastLobby;
					Remove_As_Watcher( removed_lobby, browser_id );
				}

				Insert_As_Watcher( lobby_id, browser_id );

				CServerMessageRouter.Send_Message_To_Player( new CBrowseLobbyAddRemoveMessage( lobby.Create_Summary(), removed_lobby ), browser_id );
			}
		}

		private void Find_Next_Lobby_For( CServerLobbyBrowser browser, ELobbyID removed_lobby, int previous_starting_index )
		{
			EPersistenceID player_id = browser.PlayerID;
			CBrowseLobbyAddRemoveMessage add_remove_message = new CBrowseLobbyAddRemoveMessage( null, removed_lobby );

			if ( browser.WatchedLobbyCount > 0 )
			{
				int new_lobby_index = Get_Sorted_Lobby_Index( browser.LastLobby ) + 1;
				ELobbyID next_match = Find_New_Lobby( browser, new_lobby_index );

				if ( next_match != ELobbyID.Invalid )
				{
					CServerLobby new_lobby = CServerLobbyManager.Instance.Get_Lobby( next_match );
					add_remove_message.Add_Summary( new_lobby.Create_Summary() );
					Add_As_Watcher( next_match, player_id );
				}
			}
			else if ( previous_starting_index > 0 )
			{
				// we just lost the last lobby in our visible set; try browsing backwards from the lost lobby in order to pick up some more
				List< ELobbyID > lobby_list = new List< ELobbyID >();
				Build_Backwards_Set_Aux( browser, previous_starting_index, lobby_list );

				if ( lobby_list.Count > 0 )
				{
					Browse_Lobby_List( browser, lobby_list );
				}

				lobby_list.Apply( id => add_remove_message.Add_Summary( CServerLobbyManager.Instance.Get_Lobby( id ).Create_Summary() ) );
			}

			CServerMessageRouter.Send_Message_To_Player( add_remove_message, player_id );
		}

		private void Add_As_Watcher( ELobbyID lobby_id, EPersistenceID player_id )
		{
			HashSet< EPersistenceID > watchers = Get_Watchers( lobby_id );
			watchers.Add( player_id );

			CServerLobbyBrowser browser = Get_Browser( player_id );
			browser.Add_Watched_Lobby( lobby_id );
		}

		private void Insert_As_Watcher( ELobbyID lobby_id, EPersistenceID player_id )
		{
			HashSet< EPersistenceID > watchers = Get_Watchers( lobby_id );
			watchers.Add( player_id );

			CServerLobbyBrowser browser = Get_Browser( player_id );
			browser.Insert_Watched_Lobby( lobby_id );
		}

		private void Remove_As_Watcher( ELobbyID lobby_id, EPersistenceID player_id )
		{
			HashSet< EPersistenceID > watchers = Get_Watchers( lobby_id );
			watchers.Remove( player_id );

			CServerLobbyBrowser browser = Get_Browser( player_id );
			browser.Remove_Watched_Lobby( lobby_id );
		}

		private HashSet< EPersistenceID > Get_Watchers( ELobbyID lobby_id )
		{
			HashSet< EPersistenceID > watchers = null;
			m_LobbyWatchers.TryGetValue( lobby_id, out watchers );

			return watchers;
		}

		private void Clear_Watched_Lobbies( EPersistenceID player_id )
		{
			CServerLobbyBrowser browser = Get_Browser( player_id );
			if ( browser == null )
			{
				return;
			}

			browser.WatchedLobbies.Apply( id => Get_Watchers( id ).Remove( player_id ) );
			browser.Clear_Watched_Lobbies();
		}

		// Properties
		public static CServerLobbyBrowserManager Instance { get { return m_Instance; } }

		// Fields
		private static CServerLobbyBrowserManager m_Instance = new CServerLobbyBrowserManager();

		private Dictionary< EPersistenceID, CServerLobbyBrowser > m_Browsers = new Dictionary< EPersistenceID, CServerLobbyBrowser >();
		private List< ELobbyID > m_SortedLobbies = new List< ELobbyID >();

		private Dictionary< ELobbyID, HashSet< EPersistenceID > > m_LobbyWatchers = new Dictionary< ELobbyID, HashSet< EPersistenceID > >();

		private const int MAX_WATCHED_LOBBIES = 2;
	}
}