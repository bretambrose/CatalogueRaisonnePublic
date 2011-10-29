using System;
using System.Collections.Generic;
using System.Linq;

using CRShared;
using CRShared.Chat;

namespace CRServer.Chat
{
	public class CChatServer
	{
		// Construction
		private CChatServer() {}
		static CChatServer() {}
		
		// Methods
		// Public interface

		// Non-public interface
		// Network Message handlers
		[NetworkMessageHandler]
		private void Handle_Remove_Player_From_Chat_Server_Message( CRemovePlayerFromChatServerMessage message )
		{
			Instance.Remove_Player( message.SessionID );
		}

		[NetworkMessageHandler]
		private void Handle_Announce_Player_To_Chat_Server_Message( CAnnouncePlayerToChatServerRequest message )
		{
			bool announce_success = Instance.Announce_Player( message.SessionID, message.PersistenceID, message.Name, message.IgnoreList );
			CAnnouncePlayerToChatServerResponse response = new CAnnouncePlayerToChatServerResponse( message.RequestID, announce_success );
			Instance.Send_Message_To_Game_Server( response );

			CLog.Log( ELoggingChannel.Chat, ELogLevel.Medium, String.Format( "Player {0} on session {1} announced to chat server, success = {2}.", message.Name, (int) message.SessionID, announce_success ) );		
		}

		[NetworkMessageHandler]
		private void Handle_Create_Chat_Channel_Server_Message( CCreateChatChannelRequestServerMessage message )
		{
			CCreateChatChannelResponseServerMessage create_response_msg = new CCreateChatChannelResponseServerMessage( message.RequestID );
			Instance.Request_Create_Channel( message.Config, create_response_msg );
			Instance.Send_Message_To_Game_Server( create_response_msg );

			CLog.Log( ELoggingChannel.Chat, ELogLevel.Medium, String.Format( "Attempted to create chat channel {0}, error = {1}.", message.Config.InternalName, create_response_msg.Error.ToString() ) );		
		}

		[NetworkMessageHandler]
		private void Handle_Join_Chat_Channel_Server_Message( CJoinChatChannelRequestServerMessage message )
		{
			EChannelJoinError join_error = Instance.Request_Join_Channel( message.PlayerID, message.ChannelID, message.RemoveMask );
			CJoinChatChannelResponseServerMessage join_id_response_msg = new CJoinChatChannelResponseServerMessage( message.RequestID, join_error );
			Instance.Send_Message_To_Game_Server( join_id_response_msg );

			CLog.Log( ELoggingChannel.Chat, ELogLevel.Medium, String.Format( "Player {0} attempted to join chat channel {1}, error = {2}.", 
																								  Instance.Get_Player_Name_By_Persistence_ID( message.PlayerID ), 
																								  Instance.Get_Channel_Name_By_Channel_ID( message.ChannelID ), 
																								  join_error.ToString() ) );		
		}

		[NetworkMessageHandler]
		private void Handle_Remove_From_Chat_Channel_Server_Message( CRemoveFromChatChannelServerMessage message )
		{
			CLog.Log( ELoggingChannel.Chat, ELogLevel.Medium, String.Format( "Player {0} kicked from channel {1} by server.", 
																								  Instance.Get_Player_Name_By_Persistence_ID( message.PlayerID ), 
																								  Instance.Get_Channel_Name_By_Channel_ID( message.ChannelID ) ) );		

			Instance.Remove_From_Channel( message.PlayerID, message.ChannelID, ELeftChannelReason.Kicked_By_Server, EPersistenceID.Invalid );
		}

		[NetworkMessageHandler]
		private void Handle_Destroy_Chat_Channel_Server_Message( CDestroyChatChannelServerMessage message )
		{
			CLog.Log( ELoggingChannel.Chat, ELogLevel.Medium, String.Format( "Channel {0} destroyed by server.", Instance.Get_Channel_Name_By_Channel_ID( message.ChannelID ) ) );		

			Instance.Request_Destroy_Channel( message.ChannelID );
		}

