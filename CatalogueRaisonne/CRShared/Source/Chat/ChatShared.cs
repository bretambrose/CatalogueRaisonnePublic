/*

	ChatShared.cs

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

namespace CRShared.Chat
{
	public enum EChannelID
	{
		Invalid = 0
	}

	public enum EChannelCreationError
	{
		None,
		InvalidExternalName,
		InvalidInternalName,
		InvalidInternalNamePrefix,
		DuplicateInternalName,
		UnknownPlayer,
		UnableToAutoJoin
	}
	
	public enum EChannelJoinError
	{
		None,
		Banned,
		ChannelFull,
		AlreadyJoined,
		ChannelDoesNotExist,
		NoPermission
	}

	public enum EChannelBroadcastError
	{
		None,
		Gagged,
		UnknownChannel,
		NotInChannel
	}

	public enum EChatNotification
	{
		None,
		Channel_Destroyed,
		Player_Left_Channel,
		Player_Joined_Channel,
		Player_Kicked_From_Channel,
		Player_Gagged,
		Player_Ungagged,
		New_Moderator
	}

	public enum ELeftChannelReason
	{
		None,
		Kicked_By_Mod,
		Kicked_By_Server,
		Self_Request,
		Channel_Shutdown,
		Player_Logoff,
		Swapped_By_Server
	}

	public enum ELeaveChatChannelFailureReason
	{
		None,
		Unknown_Channel,
		Not_Allowed,
		Not_A_Member
	}

	public enum EChatModOperation
	{
		None,
		Kick,
		Gag,
		Ungag,
		Transfer_Moderator
	}

	public enum EChatModOperationError
	{
		None,
		Unknown_Channel,
		Not_Moderator,
		No_Such_Player_In_Channel,
		Unable_To_Transfer_Moderator,
		Player_Already_Gagged,
		Player_Not_Gagged,
		Cannot_Do_That_To_Self
	}

	public enum EChatChannelListMembersError
	{
		None,
		Not_In_Channel,
		Not_Allowed_To_List,
		Unknown_Channel
	}

	public enum ETellError
	{	
		None,
		Unknown_Player,
		Ignored
	}
}
