using System;
using System.Collections.Generic;

using CRShared;

namespace CRShared
{		
	public class CLogRequest
	{
		// Construction
		public CLogRequest() {}
		
		// Properties
	}
			
	public class COutboundLogFrame
	{
		// Construction
		public COutboundLogFrame() {}

		// Methods
		public void Add_Request( CLogRequest request ) { m_Requests.Add( request ); }

		// Properties
		public bool Empty { get { return m_Requests.Count == 0; } }
		public IEnumerable< CLogRequest > Requests { get { return m_Requests; } }

		// Fields
		private List< CLogRequest > m_Requests = new List< CLogRequest >();	
	}	

	public class CInboundLogFrame {}
}
