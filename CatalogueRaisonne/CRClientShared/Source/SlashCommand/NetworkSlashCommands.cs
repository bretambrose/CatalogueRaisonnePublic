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
