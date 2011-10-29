using System;
using System.Collections;
using System.Collections.Generic;

namespace CRShared
{				
	public class CNetworkFrameManager : CCrossThreadManagerClass< CInboundNetworkFrame, COutboundNetworkFrame >
	{
		// Construction
		private CNetworkFrameManager() 
		{
			LogicalInterface = Create_InterfaceB();
			NetworkInterface = Create_InterfaceA();
		}
		
		static CNetworkFrameManager() {}
		
		// Methods
		
		// Properties
		public static CNetworkFrameManager Instance { get { return m_Instance; } }
		
		public ICrossThreadDataQueues< COutboundNetworkFrame, CInboundNetworkFrame > LogicalInterface { get; private set; }
		public ICrossThreadDataQueues< CInboundNetworkFrame, COutboundNetworkFrame > NetworkInterface { get; private set; }
		
		// Fields
		private static CNetworkFrameManager m_Instance = new CNetworkFrameManager();
				
	}
}