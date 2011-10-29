using System;
using System.Collections;
using System.Collections.Generic;

namespace CRShared
{				
	public class CUIFrameManager : CCrossThreadManagerClass< CUIInputFrame, CUIOutputFrame >
	{
		// Construction
		private CUIFrameManager() 
		{
			LogicalInterface = Create_InterfaceB();
			UIInterface = Create_InterfaceA();
		}
		
		static CUIFrameManager() {}
		
		// Methods
		
		// Properties
		public static CUIFrameManager Instance { get { return m_Instance; } }
		
		public ICrossThreadDataQueues< CUIOutputFrame, CUIInputFrame > LogicalInterface { get; private set; }
		public ICrossThreadDataQueues< CUIInputFrame, CUIOutputFrame > UIInterface { get; private set; }
		
		// Fields
		private static CUIFrameManager m_Instance = new CUIFrameManager();
				
	}
}