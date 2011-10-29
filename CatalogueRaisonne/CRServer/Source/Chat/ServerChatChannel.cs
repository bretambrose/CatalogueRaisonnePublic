using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;

using CRShared;
using CRShared.Chat;

namespace CRServer.Chat
{

	[Flags]
	public enum EChannelModeratorPermissions
	{
		None						= 0,

		Kick						= 1 << 0,
		TransferModerator		= 1 << 1,
		Ban						= 1 << 2,
		Gag						= 1 << 3
	}

	[Flags]
	public enum EChannelGameProperties
	{
		None							= 0,

		// Flags
		User							= 1 << 0,
		General						= 1 << 1,
		Lobby							= 1 << 2,
		MatchPlayer					= 1 << 3,
		MatchObserver				= 1 << 4,

		// Masks
		OrdinarySingletonMask	= 30,		//	( General | Lobby | MatchPlayer | MatchObserver )
		AdminSingletonMask		= 6		//	( General | Lobby )
	}
		
	public class CChatChannelConfig
	{
		// Construction
		public CChatChannelConfig( string internal_name, string external_name, EPersistenceID creator_id, EChannelGameProperties properties ) 
		{
			InternalName = internal_name;
			ExternalName = external_name;
			CreatorID = creator_id;
			GameProperties = properties;

			ModeratorPermissions = EChannelModeratorPermissions.None;
			AllowsModeration = false;
			MaxMembers = 0;
			AnnounceJoinLeave = false;
			DestroyWhenEmpty = false;
			IsMembershipClientLocked = false;
		}
		
		// Methods
		// Public interface
		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();

			return String.Format( "( [CChatChannelConfig] InternalName={0}, ExternalName={1}, CreatorID={2}, ModeratorPermissions={3}, GameProperties={4}, AllowsModeration={5}, " +
										 "MaxMembers={6}, AnnounceJoinLeave={7}, DestroyWhenEmpty={8}, IsMembershipClientLocked={9}",
										 InternalName,
										 ExternalName,
										 (long)CreatorID,
										 ModeratorPermissions.ToString(),
										 GameProperties.ToString(),
										 AllowsModeration,
										 MaxMembers,
										 AnnounceJoinLeave,
										 DestroyWhenEmpty,
										 IsMembershipClientLocked );
		}

		public EChannelCreationError Make_Valid()
		{			
			if ( CreatorID != EPersistenceID.Invalid )
			{
				// The external name must be a single valid word
				Match external_match = Regex.Match( ExternalName, @"\w+" );
				if ( external_match.Length != ExternalName.Length )
				{
					return EChannelCreationError.InvalidExternalName;
				}
			}
			else
			{
				// Server channels all begin with the '#' sign
				if ( InternalName[ 0 ] != SERVER_CHANNEL_PREFIX )
				{
					InternalName = SERVER_CHANNEL_PREFIX + InternalName;
				}	
			}
			
			return EChannelCreationError.None;
		}
		
		public static string Make_Admin_Channel( string channel_name )
		{
			return SERVER_CHANNEL_PREFIX + channel_name;
		}

		// Properties
		public string InternalName { get; private set; }
		public string ExternalName { get; private set; }
		public EPersistenceID CreatorID { get; private set; }
		public EChannelModeratorPermissions ModeratorPermissions { get; set; }
		public EChannelGameProperties GameProperties { get; set;  }
		public bool AllowsModeration { get; set; }
		public int MaxMembers { get; set; }
		public bool AnnounceJoinLeave { get; set; }
		public bool DestroyWhenEmpty { get; set; }
		public bool IsMembershipClientLocked { get; set; }

		public bool IsServerChannel { get { return InternalName[ 0 ] == SERVER_CHANNEL_PREFIX; } }
		
