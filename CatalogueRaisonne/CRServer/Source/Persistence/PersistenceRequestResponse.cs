using System;
using System.Collections.Generic;

using CRShared;

namespace CRServer
{
	public enum EPersistenceRequestID
	{
		Invalid = -1,
		Start
	}

	public enum EPersistenceRequestType
	{
		Invalid = -1,

		Get_Player_Data,
		Get_Player_ID,
		Add_Ignored_Player,
		Remove_Ignored_Player
	}

	public enum EPersistenceError
	{
		None
	}

	public delegate void DPersistenceRequestHandler( CPersistenceResponse response );
	
	public class CPersistenceRequestResponseBase
	{
		public virtual EPersistenceRequestType RequestType { get { return EPersistenceRequestType.Invalid; } }
	}

	public class CPersistenceRequest : CPersistenceRequestResponseBase
	{
		public EPersistenceRequestID RequestID { get; set; }

		// not an event, no multi-cast on this
		public DPersistenceRequestHandler Handler { get; set; }
	}

	public class CPersistenceResponse : CPersistenceRequestResponseBase
	{
		public CPersistenceResponse( EPersistenceRequestID request_id, EPersistenceError error )
		{
			RequestID = request_id;
			Error = error;
		}

		public EPersistenceRequestID RequestID { get; private set; }
		public EPersistenceError Error { get; private set; }
		public CPersistenceRequest Request { get; set; }
	}
	
	public class CGetPlayerDataPersistenceRequest : CPersistenceRequest
	{
		public CGetPlayerDataPersistenceRequest( string player_name )
		{
			PlayerName = player_name;
		}

		// Methods
		public override EPersistenceRequestType RequestType { get { return EPersistenceRequestType.Get_Player_Data; } }
				
		// Properties
		public string PlayerName { get; private set; }
	}

	public class CGetPlayerDataPersistenceResponse : CPersistenceResponse
	{
		// Construction
		public CGetPlayerDataPersistenceResponse( EPersistenceRequestID request_id, CPersistentPlayerData player_data ) :
			base( request_id, EPersistenceError.None )
		{
			PlayerData = player_data;
		}

		public CGetPlayerDataPersistenceResponse( EPersistenceRequestID request_id, EPersistenceError error ) :
			base( request_id, error )
		{
			PlayerData = null;
		}
		
		// Methods
		public override EPersistenceRequestType RequestType { get { return EPersistenceRequestType.Get_Player_Data; } }
				
		// Properties
		public CPersistentPlayerData PlayerData { get; private set; }
	}

	public class CGetPlayerIDPersistenceRequest : CPersistenceRequest
	{
		public CGetPlayerIDPersistenceRequest( string player_name )
		{
			PlayerName = player_name;
		}

		// Methods
		public override EPersistenceRequestType RequestType { get { return EPersistenceRequestType.Get_Player_ID; } }
				
		// Properties
		public string PlayerName { get; private set; }
	}

	public class CGetPlayerIDPersistenceResponse : CPersistenceResponse
	{
		// Construction
		public CGetPlayerIDPersistenceResponse( EPersistenceRequestID request_id, EPersistenceID player_id ) :
			base( request_id, EPersistenceError.None )
		{
			PlayerID = player_id;
		}

		public CGetPlayerIDPersistenceResponse( EPersistenceRequestID request_id, EPersistenceError error ) :
			base( request_id, error )
		{
		}
		
		// Methods
		public override EPersistenceRequestType RequestType { get { return EPersistenceRequestType.Get_Player_ID; } }
				
		// Properties
		public EPersistenceID PlayerID { get; private set; }
	}

	public class CAddIgnoredPlayerPersistenceRequest : CPersistenceRequest
	{
		public CAddIgnoredPlayerPersistenceRequest( EPersistenceID source_player, string ignored_player_name )
		{
			SourcePlayer = source_player;
			IgnoredPlayerName = ignored_player_name;
		}

		// Methods
		public override EPersistenceRequestType RequestType { get { return EPersistenceRequestType.Add_Ignored_Player; } }
				
		// Properties
		public string IgnoredPlayerName { get; private set; }
		public EPersistenceID SourcePlayer { get; private set; }
	}

	public class CAddIgnoredPlayerPersistenceResponse : CPersistenceResponse
	{
		// Construction
		public CAddIgnoredPlayerPersistenceResponse( EPersistenceRequestID request_id, EPersistenceID ignored_player_id ) :
			base( request_id, EPersistenceError.None )
		{
			IgnoredPlayerID = ignored_player_id;
			Result = EIgnorePlayerResult.Success;
		}

		public CAddIgnoredPlayerPersistenceResponse( EPersistenceRequestID request_id, EIgnorePlayerResult result ) :
			base( request_id, EPersistenceError.None )
		{
			IgnoredPlayerID = EPersistenceID.Invalid;
			Result = result;
		}

		public CAddIgnoredPlayerPersistenceResponse( EPersistenceRequestID request_id, EPersistenceError error ) :
			base( request_id, error )
		{
		}
		
		// Methods
		public override EPersistenceRequestType RequestType { get { return EPersistenceRequestType.Add_Ignored_Player; } }
				
		// Properties
		public EPersistenceID IgnoredPlayerID { get; private set; }
		public EIgnorePlayerResult Result { get; private set; }
	}

	public class CRemoveIgnoredPlayerPersistenceRequest : CPersistenceRequest
	{
		public CRemoveIgnoredPlayerPersistenceRequest( EPersistenceID source_player, string ignored_player_name )
		{
			SourcePlayer = source_player;
			IgnoredPlayerName = ignored_player_name;
		}

		// Methods
		public override EPersistenceRequestType RequestType { get { return EPersistenceRequestType.Remove_Ignored_Player; } }
				
		// Properties
		public string IgnoredPlayerName { get; private set; }
		public EPersistenceID SourcePlayer { get; private set; }
	}

	public class CRemoveIgnoredPlayerPersistenceResponse : CPersistenceResponse
	{
		// Construction
		public CRemoveIgnoredPlayerPersistenceResponse( EPersistenceRequestID request_id, EPersistenceID ignored_player_id ) :
			base( request_id, EPersistenceError.None )
		{
			IgnoredPlayerID = ignored_player_id;
			Result = EUnignorePlayerResult.Success;
		}

		public CRemoveIgnoredPlayerPersistenceResponse( EPersistenceRequestID request_id, EUnignorePlayerResult result ) :
			base( request_id, EPersistenceError.None )
		{
			IgnoredPlayerID = EPersistenceID.Invalid;
			Result = result;
		}

		public CRemoveIgnoredPlayerPersistenceResponse( EPersistenceRequestID request_id, EPersistenceError error ) :
			base( request_id, error )
		{
		}
		
		// Methods
		public override EPersistenceRequestType RequestType { get { return EPersistenceRequestType.Remove_Ignored_Player; } }
				
		// Properties
		public EPersistenceID IgnoredPlayerID { get; private set; }
		public EUnignorePlayerResult Result { get; private set; }
	}
}