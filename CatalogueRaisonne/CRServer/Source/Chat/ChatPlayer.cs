/*

	ChatPlayer.cs

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