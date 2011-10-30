/*

	ServerLobbyBrowser.cs

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