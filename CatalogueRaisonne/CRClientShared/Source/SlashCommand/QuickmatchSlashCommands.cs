/*

	QuickmatchSlashCommands.cs

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
