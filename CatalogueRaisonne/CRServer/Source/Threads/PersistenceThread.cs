using System;
using System.Collections.Generic;
using System.Threading;

using CRShared;

namespace CRServer
{
	public class CPersistenceThread : CTimeKeeperThread
	{
		// Construction
		private CPersistenceThread() :
			base()
		{
		}
		
		static CPersistenceThread() {}
		
		// Methods
		// Public interface
		public static Thread Allocate_Thread()
		{
			return new Thread( Instance.Start );		
		}
		
		// Protected interface and overrides
		protected override void Initialize()
		{
			base.Initialize();

			base.Initialize_Time_Keeper( TimeKeeperExecutionMode.Internal, TimeKeeperTimeUnit.RealTime, TimeKeeperAuthority.Master, PERSISTENCE_SERVICE_INTERVAL, true, false );
			m_PersistenceInterface = CPersistenceFrameManager.Instance.DBInterface;	
		}

		protected override void Build_Thread_Frames()
		{
			base.Build_Thread_Frames();

			if ( m_CurrentFrame == null )
			{
				m_CurrentFrame = new CFromPersistenceFrame();
			}
		}

		protected override void Flush_Data_Frames( long current_time )
		{
			base.Flush_Data_Frames( current_time );

			if ( !m_CurrentFrame.Empty )
			{
				m_PersistenceInterface.Send( m_CurrentFrame );
				m_CurrentFrame = null;
			}
		}
				
		protected override void Service( long current_time )
		{	
			base.Service( current_time );

			var incoming_frames = new List< CToPersistenceFrame >();
			m_PersistenceInterface.Receive( incoming_frames );
			
			incoming_frames.Apply( frame => Pre_Process_Incoming_Frame( frame ) );

			Service_Requests();		
		}

		// Private interface
		private void Pre_Process_Incoming_Frame( CToPersistenceFrame frame )
		{
			foreach ( var request in frame.Requests )
			{
				EPersistenceRequestType request_type = request.RequestType;

				List< CPersistenceRequest > request_list = null;
				if ( !SortedRequestLists.TryGetValue( request_type, out request_list ) )
				{
					request_list = new List< CPersistenceRequest >();
					SortedRequestLists.Add( request_type, request_list );
				}

				request_list.Add( request );
			}
		}

		private void Service_Requests()
		{
			SortedRequestLists.Apply( request => Process_Request_List( request.Key, request.Value ) );
			Clear_Sorted_Requests();
		}
		
		private void Clear_Sorted_Requests()
		{
			SortedRequestLists.Apply( request_list => request_list.Value.Clear() );
		}

		private void Process_Request_List( EPersistenceRequestType request_type, List< CPersistenceRequest > request_list )
		{
			switch ( request_type )
			{
				case EPersistenceRequestType.Get_Player_Data:
					Handle_Get_Player_Data_Request_Batch( request_list );
					break;
			
				case EPersistenceRequestType.Get_Player_ID:
					Handle_Get_Player_ID_Request_Batch( request_list );
					break;

				case EPersistenceRequestType.Add_Ignored_Player:
					Handle_Add_Ignored_Player_Request_Batch( request_list );
					break;

				case EPersistenceRequestType.Remove_Ignored_Player:
					Handle_Remove_Ignored_Player_Request_Batch( request_list );
					break;
			}
		}

		private void Handle_Get_Player_Data_Request_Batch( List< CPersistenceRequest > request_list )
		{
			foreach ( var request in request_list )
			{
				CGetPlayerDataPersistenceRequest get_request = request as CGetPlayerDataPersistenceRequest;

				CPersistentPlayerData player_data = new CPersistentPlayerData( Temp_Compute_Persistence_ID( get_request.PlayerName ), get_request.PlayerName );

				m_CurrentFrame.Add_Response( new CGetPlayerDataPersistenceResponse( get_request.RequestID, player_data ) );
			}
		}

		private void Handle_Get_Player_ID_Request_Batch( List< CPersistenceRequest > request_list )
		{
			foreach ( var request in request_list )
			{
				CGetPlayerIDPersistenceRequest get_request = request as CGetPlayerIDPersistenceRequest;

				EPersistenceID player_id = Temp_Compute_Persistence_ID( get_request.PlayerName );

				m_CurrentFrame.Add_Response( new CGetPlayerIDPersistenceResponse( get_request.RequestID, player_id ) );
			}
		}

		private void Handle_Add_Ignored_Player_Request_Batch( List< CPersistenceRequest > request_list )
		{
			foreach ( var request in request_list )
			{
				CAddIgnoredPlayerPersistenceRequest add_ignore_request = request as CAddIgnoredPlayerPersistenceRequest;

				EPersistenceID player_id = Temp_Compute_Persistence_ID( add_ignore_request.IgnoredPlayerName );
				if ( player_id == add_ignore_request.SourcePlayer )
				{
					m_CurrentFrame.Add_Response( new CAddIgnoredPlayerPersistenceResponse( add_ignore_request.RequestID, EIgnorePlayerResult.Cannot_Ignore_Self ) );
				}
				else
				{
					m_CurrentFrame.Add_Response( new CAddIgnoredPlayerPersistenceResponse( add_ignore_request.RequestID, player_id ) );
				}
			}
		}

		private void Handle_Remove_Ignored_Player_Request_Batch( List< CPersistenceRequest > request_list )
		{
			foreach ( var request in request_list )
			{
				CRemoveIgnoredPlayerPersistenceRequest remove_ignore_request = request as CRemoveIgnoredPlayerPersistenceRequest;

				EPersistenceID player_id = Temp_Compute_Persistence_ID( remove_ignore_request.IgnoredPlayerName );

				m_CurrentFrame.Add_Response( new CRemoveIgnoredPlayerPersistenceResponse( remove_ignore_request.RequestID, player_id ) );
			}
		}

		private static EPersistenceID Temp_Compute_Persistence_ID( string player_name )
		{
			return (EPersistenceID) player_name.ToUpper().GetHashCode();
		}

		// Properties
		public static CPersistenceThread Instance { get { return m_Instance; } }
		
		// Fields
		private static CPersistenceThread m_Instance = new CPersistenceThread();
		
		private const int PERSISTENCE_SERVICE_INTERVAL = 25;
		private ICrossThreadDataQueues< CFromPersistenceFrame, CToPersistenceFrame > m_PersistenceInterface = null;
		private CFromPersistenceFrame m_CurrentFrame = null;

		private Dictionary< EPersistenceRequestType, List< CPersistenceRequest > > SortedRequestLists = new Dictionary< EPersistenceRequestType, List< CPersistenceRequest > >();
	}
}
