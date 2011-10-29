using System;

using CRShared;

namespace CRClientShared
{
	[SlashCommand( "Quit" )]
	public class CQuitClientSlashCommand : CSlashCommand
	{
		public CQuitClientSlashCommand()
		{
		}

		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.ProcessControl; } }						
	}	
}