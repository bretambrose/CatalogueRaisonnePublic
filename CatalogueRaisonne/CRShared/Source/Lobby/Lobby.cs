using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using CRShared.Chat;

namespace CRShared
{
	public enum ELobbyID
	{
		Invalid = 0
	}

	[Flags]
	public enum ELobbyMemberType
	{
		Invalid = 0,

		Player		= 1 << 0,
		Observer		= 1 << 1
	}

	public enum ELobbyMemberState
	{
		Invalid = 0,

		Not_Ready,
		Ready,
		Disconnected
	}

	public class CLobbyConfig
	{
		// construction
		public CLobbyConfig()
		{
		}

		// Methods
		// Public interface
		public CLobbyConfig Clone( bool copy_password )
		{
			CLobbyConfig clone = new CLobbyConfig();

			clone.GameDescription = GameDescription;
			clone.GameMode = GameMode;
			clone.AllowObservers = AllowObservers;
			if ( copy_password )
			{
				clone.Password = Password;
			}		
			
			return clone;	
		}

		public void Initialize( string game_description, EGameModeType game_mode, bool allow_observers, string password )
		{
			GameDescription = game_description;
			GameMode = game_mode;
			AllowObservers = allow_observers;
			Password = password;
		}

		public override string ToString()
		{
			return String.Format( "( [CLobbyConfig] GameDescription = {0}, GameMode = {1}, AllowObservers = {2}, Password = {3} )",
										 GameDescription,
										 GameMode.ToString(),
										 AllowObservers.ToString(),
										 Password );
		}

		// Properties
		public string GameDescription { get; private set; }
		public EGameModeType GameMode { get; private set; }
		public bool AllowObservers { get; private set; }
		public string Password { get; private set; }

		public bool IsPublic { get { return Password == null || Password.Length == 0; } }
	}

	public class CLobbyMember : ISimpleClonable< CLobbyMember >
	{
		// constructors
		public CLobbyMember( EPersistenceID id )
		{
			ID = id;
			State = ELobbyMemberState.Not_Ready;
		}

		// Methods
		// Public interface
		public CLobbyMember Clone()
		{
			CLobbyMember clone = new CLobbyMember( ID );
			clone.State = State;

			return clone;
		}

		public override string ToString()
		{
			return String.Format( "( [CLobbyMember] ID = {0}, State = {1} )", (long)ID, State.ToString() );
		}

		// Properties
		public EPersistenceID ID { get; private set; }
		public ELobbyMemberState State { get; set; }
	}

	public class CLobbyState : ISimpleClonable< CLobbyState >
	{
		// constructors
		public CLobbyState( ELobbyID id, CLobbyConfig config, EPersistenceID creator )
		{
			ID = id;
			Creator = creator;
			Config = config;
			CreationTime = DateTime.Now;
			GameCount = 3;
		}

		private CLobbyState( CLobbyState source )
		{
			ID = source.ID;
			Creator = source.Creator;
			CreationTime = source.CreationTime;

			Config = source.Config.Clone( true );

			source.Members.SimpleClone( Members );
			source.PlayersBySlot.ShallowCopy( PlayersBySlot );
			source.Observers.ShallowCopy( Observers );
			source.BannedPlayers.ShallowCopy( BannedPlayers );
		}

		// methods
		// Public interface
		public override string ToString()
		{
			string member_list_string = CSharedUtils.Build_String_List( Members, n => String.Format( "{0}->{1}", (long)n.Key, n.Value.ToString() ) );
			string players_by_slot_string = CSharedUtils.Build_String_List( PlayersBySlot, n => String.Format( "{0}->{1}", n.Key, (long)n.Value ) );
			string observers_string = CSharedUtils.Build_String_List( Observers, n => String.Format( "{0}", (long)n ) );
			string banned_string = CSharedUtils.Build_String_List( BannedPlayers, n => String.Format( "{0}", (long)n ) );

			return String.Format( "( [CLobbyState] ID = {0}, Config = {1}, CreationTime = {2}, Creator = {3}, Members = ({4}), PlayersBySlot = ({5}), ObserversBySlot = ({6}), BannedPlayers = ({7}) )",
										 (int)ID,
										 Config.ToString(),
										 CreationTime.ToShortTimeString(),
										 (long)Creator,
										 member_list_string,
										 players_by_slot_string,
										 observers_string,
										 banned_string );
		}

		public CLobbyState Clone()
		{
			return new CLobbyState( this );
		}

		// Properties
		public ELobbyID ID { get; private set; }

