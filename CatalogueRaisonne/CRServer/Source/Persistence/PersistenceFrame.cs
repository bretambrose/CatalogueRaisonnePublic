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
