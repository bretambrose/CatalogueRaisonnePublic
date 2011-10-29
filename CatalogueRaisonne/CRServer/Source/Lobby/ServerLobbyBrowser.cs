using System;
using System.Collections.Generic;

using CRShared;

namespace CRServer
{
	public class CServerLobbyBrowser
	{
		// Construction
		public CServerLobbyBrowser( EPersistenceID player_id, CBrowseLobbyMatchCriteria browse_criteria, bool join_first_available )
		{
			PlayerID = player_id;

			WatchStartInterval = ELobbyID.Invalid;
			WatchEndInterval = ELobbyID.Invalid;

			BrowseCriteria = browse_criteria;

			JoinFirstAvailable = join_first_available;
		}

		// Methods
		// Public interface
		public void Clear_Watched_Lobbies()
		{
			m_WatchedLobbies.Clear();
		}

		public bool Has_Matches() 
		{ 
			return m_WatchedLobbies.Count > 0; 
		}

		public void Add_Watched_Lobby( ELobbyID lobby_id )
		{
			m_WatchedLobbies.Add( lobby_id );
		}

		public void Insert_Watched_Lobby( ELobbyID lobby_id )
		{
			if ( m_WatchedLobbies.Count == 0 )
			{
				m_WatchedLobbies.Add( lobby_id );
				return;
			}

			// dumb and linear
			int index = 0;
			while ( index < m_WatchedLobbies.Count && lobby_id > m_WatchedLobbies[ index ] )
			{
				index++;
			}

			m_WatchedLobbies.Insert( index, lobby_id );
		}

		public void Remove_Watched_Lobby( ELobbyID lobby_id )
		{
			m_WatchedLobbies.Remove( lobby_id );
		}

		public bool Is_Watching( ELobbyID lobby_id )
		{
			return m_WatchedLobbies.Contains( lobby_id );
		}

		// Properties
		public EPersistenceID PlayerID { get; private set; }
		public int WatchedLobbyCount { get { return m_WatchedLobbies.Count; } }

		public IEnumerable< ELobbyID > WatchedLobbies { get { return m_WatchedLobbies; } }
		public ELobbyID WatchStartInterval { get; private set; }
		public ELobbyID WatchEndInterval { get; private set; }
		public CBrowseLobbyMatchCriteria BrowseCriteria { get; private set; }
		public bool JoinFirstAvailable { get; private set; }

		public ELobbyID LastLobby { get { return m_WatchedLobbies[ m_WatchedLobbies.Count - 1 ]; } }
		public ELobbyID FirstLobby { get { return m_WatchedLobbies[ 0 ]; } }

		// Fields
		private List< ELobbyID > m_WatchedLobbies = new List< ELobbyID >();
	}
}