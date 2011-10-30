/*

	NetworkMessageHandler.cs

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
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace CRShared
{
	[AttributeUsage( AttributeTargets.Method )]
	public class NetworkMessageHandlerAttribute : Attribute
	{
		public NetworkMessageHandlerAttribute() :
			base()
		{
		}
	}

	public delegate void DGenericNetworkMessageHandler< T >( T message );
	public delegate void DGenericNetworkMessageHandlerBySessionID< T >( T message, ESessionID session_id );
	public delegate void DGenericNetworkMessageHandlerByPlayerID< T >( T message, EPersistenceID player_id );

	public delegate EPersistenceID DSessionIDToPersistenceID( ESessionID session_id );

	public class CNetworkMessageHandler
	{
		// embedded types
		private enum EHandlerSignature
		{
			Message,
			MessageWithSessionID,
			MessageWithPlayerID
		}

		// construction
		static CNetworkMessageHandler() {}
		private CNetworkMessageHandler() {}

		public void Initialize( DSessionIDToPersistenceID session_id_converter ) 
		{
			SessionIDConverter = session_id_converter;
		}

		// Methods
		// Public interface
		public void Find_Handlers( Assembly handler_assembly )
		{
			BindingFlags binding_flags_all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
			Type network_message_type = typeof( CNetworkMessage );
			Type open_generic_handler_type = typeof( DGenericNetworkMessageHandler<> );
			Type open_generic_session_handler_type = typeof( DGenericNetworkMessageHandlerBySessionID<> );
			Type open_generic_player_handler_type = typeof( DGenericNetworkMessageHandlerByPlayerID<> );

			foreach ( var type in handler_assembly.GetTypes() )
			{
				MethodInfo[] methods = type.GetMethods( binding_flags_all );
				foreach ( var method_info in methods )
				{
					// has this method been marked as a network message handler
					NetworkMessageHandlerAttribute handler_attribute = Attribute.GetCustomAttribute( method_info, typeof( NetworkMessageHandlerAttribute ) ) as NetworkMessageHandlerAttribute;
					if ( handler_attribute == null )
					{
						continue;
					}

					// network message handlers have 1 or 2 parameters
					ParameterInfo[] parameters = method_info.GetParameters();
					if ( parameters.Length != 1 && parameters.Length != 2 )
					{
						throw new CApplicationException( String.Format( "Network message command handler function {0} has the incorrect number of arguments (must be 1 or 2).", method_info.Name ) );
					}

					// make sure the parameter type derives from CSlashCommand
					EHandlerSignature handler_signature = EHandlerSignature.Message;
					ParameterInfo parameter = parameters[ 0 ];
					Type first_parameter_type = parameter.ParameterType;
					if ( !first_parameter_type.IsSubclassOf( network_message_type ) )
					{
						throw new CApplicationException( String.Format( "Network message handler function {0} has a first argument ({1}) that does not derive from CNetworkMessage.", method_info.Name, first_parameter_type.Name ) );
					}

					if ( parameters.Length == 2 )
					{
						Type second_parameter_type = parameters[ 1 ].ParameterType;
						if ( second_parameter_type == typeof( ESessionID ) )
						{
							handler_signature = EHandlerSignature.MessageWithSessionID;
						}
						else if ( second_parameter_type == typeof( EPersistenceID ) )
						{
							handler_signature = EHandlerSignature.MessageWithPlayerID;
						}
						else
						{
							throw new CApplicationException( String.Format( "Network message handler function {0} has a second argument ({1}) that is not a session id or player id.", method_info.Name, second_parameter_type.Name ) );
						}
					}

					CNetworkMessage message_instance = FormatterServices.GetUninitializedObject( first_parameter_type ) as CNetworkMessage;
					ENetworkMessageType message_type = message_instance.MessageType;

					// make sure we haven't already registered a handler for this slash command
					if ( Handler_Exists( message_type ) )
					{
						throw new CApplicationException( String.Format( "Duplicate network message handler detected for message ({0})!", first_parameter_type.Name ) );
					}

					// Scarrrrrrryyyyy stuff
					//
					// Original inspiration for this solution found at: 
					//		http://social.msdn.microsoft.com/Forums/en/csharplanguage/thread/fe14d396-bc35-4f98-851d-ce3c8663cd79
					//
					// By using Cast_And_Call rather than Cast and composing/invoking delegates, together with the strong cast at the end, we avoid
					// using DynamicInvoke/Invoke completely, which is a huge performance gain
					Type derived_handler_type = null;
					MethodInfo cast_and_call_method_info = null;
					switch ( handler_signature )
					{
						case EHandlerSignature.Message:
							derived_handler_type = open_generic_handler_type.MakeGenericType( first_parameter_type );
							cast_and_call_method_info = this.GetType().GetMethod( "Cast_And_Call", binding_flags_all ).MakeGenericMethod( first_parameter_type );
							break;

						case EHandlerSignature.MessageWithSessionID:
							derived_handler_type = open_generic_session_handler_type.MakeGenericType( first_parameter_type );
							cast_and_call_method_info = this.GetType().GetMethod( "Cast_And_Call_Session_ID", binding_flags_all ).MakeGenericMethod( first_parameter_type );
							break;

						case EHandlerSignature.MessageWithPlayerID:
							derived_handler_type = open_generic_player_handler_type.MakeGenericType( first_parameter_type );
							cast_and_call_method_info = this.GetType().GetMethod( "Cast_And_Call_Player_ID", binding_flags_all ).MakeGenericMethod( first_parameter_type );
							break;
					}

					if ( cast_and_call_method_info == null )
					{
						throw new CApplicationException( String.Format( "Unable to build intermediate Cast-And-Call delegate for message ({0})!", first_parameter_type.Name ) );
					}

					Delegate derived_delegate = null;
	
					if ( method_info.IsStatic )
					{
						// static handlers are a little easier
						derived_delegate = Delegate.CreateDelegate( derived_handler_type, type, method_info.Name );						
					}
					else
					{
						// non static handlers require us to go through the Instance property in order to get the class instance; the class must be a singleton
						// implemented in the standard pattern
						PropertyInfo instance_property_info = type.GetProperty( "Instance" );
						if ( instance_property_info == null )
						{
							throw new CApplicationException( String.Format( "Non-static message handler for message {0} must belong to a singleton class with a static Instance property getter.", first_parameter_type.Name ) );
						}

						object singleton_instance = instance_property_info.GetValue( null, null );
						derived_delegate = Delegate.CreateDelegate( derived_handler_type, singleton_instance, method_info.Name );
					}

					// this final handler object ends up being a single composed method call without any reflection-on-invoke, type checking, or managed<->unmanaged transitions
					// which my original solution suffered from.
					//
					// It is the run-time, reflection-based equivalent to the line in the generic Register_Handler< T > function:
					//
					//		generic_handle = delegate( CSlashCommand command ) { handler( command as T ); };
					DGenericNetworkMessageHandler< CWrappedNetworkMessage > generic_handler = ( DGenericNetworkMessageHandler< CWrappedNetworkMessage > ) 
							Delegate.CreateDelegate( typeof( DGenericNetworkMessageHandler< CWrappedNetworkMessage > ), derived_delegate, cast_and_call_method_info );

					if ( generic_handler == null )
					{
						throw new CApplicationException( String.Format( "Unable to build handler for network message {0}.", first_parameter_type.Name ) );
					}

					m_MessageHandlers.Add( message_type, generic_handler );
				}
			}
		}

		public void Register_Null_Handler< T >() where T : class
		{
			ENetworkMessageType message_type = Get_Message_Type_From_Type( typeof( T ) );
			if ( Handler_Exists( message_type ) )
			{
				throw new CApplicationException( String.Format( "Duplicate message handler detected for message type ({0})!", message_type.ToString() ) );
			}

			m_MessageHandlers.Add( message_type, delegate( CWrappedNetworkMessage message ) { } );
		}

		public void Register_Handler< T >( DGenericNetworkMessageHandler< T > handler ) where T : class
		{
			ENetworkMessageType message_type = Get_Message_Type_From_Type( typeof( T ) );
			if ( Handler_Exists( message_type ) )
			{
				throw new CApplicationException( String.Format( "Duplicate message handler detected for message type ({0})!", message_type.ToString() ) );
			}

			m_MessageHandlers.Add( message_type, delegate( CWrappedNetworkMessage message ) { handler( message.Message as T ); } );
		}

		public void Register_Handler< T >( DGenericNetworkMessageHandlerBySessionID< T > handler ) where T : class
		{
			ENetworkMessageType message_type = Get_Message_Type_From_Type( typeof( T ) );
			if ( Handler_Exists( message_type ) )
			{
				throw new CApplicationException( String.Format( "Duplicate message handler detected for message type ({0})!", message_type.ToString() ) );
			}

			m_MessageHandlers.Add( message_type, delegate( CWrappedNetworkMessage message ) { handler( message.Message as T, message.ID ); } );
		}

		public void Register_Handler< T >( DGenericNetworkMessageHandlerByPlayerID< T > handler ) where T : class
		{
			ENetworkMessageType message_type = Get_Message_Type_From_Type( typeof( T ) );
			if ( Handler_Exists( message_type ) )
			{
				throw new CApplicationException( String.Format( "Duplicate message handler detected for message type ({0})!", message_type.ToString() ) );
			}

			m_MessageHandlers.Add( message_type, delegate( CWrappedNetworkMessage message ) { handler( message.Message as T, SessionIDConverter( message.ID ) ); } );
		}

		public bool Try_Handle_Message( CWrappedNetworkMessage message )
		{
			if ( message.Message is CResponseMessage )
			{
				CResponseMessage response = message.Message as CResponseMessage;
				CRequestMessage request = response.Request;
				if ( request != null && request.Handler != null )
				{
					request.Handler( response );
				}
			}

			ENetworkMessageType message_type = message.Message.MessageType;

			DGenericNetworkMessageHandler< CWrappedNetworkMessage > handler = null;
			if ( m_MessageHandlers.TryGetValue( message_type, out handler ) )
			{
				handler( message );
				return true;
			}

			return false;
		}

		// Private interface
		private static void Cast_And_Call< T >( DGenericNetworkMessageHandler< T > handler, object o )
		{
			CWrappedNetworkMessage message = (CWrappedNetworkMessage) o;
			Object message_object = message.Message;
			handler( (T) message_object );
		}

		private static void Cast_And_Call_Session_ID< T >( DGenericNetworkMessageHandlerBySessionID< T > handler, object o )
		{
			CWrappedNetworkMessage message = (CWrappedNetworkMessage) o;
			Object message_object = message.Message;
			handler( (T) message_object, message.ID );
		}

		private static void Cast_And_Call_Player_ID< T >( DGenericNetworkMessageHandlerByPlayerID< T > handler, object o )
		{
			CWrappedNetworkMessage message = (CWrappedNetworkMessage) o;
			Object message_object = message.Message;
			handler( (T) message_object, Instance.SessionIDConverter( message.ID ) );
		}

		private ENetworkMessageType Get_Message_Type_From_Type( Type type )
		{
			CNetworkMessage message_instance = FormatterServices.GetUninitializedObject( type ) as CNetworkMessage;
			if ( message_instance == null )
			{
				throw new CApplicationException( String.Format( "Tried to register a message handler for a type ({0}) that is not a message!", type.FullName ) );
			}

			return message_instance.MessageType;
		}

		private bool Handler_Exists( ENetworkMessageType message_type )
		{
			if ( m_MessageHandlers.ContainsKey( message_type ) )
			{
				return true;
			}

			return false;
		}

		// Properties
		public static CNetworkMessageHandler Instance { get { return m_Instance; } private set { m_Instance = value; } }

		private DSessionIDToPersistenceID SessionIDConverter { get; set; }

		// Fields
		private static CNetworkMessageHandler m_Instance = new CNetworkMessageHandler();

		private Dictionary< ENetworkMessageType, DGenericNetworkMessageHandler< CWrappedNetworkMessage > > m_MessageHandlers = new Dictionary< ENetworkMessageType, DGenericNetworkMessageHandler< CWrappedNetworkMessage > >();
	}
}

