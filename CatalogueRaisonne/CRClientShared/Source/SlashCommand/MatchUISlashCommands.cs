using System;

using CRShared;

namespace CRClientShared
{
	[SlashCommand( "Match_UI_Play_Card" )]
	public class CMatchUIPlayCardSlashCommand : CSlashCommand
	{
		public CMatchUIPlayCardSlashCommand()
		{
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.MatchUI; } }

		[SlashParam( ESlashCommandParameterType.Required )]
		public ECardColor Color { get; set; }

		[SlashParam( ESlashCommandParameterType.Required )]
		public ECardValue Value { get; set; }
	}	

	[SlashCommand( "Match_UI_Discard_Card" )]
	public class CMatchUIDiscardCardSlashCommand : CSlashCommand
	{
		public CMatchUIDiscardCardSlashCommand()
		{
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.MatchUI; } }

		[SlashParam( ESlashCommandParameterType.Required )]
		public ECardColor Color { get; set; }

		[SlashParam( ESlashCommandParameterType.Required )]
		public ECardValue Value { get; set; }
	}	

	[SlashCommand( "Match_UI_Draw_From_Deck" )]
	public class CMatchUIDrawFromDeckSlashCommand : CSlashCommand
	{
		public CMatchUIDrawFromDeckSlashCommand()
		{
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.MatchUI; } }
	}	

	[SlashCommand( "Match_UI_Draw_From_Discard" )]
	public class CMatchUIDrawFromDiscardSlashCommand : CSlashCommand
	{
		public CMatchUIDrawFromDiscardSlashCommand()
		{
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.MatchUI; } }

		[SlashParam( ESlashCommandParameterType.Required )]
		public ECardColor Color { get; set; }
	}	

}