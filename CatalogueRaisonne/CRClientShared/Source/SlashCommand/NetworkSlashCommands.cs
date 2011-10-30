/*

	NetworkSlashCommands.cs

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

using CRShared;

namespace CRClientShared
{
	[SlashCommand( "Connect" )]
	public class CConnectSlashCommand : CSlashCommand
	{
		public CConnectSlashCommand()
		{
			PlayerName = "";
			ServerAddress = null;
			ServerPort = CSharedSettings.Instance.ListenerPort;
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Network; } }
		
		[SlashParam( ESlashCommandParameterType.Required )]
		public string PlayerName { get; private set; }
		
		[SlashParam( ESlashCommandParameterType.Optional )]
		public string ServerAddress { get; private set; }
		
		[SlashParam( ESlashCommandParameterType.Optional )]
		public int ServerPort { get; private set; }
	}
	
	[SlashCommand( "Disconnect" )]
	public class CDisconnectSlashCommand : CSlashCommand
	{
		public CDisconnectSlashCommand()
		{
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Network; } }
	}
}
