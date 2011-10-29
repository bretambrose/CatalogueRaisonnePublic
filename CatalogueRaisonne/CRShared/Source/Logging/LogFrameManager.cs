using System;
using System.Collections;
using System.Collections.Generic;

namespace CRShared
{				
	public class CLogFrameManager : CCrossThreadManagerClass< CInboundLogFrame, COutboundLogFrame >
	{
		// Construction
		private CLogFrameManager() 
		{
			PublicInterface = Create_InterfaceB();
			LoggingInterface = Create_InterfaceA();
		}
		
		static CLogFrameManager() {}
		
		// Methods
		
		// Properties
		public static CLogFrameManager Instance { get { return m_Instance; } }
		
		public ICrossThreadDataQueues< COutboundLogFrame, CInboundLogFrame > PublicInterface { get; private set; }
		public ICrossThreadDataQueues< CInboundLogFrame, COutboundLogFrame > LoggingInterface { get; private set; }
		
		// Fields
		private static CLogFrameManager m_Instance = new CLogFrameManager();
				
	}
}