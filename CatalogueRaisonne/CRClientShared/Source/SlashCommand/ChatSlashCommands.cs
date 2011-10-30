/*

	ChatSlashCommmands.cs

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
using CRClientShared.Chat;

namespace CRClientShared
{
	[SlashCommand( "Chat_Join_Channel" )]
	public class CJoinChatChannelSlashCommand : CSlashCommand
	{
		public CJoinChatChannelSlashCommand()
		{
			ChannelName = "";
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Chat; } }
		
		[SlashParam( ESlashCommandParameterType.Required )]
		public string ChannelName { get; private set; }
	}	

	[SlashCommand( "Chat_Leave_Channel" )]
	public class CLeaveChatChannelSlashCommand : CSlashCommand
	{
		public CLeaveChatChannelSlashCommand()
		{
			ChannelIdentifier = "";
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Chat; } }
		
		[SlashParam( ESlashCommandParameterType.Required )]
		public string ChannelIdentifier { get; private set; }
	}	

	[SlashCommand( "Number" )]
	public class CBroadcastOnChatChannelSlashCommand : CSlashCommand
	{
		public CBroadcastOnChatChannelSlashCommand()
		{
			ChatMessage = "";
		}

		public override void On_Command_Name( string command_name ) 
		{
			int channel_number = Int32.Parse( command_name );
			ChannelNumber = ( EClientChannelNumber ) channel_number;
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Chat; } }
		
		[SlashParam( ESlashCommandParameterType.Required, ConsumeAll=true )]
		public string ChatMessage { get; private set; }

		public EClientChannelNumber ChannelNumber { get; set; }
	}	

	[SlashCommand( "Chat_Mod_Kick" )]
	public class CChatKickSlashCommand : CSlashCommand
	{
		public CChatKickSlashCommand() {}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Chat; } }
		
		[SlashParam( ESlashCommandParameterType.Required )]
		public string ChannelIdentifier { get; private set; }

		[SlashParam( ESlashCommandParameterType.Required )]
		public string PlayerName { get; private set; }
	}	

	[SlashCommand( "Chat_Mod_Gag" )]
	public class CChatGagSlashCommand : CSlashCommand
	{
		public CChatGagSlashCommand() {}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Chat; } }
		
		[SlashParam( ESlashCommandParameterType.Required )]
		public string ChannelIdentifier { get; private set; }

		[SlashParam( ESlashCommandParameterType.Required )]
		public string PlayerName { get; private set; }
	}	

	[SlashCommand( "Chat_Mod_Ungag" )]
	public class CChatUngagSlashCommand : CSlashCommand
	{
		public CChatUngagSlashCommand() {}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Chat; } }
		
		[SlashParam( ESlashCommandParameterType.Required )]
		public string ChannelIdentifier { get; private set; }

		[SlashParam( ESlashCommandParameterType.Required )]
		public string PlayerName { get; private set; }
	}	

	[SlashCommand( "Chat_Mod_Transfer" )]
	public class CChatTransferSlashCommand : CSlashCommand
	{
		public CChatTransferSlashCommand() {}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Chat; } }
		
		[SlashParam( ESlashCommandParameterType.Required )]
		public string ChannelIdentifier { get; private set; }

		[SlashParam( ESlashCommandParameterType.Required )]
		public string PlayerName { get; private set; }
	}	

	[SlashCommand( "Chat_Tell" )]
	public class CChatTellSlashCommand : CSlashCommand
	{
		public CChatTellSlashCommand() {}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Chat; } }
		
		[SlashParam( ESlashCommandParameterType.Required )]
		public string PlayerName { get; private set; }

		[SlashParam( ESlashCommandParameterType.Required, ConsumeAll=true )]
		public string ChatMessage { get; private set; }
	}	

	[SlashCommand( "Chat_Reply" )]
	public class CChatReplySlashCommand : CSlashCommand
	{
		public CChatReplySlashCommand() {}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Chat; } }
		
		[SlashParam( ESlashCommandParameterType.Required, ConsumeAll=true )]
		public string ChatMessage { get; private set; }
	}

	[SlashCommand( "Chat_List_Members" )]
	public class CChatListSlashCommand : CSlashCommand
	{
		public CChatListSlashCommand() 
		{
			ChannelIdentifier = null;
		}
		
		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Chat; } }
		
		[SlashParam( ESlashCommandParameterType.Optional )]
		public string ChannelIdentifier { get; private set; }

	}	

}