		[NetworkMessageHandler]
		private void Handle_Add_Player_To_Ignore_List_Server_Message( CAddPlayerToChatIgnoreListServerMessage message )
		{
			CLog.Log( ELoggingChannel.Chat, ELogLevel.Medium, String.Format( "Player {0} is now ignoring player {1} in chat.", 
																								  Instance.Get_Player_Name_By_Persistence_ID( message.PlayerID ),
																								  Instance.Get_Player_Name_By_Persistence_ID( message.IgnoredID ) ) );		

			Instance.Add_Player_To_Ignore_List( message.PlayerID, message.IgnoredID );
		}

		[NetworkMessageHandler]
		private void Handle_Remove_Player_From_Ignore_List_Server_Message( CRemovePlayerFromChatIgnoreListServerMessage message )
		{
			CLog.Log( ELoggingChannel.Chat, ELogLevel.Medium, String.Format( "Player {0} is no longer ignoring player {1} in chat.", 
																								  Instance.Get_Player_Name_By_Persistence_ID( message.PlayerID ),
																								  Instance.Get_Player_Name_By_Persistence_ID( message.IgnoredID ) ) );		

			Instance.Remove_Player_From_Ignore_List( message.PlayerID, message.IgnoredID );
		}

		[NetworkMessageHandler]
		private void Handle_Create_Or_Join_Chat_Channel_Client_Message( CCreateOrJoinChatChannelMessage message, EPersistenceID player_id )
		{
			CJoinChatChannelResultMessage result_msg = new CJoinChatChannelResultMessage();
			Instance.Request_Create_Or_Join_Channel( player_id, message.ChannelName, result_msg );
			Instance.Send_Message_To_Player( result_msg, player_id );

			CLog.Log( ELoggingChannel.Chat, ELogLevel.Medium, String.Format( "Player {0} creater/join user channel {1} with create error = {2}, join error = {3}.", 
																								  Instance.Get_Player_Name_By_Persistence_ID( player_id ),
																								  message.ChannelName,
																								  result_msg.CreateError.ToString(),
																								  result_msg.JoinError.ToString() ) );		


		}

		[NetworkMessageHandler]
		private void Handle_Broadcast_To_Chat_Channel_Client_Message( CBroadcastToChatChannelMessage message, EPersistenceID player_id )
		{
			EChannelBroadcastError error = Instance.Broadcast_To_Channel( player_id, message.ChannelID, message.ChatMessage );
			if ( error != EChannelBroadcastError.None )
			{
				Instance.Send_Message_To_Player( new CBroadcastFailureMessage( error ), player_id );
			}

			CLog.Log( ELoggingChannel.Chat, ELogLevel.Medium, String.Format( "Player {0} broadcast '{1}' on channel {2} with error = {3}.", 
																								  Instance.Get_Player_Name_By_Persistence_ID( player_id ),
																								  message.ChatMessage,
																								  Instance.Get_Channel_Name_By_Channel_ID( message.ChannelID ),
																								  error.ToString() ) );		

		}

		[NetworkMessageHandler]
		private void Handle_Leave_Chat_Channel_Client_Message( CLeaveChatChannelMessage message, EPersistenceID player_id )
		{
			CLog.Log( ELoggingChannel.Chat, ELogLevel.Medium, String.Format( "Player {0} requesting to leave channel {1}.", 
																								  Instance.Get_Player_Name_By_Persistence_ID( player_id ),
																								  Instance.Get_Channel_Name_By_Channel_ID( message.ChannelID ) ) );		

			Instance.Remove_From_Channel( player_id, message.ChannelID, ELeftChannelReason.Self_Request, EPersistenceID.Invalid );
		}

		[NetworkMessageHandler]
		private void Handle_Player_Tell_Request( CPlayerTellRequest message, EPersistenceID player_id )
		{
			CPlayerTellResponse tell_response = new CPlayerTellResponse( message.RequestID );
			CChatPlayer target_player = Instance.Get_Player_By_Persistence_ID( Instance.Get_Persistence_ID_By_Name( message.PlayerName ) ); 
			if ( target_player == null )
			{
				tell_response.Error = ETellError.Unknown_Player;
			}
			else if ( Instance.Is_Ignoring_Player( target_player.PersistenceID, player_id ) )
			{
				tell_response.Error = ETellError.Ignored;
			}
			else
			{
				Instance.Send_Message_To_Player( new CPlayerTellMessage( Instance.Get_Player_Name_By_Persistence_ID( player_id ), message.ChatMessage ), target_player.PersistenceID );
			}

			Instance.Send_Message_To_Player( tell_response, player_id );

			CLog.Log( ELoggingChannel.Chat, ELogLevel.Medium, String.Format( "Player {0} attempted to tell '{1}' to player {2} with result = {3}.", 
																								  Instance.Get_Player_Name_By_Persistence_ID( player_id ),
																								  message.ChatMessage,
																								  message.PlayerName,
																								  tell_response.ToString() ) );		

		}

