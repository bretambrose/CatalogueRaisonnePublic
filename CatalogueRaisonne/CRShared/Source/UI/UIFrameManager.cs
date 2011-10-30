/*

	UIFrameManager.cs

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