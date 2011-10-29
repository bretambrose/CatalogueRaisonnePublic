using System;

using CRShared;

namespace CRClientShared
{
	public class CClientQuickmatchManager
	{
		// Construction
		static CClientQuickmatchManager() {}
		private CClientQuickmatchManager() 
		{
			QuickmatchRequestID = EMessageRequestID.Invalid;
		}

		// Public interface

		// Private interface
		// Slash command handlers
		[GenericHandler]
		private void Handle_Create_Quickmatch_Command( CCreateQuickmatchSlashCommand command )
		{
			CClientLogicalThread.Instance.Send_Message_To_Server( new CCreateQuickmatchRequest( command.PlayerName, command.GameCount, command.AllowObservers ) );
		}

		[GenericHandler]
		private void Handle_Respond_To_Quickmatch_Request_Command( CRespondToQuickmatchRequestSlashCommand command )
		{
			if ( QuickmatchRequestID != EMessageRequestID.Invalid )
			{
				CClientLogicalThread.Instance.Send_Message_To_Server( new CInviteToQuickmatchResponse( QuickmatchRequestID, command.Response == EQuickmatchInviteResponse.Accept ) );

				QuickmatchRequestID = EMessageRequestID.Invalid;
			}
			else
			{
				CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Quickmatch_No_Invite_Request );
			}
		}

		[GenericHandler]
		private void Handle_Cancel_Quickmatch_Command( CCancelQuickmatchSlashCommand command )
		{
			CClientLogicalThread.Instance.Send_Message_To_Server( new CCancelQuickmatchRequest() );
		}

		[GenericHandler]
		private void Handle_Create_AI_Quickmatch_Command( CCreateAIQuickmatchSlashCommand command )
		{
			;	// Not yet supported
		}

		// Network message handlers
		[NetworkMessageHandler]
		private void Handle_Create_Quickmatch_Response( CCreateQuickmatchResponse response )
		{
			switch ( response.Result )
			{
				case ECreateQuickmatchResult.Invalid_State_To_Create:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Quickmatch_Create_Invalid_State_To_Create );
					break;

				case ECreateQuickmatchResult.Target_Invalid_State_To_Create:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Quickmatch_Create_Target_Invalid_State_To_Create );
					break;

				case ECreateQuickmatchResult.Unknown_Target_Player:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Quickmatch_Create_Unknown_Target_Player );
					break;

				case ECreateQuickmatchResult.Ignored_By_Target_Player:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Quickmatch_Create_Ignored_By_Target_Player );
					break;

				case ECreateQuickmatchResult.Cannot_Invite_Yourself:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Quickmatch_Create_Cannot_Invite_Yourself );
					break;

				case ECreateQuickmatchResult.Already_Has_Pending_Quickmatch:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Quickmatch_Create_Already_Has_Pending_Quickmatch );
					break;

				case ECreateQuickmatchResult.Target_Already_Has_Pending_Quickmatch:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Quickmatch_Create_Target_Already_Has_Pending_Quickmatch );
					break;

			}
		}


		[NetworkMessageHandler]
		private void Handle_Invite_To_Quickmatch_Request( CInviteToQuickmatchRequest request )
		{
			QuickmatchRequestID = request.RequestID;

			if ( request.GameCount == 0 )
			{
				CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Quickmatch_Invite_Indefinite_Game_Count, request.PlayerName );
			}
			else
			{
				CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Quickmatch_Invite_Finite_Game_Count, request.PlayerName, request.GameCount );
			}
		}

		[NetworkMessageHandler]
		private void Handle_Cancel_Quickmatch_Response( CCancelQuickmatchResponse response )
		{
			switch ( response.Result )
			{
				case ECancelQuickmatchResult.Not_In_Quickmatch:
					CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Quickmatch_Cancel_Not_In_Quickmatch );
					break;

				default:
					break;
			}
		}

		// Properties
		static CClientQuickmatchManager Instance { get { return m_Instance; } }

		private EMessageRequestID QuickmatchRequestID { get; set; }

		// Fields
		static CClientQuickmatchManager m_Instance = new CClientQuickmatchManager();
	}
}