		[NetworkMessageHandler]
		private void Handle_Chat_Mod_Operation_Client_Message( CChatModOperationMessage mod_msg, EPersistenceID player_id )
		{
			CServerChatChannel channel = Get_Channel_By_Channel_ID( mod_msg.ChannelID );
			if ( channel == null )
			{
				Send_Message_To_Player( new CChatModOperationErrorMessage( EChatModOperationError.Unknown_Channel ), player_id );
				return;
			}

			if ( channel.Moderator != player_id )
			{
				Send_Message_To_Player( new CChatModOperationErrorMessage( EChatModOperationError.Not_Moderator ), player_id );
				return;
			}

			EPersistenceID target_id = Get_Persistence_ID_By_Name( mod_msg.PlayerName );
			if ( target_id == EPersistenceID.Invalid || !channel.Is_Member( target_id ) )
			{
				Send_Message_To_Player( new CChatModOperationErrorMessage( EChatModOperationError.No_Such_Player_In_Channel ), player_id );
				return;
			}

			if ( target_id == player_id )
			{
				Send_Message_To_Player( new CChatModOperationErrorMessage( EChatModOperationError.Cannot_Do_That_To_Self ), player_id );
				return;
			}

			switch ( mod_msg.Operation )
			{
				case EChatModOperation.Gag:
					if ( channel.Gag_Player( target_id ) )
					{
						Send_Notification_To_Channel( channel.ID, player_id, target_id, EChatNotification.Player_Gagged, EPersistenceID.Invalid );
					}
					else
					{
						Send_Message_To_Player( new CChatModOperationErrorMessage( EChatModOperationError.Player_Already_Gagged ), player_id );
					}
					break;

				case EChatModOperation.Ungag:
					if ( channel.Ungag_Player( target_id ) )
					{
						Send_Notification_To_Channel( channel.ID, player_id, target_id, EChatNotification.Player_Ungagged, EPersistenceID.Invalid );
					}
					else
					{
						Send_Message_To_Player( new CChatModOperationErrorMessage( EChatModOperationError.Player_Not_Gagged ), player_id );
					}
					break;

				case EChatModOperation.Kick:
					Remove_From_Channel( target_id, channel.ID, ELeftChannelReason.Kicked_By_Mod, channel.Moderator );
					break;

				case EChatModOperation.Transfer_Moderator:
					if ( channel.Transfer_Moderator( target_id ) )
					{
						Send_Notification_To_Channel( channel.ID, player_id, target_id, EChatNotification.New_Moderator, EPersistenceID.Invalid );
					}
					else
					{
						Send_Message_To_Player( new CChatModOperationErrorMessage( EChatModOperationError.Unable_To_Transfer_Moderator ), player_id );
					}
					break;
			}
		}

		private void Send_Message_To_Game_Server( CNetworkMessage message )
		{
			CServerLogicalThread.Instance.Send_Message( message, ESessionID.Loopback );
		}

		private void Send_Message_To_Player( CNetworkMessage message, EPersistenceID persistence_id )
		{
			CChatPlayer player = Get_Player_By_Persistence_ID( persistence_id );
			if ( player == null )
			{
				return;
			}

			CServerLogicalThread.Instance.Send_Message( message, player.SessionID );
		}
		
		private void Send_Message_To_Channel( EChannelID channel_id, CNetworkMessage message, EPersistenceID skip_id )
		{
			CServerChatChannel channel = Get_Channel_By_Channel_ID( channel_id );
			if ( channel == null )
			{
				return;
			}

			channel.Members.Where( n => n != skip_id ).Apply( pid => Send_Message_To_Player( message, pid ) );
		}
		
		private void Send_Notification_To_Channel( EChannelID channel_id, EPersistenceID player_id, EChatNotification chat_event, EPersistenceID skip_id )
		{
			Send_Notification_To_Channel( channel_id, EPersistenceID.Invalid, player_id, chat_event, skip_id );
		}