		public const char SERVER_CHANNEL_PREFIX = '#';
	}
	
	public class CServerChatChannel
	{
		// Construction
		public CServerChatChannel( EChannelID id, CChatChannelConfig config ) 
		{
			ID = id;
			m_Config = config;
			IsBeingDestroyed = false;
		}
		
		// Methods
		// Public interface
		public EChannelJoinError Join_Channel( EPersistenceID persistence_id )
		{
			if ( m_Members.Contains( persistence_id ) )
			{
				return EChannelJoinError.AlreadyJoined;
			}
			
			if ( Is_Banned( persistence_id ) )
			{
				return EChannelJoinError.Banned;
			}
			
			if ( m_Config.MaxMembers > 0 && MemberCount >= m_Config.MaxMembers )
			{
				return EChannelJoinError.ChannelFull;
			}
			
			m_Members.Add( persistence_id );
			
			return EChannelJoinError.None;
		}
		
		public bool Leave_Channel( EPersistenceID persistence_id )
		{
			if ( !m_Members.Contains( persistence_id ) )
			{
				return false;
			}
			
			m_Members.Remove( persistence_id );

			return true;
		}
		
		public bool Update_Moderator()
		{
			if ( !m_Config.AllowsModeration )
			{
				return false;
			}

			if ( Moderator != EPersistenceID.Invalid && Is_Member( Moderator ) )
			{
				return false;
			}

			if ( MemberCount == 0 )
			{
				// This may be a change, but there's no one to tell it to
				Moderator = EPersistenceID.Invalid;
				return false;
			}
			
			foreach ( var member in m_Members )
			{
				if ( !m_Gagged.Contains( member ) )
				{
					Moderator = member;
					return true;
				}
			}

			// Special case when only gagged members remain; in this case go unmoderated until a non-gagged enters
			Moderator = EPersistenceID.Invalid;
			return true;
		}

		public bool Transfer_Moderator( EPersistenceID new_moderator_id )
		{
			if ( !m_Config.AllowsModeration )
			{
				return false;
			}

			if ( !Is_Member( new_moderator_id ) || Is_Gagged( new_moderator_id ) )
			{
				return false;
			}

			Moderator = new_moderator_id;
			return true;
		}

		public bool Gag_Player( EPersistenceID gag_target_id )
		{
			if ( !m_Config.AllowsModeration )
			{
				return false;
			}

			if ( !Is_Member( gag_target_id ) || Is_Gagged( gag_target_id ) )
			{
				return false;
			}
			
			m_Gagged.Add( gag_target_id );
			return true;
		}

		public bool Ungag_Player( EPersistenceID ungag_target_id )
		{
			if ( !m_Config.AllowsModeration )
			{
				return false;
			}

			if ( !Is_Member( ungag_target_id ) || !Is_Gagged( ungag_target_id ) )
			{
				return false;
			}
			
			m_Gagged.Remove( ungag_target_id );
			return true;
		}
		
		public bool Is_Gagged( EPersistenceID persistence_id )
		{
			return m_Gagged.Contains( persistence_id );
		}

		public bool Is_Banned( EPersistenceID persistence_id )
		{
			return m_Banned.Contains( persistence_id );
		}

		public EChannelBroadcastError Check_Broadcast_Status( EPersistenceID player_id )
		{
			if ( !Is_Member( player_id ) )
			{
				return EChannelBroadcastError.NotInChannel;
			}

			if ( Is_Gagged( player_id ) )
			{
				return EChannelBroadcastError.Gagged;
			}

			return EChannelBroadcastError.None;
		}

		public bool Is_Member( EPersistenceID player_id )
		{
			return m_Members.Contains( player_id );
		}
		
		static public bool Is_Server_Channel_Name( string channel_name )
		{
			return channel_name.Length > 0 && channel_name[ 0 ] == CChatChannelConfig.SERVER_CHANNEL_PREFIX && Is_Valid_Channel_Name( channel_name );
		}
		
		static public bool Is_Client_Channel_Name( string channel_name )
		{
			return channel_name.Length > 0 && channel_name[ 0 ] != CChatChannelConfig.SERVER_CHANNEL_PREFIX && Is_Valid_Channel_Name( channel_name );
		}

		static private bool Is_Valid_Channel_Name( string channel_name )
		{
			// Channel names that are numbers are not allowed under any condition, to prevent client-side confusion
			int channel_as_number;
			return !Int32.TryParse( channel_name, out channel_as_number );
		}
				
		// Properties
		public EChannelID ID { get; private set; }
		public EPersistenceID Moderator { get; private set; }
		public int MemberCount { get { return m_Members.Count; } }
		public IEnumerable< EPersistenceID > Members { get { return m_Members; } }
		public IEnumerable< EPersistenceID > Gagged { get { return m_Gagged; } }
		public string InternalName { get { return m_Config.InternalName; } }
		public string ExternalName { get { return m_Config.ExternalName; } }
		public EChannelGameProperties GameProperties { get { return m_Config.GameProperties; } }
		public bool ShouldAnnounceJoinLeave { get { return m_Config.AnnounceJoinLeave; } }
		public bool ShouldDestroyWhenEmpty { get { return m_Config.DestroyWhenEmpty; } }
		public bool IsServerChannel { get { return m_Config.IsServerChannel; } }
		public bool IsMembershipClientLocked { get { return m_Config.IsMembershipClientLocked; } }
		public bool IsBeingDestroyed { get; set; }
		
		// Fields
		private CChatChannelConfig m_Config = null;
		private List< EPersistenceID > m_Members = new List< EPersistenceID >();
		private HashSet< EPersistenceID > m_Banned = new HashSet< EPersistenceID >();
		private HashSet< EPersistenceID > m_Gagged = new HashSet< EPersistenceID >();
	}
}