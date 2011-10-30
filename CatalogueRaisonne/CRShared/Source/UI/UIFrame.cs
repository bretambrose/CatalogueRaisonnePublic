/*

	UIFrame.cs

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