		private void Send_Notification_To_Channel( EChannelID channel_id, EPersistenceID source_player_id, EPersistenceID target_player_id, EChatNotification chat_event, EPersistenceID skip_id )
		{
			string source_player_name = Get_Player_Name_By_Persistence_ID( source_player_id );
			string target_player_name = Get_Player_Name_By_Persistence_ID( target_player_id );

			CChatClientNotificationMessage notification_message = new CChatClientNotificationMessage( channel_id, target_player_name, target_player_id, chat_event, source_player_name );
			Send_Message_To_Channel( channel_id, notification_message, skip_id );
		}
				
		private bool Announce_Player( ESessionID session_id, EPersistenceID persistence_id, string name, List< EPersistenceID > ignore_list )
		{
			if ( Get_Player_By_Session_ID( session_id ) != null || Get_Player_By_Persistence_ID( persistence_id ) != null )
			{	
				return false;
			}
			
			CChatPlayer new_player = new CChatPlayer( persistence_id, session_id, name, ignore_list );
			m_Players.Add( persistence_id, new_player );
			m_ActiveSessions.Add( session_id, persistence_id );
			m_PlayersByName.Add( name.ToUpper(), persistence_id );
			return true;
		}
		
		private void Remove_Player( ESessionID session_id )
		{
			CChatPlayer player = Get_Player_By_Session_ID( session_id );
			if ( player == null )
			{
				return;
			}
			
			List< EChannelID > channels = player.Channels.ToList();
			channels.Apply( channel_id => Remove_From_Channel( player.PersistenceID, channel_id, ELeftChannelReason.Player_Logoff, EPersistenceID.Invalid ) );

			m_ActiveSessions.Remove( player.SessionID );
			m_Players.Remove( player.PersistenceID );
			m_PlayersByName.Remove( player.Name.ToUpper() );

			CLog.Log( ELoggingChannel.Chat, ELogLevel.Medium, String.Format( "Player {0} on session {1} removed from chat server.", player.Name, (int) session_id ) );		
		}
		
		private CChatPlayer Get_Player_By_Session_ID( ESessionID session_id )
		{
			EPersistenceID persistence_id = EPersistenceID.Invalid;
			if ( !m_ActiveSessions.TryGetValue( session_id, out persistence_id ) )
			{
				return null;
			}
			
			return Get_Player_By_Persistence_ID( persistence_id );
		}
		
		private CChatPlayer Get_Player_By_Persistence_ID( EPersistenceID persistence_id )
		{
			CChatPlayer player = null;
			if ( !m_Players.TryGetValue( persistence_id, out player ) )
			{
				return null;
			}
		
			return player;
		}
		
		private void Request_Create_Channel( CChatChannelConfig channel_config, CCreateChatChannelResponseServerMessage response_msg )
		{
			response_msg.Error = channel_config.Make_Valid();
			if ( response_msg.Error != EChannelCreationError.None )
			{
				return;
			}
			
			if ( channel_config.CreatorID != EPersistenceID.Invalid )
			{
				if ( Get_Player_By_Persistence_ID( channel_config.CreatorID ) == null )
				{
					response_msg.Error = EChannelCreationError.UnknownPlayer;
					return;
				}
			}

			if ( Get_Channel_By_Internal_Name( channel_config.InternalName ) != null )
			{
				response_msg.Error = EChannelCreationError.DuplicateInternalName;
				return;
			}
			
			EChannelID channel_id = Allocate_Channel_ID();
			CServerChatChannel new_channel = new CServerChatChannel( channel_id, channel_config );
			
			m_Channels.Add( channel_id, new_channel );
			m_ChannelInternalNames.Add( channel_config.InternalName.ToUpper(), channel_id );
			
			response_msg.ChannelID = channel_id;
			response_msg.ChannelName = channel_config.ExternalName;
			
			if ( channel_config.CreatorID != EPersistenceID.Invalid )
			{
				if ( Request_Join_Channel( channel_config.CreatorID, channel_id, EChannelGameProperties.None ) != EChannelJoinError.None )
				{
					response_msg.Error = EChannelCreationError.UnableToAutoJoin;
					Request_Destroy_Channel( channel_id );
					return;
				}
			}	
		}
		
