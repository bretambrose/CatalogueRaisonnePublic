using System;

namespace CRShared
{
	[SlashCommand( "Help", AllowSymbols = true )]
	public class CHelpSlashCommand : CSlashCommand
	{
		public CHelpSlashCommand()
		{
			CommandGroupOrName = "";
		}
						
		[SlashParam( ESlashCommandParameterType.Optional )]
		public string CommandGroupOrName { get; private set; }
	}

	[SlashCommand( "SetLogLevel" )]
	public class CSetLogLevelSlashCommand : CSlashCommand
	{
		public CSetLogLevelSlashCommand()
		{
			Level = ELogLevel.Medium;
		}

		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Logging; } }
						
		[SlashParam( ESlashCommandParameterType.Required )]
		public ELogLevel Level { get; private set; }
	}

	[SlashCommand( "Crash" )]
	public class CCrashSlashCommand : CSlashCommand
	{
		public CCrashSlashCommand()
		{
		}

		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.ProcessControl; } }
						
	}
}