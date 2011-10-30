/*

	ClientPersistentDataManager.cs

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

namespace CRClientShared
{
	public class CClientPersistentDataManager
	{
		// Constructors
		static CClientPersistentDataManager() {}
		private CClientPersistentDataManager() 
		{
			PlayerData = null;
		}

		// Methods
		// Public interface
		public void On_Receive_Player_Data( CPersistentPlayerData player_data )
		{
			PlayerData = player_data;
		}

		public void Reset_Player_Data()
		{
			PlayerData = null;
		}

		public void Add_Ignored_Player( EPersistenceID ignored_id )
		{
			PlayerData.IgnoreList.Add( ignored_id );
			CClientPlayerInfoManager.Instance.Begin_Player_Listen( ignored_id, EPlayerListenReason.Ignore_List );
		}

		public void Remove_Ignored_Player( EPersistenceID ignored_id )
		{
			PlayerData.IgnoreList.Remove( ignored_id );
			CClientPlayerInfoManager.Instance.End_Player_Listen( ignored_id, EPlayerListenReason.Ignore_List );
		}

		// Properties
		public static CClientPersistentDataManager Instance { get { return m_Instance; } }

		public CPersistentPlayerData PlayerData { get; private set; }
		public IEnumerable< EPersistenceID > IgnoreList { get { return ( ( PlayerData != null ) ? PlayerData.IgnoreList : null ); } }

		// Fields
		private static CClientPersistentDataManager m_Instance = new CClientPersistentDataManager();
	}
}