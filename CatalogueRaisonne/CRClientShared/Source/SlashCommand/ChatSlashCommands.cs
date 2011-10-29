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