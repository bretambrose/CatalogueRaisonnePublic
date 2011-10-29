using System;

using CRShared;

namespace CRClientShared
{
	[SlashCommand( "Match_Leave" )]
	public class CMatchLeaveSlashCommand : CSlashCommand
	{
		public CMatchLeaveSlashCommand()
		{
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Match; } }
	}	

	[SlashCommand( "Match_Info" )]
	public class CMatchInfoSlashCommand : CSlashCommand
	{
		public CMatchInfoSlashCommand()
		{
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Match; } }
	}	

	[SlashCommand( "Match_Turn_Play_And_Draw_Deck" )]
	public class CMatchTurnPlayAndDrawDeckSlashCommand : CSlashCommand
	{
		public CMatchTurnPlayAndDrawDeckSlashCommand()
		{
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Match; } }

		[SlashParam( ESlashCommandParameterType.Required )]
		public ECardColor Color { get; set; }

		[SlashParam( ESlashCommandParameterType.Required )]
		public ECardValue Value { get; set; }
	}	

	[SlashCommand( "Match_Turn_Play_And_Draw_Discard" )]
	public class CMatchTurnPlayAndDrawDiscardSlashCommand : CSlashCommand
	{
		public CMatchTurnPlayAndDrawDiscardSlashCommand()
		{
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Match; } }

		[SlashParam( ESlashCommandParameterType.Required )]
		public ECardColor Color { get; set; }

		[SlashParam( ESlashCommandParameterType.Required )]
		public ECardValue Value { get; set; }

		[SlashParam( ESlashCommandParameterType.Required )]
		public ECardColor DrawColor { get; set; }
	}	

	[SlashCommand( "Match_Turn_Discard_And_Draw_Deck" )]
	public class CMatchTurnDiscardAndDrawDeckSlashCommand : CSlashCommand
	{
		public CMatchTurnDiscardAndDrawDeckSlashCommand()
		{
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Match; } }

		[SlashParam( ESlashCommandParameterType.Required )]
		public ECardColor Color { get; set; }

		[SlashParam( ESlashCommandParameterType.Required )]
		public ECardValue Value { get; set; }
	}	

	[SlashCommand( "Match_Turn_Discard_And_Draw_Discard" )]
	public class CMatchTurnDiscardAndDrawDiscardSlashCommand : CSlashCommand
	{
		public CMatchTurnDiscardAndDrawDiscardSlashCommand()
		{
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Match; } }

		[SlashParam( ESlashCommandParameterType.Required )]
		public ECardColor Color { get; set; }

		[SlashParam( ESlashCommandParameterType.Required )]
		public ECardValue Value { get; set; }

		[SlashParam( ESlashCommandParameterType.Required )]
		public ECardColor DrawColor { get; set; }
	}	

	[SlashCommand( "Match_Turn_Pass_Cards" )]
	public class CMatchTurnPassCardsSlashCommand : CSlashCommand
	{
		public CMatchTurnPassCardsSlashCommand()
		{
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Match; } }

		[SlashParam( ESlashCommandParameterType.Required )]
		public ECardColor Color1 { get; set; }

		[SlashParam( ESlashCommandParameterType.Required )]
		public ECardValue Value1 { get; set; }

		[SlashParam( ESlashCommandParameterType.Required )]
		public ECardColor Color2 { get; set; }

		[SlashParam( ESlashCommandParameterType.Required )]
		public ECardValue Value2 { get; set; }
	}	

	[SlashCommand( "Match_End_Turn" )]
	public class CMatchEndTurnSlashCommand : CSlashCommand
	{
		public CMatchEndTurnSlashCommand()
		{
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Match; } }
	}	

	[SlashCommand( "Match_Reset_Turn" )]
	public class CMatchResetTurnSlashCommand : CSlashCommand
	{
		public CMatchResetTurnSlashCommand()
		{
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Match; } }
	}	

	[SlashCommand( "Match_Play_Shortform" )]
	public class CMatchPlayShortformSlashCommand : CSlashCommand
	{
		public CMatchPlayShortformSlashCommand()
		{
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Match; } }

		[SlashParam( ESlashCommandParameterType.Required )]
		public string CardShortform { get; set; }

		[SlashParam( ESlashCommandParameterType.Required )]
		public string DrawCode { get; set; }
	}	

	[SlashCommand( "Match_Discard_Shortform" )]
	public class CMatchDiscardShortformSlashCommand : CSlashCommand
	{
		public CMatchDiscardShortformSlashCommand()
		{
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Match; } }

		[SlashParam( ESlashCommandParameterType.Required )]
		public string CardShortform { get; set; }

		[SlashParam( ESlashCommandParameterType.Required )]
		public string DrawCode { get; set; }
	}

	[SlashCommand( "Match_Pass_Shortform" )]
	public class CMatchPassShortformSlashCommand : CSlashCommand
	{
		public CMatchPassShortformSlashCommand()
		{
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Match; } }

		[SlashParam( ESlashCommandParameterType.Required )]
		public string CardShortform1 { get; set; }

		[SlashParam( ESlashCommandParameterType.Required )]
		public string CardShortform2 { get; set; }
	}

	[SlashCommand( "Match_Score" )]
	public class CMatchScoreSlashCommand : CSlashCommand
	{
		public CMatchScoreSlashCommand()
		{
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Match; } }
	}

	[SlashCommand( "Match_Continue" )]
	public class CMatchContinueSlashCommand : CSlashCommand
	{
		public CMatchContinueSlashCommand()
		{
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Match; } }

		[SlashParam( ESlashCommandParameterType.Required )]
		public bool Continue { get; set; }
	}
}