		private EChannelJoinError Request_Join_Channel( EPersistenceID player_id, EChannelID channel_id, EChannelGameProperties remove_mask )
		{
			Remove_Player_From_Channels( player_id, remove_mask );

			CServerChatChannel channel = Get_Channel_By_Channel_ID( channel_id );
			if ( channel == null )
			{
				return EChannelJoinError.ChannelDoesNotExist;
			}

			EChannelJoinError error = channel.Join_Channel( player_id );
			if ( error == EChannelJoinError.None )
			{
				channel.Update_Moderator();
				Add_Channel_To_Player( player_id, channel_id );

				CJoinChatChannelResultMessage result_msg = new CJoinChatChannelResultMessage();
				result_msg.ChannelID = channel_id;
				result_msg.ChannelName = channel.ExternalName;
				result_msg.AnnounceJoinLeave = channel.ShouldAnnounceJoinLeave;

				channel.Members.Apply( n => result_msg.Add_Member( n ) );
				channel.Gagged.Apply( n => result_msg.Add_Gagged( n ) );

				Send_Notification_To_Channel( channel_id, player_id, EChatNotification.Player_Joined_Channel, player_id );

				if ( channel.Update_Moderator() )
				{
					Send_Notification_To_Channel( channel.ID, channel.Moderator, EChatNotification.New_Moderator, EPersistenceID.Invalid );
				}

				result_msg.Moderator = channel.Moderator;
				Send_Message_To_Player( result_msg, player_id );
			}

			return error;
		}

		private void Remove_Player_From_Channels( EPersistenceID player_id, EChannelGameProperties remove_mask )
		{
			CChatPlayer player = Get_Player_By_Persistence_ID( player_id );
			if ( player == null || remove_mask == EChannelGameProperties.None )
			{
				return;
			}

			List< EChannelID > remove_channels = new List< EChannelID >();

			player.Channels.Where( channel_id => ( Get_Channel_By_Channel_ID( channel_id ).GameProperties & remove_mask ) != 0 )
								.Apply( cid => remove_channels.Add( cid ) );
			remove_channels.Apply( n => Remove_From_Channel( player_id, n, ELeftChannelReason.Swapped_By_Server, EPersistenceID.Invalid ) );
		}

		private void Request_Create_Or_Join_Channel( EPersistenceID player_id, string channel_name, CJoinChatChannelResultMessage result_msg )
		{
			if ( Get_Player_By_Persistence_ID( player_id ) == null )
			{
				result_msg.CreateError = EChannelCreationError.UnknownPlayer;
				return;
			}

			if ( !CServerChatChannel.Is_Client_Channel_Name( channel_name ) )
			{
				result_msg.CreateError = EChannelCreationError.InvalidInternalName;
				return;
			}

			CServerChatChannel channel = Get_Channel_By_Internal_Name( channel_name );
			if ( channel == null )
			{
				CChatChannelConfig channel_config = new CChatChannelConfig( channel_name, channel_name, player_id, EChannelGameProperties.User );
				channel_config.DestroyWhenEmpty = true;
				channel_config.AllowsModeration = true;

				result_msg.CreateError = channel_config.Make_Valid();
				if ( result_msg.CreateError != EChannelCreationError.None )
				{
					return;
				}

				EChannelID channel_id = Allocate_Channel_ID();
				channel = new CServerChatChannel( channel_id, channel_config );
			
				m_Channels.Add( channel_id, channel );
				m_ChannelInternalNames.Add( channel_config.InternalName.ToUpper(), channel_id );			
			}

			if ( channel.IsServerChannel )
			{
				result_msg.JoinError = EChannelJoinError.NoPermission;
				return;
			}

			result_msg.JoinError = channel.Join_Channel( player_id );
			if ( result_msg.JoinError == EChannelJoinError.None )
			{
				channel.Update_Moderator();
				Add_Channel_To_Player( player_id, channel.ID );

				result_msg.ChannelID = channel.ID;
				result_msg.ChannelName = channel.ExternalName;
				result_msg.AnnounceJoinLeave = channel.ShouldAnnounceJoinLeave;
				channel.Members.Apply( id => result_msg.Add_Member( id ) );
				channel.Gagged.Apply( id => result_msg.Add_Gagged( id ) );
	
				Send_Notification_To_Channel( channel.ID, player_id, EChatNotification.Player_Joined_Channel, player_id );

				if ( channel.Update_Moderator() )
				{
					Send_Notification_To_Channel( channel.ID, channel.Moderator, EChatNotification.New_Moderator, EPersistenceID.Invalid );
				}

				result_msg.Moderator = channel.Moderator;
			}
			else
			{
				Request_Destroy_Channel( channel.ID );
			}
		}

