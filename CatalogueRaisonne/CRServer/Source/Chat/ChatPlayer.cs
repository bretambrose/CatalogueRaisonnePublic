using System;
using System.Collections.Generic;

using CRShared;
using CRShared.Chat;

namespace CRServer.Chat
{
	public class CChatPlayer
	{
		// Construction
		public CChatPlayer( EPersistenceID persistence_id, ESessionID session_id, string name, List< EPersistenceID > ignore_list )
		{
			PersistenceID = persistence_id;
			SessionID = session_id;
			Name = name;
			ignore_list.ShallowCopy( m_IgnoreList );
		}
		
		// Methods
		// Public interface
		public void Add_Channel( EChannelID channel_id )
		{
			m_Channels.Add( channel_id );
		}

		public void Remove_Channel( EChannelID channel_id )
		{
			m_Channels.Remove( channel_id );
		}

		public bool Is_Ignored( EPersistenceID player_id )
		{
			return m_IgnoreList.Contains( player_id );
		}

		public void Add_Ignored_Player( EPersistenceID player_id )
		{
			m_IgnoreList.Add( player_id );
		}

		public void Remove_Ignored_Player( EPersistenceID player_id )
		{
			m_IgnoreList.Remove( player_id );
		}

		// Properties
		public EPersistenceID PersistenceID { get; private set; }
		public ESessionID SessionID { get; private set; }
		public string Name { get; private set; }
		public IEnumerable< EChannelID > Channels { get { return m_Channels; } }	

		// Fields
		private HashSet< EChannelID > m_Channels = new HashSet< EChannelID >();
		private HashSet< EPersistenceID > m_IgnoreList = new HashSet< EPersistenceID >();
	}
}