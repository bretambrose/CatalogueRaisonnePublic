using System;

using CRShared;

namespace CRClientShared
{
	[SlashCommand( "Social_Ignore" )]
	public class CIgnoreSlashCommand : CSlashCommand
	{
		public CIgnoreSlashCommand()
		{
			PlayerName = "";
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Social; } }

		[SlashParam( ESlashCommandParameterType.Required )]
		public string PlayerName { get; private set; }
	}	

	[SlashCommand( "Social_Unignore" )]
	public class CUnignoreSlashCommand : CSlashCommand
	{
		public CUnignoreSlashCommand()
		{
			PlayerName = "";
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Social; } }

		[SlashParam( ESlashCommandParameterType.Required )]
		public string PlayerName { get; private set; }
	}	
	
	[SlashCommand( "Social_Ignore_List" )]
	public class CIgnoreListSlashCommand : CSlashCommand
	{
		public CIgnoreListSlashCommand()
		{
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Social; } }
	}							
}