		public CLobbyConfig Config { get; private set; }
		public string GameDescription { get { return Config.GameDescription; } }
		public EGameModeType GameMode { get { return Config.GameMode; } }
		public uint MaxPlayers { get { return CGameModeUtils.Player_Count_For_Game_Mode( GameMode ); } }
		public string Password { get { return Config.Password; } }
		public bool IsPublic { get { return Config.IsPublic; } }
		public bool AllowObservers { get { return Config.AllowObservers; } }
		
		public uint PlayerCount { get { return (uint) PlayersBySlot.Count; } }
		public uint ObserverCount { get { return (uint) Observers.Count; } }

		public EPersistenceID Creator { get; private set; }
		public DateTime CreationTime { get; internal set; }
		public uint GameCount { get; set; }

		public Dictionary< EPersistenceID, CLobbyMember > Members { get { return m_Members; } }
		public Dictionary< uint, EPersistenceID > PlayersBySlot { get { return m_PlayersBySlot; } }
		public HashSet< EPersistenceID > Observers { get { return m_Observers; } }
		public HashSet< EPersistenceID > BannedPlayers { get { return m_BannedPlayers; } }

		public IEnumerable< EPersistenceID > Players { get { return PlayersBySlot.Select( n => n.Value ); } }

		// Fields
		private Dictionary< EPersistenceID, CLobbyMember > m_Members = new Dictionary< EPersistenceID, CLobbyMember >();
		private Dictionary< uint, EPersistenceID > m_PlayersBySlot = new Dictionary< uint, EPersistenceID >();
		private HashSet< EPersistenceID > m_Observers = new HashSet< EPersistenceID >();
		private HashSet< EPersistenceID > m_BannedPlayers = new HashSet< EPersistenceID >();
	}	

	public class CLobby
	{
		// constructors
		protected CLobby( ELobbyID id, CLobbyConfig config, EPersistenceID creator )
		{
			LobbyState = new CLobbyState( id, config, creator );
		}

		protected CLobby( CLobbyState lobby_state )
		{
			LobbyState = lobby_state;
		}

		// Methods
		// Public interface
		public CLobbyState Clone_State()
		{
			return LobbyState.Clone();
		}

		public void Remove_Member( EPersistenceID player_id, bool send_browse_notices )
		{
			LobbyState.Members.Remove( player_id );
			
			bool were_players_full = LobbyState.PlayerCount == CGameModeUtils.Player_Count_For_Game_Mode( GameMode );

			var player_pair = LobbyState.PlayersBySlot.FirstOrDefault( n => n.Value == player_id );
			if ( player_pair.Value == player_id )
			{
				LobbyState.PlayersBySlot.Remove( player_pair.Key );
				Mark_Players_Not_Ready();
				if ( send_browse_notices && were_players_full )
				{
					On_Lobby_Full_To_Not_Full();
				}
			}

			LobbyState.Observers.FirstOrDefault( n => n == player_id );

			if ( send_browse_notices )
			{
				On_Lobby_Delta_Change();
			}
		}

		public void Move_Member( EPersistenceID player_id, ELobbyMemberType destination_type, uint destination_index )
		{
			uint old_index;
			ELobbyMemberType old_type;

			Get_Member_Info( player_id, out old_index, out old_type );

			bool were_players_full = LobbyState.PlayerCount == CGameModeUtils.Player_Count_For_Game_Mode( GameMode );

			Unmark_Member_Location( player_id, old_type, old_index );
			Mark_Member_Location( player_id, destination_type, destination_index );

			On_Lobby_Delta_Change();

			if ( were_players_full && LobbyState.PlayerCount < CGameModeUtils.Player_Count_For_Game_Mode( GameMode ) )
			{
				On_Lobby_Full_To_Not_Full();
			}
		}

		public void Swap_Members( EPersistenceID player_id1, EPersistenceID player_id2 )
		{
			uint old_index1;
			ELobbyMemberType old_type1;

			Get_Member_Info( player_id1, out old_index1, out old_type1 );
			Unmark_Member_Location( player_id1, old_type1, old_index1 );

			uint old_index2;
			ELobbyMemberType old_type2;

			Get_Member_Info( player_id2, out old_index2, out old_type2 );
			Unmark_Member_Location( player_id2, old_type2, old_index2 );

			Mark_Member_Location( player_id1, old_type2, old_index2 );
			Mark_Member_Location( player_id2, old_type1, old_index1 );
		}

