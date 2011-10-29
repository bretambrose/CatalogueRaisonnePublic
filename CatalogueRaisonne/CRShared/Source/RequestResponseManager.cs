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