		private void Request_Destroy_Channel( EChannelID channel_id )
		{
			CServerChatChannel channel = Get_Channel_By_Channel_ID( channel_id );
			if ( channel == null )
			{
				return;
			}

			channel.IsBeingDestroyed = true;

			List< EPersistenceID > members = channel.Members.ToList();
			members.Apply( player_id => Remove_From_Channel( player_id, channel_id, ELeftChannelReason.Channel_Shutdown, EPersistenceID.Invalid ) );

			m_Channels.Remove( channel_id );
			m_ChannelInternalNames.Remove( channel.InternalName.ToUpper() );
		}

		private EChannelBroadcastError Broadcast_To_Channel( EPersistenceID player_id, EChannelID channel_id, string chat_message )
		{
			CChatPlayer player = Get_Player_By_Persistence_ID( player_id );
			if ( player == null )
			{
				return EChannelBroadcastError.None;
			}

			CServerChatChannel channel = Get_Channel_By_Channel_ID( channel_id );
			if ( channel == null )
			{
				return EChannelBroadcastError.UnknownChannel;
			}

			EChannelBroadcastError error = channel.Check_Broadcast_Status( player_id );
			if ( error != EChannelBroadcastError.None )
			{
				return error;
			}

			CPlayerChatMessage chat_msg = new CPlayerChatMessage( channel_id, player.Name, chat_message );
			channel.Members.Where( member_id => !Is_Ignoring_Player( member_id, player_id ) ).Apply( pid => Send_Message_To_Player( chat_msg, pid ) );

			return EChannelBroadcastError.None;
		}

		private bool Remove_From_Channel( EPersistenceID player_id, EChannelID channel_id, ELeftChannelReason reason, EPersistenceID source_player_id )
		{
			CServerChatChannel channel = Get_Channel_By_Channel_ID( channel_id );
			if ( channel == null )
			{
				Send_Message_To_Player( new CLeaveChatChannelFailureMessage( ELeaveChatChannelFailureReason.Unknown_Channel ), player_id );
				return false;
			}

			if ( reason == ELeftChannelReason.Self_Request && channel.IsMembershipClientLocked )
			{
				Send_Message_To_Player( new CLeaveChatChannelFailureMessage( ELeaveChatChannelFailureReason.Not_Allowed ), player_id );
				return false;
			}

			if ( !channel.Leave_Channel( player_id ) )
			{
				Send_Message_To_Player( new CLeaveChatChannelFailureMessage( ELeaveChatChannelFailureReason.Not_A_Member ), player_id );
				return false;
			}

			if ( reason != ELeftChannelReason.Channel_Shutdown )
			{
				Send_Notification_To_Channel( channel_id, source_player_id, player_id, Compute_Notification_From_Leave_Reason( reason ), player_id );
			}

			if ( channel.Update_Moderator() && reason != ELeftChannelReason.Channel_Shutdown )
			{
				Send_Notification_To_Channel( channel.ID, channel.Moderator, EChatNotification.New_Moderator, EPersistenceID.Invalid );
			}

			Remove_Channel_From_Player( player_id, channel_id );
			Send_Message_To_Player( new CLeftChatChannelMessage( channel_id, reason, Get_Player_Name_By_Persistence_ID( source_player_id ) ), player_id );

			if ( channel.MemberCount == 0 && channel.ShouldDestroyWhenEmpty && !channel.IsBeingDestroyed )
			{
				Request_Destroy_Channel( channel.ID );
			}

			return true;
		}

		private EChatNotification Compute_Notification_From_Leave_Reason( ELeftChannelReason reason )
		{
			switch ( reason )
			{
				case ELeftChannelReason.Channel_Shutdown:
				case ELeftChannelReason.Self_Request:
				case ELeftChannelReason.Player_Logoff:
				case ELeftChannelReason.Swapped_By_Server:
					return EChatNotification.Player_Left_Channel;

				case ELeftChannelReason.Kicked_By_Mod:
				case ELeftChannelReason.Kicked_By_Server:
					return EChatNotification.Player_Kicked_From_Channel;
			}

			return EChatNotification.None;
		}

