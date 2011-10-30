/*

	LobbyBrowsing.cs

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

namespace CRShared
{
	public class CLobbySummary
	{
		// constructors
		public CLobbySummary() {}

		// Methods
		// Public interface
		public void Initialize( ELobbyID id, CLobbyConfig config, DateTime creation_time, EPersistenceID creator, uint player_count, uint observer_count )
		{
			Config = config.Clone( false );
			
			ID = id;
			CreationTime = creation_time;
			Creator = creator;
			PlayerCount = player_count;
			ObserverCount = observer_count;
		}

		public void Apply_Delta( CLobbySummaryDelta delta )
		{
			PlayerCount = delta.PlayerCount;
			ObserverCount = delta.ObserverCount;
		}

		public override string ToString()
		{
			return String.Format( "( [CLobbySummary] ID = {0}, Config = {1}, CreationTime = {2}, Creator = {3}, PlayerCount = {4}, ObserverCount = {5})",
										 ID,
										 Config.ToString(),
										 CreationTime.ToShortTimeString(),
										 (int)Creator,
										 PlayerCount,
										 ObserverCount );
		}

		// Properties
		public ELobbyID ID { get; private set; }

		private CLobbyConfig Config { get; set; }
		public string GameDescription { get { return Config.GameDescription; } }
		public EGameModeType GameMode { get { return Config.GameMode; } }
		public bool AllowObservers { get { return Config.AllowObservers; } }

		public DateTime CreationTime { get; private set; }
		public EPersistenceID Creator { get; private set; }

		public uint PlayerCount { get; private set; }
		public uint ObserverCount { get; private set; }
	}

	public class CLobbySummaryDelta
	{
		// constructors
		public CLobbySummaryDelta() {}

		// Methods
		// Public interface
		public void Initialize( ELobbyID id, uint player_count, uint observer_count )
		{
			ID = id;
			PlayerCount = player_count;
			ObserverCount = observer_count;
		}

		public override string ToString()
		{
			return String.Format( "( [CLobbySummaryDelta] ID = {0}, PlayerCount = {1}, ObserverCount = {2} )", (int)ID, PlayerCount, ObserverCount );
		}

		// Properties
		public ELobbyID ID { get; private set; }
		public uint PlayerCount { get; private set; }
		public uint ObserverCount { get; private set; }
	}

	public class CBrowseLobbyMatchCriteria
	{
		// Construction
		public CBrowseLobbyMatchCriteria( EGameModeType game_mode_mask, ELobbyMemberType lobby_member_type_mask )
		{
			GameModeMask = game_mode_mask;
			LobbyMemberTypeMask = lobby_member_type_mask;
		}

		// Methods
		// Public interface
		public override string ToString()
		{
			return String.Format( "( [CBrowseLobbyMatchCriteria] GameModeMask={0}, LobbyMemberTypeMask={1} )", GameModeMask.ToString(), LobbyMemberTypeMask.ToString() );
		}

		// Properties
		public EGameModeType GameModeMask { get; private set; }
		public ELobbyMemberType LobbyMemberTypeMask { get; private set; }
	}
}
