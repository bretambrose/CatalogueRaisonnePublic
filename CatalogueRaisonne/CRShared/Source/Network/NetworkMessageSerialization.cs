/*

	NetworkMessageSerialization.cs

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
using System.Reflection.Emit;
using System.IO;
using System.Runtime.Serialization;
using System.Linq;

namespace CRShared
{
	public delegate void DSerializationHandler< T >( T handleable, BinaryWriter writer );
	public delegate void DDeserializationHandler< T >( T handleable, BinaryReader reader );

	[AttributeUsage( AttributeTargets.Class )]
	public class NetworkMessageAttribute : Attribute
	{
		public NetworkMessageAttribute() :
			base()
		{
		}
	}

	[AttributeUsage( AttributeTargets.Field )]
	public class DontSerializeAttribute : Attribute
	{
		public DontSerializeAttribute() :
			base()
		{
		}
	}

	public class CNetworkMessageSerializationManager
	{
		// Construction
		private CNetworkMessageSerializationManager() {}
		static CNetworkMessageSerializationManager() {}

		// Methods
		// Public interface
		public static CNetworkMessage Deserialize( MemoryStream stream )
		{
			BinaryReader reader = new BinaryReader( stream );

			ENetworkMessageType network_message_type = (ENetworkMessageType) reader.ReadUInt32();
			Type message_type = null;
			if ( !m_MessageTypes.TryGetValue( network_message_type, out message_type ) )
			{
				throw new CApplicationException( String.Format( "No registered type object for message type {0}", network_message_type.ToString() ) );
			}

			DDeserializationHandler< CNetworkMessage > deserializer = Get_Deserialization_Handler( message_type );
			if ( deserializer == null )
			{
				throw new CApplicationException( String.Format( "Could not find a deserializer for message type {0}", message_type.Name ) );
			}

			CNetworkMessage message = FormatterServices.GetUninitializedObject( message_type ) as CNetworkMessage;
			if ( message == null )
			{
				throw new CApplicationException( String.Format( "Network message type {0} could not be instantiated with low-level formatter services.", message_type.Name ) );
			}

			deserializer( message, reader );

			return message;
		}

		public static void Serialize( CNetworkMessage message, MemoryStream stream )
		{
			DSerializationHandler< CNetworkMessage > serializer = Get_Serialization_Handler( message.GetType() );
			if ( serializer == null )
			{
				throw new CApplicationException( String.Format( "No seralization delegate exists for message type {0}", message.GetType().Name ) );
			}

			ENetworkMessageType message_type = message.MessageType;

			BinaryWriter writer = new BinaryWriter( stream );
			stream.SetLength( 0 );

			// place holder for message length
			writer.Write( (uint) 0 );					
			writer.Write( (uint) message_type );	

			// write all message data
			serializer( message, writer );

			// rewind and fill in message length
			uint total_length = (uint) stream.Length;
			stream.Position = 0;
			writer.Write( total_length );				

			// move back to end of stream
			stream.Position = total_length;			
		}

		public static void Register_Assembly( Assembly assembly )
		{
			m_RegisteredAssemblies.Add( assembly );
		}

		public static void Build_Serialization_Objects()
		{
			Register_Builtin_Read_Write_Methods();

			Type base_message_type = typeof( CNetworkMessage );

			foreach ( var assembly in m_RegisteredAssemblies )
			{
				foreach ( var type in assembly.GetTypes() )
				{
					Process_Type( type );

					// has this method been marked as a network message
					NetworkMessageAttribute message_attribute = Attribute.GetCustomAttribute( type, typeof( NetworkMessageAttribute ) ) as NetworkMessageAttribute;
					if ( message_attribute == null )
					{
						continue;
					}

					if ( !type.IsSubclassOf( base_message_type ) )
					{
						throw new CApplicationException( String.Format( "Network message flagged type {0} does not derive from CNetworkMessage.", type.Name ) );
					}

					if ( Serialization_Delegates_Exist( type ) )
					{
						throw new CApplicationException( String.Format( "Network message type {0} has a duplicate serialization delegate.", type.Name ) );
					}

					CNetworkMessage message_instance = FormatterServices.GetUninitializedObject( type ) as CNetworkMessage;
					if ( message_instance == null )
					{
						throw new CApplicationException( String.Format( "Network message type {0} could not be instantiated with low-level formatter services.", type.Name ) );
					}

					ENetworkMessageType message_type = message_instance.MessageType;
					if ( message_type == ENetworkMessageType.Message_Invalid )
					{
						throw new CApplicationException( String.Format( "Network message type {0} has an invalid value for its MessageType property.", type.Name ) );
					}

					Type existing_type = null;
					if ( m_MessageTypes.TryGetValue( message_type, out existing_type ) )
					{
						throw new CApplicationException( String.Format( "Network message type {0} has a duplicate registration conflict with type {1}.", type.Name, existing_type.Name ) );
					}

					m_MessageTypes.Add( message_instance.MessageType, type );

					Build_Serializers( type );
				}
			}

			Build_All_Reference_Serializers();

			Build_All_Serialization_Delegates();
		}

		// Private interface
		private static void Build_Serializers( Type type )
		{
			if ( type.IsInterface )
			{
				return;
			}

			DynamicMethod serialization_builder = new DynamicMethod( "Serialize_" + type.Name, null, new [] { type, typeof( BinaryWriter ) }, type, true );
			DynamicMethod deserialization_builder =  new DynamicMethod( "Deserialize_" + type.Name, null, new [] { type, typeof( BinaryReader ) }, type, true );

			m_SerializeDynamicMethods.Add( type, serialization_builder );
			m_DeserializeDynamicMethods.Add( type, deserialization_builder );

			Build_Serialization_Methods( type, serialization_builder.GetILGenerator(), deserialization_builder.GetILGenerator() );
		}

		private static void Build_All_Serialization_Delegates()
		{
			foreach ( var builder_pair in m_SerializeDynamicMethods )
			{
				Type base_message_type = typeof( CNetworkMessage );
				Type builder_type = builder_pair.Key;

				if ( builder_type.IsSubclassOf( base_message_type ) )
				{
					DSerializationHandler< CNetworkMessage > generic_serializer = null;
					DDeserializationHandler< CNetworkMessage > generic_deserializer = null;

					Build_Serialization_Delegates< CNetworkMessage >( builder_type, out generic_serializer, out generic_deserializer );

					m_SerializationDelegates.Add( builder_type, generic_serializer );
					m_DeserializationDelegates.Add( builder_type, generic_deserializer );
				}
				else
				{
					DSerializationHandler< object > generic_serializer = null;
					DDeserializationHandler< object > generic_deserializer = null;

					Build_Serialization_Delegates< object >( builder_type, out generic_serializer, out generic_deserializer );

					m_ReferenceSerializers.Add( builder_type, generic_serializer );
					m_ReferenceDeserializers.Add( builder_type, generic_deserializer );
				}
			}
		}

		private static void Serialize_Cast_And_Call< T >( DSerializationHandler< T > serializer, object o, BinaryWriter writer )
      {
         serializer( (T) o, writer );
      }

		private static void Deserialize_Cast_And_Call< T >( DDeserializationHandler< T > deserializer, object o, BinaryReader reader )
      {
         deserializer( (T) o, reader );
      }

		private static void Build_Serialization_Delegates< T >( Type type, out DSerializationHandler< T > serializer, out DDeserializationHandler< T > deserializer )
		{
			BindingFlags binding_flags_all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

			Type open_serialization_handler_type = typeof( DSerializationHandler<> );
			Type open_deserialization_handler_type = typeof( DDeserializationHandler<> );

			MethodInfo generic_serialize_cast_and_call_method_info = typeof( CNetworkMessageSerializationManager ).GetMethod( "Serialize_Cast_And_Call", binding_flags_all );
			MethodInfo generic_deserialize_cast_and_call_method_info = typeof( CNetworkMessageSerializationManager ).GetMethod( "Deserialize_Cast_And_Call", binding_flags_all );

			Type derived_serializer_type = open_serialization_handler_type.MakeGenericType( type );
			Type derived_deserializer_type = open_deserialization_handler_type.MakeGenericType( type );

			DynamicMethod serialization_method = m_SerializeDynamicMethods[ type ];
			DynamicMethod deserialization_method = m_DeserializeDynamicMethods[ type ];

			Delegate derived_serialization_delegate = serialization_method.CreateDelegate( derived_serializer_type );
			Delegate derived_deserialization_delegate = deserialization_method.CreateDelegate( derived_deserializer_type );

			MethodInfo derived_serialize_cast_and_call_method_info = generic_serialize_cast_and_call_method_info.MakeGenericMethod( type );
			MethodInfo derived_deserialize_cast_and_call_method_info = generic_deserialize_cast_and_call_method_info.MakeGenericMethod( type );

			serializer = ( DSerializationHandler< T > )
				Delegate.CreateDelegate( typeof( DSerializationHandler< T > ), derived_serialization_delegate, derived_serialize_cast_and_call_method_info );

			deserializer = ( DDeserializationHandler< T > )
				Delegate.CreateDelegate( typeof( DDeserializationHandler< T > ), derived_deserialization_delegate, derived_deserialize_cast_and_call_method_info );

		}

		private static void Build_Serialization_Methods( Type type, ILGenerator serialize_il_gen, ILGenerator deserialize_il_gen )
		{
			BindingFlags binding_flags_local_non_static = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

			Type iterative_type = type;

			while ( Is_Registered_Type( iterative_type ) )
			{
				foreach ( FieldInfo field_info in iterative_type.GetFields( binding_flags_local_non_static ) )
				{
					DontSerializeAttribute dont_serialize_attribute = Attribute.GetCustomAttribute( field_info, typeof( DontSerializeAttribute ) ) as DontSerializeAttribute;
					if ( dont_serialize_attribute != null )
					{
						continue;
					}

					serialize_il_gen.Emit( OpCodes.Ldarg_1 );												// Stack: writer
					serialize_il_gen.Emit( OpCodes.Ldarg_0 );												// Stack: writer, object_to_serialize
					serialize_il_gen.Emit( OpCodes.Ldfld, field_info );								// Stack: writer, field_value

					Serialize_Thing_On_Stack( serialize_il_gen, field_info.FieldType );

					deserialize_il_gen.Emit( OpCodes.Ldarg_0 );											// Stack: object_to_deserialize
					deserialize_il_gen.Emit( OpCodes.Ldarg_1 );											// Stack: object_to_deserialize, reader

					Deserialize_Thing_To_Stack( deserialize_il_gen, field_info.FieldType );		// Stack: object_to_deserialize, deserialized_field

					deserialize_il_gen.Emit( OpCodes.Stfld, field_info );								// Stack:
				}

				iterative_type = iterative_type.BaseType;
			}

			serialize_il_gen.Emit( OpCodes.Ret );
			deserialize_il_gen.Emit( OpCodes.Ret );
		}

		private static void Serialize_Thing_On_Stack( ILGenerator serialize_il_gen, Type type )
		{
			// Stack: writer, thing
			On_Type_Encountered( type );

			if ( type == typeof( DateTime ) )
			{
				LocalBuilder local_time = serialize_il_gen.DeclareLocal( typeof( DateTime ) );

				serialize_il_gen.Emit( OpCodes.Stloc, local_time );
				serialize_il_gen.Emit( OpCodes.Ldloca, local_time );
				serialize_il_gen.Emit( OpCodes.Call, typeof( DateTime ).GetMethod( "ToBinary" ) );
				serialize_il_gen.Emit( OpCodes.Callvirt, m_PrimitiveWriteMethods[ typeof( long ) ] );
			}
			else if ( type.IsValueType )
			{
				Type thing_io_type = type;
				if ( type.IsEnum )
				{
					thing_io_type = Enum.GetUnderlyingType( type );
				}
				
				serialize_il_gen.Emit( OpCodes.Callvirt, m_PrimitiveWriteMethods[ thing_io_type ] );		
			}
			else if ( type == typeof( string ) )
			{
				Serialize_String_On_Stack( serialize_il_gen );
			}
			else if ( Is_Unary_Collection( type ) )
			{
				Serialize_Unary_Collection_On_Stack( serialize_il_gen, type );
			}
			else if ( Is_Binary_Collection( type ) )
			{
				Serialize_Binary_Collection_On_Stack( serialize_il_gen, type );
			}
			else
			{
				BindingFlags binding_flags_all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
				MethodInfo ref_serialize_method_info = typeof( CNetworkMessageSerializationManager ).GetMethod( "Serialize_Reference_Object", binding_flags_all );
				serialize_il_gen.Emit( OpCodes.Call, ref_serialize_method_info );
			}
		}

		private static void Deserialize_Thing_To_Stack( ILGenerator deserialize_il_gen, Type type )
		{
			On_Type_Encountered( type );

			if ( type.IsValueType )
			{
				Type thing_io_type = type;
				if ( type.IsEnum )
				{
					thing_io_type = Enum.GetUnderlyingType( type );
				}
				else if ( type == typeof( DateTime ) )
				{
					thing_io_type = typeof( long );
				}

				deserialize_il_gen.Emit( OpCodes.Callvirt, m_PrimitiveReadMethods[ thing_io_type ] );			// Stack: ..., primitive_value
				if ( type == typeof( DateTime ) )
				{
					deserialize_il_gen.Emit( OpCodes.Call, typeof( DateTime ).GetMethod( "FromBinary" ) );					// Stack: ..., date_time as DateTime
				}
			}
			else if ( type == typeof( string ) )
			{
				Deserialize_String_To_Stack( deserialize_il_gen );
			}
			else if ( Is_Unary_Collection( type ) )
			{
				Deserialize_Unary_Collection_To_Stack( deserialize_il_gen, type );
			}
			else if ( Is_Binary_Collection( type ) )
			{
				Deserialize_Binary_Collection_To_Stack( deserialize_il_gen, type );
			}
			else
			{
				BindingFlags binding_flags_all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
				MethodInfo ref_deserialize_method_info = typeof( CNetworkMessageSerializationManager ).GetMethod( "Deserialize_Reference_Object", binding_flags_all );
				deserialize_il_gen.Emit( OpCodes.Call, ref_deserialize_method_info );										
				deserialize_il_gen.Emit( OpCodes.Castclass, type );										
			}
		}

		private static void Serialize_String_On_Stack( ILGenerator serialize_il_gen )
		{
			// For serialization, assume string was just loaded to top of stack, with the writer underneath it, end with empty stack (relative)
			Label s_start_non_null_serialize = serialize_il_gen.DefineLabel();
			Label s_end_serialize = serialize_il_gen.DefineLabel();

			LocalBuilder local_string = serialize_il_gen.DeclareLocal( typeof( string ) );
			LocalBuilder local_writer = serialize_il_gen.DeclareLocal( typeof( BinaryWriter ) );
																																		// Stack: writer, string
			serialize_il_gen.Emit( OpCodes.Stloc, local_string );														// Stack: writer
			serialize_il_gen.Emit( OpCodes.Stloc, local_writer );														// Stack:

			serialize_il_gen.Emit( OpCodes.Ldloc, local_string );														// Stack: string
			serialize_il_gen.Emit( OpCodes.Brtrue_S, s_start_non_null_serialize );								// Stack:

			serialize_il_gen.Emit( OpCodes.Ldloc, local_writer );														// Stack: writer
			serialize_il_gen.Emit( OpCodes.Ldc_I4_0 );																	// Stack: writer, 0
			serialize_il_gen.Emit( OpCodes.Callvirt, m_PrimitiveWriteMethods[ typeof( bool ) ] );			// Stack:
			serialize_il_gen.Emit( OpCodes.Br, s_end_serialize );

			serialize_il_gen.MarkLabel( s_start_non_null_serialize );

			serialize_il_gen.Emit( OpCodes.Ldloc, local_writer );														// Stack: writer
			serialize_il_gen.Emit( OpCodes.Ldc_I4_1 );																	// Stack: writer, 1
			serialize_il_gen.Emit( OpCodes.Callvirt, m_PrimitiveWriteMethods[ typeof( bool ) ] );			// Stack:

			serialize_il_gen.Emit( OpCodes.Ldloc, local_writer );														// Stack: writer
			serialize_il_gen.Emit( OpCodes.Ldloc, local_string );														// Stack: writer, string
			serialize_il_gen.Emit( OpCodes.Callvirt, m_PrimitiveWriteMethods[ typeof( string ) ] );		// Stack:

			serialize_il_gen.MarkLabel( s_end_serialize );
		}

		private static void Deserialize_String_To_Stack( ILGenerator deserialize_il_gen )
		{
			// Deserialization
			// Assume we have the reader on the stack, end with string on the stack
			Label d_start_non_null_deserialize = deserialize_il_gen.DefineLabel();
			Label d_end_deserialize = deserialize_il_gen.DefineLabel();

			LocalBuilder local_reader = deserialize_il_gen.DeclareLocal( typeof( BinaryReader ) );
																																			// Stack: ..., reader
			deserialize_il_gen.Emit( OpCodes.Stloc, local_reader );														// Stack: ...

			deserialize_il_gen.Emit( OpCodes.Ldloc, local_reader );														// Stack: ..., reader
			deserialize_il_gen.Emit( OpCodes.Callvirt, m_PrimitiveReadMethods[ typeof( bool ) ] );				// Stack: ..., is_non_null?
			deserialize_il_gen.Emit( OpCodes.Brtrue, d_start_non_null_deserialize );
			deserialize_il_gen.Emit( OpCodes.Ldnull );
			deserialize_il_gen.Emit( OpCodes.Br, d_end_deserialize );

			deserialize_il_gen.MarkLabel( d_start_non_null_deserialize );

			deserialize_il_gen.Emit( OpCodes.Ldloc, local_reader );														// Stack: ..., reader
			deserialize_il_gen.Emit( OpCodes.Callvirt, m_PrimitiveReadMethods[ typeof( string ) ] );			// Stack: ..., string

			deserialize_il_gen.MarkLabel( d_end_deserialize );
		}

		private static void Serialize_Unary_Collection_On_Stack( ILGenerator serialize_il_gen, Type unary_container_type )
		{
			Type collection_element_type = unary_container_type.GetGenericArguments()[ 0 ];
			On_Type_Encountered( collection_element_type );

			Type generic_enumerator_type = typeof( IEnumerator<> );
			Type bound_enumerator_type = generic_enumerator_type.MakeGenericType( collection_element_type );
			Type enumerator_type = bound_enumerator_type.GetInterface( "IEnumerator" );

			Type generic_enumerable_type = typeof( IEnumerable<> );
			Type bound_enumerable_type = generic_enumerable_type.MakeGenericType( collection_element_type );
			Type enumerable_type = bound_enumerator_type.GetInterface( "IEnumerable" );

			Label s_start_non_null_serialize = serialize_il_gen.DefineLabel();
			Label s_start_loop = serialize_il_gen.DefineLabel();
			Label s_end_loop = serialize_il_gen.DefineLabel();

			LocalBuilder local_enumerator = serialize_il_gen.DeclareLocal( bound_enumerator_type );

			LocalBuilder local_container = serialize_il_gen.DeclareLocal( unary_container_type );
			LocalBuilder local_writer = serialize_il_gen.DeclareLocal( typeof( BinaryWriter ) );
																																		// Stack: writer, container
			serialize_il_gen.Emit( OpCodes.Stloc, local_container );													// Stack: writer
			serialize_il_gen.Emit( OpCodes.Stloc, local_writer );														// Stack:
			serialize_il_gen.Emit( OpCodes.Ldloc, local_container );													// Stack: container

			/*
				// CIL code for the following loop:	
				if ( collection == null )
				{
					writer.Write( 0 );							// signal to deserialize that this was a null object
					return;
				}
			 
				writer.Write( 1 );								// signal to deserialize that this was a valid object
				serialize( collection.Length );
			 
				var enumerator = collection.GetEnumerator();
				while ( enumerator.MoveNext() )
				{
					serialize( enumerator.Current() );
				}
			 
			*/

			serialize_il_gen.Emit( OpCodes.Brtrue_S, s_start_non_null_serialize );									// Stack:

			serialize_il_gen.Emit( OpCodes.Ldloc, local_writer );															// Stack: writer
			serialize_il_gen.Emit( OpCodes.Ldc_I4_0 );																		// Stack: writer, 0
			serialize_il_gen.Emit( OpCodes.Callvirt, m_PrimitiveWriteMethods[ typeof( bool ) ] );				// Stack:
			serialize_il_gen.Emit( OpCodes.Br, s_end_loop );

			serialize_il_gen.MarkLabel( s_start_non_null_serialize );

			serialize_il_gen.Emit( OpCodes.Ldloc, local_writer );															// Stack: writer
			serialize_il_gen.Emit( OpCodes.Ldc_I4_1 );																		// Stack: writer, 1
			serialize_il_gen.Emit( OpCodes.Callvirt, m_PrimitiveWriteMethods[ typeof( bool ) ] );				// Stack:

			serialize_il_gen.Emit( OpCodes.Ldloc, local_writer );															// Stack: writer
			serialize_il_gen.Emit( OpCodes.Ldloc, local_container );														// Stack: writer, collection
			serialize_il_gen.Emit( OpCodes.Callvirt, unary_container_type.GetProperty( "Count" ).GetGetMethod() );		// Stack: writer, collection length
			serialize_il_gen.Emit( OpCodes.Callvirt, m_PrimitiveWriteMethods[ typeof( int ) ] );				// Stack: 

			serialize_il_gen.Emit( OpCodes.Ldloc, local_container );														// Stack: collection
			serialize_il_gen.Emit( OpCodes.Callvirt, bound_enumerable_type.GetMethod( "GetEnumerator" ) );	// Stack: enumerator< T >
			serialize_il_gen.Emit( OpCodes.Stloc, local_enumerator );													// Stack:

			serialize_il_gen.MarkLabel( s_start_loop );

			serialize_il_gen.Emit( OpCodes.Ldloc, local_enumerator );													// Stack: enumerator
			serialize_il_gen.Emit( OpCodes.Callvirt, enumerator_type.GetMethod( "MoveNext" ) );					// Stack: not_done
			serialize_il_gen.Emit( OpCodes.Brfalse, s_end_loop );															// Stack:

			serialize_il_gen.Emit( OpCodes.Ldloc, local_writer );															// Stack: writer				
			serialize_il_gen.Emit( OpCodes.Ldloc, local_enumerator );													// Stack: writer, enumerator
			serialize_il_gen.Emit( OpCodes.Callvirt, bound_enumerator_type.GetProperty( "Current" ).GetGetMethod() );	// Stack: writer, enumerator[ current ]
			
			Serialize_Thing_On_Stack( serialize_il_gen, collection_element_type );									// Stack:

			serialize_il_gen.Emit( OpCodes.Br, s_start_loop );

			serialize_il_gen.MarkLabel( s_end_loop );
		}

		private static void Deserialize_Unary_Collection_To_Stack( ILGenerator deserialize_il_gen, Type unary_container_type )
		{
			Type collection_element_type = unary_container_type.GetGenericArguments()[ 0 ];
			On_Type_Encountered( collection_element_type );

			Type generic_collection_type = typeof( ICollection<> );
			Type bound_collection_type = generic_collection_type.MakeGenericType( collection_element_type );

			Label d_top_loop = deserialize_il_gen.DefineLabel();
			Label d_end_loop = deserialize_il_gen.DefineLabel();
			Label d_pre_init_loop = deserialize_il_gen.DefineLabel();
			Label d_start_non_null_deserialize = deserialize_il_gen.DefineLabel();

			LocalBuilder local_collection = deserialize_il_gen.DeclareLocal( bound_collection_type );
			LocalBuilder local_i = deserialize_il_gen.DeclareLocal( typeof( int ) );
			LocalBuilder local_reader = deserialize_il_gen.DeclareLocal( typeof( BinaryReader ) );
			LocalBuilder local_container = deserialize_il_gen.DeclareLocal( unary_container_type );

			/*
				// CIL code for the following loop:	
			 
				bool non_null = reader.ReadBool();
				if ( !non_null )
				{
					return;
				}
			 
				if ( collection == null )
				{
					collection = new CollectionType< T >();
				}
			 
				for ( int collection_length = reader.ReadInt32(); i > 0; i-- )
				{
					collection.Add( deserialize_ref_type( reader ) );
				}
			 
			*/
																																	// Stack: ..., reader
			deserialize_il_gen.Emit( OpCodes.Stloc, local_reader );												// Stack: ...

			deserialize_il_gen.Emit( OpCodes.Ldloc, local_reader );												// Stack: ..., reader
			deserialize_il_gen.Emit( OpCodes.Callvirt, m_PrimitiveReadMethods[ typeof( bool ) ] );		// Stack: ..., is_non_null?
			deserialize_il_gen.Emit( OpCodes.Brtrue, d_start_non_null_deserialize );						// Stack: ...
			deserialize_il_gen.Emit( OpCodes.Ldnull );																// Stack: ..., null
			deserialize_il_gen.Emit( OpCodes.Stloc, local_container );											// Stack: ...
			deserialize_il_gen.Emit( OpCodes.Br, d_end_loop );

			deserialize_il_gen.MarkLabel( d_start_non_null_deserialize );

			deserialize_il_gen.Emit( OpCodes.Ldloc, local_reader );														// Stack: reader
			deserialize_il_gen.Emit( OpCodes.Callvirt, m_PrimitiveReadMethods[ typeof( int ) ] );				// Stack: collection size
			deserialize_il_gen.Emit( OpCodes.Stloc, local_i );																// Stack:

			deserialize_il_gen.Emit( OpCodes.Newobj, unary_container_type.GetConstructor( new Type[0] ) );	// Stack: default constructed collection< T >
			deserialize_il_gen.Emit( OpCodes.Stloc, local_container );													// Stack:

			deserialize_il_gen.MarkLabel( d_pre_init_loop );

			deserialize_il_gen.Emit( OpCodes.Ldloc, local_container );													// Stack: collection< T > (field_value)
			deserialize_il_gen.Emit( OpCodes.Stloc, local_collection );													// Stack:

			deserialize_il_gen.MarkLabel( d_top_loop );

			deserialize_il_gen.Emit( OpCodes.Ldloc, local_i );																// Stack: i
			deserialize_il_gen.Emit( OpCodes.Ldc_I4, 0 );																	// Stack: i, 0
			deserialize_il_gen.Emit( OpCodes.Beq, d_end_loop );															// Stack:

			deserialize_il_gen.Emit( OpCodes.Ldloc, local_collection );													// Stack: collection
			deserialize_il_gen.Emit( OpCodes.Ldloc, local_reader );														// Stack: collection, reader

			Deserialize_Thing_To_Stack( deserialize_il_gen, collection_element_type );								// Stack: collection, new_collection_member

			deserialize_il_gen.Emit( OpCodes.Callvirt, bound_collection_type.GetMethod( "Add" ) );				// Stack:

			deserialize_il_gen.Emit( OpCodes.Ldloc, local_i );																// Stack: i
			deserialize_il_gen.Emit( OpCodes.Ldc_I4, 1 );																	// Stack: i, 1
			deserialize_il_gen.Emit( OpCodes.Sub );																			// Stack: i - 1
			deserialize_il_gen.Emit( OpCodes.Stloc, local_i );																// Stack:
			deserialize_il_gen.Emit( OpCodes.Br, d_top_loop );

			deserialize_il_gen.MarkLabel( d_end_loop );

			deserialize_il_gen.Emit( OpCodes.Ldloc, local_container );													// Stack: ..., null or deserialized_collection
		}

		private static void Serialize_Binary_Collection_On_Stack( ILGenerator serialize_il_gen, Type binary_container_type )
		{
			Type key_type = binary_container_type.GetGenericArguments()[ 0 ];
			On_Type_Encountered( key_type );

			Type value_type = binary_container_type.GetGenericArguments()[ 1 ];
			On_Type_Encountered( value_type );

			Type generic_key_value_pair_type = typeof( KeyValuePair<,> );
			Type bound_key_value_pair_type = generic_key_value_pair_type.MakeGenericType( new Type[] { key_type, value_type } );

			Type generic_enumerator_type = typeof( IEnumerator<> );
			Type bound_enumerator_type = generic_enumerator_type.MakeGenericType( bound_key_value_pair_type );
			Type enumerator_type = bound_enumerator_type.GetInterface( "IEnumerator" );

			Type generic_enumerable_type = typeof( IEnumerable<> );
			Type bound_enumerable_type = generic_enumerable_type.MakeGenericType( bound_key_value_pair_type );
			Type enumerable_type = bound_enumerator_type.GetInterface( "IEnumerable" );

			Label s_start_non_null_serialize = serialize_il_gen.DefineLabel();
			Label s_start_loop = serialize_il_gen.DefineLabel();
			Label s_end_loop = serialize_il_gen.DefineLabel();

			LocalBuilder local_enumerator = serialize_il_gen.DeclareLocal( bound_enumerator_type );
			LocalBuilder local_key_value_pair = serialize_il_gen.DeclareLocal( bound_key_value_pair_type );

			LocalBuilder local_container = serialize_il_gen.DeclareLocal( binary_container_type );
			LocalBuilder local_writer = serialize_il_gen.DeclareLocal( typeof( BinaryWriter ) );

																																		// Stack: writer, container
			serialize_il_gen.Emit( OpCodes.Stloc, local_container );													// Stack: writer
			serialize_il_gen.Emit( OpCodes.Stloc, local_writer );														// Stack:
			serialize_il_gen.Emit( OpCodes.Ldloc, local_container );													// Stack: container

			/*
				// CIL code for the following loop:	
				if ( dictionary == null )
				{
					writer.Write( false );							// signal to deserialize that this was a null object
					return;
				}
			 
				writer.Write( true );								// signal to deserialize that this was a valid object
				serialize( dictionary.Length );
			 
				var enumerator = dictionary.GetEnumerator();
				while ( enumerator.MoveNext() )
				{
					var key_value_pair = enumerator.Current();
					var key = key_value_pair.Key;
					var value = key_value_pair.Value;
			 
					serialize( key );
					serialize( value );
				}
			 
			*/

			serialize_il_gen.Emit( OpCodes.Brtrue_S, s_start_non_null_serialize );									// Stack:

			serialize_il_gen.Emit( OpCodes.Ldloc, local_writer );															// Stack: writer
			serialize_il_gen.Emit( OpCodes.Ldc_I4_0 );																		// Stack: writer, 0
			serialize_il_gen.Emit( OpCodes.Callvirt, m_PrimitiveWriteMethods[ typeof( bool ) ] );				// Stack:
			serialize_il_gen.Emit( OpCodes.Br, s_end_loop );

			serialize_il_gen.MarkLabel( s_start_non_null_serialize );

			serialize_il_gen.Emit( OpCodes.Ldloc, local_writer );															// Stack: writer
			serialize_il_gen.Emit( OpCodes.Ldc_I4_1 );																		// Stack: writer, 1
			serialize_il_gen.Emit( OpCodes.Callvirt, m_PrimitiveWriteMethods[ typeof( bool ) ] );				// Stack:

			serialize_il_gen.Emit( OpCodes.Ldloc, local_writer );															// Stack: writer
			serialize_il_gen.Emit( OpCodes.Ldloc, local_container );														// Stack: writer, container
			serialize_il_gen.Emit( OpCodes.Callvirt, binary_container_type.GetProperty( "Count" ).GetGetMethod() );	// Stack: writer, container length
			serialize_il_gen.Emit( OpCodes.Callvirt, m_PrimitiveWriteMethods[ typeof( int ) ] );				// Stack: 

			serialize_il_gen.Emit( OpCodes.Ldloc, local_container );														// Stack: container
			serialize_il_gen.Emit( OpCodes.Callvirt, bound_enumerable_type.GetMethod( "GetEnumerator" ) );	// Stack: enumerator< T >
			serialize_il_gen.Emit( OpCodes.Stloc, local_enumerator );													// Stack:

			serialize_il_gen.MarkLabel( s_start_loop );

			serialize_il_gen.Emit( OpCodes.Ldloc, local_enumerator );													// Stack: enumerator
			serialize_il_gen.Emit( OpCodes.Callvirt, enumerator_type.GetMethod( "MoveNext" ) );					// Stack: not_done
			serialize_il_gen.Emit( OpCodes.Brfalse, s_end_loop );															// Stack:

			serialize_il_gen.Emit( OpCodes.Ldloc, local_enumerator );													// Stack: enumerator
			serialize_il_gen.Emit( OpCodes.Callvirt, bound_enumerator_type.GetProperty( "Current" ).GetGetMethod() );	// Stack: enumerator[ current ]
			serialize_il_gen.Emit( OpCodes.Stloc, local_key_value_pair );												// Stack:

			serialize_il_gen.Emit( OpCodes.Ldloc, local_writer );															// Stack: writer			
			serialize_il_gen.Emit( OpCodes.Ldloca, local_key_value_pair );												// Stack: writer, kv_pair				 																		
			serialize_il_gen.Emit( OpCodes.Callvirt, bound_key_value_pair_type.GetProperty( "Key" ).GetGetMethod() );	// Stack: writer, kv_pair.Key																		

			// Serialize key
			Serialize_Thing_On_Stack( serialize_il_gen, key_type );														// Stack:

			serialize_il_gen.Emit( OpCodes.Ldloc, local_writer );															// Stack: writer				
			serialize_il_gen.Emit( OpCodes.Ldloca, local_key_value_pair );												// Stack: writer, kv_pair
			serialize_il_gen.Emit( OpCodes.Callvirt, bound_key_value_pair_type.GetProperty( "Value" ).GetGetMethod() );	// Stack: writer, kv_pair.Value

			// Serialize value
			Serialize_Thing_On_Stack( serialize_il_gen, value_type );														// Stack:

			serialize_il_gen.Emit( OpCodes.Br, s_start_loop );

			serialize_il_gen.MarkLabel( s_end_loop );
		}

		private static void Deserialize_Binary_Collection_To_Stack( ILGenerator deserialize_il_gen, Type binary_container_type )
		{
			Type key_type = binary_container_type.GetGenericArguments()[ 0 ];
			On_Type_Encountered( key_type );

			Type value_type = binary_container_type.GetGenericArguments()[ 1 ];
			On_Type_Encountered( value_type );

			Label d_top_loop = deserialize_il_gen.DefineLabel();
			Label d_end_loop = deserialize_il_gen.DefineLabel();
			Label d_pre_init_loop = deserialize_il_gen.DefineLabel();
			Label d_start_non_null_deserialize = deserialize_il_gen.DefineLabel();

			LocalBuilder local_dictionary = deserialize_il_gen.DeclareLocal( binary_container_type );
			LocalBuilder local_i = deserialize_il_gen.DeclareLocal( typeof( int ) );
			LocalBuilder local_reader = deserialize_il_gen.DeclareLocal( typeof( BinaryReader ) );
			LocalBuilder local_container = deserialize_il_gen.DeclareLocal( binary_container_type );

			/*
				// CIL code for the following loop:	
			 
				bool non_null = reader.ReadBool();
				if ( !non_null )
				{
					return;
				}
			 
				if ( dictionary == null )
				{
					dictionary = new DictionaryType< K, V >();
				}
			 
				for ( int i = reader.ReadInt32(); i > 0; i-- )
				{
					var key = deserialize( reader );
					var value = deserialize( reader );
			 
					dictionary.Add( key, value );
				}
			 
			*/

																																	// Stack: ..., reader
			deserialize_il_gen.Emit( OpCodes.Stloc, local_reader );												// Stack: ...

			deserialize_il_gen.Emit( OpCodes.Ldloc, local_reader );												// Stack: ..., reader
			deserialize_il_gen.Emit( OpCodes.Callvirt, m_PrimitiveReadMethods[ typeof( bool ) ] );		// Stack: ..., is_dictionary_non_null?
			deserialize_il_gen.Emit( OpCodes.Brtrue, d_start_non_null_deserialize );						// Stack: ...
			deserialize_il_gen.Emit( OpCodes.Ldnull );																// Stack: ..., null
			deserialize_il_gen.Emit( OpCodes.Stloc, local_container );											// Stack: ...
			deserialize_il_gen.Emit( OpCodes.Br, d_end_loop );														// Stack: ...

			deserialize_il_gen.MarkLabel( d_start_non_null_deserialize );

			deserialize_il_gen.Emit( OpCodes.Ldloc, local_reader );														// Stack: reader
			deserialize_il_gen.Emit( OpCodes.Callvirt, m_PrimitiveReadMethods[ typeof( int ) ] );				// Stack: length
			deserialize_il_gen.Emit( OpCodes.Stloc, local_i );																// Stack:

			deserialize_il_gen.Emit( OpCodes.Newobj, binary_container_type.GetConstructor( new Type[ 0 ] ) );	// Stack: default constructed dictionary< K, V >
			deserialize_il_gen.Emit( OpCodes.Stloc, local_container );														// Stack:

			deserialize_il_gen.MarkLabel( d_pre_init_loop );

			deserialize_il_gen.Emit( OpCodes.Ldloc, local_container );													// Stack: dictionary< K, V > 
			deserialize_il_gen.Emit( OpCodes.Stloc, local_dictionary );													// Stack:

			deserialize_il_gen.MarkLabel( d_top_loop );

			deserialize_il_gen.Emit( OpCodes.Ldloc, local_i );																// Stack: i
			deserialize_il_gen.Emit( OpCodes.Ldc_I4, 0 );																	// Stack: i, 0
			deserialize_il_gen.Emit( OpCodes.Beq, d_end_loop );															// Stack:

			// Read key
			deserialize_il_gen.Emit( OpCodes.Ldloc, local_dictionary );													// Stack: dictionary
			deserialize_il_gen.Emit( OpCodes.Ldloc, local_reader );														// Stack: dictionary, reader

			Deserialize_Thing_To_Stack( deserialize_il_gen, key_type );													// Stack: dictionary, key

			// Read value
			deserialize_il_gen.Emit( OpCodes.Ldloc, local_reader );														// Stack: dictionary, key, reader

			Deserialize_Thing_To_Stack( deserialize_il_gen, value_type );												// Stack: dictionary, key, value

			deserialize_il_gen.Emit( OpCodes.Callvirt, binary_container_type.GetMethod( "Add" ) );				// Stack:

			deserialize_il_gen.Emit( OpCodes.Ldloc, local_i );																// Stack: i
			deserialize_il_gen.Emit( OpCodes.Ldc_I4_1 );																		// Stack: i, 1
			deserialize_il_gen.Emit( OpCodes.Sub );																			// Stack: i - 1
			deserialize_il_gen.Emit( OpCodes.Stloc, local_i );																// Stack:
			deserialize_il_gen.Emit( OpCodes.Br, d_top_loop );

			deserialize_il_gen.MarkLabel( d_end_loop );

			deserialize_il_gen.Emit( OpCodes.Ldloc, local_container );													// Stack: ..., null or deserialized_collection
		}

		private static void Serialize_Reference_Object( BinaryWriter writer, object reference_object )
		{
			writer.Write( reference_object != null );
			if ( reference_object == null )
			{
				return;
			}

			Type object_type = reference_object.GetType();
			uint type_code = 0;
			if ( !m_TypesToTypeCodes.TryGetValue( object_type, out type_code ) )
			{
				throw new CApplicationException( String.Format( "Type {0} doesn't have a type code registered for it", object_type.Name ) );
			}

			writer.Write( type_code );

			DSerializationHandler< object > serializer = m_ReferenceSerializers[ object_type ];
			serializer( reference_object, writer );
		}

		private static object Deserialize_Reference_Object( BinaryReader reader )
		{
			bool is_non_null = reader.ReadBoolean();
			if ( !is_non_null )
			{
				return null;
			}

			uint type_code = reader.ReadUInt32();

			Type object_type = null;
			if ( !m_TypeCodesToTypes.TryGetValue( type_code, out object_type ) )
			{
				throw new CApplicationException( String.Format( "Unknown type code {0} encountered while deserializing a message", type_code ) );
			}

			object reference_object = FormatterServices.GetUninitializedObject( object_type );

			DDeserializationHandler< object > deserializer = m_ReferenceDeserializers[ object_type ];
			deserializer( reference_object, reader );

			return reference_object;
		}

		private static bool Serialization_Delegates_Exist( Type type )
		{
			return m_SerializationDelegates.ContainsKey( type ) && m_DeserializationDelegates.ContainsKey( type );
		}

		private static void Register_Builtin_Read_Write_Methods()
		{
			m_PrimitiveWriteMethods.Add( typeof( Int32 ), typeof( BinaryWriter ).GetMethod( "Write", new Type[] { typeof( Int32 ) } ) );
			m_PrimitiveReadMethods.Add( typeof( Int32 ), typeof( BinaryReader ).GetMethod( "ReadInt32" ) );

			m_PrimitiveWriteMethods.Add( typeof( UInt32 ), typeof( BinaryWriter ).GetMethod( "Write", new Type[] { typeof( UInt32 ) } ) );
			m_PrimitiveReadMethods.Add( typeof( UInt32 ), typeof( BinaryReader ).GetMethod( "ReadUInt32" ) );

			m_PrimitiveWriteMethods.Add( typeof( Int16 ), typeof( BinaryWriter ).GetMethod( "Write", new Type[] { typeof( Int16 ) } ) );
			m_PrimitiveReadMethods.Add( typeof( Int16 ), typeof( BinaryReader ).GetMethod( "ReadInt16" ) );

			m_PrimitiveWriteMethods.Add( typeof( UInt16 ), typeof( BinaryWriter ).GetMethod( "Write", new Type[] { typeof( UInt16 ) } ) );
			m_PrimitiveReadMethods.Add( typeof( UInt16 ), typeof( BinaryReader ).GetMethod( "ReadUInt16" ) );

			m_PrimitiveWriteMethods.Add( typeof( Int64 ), typeof( BinaryWriter ).GetMethod( "Write", new Type[] { typeof( Int64 ) } ) );
			m_PrimitiveReadMethods.Add( typeof( Int64 ), typeof( BinaryReader ).GetMethod( "ReadInt64" ) );

			m_PrimitiveWriteMethods.Add( typeof( UInt64 ), typeof( BinaryWriter ).GetMethod( "Write", new Type[] { typeof( UInt64 ) } ) );
			m_PrimitiveReadMethods.Add( typeof( UInt64 ), typeof( BinaryReader ).GetMethod( "ReadUInt64" ) );

			m_PrimitiveWriteMethods.Add( typeof( double ), typeof( BinaryWriter ).GetMethod( "Write", new Type[] { typeof( double ) } ) );
			m_PrimitiveReadMethods.Add( typeof( double ), typeof( BinaryReader ).GetMethod( "ReadDouble" ) );

			m_PrimitiveWriteMethods.Add( typeof( float ), typeof( BinaryWriter ).GetMethod( "Write", new Type[] { typeof( float ) } ) );
			m_PrimitiveReadMethods.Add( typeof( float ), typeof( BinaryReader ).GetMethod( "ReadSingle" ) );

			m_PrimitiveWriteMethods.Add( typeof( bool ), typeof( BinaryWriter ).GetMethod( "Write", new Type[] { typeof( bool ) } ) );
			m_PrimitiveReadMethods.Add( typeof( bool ), typeof( BinaryReader ).GetMethod( "ReadBoolean" ) );

			m_PrimitiveWriteMethods.Add( typeof( byte ), typeof( BinaryWriter ).GetMethod( "Write", new Type[] { typeof( byte ) } ) );
			m_PrimitiveReadMethods.Add( typeof( byte ), typeof( BinaryReader ).GetMethod( "ReadByte" ) );

			m_PrimitiveWriteMethods.Add( typeof( sbyte ), typeof( BinaryWriter ).GetMethod( "Write", new Type[] { typeof( sbyte ) } ) );
			m_PrimitiveReadMethods.Add( typeof( sbyte ), typeof( BinaryReader ).GetMethod( "ReadSByte" ) );

			m_PrimitiveWriteMethods.Add( typeof( char ), typeof( BinaryWriter ).GetMethod( "Write", new Type[] { typeof( char ) } ) );
			m_PrimitiveReadMethods.Add( typeof( char ), typeof( BinaryReader ).GetMethod( "ReadChar" ) );

			m_PrimitiveWriteMethods.Add( typeof( string ), typeof( BinaryWriter ).GetMethod( "Write", new Type[] { typeof( string ) } ) );
			m_PrimitiveReadMethods.Add( typeof( string ), typeof( BinaryReader ).GetMethod( "ReadString" ) );
		}

		private static void Process_Type( Type type )
		{
			uint type_code = (uint) type.Name.GetHashCode();

			m_TypeCodesToTypes[ type_code ] = type;
			m_TypesToTypeCodes[ type ] = type_code;

			if ( type.IsValueType || type.IsGenericType )
			{
				return;
			}

			Type base_type = type.BaseType;
			if ( base_type != null && Is_Registered_Type( base_type ) )
			{
				Add_Derived_Pair_Link( base_type, type );
			}

			TypeFilter interface_filter = new TypeFilter( (x, y) => true );
			Type []interfaces = type.FindInterfaces( interface_filter, "" );
			foreach ( Type interface_type in interfaces )
			{
				if ( !interface_type.IsGenericType && Is_Registered_Type( interface_type ) )
				{
					Add_Derived_Pair_Link( interface_type, type );
				}
			}
		}

		private static void Add_Derived_Pair_Link( Type base_type, Type type )
		{
			List< Type > derived_types = null;
			if ( !m_DerivedTypes.TryGetValue( base_type, out derived_types ) )
			{
				derived_types = new List< Type >();
				m_DerivedTypes[ base_type ] = derived_types;
			}

			derived_types.Add( type );
		}

		private static List< Type > Get_Derived_Types( Type base_type )
		{
			List< Type > derived_types = null;
			m_DerivedTypes.TryGetValue( base_type, out derived_types );

			return derived_types;
		}

		private static DSerializationHandler< CNetworkMessage > Get_Serialization_Handler( Type type )
		{
			DSerializationHandler< CNetworkMessage > handler = null;
			m_SerializationDelegates.TryGetValue( type, out handler );

			return handler;
		}

		private static DDeserializationHandler< CNetworkMessage > Get_Deserialization_Handler( Type type )
		{
			DDeserializationHandler< CNetworkMessage > handler = null;
			m_DeserializationDelegates.TryGetValue( type, out handler );

			return handler;
		}

		private static bool Is_Registered_Type( Type type )
		{
			var assembly = m_RegisteredAssemblies.FirstOrDefault( n => n.GetType( type.FullName ) != null );
			return assembly != null;
		}

		private static bool Has_Type_Been_Processed( Type type )
		{
			return m_SerializeDynamicMethods.ContainsKey( type );
		}

		private static void On_Type_Encountered( Type type )
		{
			if ( type.IsValueType || !Is_Registered_Type( type ) )
			{
				return;
			}

			m_SeenUserRefTypes.Add( type );
		}

		private static void Build_All_Reference_Serializers()
		{
			while( m_SeenUserRefTypes.Count > 0 )
			{
				m_SeenUserRefTypes.Apply( n => m_OpenTypes.Push( n ) );
				m_SeenUserRefTypes.Clear();

				while( m_OpenTypes.Count > 0 )
				{
					Type type = m_OpenTypes.Pop();

					if ( Has_Type_Been_Processed( type ) )
					{
						continue;
					}

					Build_Serializers( type );

					List< Type > derived_types = Get_Derived_Types( type );
					if ( derived_types != null )
					{
						derived_types.Where( n => !Has_Type_Been_Processed( n ) ).Apply( x => m_OpenTypes.Push( x ) );
					}
				}
			}
		}

		private static bool Is_Unary_Collection( Type type )
		{
			return type.IsGenericType && ( type.GetGenericTypeDefinition() == typeof( List<> ) || type.GetGenericTypeDefinition() == typeof( HashSet<> ) );
		}

		private static bool Is_Binary_Collection( Type type )
		{
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof( Dictionary< , > );
		}

		private static Dictionary< ENetworkMessageType, Type > m_MessageTypes = new Dictionary< ENetworkMessageType, Type >();

		private static Dictionary< Type, DSerializationHandler< CNetworkMessage > > m_SerializationDelegates = new Dictionary< Type, DSerializationHandler< CNetworkMessage > >();
		private static Dictionary< Type, DDeserializationHandler< CNetworkMessage > > m_DeserializationDelegates = new Dictionary< Type, DDeserializationHandler< CNetworkMessage > >();

		private static Dictionary< Type, DSerializationHandler< object > > m_ReferenceSerializers = new Dictionary< Type, DSerializationHandler< object > >();
		private static Dictionary< Type, DDeserializationHandler< object > > m_ReferenceDeserializers = new Dictionary< Type, DDeserializationHandler< object > >();

		private static Dictionary< Type, MethodInfo > m_PrimitiveReadMethods = new Dictionary< Type, MethodInfo >();
		private static Dictionary< Type, MethodInfo > m_PrimitiveWriteMethods = new Dictionary< Type, MethodInfo >();

		private static Dictionary< uint, Type > m_TypeCodesToTypes = new Dictionary< uint, Type >();
		private static Dictionary< Type, uint > m_TypesToTypeCodes = new Dictionary< Type, uint >();

		private static List< Assembly > m_RegisteredAssemblies = new List< Assembly >();

		private static Dictionary< Type, List< Type > > m_DerivedTypes = new Dictionary< Type, List< Type > >();

		private static HashSet< Type > m_SeenUserRefTypes = new HashSet< Type >();
		private static Stack< Type > m_OpenTypes = new Stack< Type >();

		private static Dictionary< Type, DynamicMethod > m_SerializeDynamicMethods = new Dictionary< Type, DynamicMethod >();
		private static Dictionary< Type, DynamicMethod > m_DeserializeDynamicMethods = new Dictionary< Type, DynamicMethod >();
	}
}