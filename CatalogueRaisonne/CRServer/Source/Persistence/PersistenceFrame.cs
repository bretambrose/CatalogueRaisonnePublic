/*

	PersistenceFrame.cs

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

namespace CRServer
{
	public class CFromPersistenceFrame 
	{
		// Construction
		public CFromPersistenceFrame() {}

		// Methods
		public void Add_Response( CPersistenceResponse response ) { m_Responses.Add( response ); }

		// Properties
		public IEnumerable< CPersistenceResponse > Responses { get { return m_Responses; } }
		public bool Empty { get { return m_Responses.Count == 0; } }

		// Fields
		private List< CPersistenceResponse > m_Responses = new List< CPersistenceResponse >();	
	}
	
	public class CToPersistenceFrame 
	{
		// Construction
		public CToPersistenceFrame() {}

		// Methods
		public void Add_Request( CPersistenceRequest request ) { m_Requests.Add( request ); }

		// Properties
		public IEnumerable< CPersistenceRequest > Requests { get { return m_Requests; } }
		public bool Empty { get { return m_Requests.Count == 0; } }

		// Fields
		private List< CPersistenceRequest > m_Requests = new List< CPersistenceRequest >();	
	}	

	public class CPersistenceFrameManager : CCrossThreadManagerClass< CFromPersistenceFrame, CToPersistenceFrame >
	{
		// Construction
		private CPersistenceFrameManager() 
		{
			LogicalInterface = Create_InterfaceB();
			DBInterface = Create_InterfaceA();
		}
		
		static CPersistenceFrameManager() {}
		
		// Methods
		
		// Properties
		public static CPersistenceFrameManager Instance { get { return m_Instance; } }
		
		public ICrossThreadDataQueues< CToPersistenceFrame, CFromPersistenceFrame > LogicalInterface { get; private set; }
		public ICrossThreadDataQueues< CFromPersistenceFrame, CToPersistenceFrame > DBInterface { get; private set; }
		
		// Fields
		private static CPersistenceFrameManager m_Instance = new CPersistenceFrameManager();
				
	}
}
