using System;
using System.Reflection;

using CRShared;

namespace CRServer
{
	public enum EServerTextID
	{
		Invalid = -1,

		Server_Startup_Greeting,
		Server_Player_Connected_Notice,
		Server_Player_Reconnected_Notice,
		Server_Player_Linkdead_Notice,
		Server_Player_Removed_Notice,
		Server_Player_Linkdead_Timeout_Notice,
		Server_Lobby_Channel_Name,
		Server_Match_Channel_Name,
		Server_Observer_Channel_Name,

		// Commands
		Server_Command_Name_Shutdown,
		Server_Help_Command_Shutdown,

		// Command params
		Server_Command_Shutdown_Param_Delay
	}

	public class CServerResource : CSharedResource
	{
		public static void Initialize_Resources()
		{
			CSharedResource.Initialize_Shared_Resources();

			CResourceManager.Instance.Initialize_Assembly_Resources< EServerTextID >( "CRServer.Source.Resources.ServerResources", Assembly.GetExecutingAssembly(), EResourceType.Text );
		}							
	}
}