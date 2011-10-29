using System;
using System.Collections.Generic;
using System.Text;

namespace CRShared
{
	[NetworkMessage]
	public class CClientHelloRequest : CRequestMessage
	{
		// Construction
		public CClientHelloRequest( string name )
		{
			Name = name;
		}
		
		// Methods
		public override string ToString()
		{
			return String.Format( "( [CClientHelloRequest] Name = {0} )", Name );
		}
				
		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Client_Hello_Request; } }

		public string Name { get; private set; }
	}

	public enum EConnectRefusalReason
	{
		None = 0,

		Unknown,
		Name_Already_Connected,
		Invalid_Password,
		Unable_To_Announce_To_Chat_Server,
		Unable_To_Join_Required_Chat_Channel,
		Internal_Persistence_Error
	}

	[NetworkMessage]
	public class CClientHelloResponse : CResponseMessage
	{
		// Construction
		public CClientHelloResponse( EMessageRequestID request_id, CPersistentPlayerData player_data ) :
			base( request_id )
		{
			PlayerData = player_data.Clone();
			Reason = EConnectRefusalReason.None;
		}

		public CClientHelloResponse( EMessageRequestID request_id, EConnectRefusalReason reason ) :
			base( request_id )
		{
			Reason = reason;
		}
		
		// Methods
		public override string ToString()
		{
			return String.Format( "( [CClientHelloResponse] PlayerData = {0}, Reason = {1} )", ( PlayerData != null ) ? PlayerData.ToString() : "null", Reason.ToString() );
		}
				
		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Client_Hello_Response; } }

		public CPersistentPlayerData PlayerData { get; private set; }
		public EConnectRefusalReason Reason { get; private set; }
	}

	[NetworkMessage]
	public class CConnectionDroppedMessage : CNetworkMessage
	{
		// Construction
		public CConnectionDroppedMessage( EConnectRefusalReason reason ) :
			base()
		{
			Reason = reason;
		}
		
		// Methods
		public override string ToString()
		{
			return String.Format( "( [CConnectionDroppedMessage] Reason = {0} )", Reason.ToString() );
		}
				
		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Connection_Dropped; } }

		public EConnectRefusalReason Reason { get; private set; }
	}

	[NetworkMessage]
	public class CDisconnectRequestMessage : CNetworkMessage
	{
		// Construction
		public CDisconnectRequestMessage() {}
		
		// Methods
		public override string ToString()
		{
			return "( [CDisconnectRequestMessage] )";
		}	

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Disconnect_Request; } }
	}	
	
	[NetworkMessage]
	public class CDisconnectResultMessage : CNetworkMessage
	{
		// Construction
		public CDisconnectResultMessage( EDisconnectReason reason )
		{
			Reason = reason;
		}
		
		// Methods
		public override string ToString()
		{
			return String.Format( "( [CDisconnectResultMessage] Reason = {0} )", Reason.ToString() );
		}
				
		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Disconnect_Result; } }

		public EDisconnectReason Reason { get; private set; }
	}	
	
	[NetworkMessage]
	public class CPingMessage : CNetworkMessage
	{
		// Construction
		public CPingMessage() {}
		
		// Methods
		public override string ToString()
		{
			return "( [CPingMessage] )";
		}
		
		// Properties		
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Ping; } }
	}		
	
	[NetworkMessage]
	public class CQueryPlayerInfoRequest : CRequestMessage
	{
		// Construction
		public CQueryPlayerInfoRequest() 
		{
			IDs = new List< EPersistenceID >();
		}
		
		// Methods
		public override string ToString()
		{
			return String.Format( "( [CQueryPlayerInfoRequest] IDs = ( {0} ) )", CSharedUtils.Build_String_List( IDs, n => ((long) n).ToString() ) );
		}
		
		public void Add_Player( EPersistenceID id ) { IDs.Add( id ); }
				
		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Query_Player_Info_Request; } }

		public List< EPersistenceID > IDs { get; private set; }		
	}
	
	[NetworkMessage]
	public class CQueryPlayerInfoResponse : CResponseMessage
	{
		// Construction
		public CQueryPlayerInfoResponse( EMessageRequestID request_id ) :
			base( request_id )
		{
			Infos = new List< CPlayerInfo >();
		}
		
		// Methods
		public override string ToString()
		{
			return String.Format( "( [CQueryPlayerInfoResponse] PlayerInfos = ( {0} ) )", CSharedUtils.Build_String_List( Infos ) );
		}
				
		public void Add_Player_Info( CPlayerInfo player_info ) { Infos.Add( player_info ); }

		// Properties
		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Query_Player_Info_Response; } }

		public List< CPlayerInfo > Infos { get; private set; }
	}	
}