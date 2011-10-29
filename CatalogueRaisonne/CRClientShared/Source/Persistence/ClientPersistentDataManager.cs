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