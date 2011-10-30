/*

	MatchUISlashCommands.cs

	(c) Copyright 2010-2011, Bret Ambrose (mailto:bretambrose@gmail.com).

	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program.  If not, see <http://www.gnu.org/licenses/>.
 
*/

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