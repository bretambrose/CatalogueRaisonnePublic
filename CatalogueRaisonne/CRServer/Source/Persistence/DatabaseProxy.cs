/*

	DatabaseProxy.cs

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

namespace CRServer
{
	public delegate void DPersistenceIDRequestHandler( string player_name, EPersistenceID player_id );

	public class CDatabaseProxy
	{
		// Constructors
		static CDatabaseProxy() {}
		private CDatabaseProxy()
		{
		}

		// Methods
		// Public interface
		public void Submit_Request( CPersistenceRequest request )
		{
			request.RequestID = Allocate_Request_ID();

			m_UnprocessedRequests.Add( request );
		}

		public void Submit_Response( CPersistenceResponse response )
		{
			m_UnprocessedResponses.Add( response );
		}

		public void Service_Requests()
		{
			m_UnprocessedRequests.Apply( request => Process_Request_Local( request ) );
			m_UnprocessedRequests.Clear();
		}

		public void Service_Responses()
		{
			m_UnprocessedResponses.Apply( response => Process_Response( response ) );
			m_UnprocessedResponses.Clear();
		}

		public EPersistenceID Get_Persistence_ID_By_Name_Sync( string name )
		{
			EPersistenceID player_id = EPersistenceID.Invalid;
			m_KnownPlayers.TryGetValue( name.ToUpper(), out player_id );

			return player_id;
		}

		public void Get_Persistence_ID_By_Name_ASync( string name, DPersistenceIDRequestHandler request_handler )
		{
			CGetPlayerIDPersistenceRequest id_request = new CGetPlayerIDPersistenceRequest( name );
			id_request.Handler = delegate( CPersistenceResponse response ) 
				{
					CGetPlayerIDPersistenceResponse id_response = response as CGetPlayerIDPersistenceResponse;
					EPersistenceID player_id = id_response.PlayerID;

					request_handler( name, player_id );
				};

			Submit_Request( id_request );
		}

		public CPersistentPlayerData Get_Player_Data( EPersistenceID player_id )
		{
			CPersistentPlayerData player_data = null;
			m_PlayerDataByID.TryGetValue( player_id, out player_data );

			return player_data;
		}

		// Non-public interface
		private void Process_Response( CPersistenceResponse response )
		{
			EPersistenceRequestID request_id = response.RequestID;
			CPersistenceRequest request = null;
			if ( !m_PendingRequests.TryGetValue( request_id, out request ) )
			{
				throw new CApplicationException( "Could not match persistence response with a pending request" );
			}

			Process_Response_Local( request, response );
			request.Handler( response );

			m_PendingRequests.Remove( request_id );
		}

		private void Process_Response_Local( CPersistenceRequest request, CPersistenceResponse response )
		{
			switch ( response.RequestType )
			{
				case EPersistenceRequestType.Get_Player_Data:
					CGetPlayerDataPersistenceRequest get_data_request = request as CGetPlayerDataPersistenceRequest;
					CGetPlayerDataPersistenceResponse get_data_response = response as CGetPlayerDataPersistenceResponse;
					if ( get_data_response.PlayerData != null )
					{
						Register_Known_Player( get_data_request.PlayerName, get_data_response.PlayerData.ID );
						Register_Player_Data( get_data_response.PlayerData );
					}
					break;

				case EPersistenceRequestType.Get_Player_ID:
					CGetPlayerIDPersistenceRequest get_id_request = request as CGetPlayerIDPersistenceRequest;
					CGetPlayerIDPersistenceResponse get_id_response = response as CGetPlayerIDPersistenceResponse;
					if ( get_id_response.PlayerID != EPersistenceID.Invalid )
					{
						Register_Known_Player( get_id_request.PlayerName, get_id_response.PlayerID );
					}
					break;

				case EPersistenceRequestType.Add_Ignored_Player:
					CAddIgnoredPlayerPersistenceRequest add_ignore_request = request as CAddIgnoredPlayerPersistenceRequest;
					CAddIgnoredPlayerPersistenceResponse add_ignore_response = response as CAddIgnoredPlayerPersistenceResponse;
					if ( add_ignore_response.Error == EPersistenceError.None && add_ignore_response.Result == EIgnorePlayerResult.Success )
					{
						CPersistentPlayerData add_player_data = Get_Player_Data( add_ignore_request.SourcePlayer );
						if ( add_player_data != null )
						{
							add_player_data.IgnoreList.Add( add_ignore_response.IgnoredPlayerID );
						}
					}
					break;

				case EPersistenceRequestType.Remove_Ignored_Player:
					CRemoveIgnoredPlayerPersistenceRequest remove_ignore_request = request as CRemoveIgnoredPlayerPersistenceRequest;
					CRemoveIgnoredPlayerPersistenceResponse remove_ignore_response = response as CRemoveIgnoredPlayerPersistenceResponse;
					if ( remove_ignore_response.Error == EPersistenceError.None && remove_ignore_response.Result == EUnignorePlayerResult.Success )
					{
						CPersistentPlayerData remove_player_data = Get_Player_Data( remove_ignore_request.SourcePlayer );
						if ( remove_player_data != null )
						{
							remove_player_data.IgnoreList.Remove( remove_ignore_response.IgnoredPlayerID );
						}
					}
					break;
			}
		}

		private void Process_Request_Local( CPersistenceRequest request )
		{
			m_PendingRequests.Add( request.RequestID, request );
			CPersistenceResponse response = null;

			switch ( request.RequestType )
			{
				case EPersistenceRequestType.Get_Player_Data:
					response = Handle_Get_Player_Data_Request_Locally( request as CGetPlayerDataPersistenceRequest );
					break;

				case EPersistenceRequestType.Get_Player_ID:
					response = Handle_Get_Player_ID_Request_Locally( request as CGetPlayerIDPersistenceRequest );
					break;

				case EPersistenceRequestType.Add_Ignored_Player:
				case EPersistenceRequestType.Remove_Ignored_Player:
					break;
			}

			if ( response == null )
			{
				CServerLogicalThread.Instance.Submit_Persistence_Request( request );
			}
			else
			{
				m_UnprocessedResponses.Add( response );
			}
		}

		private CPersistenceResponse Handle_Get_Player_ID_Request_Locally( CGetPlayerIDPersistenceRequest request )
		{
			EPersistenceID player_id = EPersistenceID.Invalid;
			if ( !m_KnownPlayers.TryGetValue( request.PlayerName.ToUpper(), out player_id ) )
			{
				return null;
			}

			return new CGetPlayerIDPersistenceResponse( request.RequestID, player_id );
		}

		private CGetPlayerDataPersistenceResponse Handle_Get_Player_Data_Request_Locally( CGetPlayerDataPersistenceRequest request )
		{
			EPersistenceID player_id = EPersistenceID.Invalid;
			if ( !m_KnownPlayers.TryGetValue( request.PlayerName.ToUpper(), out player_id ) )
			{
				return null;
			}

			CPersistentPlayerData player_data = null;
			if ( !m_PlayerDataByID.TryGetValue( player_id, out player_data ) )
			{
				return null;
			}

			return new CGetPlayerDataPersistenceResponse( request.RequestID, player_data );
		}

		private EPersistenceRequestID Allocate_Request_ID()
		{
			return m_NextID++;
		}

		private void Register_Known_Player( string player_name, EPersistenceID player_id )
		{
			string upper_name = player_name.ToUpper();

			if ( !m_KnownPlayers.ContainsKey( upper_name ) )
			{
				m_KnownPlayers.Add( upper_name, player_id );
			}
		}

		private void Register_Player_Data( CPersistentPlayerData player_data )
		{
			if ( !m_PlayerDataByID.ContainsKey( player_data.ID ) )
			{
				m_PlayerDataByID.Add( player_data.ID, player_data );
			}
		}
				
		// Properties
		public static CDatabaseProxy Instance { get { return m_Instance; } private set { m_Instance = value; } }

		// Fields
		private static CDatabaseProxy m_Instance = new CDatabaseProxy();

		private List< CPersistenceRequest > m_UnprocessedRequests = new List< CPersistenceRequest >();
		private List< CPersistenceResponse > m_UnprocessedResponses = new List< CPersistenceResponse >();

		private Dictionary< EPersistenceRequestID, CPersistenceRequest > m_PendingRequests = new Dictionary< EPersistenceRequestID, CPersistenceRequest >();

		private Dictionary< string, EPersistenceID > m_KnownPlayers = new Dictionary< string, EPersistenceID >();
		private Dictionary< EPersistenceID, CPersistentPlayerData > m_PlayerDataByID = new Dictionary< EPersistenceID, CPersistentPlayerData >();

		private EPersistenceRequestID m_NextID = EPersistenceRequestID.Start;
	}
}
