using System;
using System.Collections.Generic;

namespace CRShared
{

	
	public class CUIOutputFrame 
	{
		// Construction
		public CUIOutputFrame() {}

		// Methods
		public void Add_Notification( CUILogicNotification notification )
		{
			m_Notifications.Add( notification );
		}

		// Properties
		public bool Empty { get { return m_Notifications.Count == 0; } }
		public IEnumerable< CUILogicNotification > Notifications { get { return m_Notifications; } }
		
		// Fields
		private List< CUILogicNotification > m_Notifications = new List< CUILogicNotification >();
	}
	
	public class CUIInputFrame 
	{
		// Construction
		public CUIInputFrame() {}

		// Methods
		public void Add_Request( CUIInputRequest request ) 
		{ 
			m_Requests.Add( request ); 
		}

		// Properties
		public bool Empty { get { return m_Requests.Count == 0; } }
		public IEnumerable< CUIInputRequest > Requests { get { return m_Requests; } }

		// Fields
		private List< CUIInputRequest > m_Requests = new List< CUIInputRequest >();	
	}	
}
