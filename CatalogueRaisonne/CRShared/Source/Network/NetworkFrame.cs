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
