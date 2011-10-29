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