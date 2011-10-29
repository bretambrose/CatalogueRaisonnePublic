using System;

using CRShared;

namespace CRClientShared
{
	public class CClientSocialManager
	{
		// constructors
		static CClientSocialManager() {}
		private CClientSocialManager() {}

		// Methods

		// Non-public interface
		// Slash command handlers
		[GenericHandler]
		private void Handle_Ignore_Command( CIgnoreSlashCommand command )
		{
			CClientLogicalThread.Instance.Send_Message_To_Server( new CIgnorePlayerRequest( command.PlayerName ) );					
		}

		[GenericHandler]
		private void Handle_Unignore_Command( CUnignoreSlashCommand command )
		{
			CClientLogicalThread.Instance.Send_Message_To_Server( new CUnignorePlayerRequest( command.PlayerName ) );					
		}

		[GenericHandler]
		private void Handle_Ignore_List_Command( CIgnoreListSlashCommand command )
		{
			Print_Ignore_List();					
		}

		// Network message handlers
		[NetworkMessageHandler]
		private void Handle_Ignore_Player_Response( CIgnorePlayerResponse response )
		{
			EClientTextID text_id = EClientTextID.Invalid;

			switch ( response.Result )
			{
				case EIgnorePlayerResult.Already_Ignored:
					text_id = EClientTextID.Client_Social_Ignore_Already_Ignored;
					break;

				case EIgnorePlayerResult.Persistence_Error:
					text_id = EClientTextID.Client_Social_Ignore_Persistence_Error;
					break;

				case EIgnorePlayerResult.Success:
					CClientPersistentDataManager.Instance.Add_Ignored_Player( response.IgnoredID );
					text_id = EClientTextID.Client_Social_Ignore_Success;
					break;

				case EIgnorePlayerResult.Unknown_Player:
					text_id = EClientTextID.Client_Social_Ignore_Unknown_Player;
					break;

				case EIgnorePlayerResult.Cannot_Ignore_Self:
					text_id = EClientTextID.Client_Social_Ignore_Cannot_Ignore_Self;
					break;
			}

			if ( text_id != EClientTextID.Invalid )
			{
				CIgnorePlayerRequest ignore_request = response.Request as CIgnorePlayerRequest;
				CClientResource.Output_Text< EClientTextID >( text_id, ignore_request.PlayerName );
			}
		}

		[NetworkMessageHandler]
		private void Handle_Unignore_Player_Response( CUnignorePlayerResponse response )
		{
			EClientTextID text_id = EClientTextID.Invalid;

			switch ( response.Result )
			{
				case EUnignorePlayerResult.Not_Already_Ignored:
					text_id = EClientTextID.Client_Social_Unignore_Not_Already_Ignored;
					break;

				case EUnignorePlayerResult.Persistence_Error:
					text_id = EClientTextID.Client_Social_Unignore_Persistence_Error;
					break;

				case EUnignorePlayerResult.Success:
					CClientPersistentDataManager.Instance.Remove_Ignored_Player( response.IgnoredID );
					text_id = EClientTextID.Client_Social_Unignore_Success;
					break;

				case EUnignorePlayerResult.Unknown_Player:
					text_id = EClientTextID.Client_Social_Unignore_Unknown_Player;
					break;
			}

			if ( text_id != EClientTextID.Invalid )
			{
				CUnignorePlayerRequest unignore_request = response.Request as CUnignorePlayerRequest;
				CClientResource.Output_Text< EClientTextID >( text_id, unignore_request.PlayerName );
			}
		}

		private void Print_Ignore_List()
		{
			CClientResource.Output_Text< EClientTextID >( EClientTextID.Client_Social_Ignore_List_Header );
			CClientResource.Output_Text( CSharedUtils.Build_String_List( CClientPersistentDataManager.Instance.IgnoreList, pid => CClientPlayerInfoManager.Instance.Get_Player_Name( pid ) ) );
		}

		// Properties
		public static CClientSocialManager Instance { get { return m_Instance; } }
		
		// Fields
		private static CClientSocialManager m_Instance = new CClientSocialManager(); 
	}
}