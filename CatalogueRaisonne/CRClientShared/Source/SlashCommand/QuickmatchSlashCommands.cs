using System;

using CRShared;

namespace CRClientShared
{
	[SlashCommand( "Create_Quickmatch" )]
	public class CCreateQuickmatchSlashCommand : CSlashCommand
	{
		public CCreateQuickmatchSlashCommand()
		{
			PlayerName = null;
			GameCount = 0;
			AllowObservers = true;
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Quickmatch; } }
		
		[SlashParam( ESlashCommandParameterType.Required )]
		public string PlayerName { get; private set; }

		[SlashParam( ESlashCommandParameterType.Optional )]
		public uint GameCount { get; private set; }

		[SlashParam( ESlashCommandParameterType.Optional )]
		public bool AllowObservers { get; private set; }

	}	

	public enum EQuickmatchInviteResponse
	{
		Accept,
		Decline
	}

	[SlashCommand( "Respond_To_Quickmatch_Request" )]
	public class CRespondToQuickmatchRequestSlashCommand : CSlashCommand
	{
		public CRespondToQuickmatchRequestSlashCommand()
		{
			Response = EQuickmatchInviteResponse.Decline;
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Quickmatch; } }
		
		[SlashParam( ESlashCommandParameterType.Required )]
		public EQuickmatchInviteResponse Response { get; private set; }

	}	

	[SlashCommand( "Cancel_Quickmatch" )]
	public class CCancelQuickmatchSlashCommand : CSlashCommand
	{
		public CCancelQuickmatchSlashCommand()
		{
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Quickmatch; } }
	}	

	[SlashCommand( "Create_AI_Quickmatch" )]
	public class CCreateAIQuickmatchSlashCommand : CSlashCommand
	{
		public CCreateAIQuickmatchSlashCommand()
		{
			GameCount = 0;
			AllowObservers = true;
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Quickmatch; } }
		
		[SlashParam( ESlashCommandParameterType.Optional )]
		public uint GameCount { get; private set; }

		[SlashParam( ESlashCommandParameterType.Optional )]
		public bool AllowObservers { get; private set; }

	}	
}
