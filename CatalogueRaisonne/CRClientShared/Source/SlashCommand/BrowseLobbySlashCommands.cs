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