		public void Get_Member_Info( EPersistenceID player_id, out uint member_index, out ELobbyMemberType member_type )
		{
			var player_pair = LobbyState.PlayersBySlot.FirstOrDefault( n => n.Value == player_id );
			if ( player_pair.Value != EPersistenceID.Invalid )
			{
				member_index = player_pair.Key;
				member_type = ELobbyMemberType.Player;
				return;
			}

			if ( LobbyState.Observers.Contains( player_id ) )
			{
				member_index = 0;
				member_type = ELobbyMemberType.Observer;
				return;
			}

			throw new CApplicationException( "Queried lobby member info for player who is not in the lobby" );
		}

		public ELobbyMemberState Get_Member_State( EPersistenceID player_id )
		{
			return LobbyState.Members[ player_id ].State;
		}

		public void Set_Member_State( EPersistenceID player_id, ELobbyMemberState new_state )
		{
			LobbyState.Members[ player_id ].State = new_state;
		}
		
		public bool Is_Member( EPersistenceID player_id )
		{
			return LobbyState.Members.ContainsKey( player_id );
		}

		public bool Is_Banned( EPersistenceID player_id )
		{
			return LobbyState.BannedPlayers.Contains( player_id );
		}

		public void Ban_Player( EPersistenceID player_id )
		{
			LobbyState.BannedPlayers.Add( player_id );
		}

		public void Unban_Player( EPersistenceID player_id )
		{
			LobbyState.BannedPlayers.Remove( player_id );
		}

		public EPersistenceID Get_Player_At( ELobbyMemberType member_type, uint index )
		{
			EPersistenceID player_id = EPersistenceID.Invalid;

			if ( member_type == ELobbyMemberType.Player )
			{
				LobbyState.PlayersBySlot.TryGetValue( index, out player_id );
			}

			return player_id;
		}

		public bool Are_All_Player_Slots_Filled()
		{
			return LobbyState.PlayerCount == CGameModeUtils.Player_Count_For_Game_Mode( GameMode );
		}

		public bool Are_All_Players_Ready()
		{
			return LobbyState.PlayersBySlot.Select( n => LobbyState.Members[ n.Value ].State == ELobbyMemberState.Ready ).Aggregate( true, ( n, ready ) => n && ready );
		}

		// Protected interface
		protected void Mark_Member_Location( EPersistenceID player_id, ELobbyMemberType member_type, uint index )
		{
			if ( member_type == ELobbyMemberType.Observer )
			{
				LobbyState.Observers.Add( player_id );
			}
			else if ( member_type == ELobbyMemberType.Player )
			{
				LobbyState.PlayersBySlot.Add( index, player_id );
				Mark_Players_Not_Ready();
			}
		}

		protected void Unmark_Member_Location( EPersistenceID player_id, ELobbyMemberType member_type, uint index )
		{
			if ( member_type == ELobbyMemberType.Observer )
			{
				LobbyState.Observers.Remove( player_id );
			}
			else if ( member_type == ELobbyMemberType.Player )
			{
				LobbyState.PlayersBySlot.Remove( index );
				Mark_Players_Not_Ready();
			}
		}

		protected void Mark_Players_Not_Ready()
		{
			LobbyState.Members.Where( n => n.Value.State == ELobbyMemberState.Ready ).Apply( n => n.Value.State = ELobbyMemberState.Not_Ready );
		}

		protected uint Get_Max_Player_Count()
		{
			return  CGameModeUtils.Player_Count_For_Game_Mode( LobbyState.GameMode );
		}

		protected virtual void On_Lobby_Full_To_Not_Full() {}
		protected virtual void On_Lobby_Delta_Change() {}

		// Properties
		public ELobbyID ID { get { return LobbyState.ID; } }

		public string GameDescription { get { return LobbyState.Config.GameDescription; } }
		public EGameModeType GameMode { get { return LobbyState.Config.GameMode; } }
		public string Password { get { return LobbyState.Config.Password; } }
		public bool AllowObservers { get { return LobbyState.Config.AllowObservers; } }

		public DateTime CreationTime { get { return LobbyState.CreationTime; } }
		public EPersistenceID Creator { get { return LobbyState.Creator; } }
		public bool IsPublic { get { return LobbyState.IsPublic; } }

		public IEnumerable< EPersistenceID > MemberIDs { get { return LobbyState.Members.Select( n => n.Key ); } }
		public IEnumerable< CLobbyMember > Members { get { return LobbyState.Members.Select( n => n.Value ); } }

		public IEnumerable< EPersistenceID > ConnectedMemberIDs { get { return LobbyState.Members.Where( n => n.Value.State != ELobbyMemberState.Disconnected ).Select( n => n.Key ); } }

		public CLobbyState LobbyState { get; private set; }
	}

}
