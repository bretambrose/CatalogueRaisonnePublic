using System;

using CRShared;

namespace CRServer
{
	[SlashCommand( "Shutdown" )]
	public class CShutdownServerSlashCommand : CSlashCommand
	{
		public CShutdownServerSlashCommand()
		{
			Delay = 0;
		}

		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.ProcessControl; } }
						
		[SlashParam( ESlashCommandParameterType.Optional )]
		public int Delay { get; private set; }
	}
}