		private CServerChatChannel Get_Channel_By_Internal_Name( string internal_name )
		{
			EChannelID channel_id = EChannelID.Invalid;
			if ( !m_ChannelInternalNames.TryGetValue( internal_name.ToUpper(), out channel_id ) )
			{
				return null;
			}
			
			return Get_Channel_By_Channel_ID( channel_id );		
		}
		
		private CServerChatChannel Get_Channel_By_Channel_ID( EChannelID channel_id )
		{
			CServerChatChannel channel = null;
			if ( !m_Channels.TryGetValue( channel_id, out channel ) )
			{
				return null;
			}
			
			return channel;
		}

		private string Get_Channel_Name_By_Channel_ID( EChannelID channel_id )
		{
			CServerChatChannel channel = Get_Channel_By_Channel_ID( channel_id );
			if ( channel != null )
			{
				return channel.InternalName;
			}

			return "Uknown Channel";
		}
				
		private EChannelID Allocate_Channel_ID()
		{
			return ++m_CurrentChannelID;
		}

		private EPersistenceID Get_Persistence_ID_By_Session_ID( ESessionID session_id )
		{
			EPersistenceID persistence_id = EPersistenceID.Invalid;
			m_ActiveSessions.TryGetValue( session_id, out persistence_id );

			return persistence_id;
		}

		private string Get_Player_Name_By_Persistence_ID( EPersistenceID player_id )
		{
			CChatPlayer player = Get_Player_By_Persistence_ID( player_id );
			if ( player == null )
			{
				return "Unknown Player";
			}

			return player.Name;
		}

		private EPersistenceID Get_Persistence_ID_By_Name( string player_name )
		{
			EPersistenceID player_id = EPersistenceID.Invalid;
			m_PlayersByName.TryGetValue( player_name.ToUpper(), out player_id );

			return player_id;
		}

		private void Remove_Channel_From_Player( EPersistenceID player_id, EChannelID channel_id )
		{
			CChatPlayer player = Get_Player_By_Persistence_ID( player_id );
			if ( player != null )
			{
				player.Remove_Channel( channel_id );
			}
		}

		private void Add_Channel_To_Player( EPersistenceID player_id, EChannelID channel_id )
		{
			CChatPlayer player = Get_Player_By_Persistence_ID( player_id );
			if ( player != null )
			{
				player.Add_Channel( channel_id );
			}
		}

		private void Add_Player_To_Ignore_List( EPersistenceID player_id, EPersistenceID ignored_id )
		{
			CChatPlayer player_data = Get_Player_By_Persistence_ID( player_id );
			if ( player_data != null )
			{
				player_data.Add_Ignored_Player( ignored_id );
			}
		}

		private void Remove_Player_From_Ignore_List( EPersistenceID player_id, EPersistenceID ignored_id )
		{
			CChatPlayer player_data = Get_Player_By_Persistence_ID( player_id );
			if ( player_data != null )
			{
				player_data.Remove_Ignored_Player( ignored_id );
			}
		}

		private bool Is_Ignoring_Player( EPersistenceID source_player_id, EPersistenceID player_id )
		{
			CChatPlayer player_data = Get_Player_By_Persistence_ID( source_player_id );
			if ( player_data == null )
			{
				return false;
			}

			return player_data.Is_Ignored( player_id );
		}
						
		// Properties
		public static CChatServer Instance { get { return m_Instance; } }
		
		// Fields
		private static CChatServer m_Instance = new CChatServer();
		
		private Dictionary< ESessionID, EPersistenceID > m_ActiveSessions = new Dictionary< ESessionID, EPersistenceID >();
		private Dictionary< EPersistenceID, CChatPlayer > m_Players = new Dictionary< EPersistenceID, CChatPlayer >();
		private Dictionary< string, EPersistenceID > m_PlayersByName = new Dictionary< string, EPersistenceID >();
		
		private Dictionary< EChannelID, CServerChatChannel > m_Channels = new Dictionary< EChannelID, CServerChatChannel >();
		private Dictionary< string, EChannelID > m_ChannelInternalNames = new Dictionary< string, EChannelID >();
		
		private EChannelID m_CurrentChannelID = EChannelID.Invalid;
	}
}
