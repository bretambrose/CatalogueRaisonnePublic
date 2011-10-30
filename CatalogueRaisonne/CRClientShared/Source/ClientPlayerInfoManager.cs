/*

	ClientPlayerInfoManager.cs

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
using System.Diagnostics;

using CRShared;
using CRShared.Chat;

namespace CRClientShared
{
	public enum EPlayerListenReason
	{
		Self,
		In_Chat_Channel,
		In_Lobby,
		Banned_From_Lobby,
		Ignore_List,
		Friend_List,
		Browsed_Lobby_Creator,
		In_Match
	}

	public class CClientPlayerInfo : CPlayerInfo
	{
		// Construction
		public CClientPlayerInfo( EPersistenceID persistence_id ) :
			base( persistence_id )
		{
			Initialized = false;
		}

		public CClientPlayerInfo( EPersistenceID persistence_id, string player_name ) :
			base( persistence_id )
		{
			if ( player_name != null )
			{
				Name = player_name;
			}

			Initialized = false;
		}

		// Methods
		// Public interface
		public void Add_Listen_Ref( EPlayerListenReason reason )
		{
			if ( !m_ReasonCounts.ContainsKey( reason ) )
			{
				m_ReasonCounts[ reason ] = 1;
				return;
			}

			int current_value = m_ReasonCounts[ reason ];
			m_ReasonCounts[ reason ] = current_value + 1;
		}

		public void Remove_Listen_Ref( EPlayerListenReason reason )
		{
			if ( !m_ReasonCounts.ContainsKey( reason ) )
			{
				throw new CApplicationException( "Unbalanced player reference count" );
			}

			int current_value = m_ReasonCounts[ reason ];
			if ( current_value < 1 )
			{
				throw new CApplicationException( "Unbalanced player reference count" );
			}

			if ( current_value == 1 )
			{
				m_ReasonCounts.Remove( reason );
			}
			else
			{
				m_ReasonCounts[ reason ] = current_value - 1;
			}
		}

		public bool Is_Listening_Due_To( EPlayerListenReason reason )
		{
			return m_ReasonCounts.ContainsKey( reason );
		}

		public bool Has_References()
		{
			return m_ReasonCounts.Count > 0;
		}

		public void On_Player_Info( CPlayerInfo info )
		{
			Name = info.Name;
			Initialized = true;
		}

		// Properties
		public bool Initialized { get; private set; }
		public bool InLobby { get { return Is_Listening_Due_To( EPlayerListenReason.In_Lobby ); } }
		public bool IsSelf { get { return Is_Listening_Due_To( EPlayerListenReason.Self ); } }

		// Fields
		private Dictionary< EPlayerListenReason, int > m_ReasonCounts = new Dictionary< EPlayerListenReason, int >();
	}

	public class CClientPlayerInfoManager
	{
		// Subclasses
		private class CQueryPlayerInfoTask : CRecurrentTask
		{
			public CQueryPlayerInfoTask() :
				base( 0L, QUERY_INTERVAL )
			{
			}

			public override void Execute( long current_time )
			{
				CClientPlayerInfoManager.Instance.Service_Query();
			}

			private const long QUERY_INTERVAL = 100L;
		}

		// Construction
		CClientPlayerInfoManager() {}
		static CClientPlayerInfoManager () {}

		// Methods
		// Public interface
		public void Initialize()
		{
			CClientLogicalThread.Instance.TaskScheduler.Add_Scheduled_Task( new CQueryPlayerInfoTask() );
		}

		public CClientPlayerInfo Get_Player_Info( EPersistenceID id )
		{
			CClientPlayerInfo player_info = null;
			m_Players.TryGetValue( id, out player_info );
			return player_info;
		}

		public void Begin_Player_Listen( EPersistenceID player_id, EPlayerListenReason reason )
		{
			Begin_Player_Listen( player_id, null, reason );
		}

		public void Begin_Player_Listen( EPersistenceID player_id, string player_name, EPlayerListenReason reason )
		{
			CClientPlayerInfo player_info = Get_Player_Info( player_id );
			if ( player_info == null )
			{
				player_info = new CClientPlayerInfo( player_id );
				m_Players.Add( player_id, player_info );
				Query_Player( player_id );
			}

			if ( player_name != null && player_info.Name == null )
			{
				player_info.Name = player_name;
			}

			player_info.Add_Listen_Ref( reason );
		}

		public void End_Player_Listen( EPersistenceID player_id, EPlayerListenReason reason )
		{
			CClientPlayerInfo player_info = Get_Player_Info( player_id );
			Debug.Assert( player_info != null );

			player_info.Remove_Listen_Ref( reason );
			if ( !player_info.Has_References() )
			{
				Remove_Player( player_id );
			}
		}

		public void Reset()
		{
			m_Players.Clear();
			m_QueryPlayers.Clear();
		}

		public string Get_Player_Name( EPersistenceID id )
		{
			CPlayerInfo player_info = Get_Player_Info( id );
			if ( player_info != null && player_info.Name != null )
			{
				return player_info.Name;
			}
			else
			{
				return CSharedResource.Get_Text< ESharedTextID >( ESharedTextID.Shared_Unknown_Player, id );
			}
		}

		public EPersistenceID Get_Player_ID_By_Name( string player_name )
		{
			foreach ( var player_info in m_Players )
			{
				if ( String.Compare( player_info.Value.Name, player_name, true ) == 0 )
				{
					return player_info.Key;
				}
			}

			return EPersistenceID.Invalid;
		}

		// Private interface
		// Network Message handlers
		[NetworkMessageHandler]
		private void On_Query_Player_Info_Response( CQueryPlayerInfoResponse response )
		{
			foreach ( var response_info in response.Infos )
			{
				CClientPlayerInfo player_info = Get_Player_Info( response_info.PersistenceID );
				if ( player_info != null )
				{
					player_info.On_Player_Info( response_info );
				}
			}
		}

		private void Query_Player( EPersistenceID player_id )
		{
			m_QueryPlayers.Add( player_id );
		}

		private void Service_Query()
		{
			if ( m_QueryPlayers.Count == 0 )
			{
				return;
			}

			CQueryPlayerInfoRequest request = new CQueryPlayerInfoRequest();
			m_QueryPlayers.Apply( id => request.Add_Player( id ) );

			CClientLogicalThread.Instance.Send_Message( request, ESessionID.First );
			m_QueryPlayers.Clear();
		}

		private void Remove_Player( EPersistenceID id )
		{
			m_Players.Remove( id );
		}

		// Properties
		public static CClientPlayerInfoManager Instance { get { return m_Instance; } }

		// Fields
		private static CClientPlayerInfoManager m_Instance = new CClientPlayerInfoManager();

		private Dictionary< EPersistenceID, CClientPlayerInfo > m_Players = new Dictionary< EPersistenceID, CClientPlayerInfo >();
		private HashSet< EPersistenceID > m_QueryPlayers = new HashSet<EPersistenceID>();
	}
}