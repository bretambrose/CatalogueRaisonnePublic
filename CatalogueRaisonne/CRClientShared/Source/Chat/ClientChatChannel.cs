using System;
using System.Collections.Generic;
using System.Text;

using CRShared;
using CRShared.Chat;

namespace CRClientShared.Chat
{
	public class CClientChatChannel
	{
		// Construction
		public CClientChatChannel( EChannelID channel_id, string channel_name, EClientChannelNumber channel_number, bool announce_join_leave )
		{
			ChannelID = channel_id;
			ChannelName = channel_name;
			ChannelNumber = channel_number;
			AnnounceJoinLeave = announce_join_leave;
		}

		// Methods
		// Public interface
		public void Add_Player( EPersistenceID id ) 
		{ 
			m_Members.Add( id );
			CClientPlayerInfoManager.Instance.Begin_Player_Listen( id, EPlayerListenReason.In_Chat_Channel ); 
		}

		public void Remove_Player( EPersistenceID id ) 
		{ 
			m_Members.Remove( id ); 
			CClientPlayerInfoManager.Instance.End_Player_Listen( id, EPlayerListenReason.In_Chat_Channel ); 
		}

		public void Set_Moderator( EPersistenceID id ) 
		{ 
			Moderator = id; 
		}
		
		public void Set_Gag_Status( EPersistenceID id, bool gagged )
		{
			if ( gagged )
			{
				m_Gagged.Add( id );
			}
			else
			{
				m_Gagged.Remove( id );
			}
		}

		public bool Is_Gagged( EPersistenceID id )
		{
			return m_Gagged.Contains( id );
		}

		public string Build_Channel_Listing( string channel_name )
		{
			string member_list_string = CSharedUtils.Build_String_List( m_Members,
																							id => CClientPlayerInfoManager.Instance.Get_Player_Name( id ),
																							id => ( id == Moderator ) ? "*" : ( m_Gagged.Contains( id ) ? 
																								CClientResource.Get_Text( EClientTextID.Client_Chat_Channel_Listing_Gagged_Status ) : "" ) );

			return CClientResource.Get_Text( EClientTextID.Client_Chat_Channel_Listing, channel_name, member_list_string );
		}

		// Properties
		public EChannelID ChannelID { get; private set; }
		public string ChannelName { get; private set; }
		public EClientChannelNumber ChannelNumber { get; private set; }
		public IEnumerable< EPersistenceID > Members { get { return m_Members; } }
		public EPersistenceID Moderator { get; set; }
		public bool AnnounceJoinLeave { get; private set; }

		// Fields
		private HashSet< EPersistenceID > m_Members = new HashSet< EPersistenceID >();
		private HashSet< EPersistenceID > m_Gagged = new HashSet< EPersistenceID >();
	}
}