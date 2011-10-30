/*

	RequestResponseManager.cs

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
	public delegate void DRequestResponseHandler( CResponseMessage response );

	public class CRequestResponseManager
	{
		// Construction
		public CRequestResponseManager()
		{
		}

		// Methods
		// Public interface
		public void Process_Incoming_Message( CNetworkMessage message )
		{
			if ( message is CResponseMessage )
			{
				CResponseMessage response = message as CResponseMessage;
				EMessageRequestID request_id = response.RequestID;
				CRequestMessage request = null;

				if ( m_OutstandingRequests.TryGetValue( request_id, out request ) )
				{
					response.Request = request;
					m_OutstandingRequests.Remove( request_id );
				}
				else if ( response.RequiresRequest )
				{
					throw new CAppNetworkingException( "Response has unknown request id." );
				}
			}
		}

		public void Process_Outgoing_Message( CNetworkMessage message )
		{
			if ( message is CRequestMessage )
			{
				CRequestMessage request = message as CRequestMessage;
				request.RequestID = m_NextRequestID++;
				m_OutstandingRequests.Add( request.RequestID, request );
			}
		}

		// Fields
		private Dictionary< EMessageRequestID, CRequestMessage > m_OutstandingRequests = new Dictionary< EMessageRequestID, CRequestMessage >();
		private EMessageRequestID m_NextRequestID = EMessageRequestID.Start;
	}
}