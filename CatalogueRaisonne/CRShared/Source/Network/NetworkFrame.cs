/*

	NetworkFrame.cs

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
	public class CWrappedNetworkMessage
	{
		// Construction
		public CWrappedNetworkMessage( CNetworkMessage message, ESessionID id )
		{
			Message = message;
			ID = id;
		}
		
		// Properties
		public CNetworkMessage Message { get; private set; }
		public ESessionID ID { get; private set; } 		
	}
	
	public class CNetworkFrame
	{	
		// Construction
		public CNetworkFrame() {}
		
		// Methods		
		public void Add_Message( CNetworkMessage message, ESessionID id ) { m_Messages.Add( new CWrappedNetworkMessage( message, id ) ); }
		
		// properties
		virtual public bool Empty { get { return m_Messages.Count == 0; } }
		public IEnumerable< CWrappedNetworkMessage > Messages { get { return m_Messages; } }
		
		// Fields
		private List< CWrappedNetworkMessage > m_Messages = new List< CWrappedNetworkMessage >();
	}

	public enum ENetworkEvent
	{
		Connection_Success,
		Connection_Failure,
		Disconnection,
		Unable_To_Send_Message
	}
	
	public class CNetworkEvent
	{
		// Construction
		public CNetworkEvent( ENetworkEvent event_type, ESessionID id )
		{
			NetworkEvent = event_type;
			ID = id;
		}
		
		// Properties
		public ENetworkEvent NetworkEvent { get; private set; }
		public ESessionID ID { get; private set; } 
	}
	
	public class CInboundNetworkFrame : CNetworkFrame
	{
		// Construction
		public CInboundNetworkFrame() {}

		// Methods
		public void Add_Event( ENetworkEvent event_type, ESessionID id ) { m_Events.Add( new CNetworkEvent( event_type, id ) ); }

		// Properties
		override public bool Empty { get { return m_Events.Count == 0 && base.Empty; } }
		public IEnumerable< CNetworkEvent > Events { get { return m_Events; } }
				
		// Fields
		private List< CNetworkEvent > m_Events = new List< CNetworkEvent >();		
	}
	
	public class COutboundNetworkFrame : CNetworkFrame
	{
		// Construction
		public COutboundNetworkFrame() {}

		// Methods
		public void Add_Operation( CNetworkOperation operation ) { m_Operations.Add( operation ); }

		// Properties
		override public bool Empty { get { return m_Operations.Count == 0 && base.Empty; } }
		public IEnumerable< CNetworkOperation > Operations { get { return m_Operations; } }

		// Fields
		private List< CNetworkOperation > m_Operations = new List< CNetworkOperation >();	
	}	
}
