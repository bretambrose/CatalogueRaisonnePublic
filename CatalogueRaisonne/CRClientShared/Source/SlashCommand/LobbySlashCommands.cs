/*

	LobbySlashCommands.cs

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
	[SlashCommand( "Lobby_Create" )]
	public class CCreateLobbySlashCommand : CSlashCommand
	{
		public CCreateLobbySlashCommand()
		{
			GameMode = EGameModeType.Two_Players;
			Password = null;
			GameDescription = null;
			AllowObservers = true;
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Lobby; } }

		[SlashParam( ESlashCommandParameterType.Required )]
		public string GameDescription { get; private set; }
		
		[SlashParam( ESlashCommandParameterType.Required )]
		public EGameModeType GameMode { get; private set; }

		[SlashParam( ESlashCommandParameterType.Optional )]
		public bool AllowObservers { get; private set; }

		[SlashParam( ESlashCommandParameterType.Optional )]
		public string Password { get; private set; }
	}	

	[SlashCommand( "Lobby_Join_By_Player" )]
	public class CJoinLobbyByPlayerSlashCommand : CSlashCommand
	{
		public CJoinLobbyByPlayerSlashCommand()
		{
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Lobby; } }
		
		[SlashParam( ESlashCommandParameterType.Required )]
		public string PlayerName { get; private set; }

		[SlashParam( ESlashCommandParameterType.Optional )]
		public string Password { get; private set; }
	}	

	[SlashCommand( "Lobby_Join_By_ID" )]
	public class CJoinLobbyByIDSlashCommand : CSlashCommand
	{
		public CJoinLobbyByIDSlashCommand()
		{
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Lobby; } }
		
		[SlashParam( ESlashCommandParameterType.Required )]
		public uint LobbyID { get; private set; }
	}	

	[SlashCommand( "Lobby_Info" )]
	public class CLobbyInfoSlashCommand : CSlashCommand
	{
		public CLobbyInfoSlashCommand()
		{
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Lobby; } }
		
	}	

	[SlashCommand( "Lobby_Leave" )]
	public class CLobbyLeaveSlashCommand : CSlashCommand
	{
		public CLobbyLeaveSlashCommand()
		{
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Lobby; } }
		
	}
	
	[SlashCommand( "Lobby_Destroy" )]
	public class CLobbyDestroySlashCommand : CSlashCommand
	{
		public CLobbyDestroySlashCommand()
		{
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Lobby; } }
		
	}	
	
	[SlashCommand( "Lobby_Kick" )]
	public class CLobbyKickSlashCommand : CSlashCommand
	{
		public CLobbyKickSlashCommand()
		{
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Lobby; } }
		
		[SlashParam( ESlashCommandParameterType.Required )]
		public string PlayerName { get; private set; }

	}	
	
	[SlashCommand( "Lobby_Ban" )]
	public class CLobbyBanSlashCommand : CSlashCommand
	{
		public CLobbyBanSlashCommand()
		{
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Lobby; } }
		
		[SlashParam( ESlashCommandParameterType.Required )]
		public string PlayerName { get; private set; }

	}	
	
	[SlashCommand( "Lobby_Unban" )]
	public class CLobbyUnbanSlashCommand : CSlashCommand
	{
		public CLobbyUnbanSlashCommand()
		{
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Lobby; } }
		
		[SlashParam( ESlashCommandParameterType.Required )]
		public string PlayerName { get; private set; }

	}	
	
	[SlashCommand( "Lobby_State" )]
	public class CChangeLobbyPlayerStateSlashCommand : CSlashCommand
	{
		public CChangeLobbyPlayerStateSlashCommand()
		{
			State = ELobbyMemberState.Not_Ready;
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Lobby; } }
		
		[SlashParam( ESlashCommandParameterType.Required )]
		public ELobbyMemberState State { get; private set; }

	}	
	
	[SlashCommand( "Lobby_Move" )]
	public class CLobbyMoveSlashCommand : CSlashCommand
	{
		public CLobbyMoveSlashCommand()
		{
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Lobby; } }
		
		[SlashParam( ESlashCommandParameterType.Required )]
		public string PlayerName { get; private set; }

		[SlashParam( ESlashCommandParameterType.Required )]
		public ELobbyMemberType DestinationCategory { get; private set; }

		[SlashParam( ESlashCommandParameterType.Required )]
		public uint DestinationIndex { get; private set; }

	}	
	
	[SlashCommand( "Lobby_Start_Match" )]
	public class CLobbyStartMatchSlashCommand : CSlashCommand
	{
		public CLobbyStartMatchSlashCommand()
		{
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Lobby; } }
	}	
	
	[SlashCommand( "Lobby_Change_Game_Count" )]
	public class CLobbyChangeGameCountSlashCommand : CSlashCommand
	{
		public CLobbyChangeGameCountSlashCommand()
		{
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Lobby; } }
		
		[SlashParam( ESlashCommandParameterType.Required )]
		public uint GameCount { get; private set; }

	}										
}