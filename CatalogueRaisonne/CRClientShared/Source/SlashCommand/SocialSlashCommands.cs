/*

	SocialSlashCommands.cs

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