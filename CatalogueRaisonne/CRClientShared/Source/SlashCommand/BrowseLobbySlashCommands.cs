/*

	BrowseLobbySlashCommands.cs

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
	[SlashCommand( "Browse_Lobby_Start" )]
	public class CStartBrowseLobbySlashCommand : CSlashCommand
	{
		public CStartBrowseLobbySlashCommand()
		{
			GameModes = EGameModeType.Invalid;
			MemberTypes = ELobbyMemberType.Player;
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Browse; } }
		
		[SlashParam( ESlashCommandParameterType.Required )]
		public EGameModeType GameModes { get; private set; }

		[SlashParam( ESlashCommandParameterType.Required )]
		public bool JoinFirstAvailable { get; private set; }

		[SlashParam( ESlashCommandParameterType.Optional )]
		public ELobbyMemberType MemberTypes { get; private set; }

	}	

	[SlashCommand( "Browse_Lobby_End" )]
	public class CEndBrowseLobbySlashCommand : CSlashCommand
	{
		public CEndBrowseLobbySlashCommand()
		{
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Browse; } }
	}	

	[SlashCommand( "Browse_Lobby_Next_Set" )]
	public class CBrowseNextLobbySetSlashCommand : CSlashCommand
	{
		public CBrowseNextLobbySetSlashCommand()
		{
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Browse; } }
	}	

	[SlashCommand( "Browse_Lobby_Previous_Set" )]
	public class CBrowsePreviousLobbySetSlashCommand : CSlashCommand
	{
		public CBrowsePreviousLobbySetSlashCommand()
		{
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Browse; } }
	}	

	[SlashCommand( "Browse_Lobby_List" )]
	public class CBrowseLobbyListSlashCommand : CSlashCommand
	{
		public CBrowseLobbyListSlashCommand()
		{
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Browse; } }
	}	
}