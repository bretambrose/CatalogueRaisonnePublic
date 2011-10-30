/*

	LogFrame.cs

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
