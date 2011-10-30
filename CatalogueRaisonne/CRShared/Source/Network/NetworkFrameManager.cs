/*

	NetworkFrameManager.cs

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