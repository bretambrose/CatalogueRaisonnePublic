/*

	ServerResource.cs

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