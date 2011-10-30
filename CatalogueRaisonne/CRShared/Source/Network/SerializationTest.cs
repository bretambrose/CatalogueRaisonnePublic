/*

	SerializationTest.cs

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
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;

using CRShared.Chat;

namespace CRShared
{

	public enum ETestULongEnum : ulong
	{
		Invalid = 0,

		Test
	}

	public enum ETestEnum
	{
		Invalid = 0,

		Test1,
		Test2
	}

	public class CTestEmbedded
	{
		public CTestEmbedded()
		{
			m_TestString = null;
			m_TestInt = 0;
		}

		public CTestEmbedded( int test_int, string test_string )
		{
			m_TestString = test_string;
			m_TestInt = test_int;
		}

		private int m_TestInt;
		private string m_TestString;
	}

	public class CTestBase
	{
		public CTestBase( bool test_bool )
		{
			m_TestBool = test_bool;
		}

		private bool m_TestBool = false;
	}

	public class CTestDerived1 : CTestBase
	{
		public CTestDerived1( string test_string, bool test_bool ) :
			base( test_bool )
		{
			m_TestString = test_string;
		}

		private string m_TestString = null;
	}

	public class CTestDerived2 : CTestBase
	{
		public CTestDerived2( long test_long, bool test_bool ) :
			base( test_bool )
		{
			m_TestLong = test_long;
		}

		public bool Is_Test_Data_Zero() { return m_TestLong == 0; }

		private long m_TestLong = 0;
	}

	public interface ITest
	{
		object Clone();
	}

	public class CInterfaceTest : ITest
	{
		public CInterfaceTest( int test_int ) { m_TestInt = test_int; }

		public object Clone()
		{
			return new CInterfaceTest( m_TestInt );
		}

		private int m_TestInt = 0;
	}

	[NetworkMessage]
	public class CNetworkMessageTest : CNetworkMessage
	{
		public CNetworkMessageTest( int test_int ) :
			base()
		{
			// m_TestInt32 = test_int;
		}

		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Serialization_Test; } }

		public DateTime m_TestTime = DateTime.Now;

		public string m_TestString = "";

		public ETestEnum m_TestEnum = ETestEnum.Invalid;
		public ETestULongEnum m_TestULongEnum = ETestULongEnum.Invalid;

		public int m_TestInt32 = 0;
		public uint m_TestUInt32 = 0;
		public short m_TestInt16 = 0;
		public ushort m_TestUInt16 = 0;
		public byte m_TestByte = 0;
		public sbyte m_TestSByte = 0;
		public long m_TestInt64 = 0;
		public ulong m_TestUInt64 = 0;
		public double m_TestDouble = 0.0;
		public float m_TestFloat = 0.0f;
		public char m_TestChar = 'A';
		public bool m_TestBool = false;

		public CTestEmbedded m_NullTest = null;
		public CTestEmbedded m_EmbeddedTest = null;

		public CTestBase m_TestBase = null;
		public CTestBase m_TestDerived1 = null;
		public CTestBase m_TestDerived2 = null;

		public DateTime m_Now = DateTime.Now;

		public List< int > m_PrimitiveListTest = new List< int >();

		public HashSet< int > m_PrimitiveSetTest = new HashSet< int >();

		public List< int > m_NullListTest = null;

		public List< CTestBase > m_RefListTest = new List< CTestBase >();

		public Dictionary< int, int > m_NullDictionaryTest = null;

		public Dictionary< int, int > m_ValueValueTest = new Dictionary< int, int >();

		public Dictionary< int, CTestBase > m_ValueRefTest = new Dictionary< int, CTestBase >();

		public Dictionary< CTestBase, int > m_RefValueTest = new Dictionary< CTestBase, int >();

		public Dictionary< CTestBase, CTestBase > m_RefRefTest = new Dictionary< CTestBase, CTestBase >();

		public List< List< int > > m_NullListOfPrimList = null;

		public List< List< int > > m_ListOfPrimList = new List< List< int > >();
		public List< HashSet< int > > m_ListOfPrimSet = new List< HashSet< int > >();
		public List< List< string > > m_ListOfStringList = new List< List< string > >();
		public List< List< CTestBase > > m_ListOfRefList = new List< List< CTestBase > >();
		public List< List< DateTime > > m_ListOfDateTimeList = new List< List< DateTime > >();
		public List< List< ETestEnum > > m_ListOfEnumList = new List< List< ETestEnum > >();
		public List< List< List< int > > > m_ListOfListOfPrimList = new List< List< List< int > > >();
		public List< Dictionary< int, int > > m_ListOfPrimDictionary = new List< Dictionary< int, int > >();

		public Dictionary< int, List< int > > m_NullDictionaryOfPrimList = null;

		public Dictionary< int, Dictionary< int, int > > m_DictOfPrimDict = new Dictionary< int, Dictionary< int, int > >();
		public Dictionary< ETestEnum, Dictionary< int, string > > m_DictOfStringDict = new Dictionary< ETestEnum, Dictionary< int, string > >();
		public Dictionary< string, Dictionary< int, DateTime > > m_StringDictOfTimeDict = new Dictionary< string, Dictionary< int, DateTime > >();
		public Dictionary< string, Dictionary< ETestEnum, CTestBase > > m_StringDictOfRefDict = new Dictionary< string, Dictionary< ETestEnum, CTestBase > >();

	}

	[NetworkMessage]
	public class CNetworkMessageTest2 : CNetworkMessage
	{
		public CNetworkMessageTest2() :
			base()
		{
		}

		public override ENetworkMessageType MessageType { get { return ENetworkMessageType.Message_Serialization_Test2; } }

		public ITest m_Interface = null;

		public List< ITest > m_InterfaceTests = new List< ITest >();
	
		public Dictionary< int, int > m_NullDictionary = null;

		public Dictionary< int, int > m_PrimPrim = new Dictionary< int, int >();
		public Dictionary< int, DateTime > m_PrimTime = new Dictionary< int, DateTime >();
		public Dictionary< int, EPersistenceID > m_PrimEnum = new Dictionary< int, EPersistenceID >();
		public Dictionary< int, string > m_PrimString = new Dictionary< int, string >();
		public Dictionary< EPersistenceID, int > m_EnumPrim = new Dictionary< EPersistenceID, int >();
		public Dictionary< DateTime, int > m_TimePrim = new Dictionary< DateTime, int >();
		public Dictionary< string, int > m_StringPrim = new Dictionary< string, int >();

		public Dictionary< CTestBase, int > m_RefPrim = new Dictionary< CTestBase, int >();
		public Dictionary< int, CTestBase > m_PrimRef = new Dictionary< int, CTestBase >();
		public Dictionary< CTestBase, CTestBase > m_RefRef = new Dictionary< CTestBase, CTestBase >();

	}

	public class CSerializationTester
	{

		public static void Test_Basic_Functionality()
		{
			// test
			MemoryStream test_stream = new MemoryStream();

			CNetworkMessageTest2 test2 = new CNetworkMessageTest2();

			test2.m_Interface = new CInterfaceTest( 5 );

			test2.m_InterfaceTests.Add( new CInterfaceTest( 5 ) );
			test2.m_InterfaceTests.Add( new CInterfaceTest( 6 ) );

			test2.m_PrimPrim.Add( 1, 2 );
			test2.m_PrimPrim.Add( 3, 4 );
			test2.m_PrimTime.Add( 4, DateTime.Now );
			test2.m_PrimTime.Add( 5, DateTime.Now );
			test2.m_PrimEnum.Add( 6, (EPersistenceID) 10 );
			test2.m_PrimEnum.Add( 7, (EPersistenceID) 20 );
			test2.m_PrimString.Add( 6, "String1" );
			test2.m_PrimString.Add( 7, "Blah" );
			test2.m_EnumPrim.Add( (EPersistenceID) 20, 40 );
			test2.m_EnumPrim.Add( (EPersistenceID) 30, 40 );
			test2.m_TimePrim.Add( DateTime.Now, 50 );
			test2.m_StringPrim.Add( "Key1", 40 );
			test2.m_StringPrim.Add( "Key2", 80 );

			CNetworkMessageSerializationManager.Serialize( test2, test_stream );
			test_stream.Position = 4;

			CNetworkMessageTest2 deserialized_test2 = CNetworkMessageSerializationManager.Deserialize( test_stream ) as CNetworkMessageTest2;

			test_stream.Position = 0;

			CNetworkMessageTest test = new CNetworkMessageTest( 21 );

			test.m_TestInt32 = 1;
			test.m_TestUInt32 = 2;
			test.m_TestInt16 = 3;
			test.m_TestUInt16 = 4;
			test.m_TestByte = 5;
			test.m_TestSByte = 6;
			test.m_TestInt64 = 7;
			test.m_TestUInt64 = 8;
			test.m_TestDouble = 9.0;
			test.m_TestFloat = 10.0f;
			test.m_TestChar = 'Z';
			test.m_TestBool = true;
			test.m_TestEnum = ETestEnum.Test2;
			test.m_TestULongEnum = ETestULongEnum.Test;
			test.m_TestString = "Test";
			test.m_EmbeddedTest = new CTestEmbedded( 5, "EmbeddedString" );

			test.m_TestBase = new CTestBase( true );
			test.m_TestDerived1 = new CTestDerived1( "Derived1!", true );
			test.m_TestDerived2 = new CTestDerived2( 42, true );

			test.m_PrimitiveListTest.Add( 1 );
			test.m_PrimitiveListTest.Add( 3 );

			test.m_PrimitiveSetTest.Add( 21 );
			test.m_PrimitiveSetTest.Add( 42 );

			test.m_RefListTest.Add( new CTestBase( true ) );
			test.m_RefListTest.Add( new CTestDerived1( "ListDerived1!", true ) );
			test.m_RefListTest.Add( new CTestDerived2( 256, true ) );

			test.m_ValueValueTest.Add( 1, 5 );
			test.m_ValueValueTest.Add( 2, 10 );

			test.m_ValueRefTest.Add( 3, new CTestDerived1( "ValueRefDerived1!", true ) );
			test.m_ValueRefTest.Add( 8, new CTestDerived2( 65536, true ) );

			test.m_RefValueTest.Add( new CTestDerived1( "Derived1Key!", true ), 20 );
			test.m_RefValueTest.Add( new CTestDerived2( 1024, true ), 50 );

			test.m_RefRefTest.Add( new CTestDerived1( "Derived1Key!", true ), new CTestBase( true ) );
			test.m_RefRefTest.Add( new CTestDerived2( 512, true ), new CTestDerived1( "Derived1Value!", true ) );

			List< int > prim_list1 = new List< int >();
			List< int > prim_list2 = new List< int >();
			prim_list2.Add( 5 );
			prim_list2.Add( 10 );

			test.m_ListOfPrimList.Add( null );
			test.m_ListOfPrimList.Add( prim_list1 );
			test.m_ListOfPrimList.Add( null );
			test.m_ListOfPrimList.Add( prim_list2 );
			test.m_ListOfPrimList.Add( null );

			HashSet< int > prim_set1 = new HashSet< int >();
			HashSet< int > prim_set2 = new HashSet< int >();
			prim_set2.Add( 6 );
			prim_set2.Add( 12 );

			test.m_ListOfPrimSet.Add( prim_set1 );
			test.m_ListOfPrimSet.Add( prim_set2 );

			List< string > string_list1 = new List< string >();
			List< string > string_list2 = new List< string >();
			string_list1.Add( "" );
			string_list2.Add( "TestString" );
			string_list2.Add( null );

			test.m_ListOfStringList.Add( string_list1 );
			test.m_ListOfStringList.Add( string_list2 );

			List< CTestBase > ref_list1 = new List< CTestBase >();
			ref_list1.Add( new CTestDerived1( "Listttttttt", true ) );
			ref_list1.Add( null );
			ref_list1.Add( new CTestDerived2( 666, false ) );

			test.m_ListOfRefList.Add( ref_list1 );

			List< DateTime > date_time_list = new List< DateTime >();
			date_time_list.Add( DateTime.Now );

			test.m_ListOfDateTimeList.Add( date_time_list );

			List< ETestEnum > enum_list = new List< ETestEnum >();
			enum_list.Add( ETestEnum.Test1 );
			enum_list.Add( ETestEnum.Test2 );

			test.m_ListOfEnumList.Add( enum_list );

			List< int > int_list = new List< int >();
			int_list.Add( 500 );
			int_list.Add( 1000 );

			List< List< int > > int_list_list = new List< List< int > >();
			int_list_list.Add( int_list );

			test.m_ListOfListOfPrimList.Add( int_list_list );

			Dictionary< int, int > prim_dictionary1 = new Dictionary< int, int >();
			prim_dictionary1.Add( 1, 2 );
			prim_dictionary1.Add( 3, 6 );

			Dictionary< int, int > prim_dictionary2 = new Dictionary< int, int >();
			prim_dictionary2.Add( 10, 20 );
			prim_dictionary2.Add( 30, 60 );

			test.m_ListOfPrimDictionary.Add( null );
			test.m_ListOfPrimDictionary.Add( prim_dictionary1 );
			test.m_ListOfPrimDictionary.Add( null );
			test.m_ListOfPrimDictionary.Add( prim_dictionary2 );
			test.m_ListOfPrimDictionary.Add( null );

			Dictionary< int, int > prim_dict1 = new Dictionary< int, int >();
			prim_dict1.Add( 100, 200 );
			prim_dict1.Add( 300, 600 );

			Dictionary< int, int > prim_dict2 = new Dictionary< int, int >();
			prim_dict2.Add( -4, -2 );
			prim_dict2.Add( -666, -333 );

			test.m_DictOfPrimDict.Add( 50, prim_dict1 );
			test.m_DictOfPrimDict.Add( 100, prim_dict2 );
			test.m_DictOfPrimDict.Add( 500, null );

			Dictionary< int, string > string_dict1 = new Dictionary< int, string >();
			string_dict1.Add( 100, "BlahBlah" );
			string_dict1.Add( 300, null );

			Dictionary< int, string > string_dict2 = new Dictionary< int, string >();
			string_dict2.Add( -4, "NEGATIVE" );
			string_dict2.Add( -666, "" );

			test.m_DictOfStringDict.Add( ETestEnum.Test1, string_dict1 );
			test.m_DictOfStringDict.Add( ETestEnum.Test2, string_dict2 );

			Dictionary< int, DateTime > time_dict1 = new Dictionary< int, DateTime >();
			time_dict1.Add( 0, DateTime.Now );

			test.m_StringDictOfTimeDict.Add( "Bonzai", time_dict1 );

			Dictionary< ETestEnum, CTestBase > ref_dict1 = new Dictionary< ETestEnum, CTestBase >();
			ref_dict1.Add( ETestEnum.Test1, new CTestDerived1( "DFJKLJKL", true ) );
			ref_dict1.Add( ETestEnum.Test2, new CTestDerived2( 333, false ) );

			test.m_StringDictOfRefDict.Add( "Refffff", ref_dict1 );

			CNetworkMessageSerializationManager.Serialize( test, test_stream );
			test_stream.Position = 4;

			CNetworkMessage deserialized_test = CNetworkMessageSerializationManager.Deserialize( test_stream );
		}

		public static void Test_Serialize_All_Messages()
		{
			List< CNetworkMessage > messages = new List< CNetworkMessage >();

			Build_Match_Message_Samples( messages );
			Build_Browse_Lobby_Message_Samples( messages );
			Build_Chat_Message_Samples( messages );
			Build_Connection_Message_Samples( messages );
			Build_Lobby_Message_Samples( messages );
			Build_Social_Message_Samples( messages );

			int total_new_size = 0;
			MemoryStream test_stream = new MemoryStream();

			foreach ( var message in messages )
			{
				test_stream.Position = 0;
				test_stream.SetLength( 0 );
				CNetworkMessageSerializationManager.Serialize( message, test_stream );

				test_stream.Position = 4;
				CNetworkMessage deserialized_test = CNetworkMessageSerializationManager.Deserialize( test_stream );
				total_new_size += (int) test_stream.Position;

				string pre_serialize = message.ToString();
				string post_serialize = deserialized_test.ToString();
				if ( pre_serialize != post_serialize )
				{
					Console.WriteLine( "SERIALIZATION MISMATCH:" );
					Console.WriteLine( pre_serialize );
					Console.WriteLine( post_serialize );
				}
			}
		}

		private static void Build_Chat_Message_Samples( List< CNetworkMessage > message_list )
		{
			message_list.Add( new CBroadcastToChatChannelMessage( EChannelID.Invalid, "This is a sample chat broadcast" ) );
			message_list.Add( new CPlayerChatMessage( EChannelID.Invalid, "PlayerName", "A sample player chat message" ) );
			message_list.Add( new CBroadcastFailureMessage( EChannelBroadcastError.UnknownChannel ) );
			message_list.Add( new CCreateOrJoinChatChannelMessage( "SampleChatChannel" ) );

			CJoinChatChannelResultMessage join_result = new CJoinChatChannelResultMessage();
			join_result.ChannelID = EChannelID.Invalid;
			join_result.ChannelName = "JoinedChannel";
			join_result.AnnounceJoinLeave = true;
			join_result.CreateError = EChannelCreationError.InvalidExternalName;
			join_result.JoinError = EChannelJoinError.ChannelDoesNotExist;
			join_result.Moderator = ( EPersistenceID ) 5;
			for ( int i = 0; i < 20; i++ )
			{
				join_result.Add_Member( (EPersistenceID) i );
			}
			for ( int i = 11; i < 14; i++ )
			{
				join_result.Add_Gagged( (EPersistenceID) i );
			}

			message_list.Add( join_result );
			message_list.Add( new CLeaveChatChannelMessage( EChannelID.Invalid ) );
			message_list.Add( new CLeaveChatChannelFailureMessage( ELeaveChatChannelFailureReason.Not_A_Member ) );
			message_list.Add( new CChatClientNotificationMessage( (EChannelID) 2, "SomeTargetPlayer", (EPersistenceID) 3, EChatNotification.Player_Kicked_From_Channel, "SomeSourcePlayer" ) );
			message_list.Add( new CLeftChatChannelMessage( (EChannelID) 3, ELeftChannelReason.Channel_Shutdown, "SomeLeavingPlayer" ) );
			message_list.Add( new CChatModOperationMessage( (EChannelID) 4, EChatModOperation.Ungag, "SomeUngaggedPlayer" ) );
			message_list.Add( new CChatModOperationErrorMessage( EChatModOperationError.No_Such_Player_In_Channel ) );
			message_list.Add( new CPlayerTellMessage( "SomePlayer", "A chat tell from one player to another" ) );
			message_list.Add( new CPlayerTellRequest( "SirTalkyTalk", "SirTalkyTalk's steamy tell to his secret lover." ) );
			message_list.Add( new CPlayerTellResponse( EMessageRequestID.Start ) );
		}

		private static void Build_Connection_Message_Samples( List< CNetworkMessage > message_list )
		{
			message_list.Add( new CClientHelloRequest( "SomeLoginName" ) );
			message_list.Add( new CClientHelloResponse( EMessageRequestID.Start, new CPersistentPlayerData( (EPersistenceID) 1, "PlayerName" ) ) );
			message_list.Add( new CConnectionDroppedMessage( EConnectRefusalReason.Name_Already_Connected ) );
			message_list.Add( new CDisconnectRequestMessage() );
			message_list.Add( new CDisconnectResultMessage( EDisconnectReason.Client_Request_Quit ) );
			message_list.Add( new CPingMessage() );

			CQueryPlayerInfoRequest query_info_request = new CQueryPlayerInfoRequest();
			for ( int i = 0; i < 5; i++ )
			{
				query_info_request.Add_Player( (EPersistenceID) i );
			}
			message_list.Add( query_info_request );

			CQueryPlayerInfoResponse query_info_response = new CQueryPlayerInfoResponse( EMessageRequestID.Start );
			for ( int i = 0; i < 5; i++ )
			{
				query_info_response.Add_Player_Info( new CPlayerInfo( (EPersistenceID) i, "SomePlayer" + i.ToString() ) );
			}
			message_list.Add( query_info_response );
		}

		private static void Build_Lobby_Message_Samples( List< CNetworkMessage > message_list )
		{
			CLobbyConfig lobby_config = new CLobbyConfig();
			lobby_config.Initialize( "SampleLobbyConfig", EGameModeType.Four_Players, true, "SamplePassword" );

			message_list.Add( new CCreateLobbyRequest( lobby_config ) );
			message_list.Add( new CCreateLobbyFailure( EMessageRequestID.Start, ECreateLobbyFailureReason.Invalid_Config_Data ) );

			CLobbyState lobby_state = new CLobbyState( (ELobbyID) 666, lobby_config, (EPersistenceID) 5 );
			for ( int i = 0; i < 4; i++ )
			{
				lobby_state.Members.Add( (EPersistenceID) i, new CLobbyMember( (EPersistenceID) i ) );
				lobby_state.PlayersBySlot.Add( (uint) i, (EPersistenceID) i );
			}

			for ( int i = 0; i < 2; i++ )
			{
				lobby_state.Members.Add( (EPersistenceID) (i + 4), new CLobbyMember( (EPersistenceID) (i + 4) ) );
				lobby_state.Observers.Add( (EPersistenceID) (i + 4) );
			}

			message_list.Add( new CCreateLobbySuccess( EMessageRequestID.Start, lobby_state ) );
			message_list.Add( new CJoinLobbyByPlayerRequest( "SomePlayerName", "SomePassword" ) );
			message_list.Add( new CJoinLobbyByIDRequest( (ELobbyID) 5 ) );
			message_list.Add( new CJoinLobbyFailure( EMessageRequestID.Start, EJoinLobbyFailureReason.Creator_Is_Ignoring_You ) );
			message_list.Add( new CJoinLobbySuccess( EMessageRequestID.Start, lobby_state ) );
			message_list.Add( new CLeaveLobbyRequest() );
			message_list.Add( new CLeaveLobbyResponse( EMessageRequestID.Start, ELeaveLobbyFailureReason.Creator_Cannot_Leave ) );

			message_list.Add( new CLobbyOperationMessage( new CLobbyMemberJoinedOperation( (EPersistenceID) 4, "LeavingPlayer", ELobbyMemberType.Player, 1 ) ) );
			message_list.Add( new CLobbyOperationMessage( new CLobbyMemberLeftOperation( (EPersistenceID) 4, ERemovedFromLobbyReason.Lobby_Destroyed_By_Creator ) ) );
			message_list.Add( new CLobbyOperationMessage( new CLobbyMemberMovedOperation( (EPersistenceID) 4, ELobbyMemberType.Player, 1 ) ) );
			message_list.Add( new CLobbyOperationMessage( new CLobbyMembersSwappedOperation( (EPersistenceID) 4, (EPersistenceID) 5 ) ) );
			message_list.Add( new CLobbyOperationMessage( new CLobbyMemberChangeStateOperation( (EPersistenceID) 4, ELobbyMemberState.Ready ) ) );
			message_list.Add( new CLobbyOperationMessage( new CLobbyPlayerBannedOperation( (EPersistenceID) 4 ) ) );
			message_list.Add( new CLobbyOperationMessage( new CLobbyPlayerUnbannedOperation( (EPersistenceID) 4 ) ) );

			message_list.Add( new CDestroyLobbyRequest() );
			message_list.Add( new CDestroyLobbyResponse( EMessageRequestID.Start, EDestroyLobbyFailureReason.Not_In_A_Lobby ) );

			message_list.Add( new CLobbyChangeMemberStateMessage( ELobbyMemberState.Disconnected ) );
			message_list.Add( new CKickPlayerFromLobbyRequest( "PlayerToKick" ) );
			message_list.Add( new CKickPlayerFromLobbyResponse( EMessageRequestID.Start, EKickPlayerFromLobbyError.Cannot_Kick_Self ) );
			message_list.Add( new CBanPlayerFromLobbyRequest( "PlayerToBan" ) );
			message_list.Add( new CBanPlayerFromLobbyResponse( EMessageRequestID.Start, EBanPlayerFromLobbyError.Not_Lobby_Creator ) );
			message_list.Add( new CUnbanPlayerFromLobbyRequest( "PlayerToUnban" ) );
			message_list.Add( new CUnbanPlayerFromLobbyResponse( EMessageRequestID.Start, EUnbanPlayerFromLobbyError.Player_Not_Banned ) );
			message_list.Add( new CUnbannedFromLobbyNotificationMessage( "UnbannedPlayer" ) );
			message_list.Add( new CMovePlayerInLobbyRequest( (EPersistenceID) 10, ELobbyMemberType.Player, 1 ) );
			message_list.Add( new CMovePlayerInLobbyResponse( EMessageRequestID.Start, EMovePlayerInLobbyError.Invalid_Move_Destination ) );
			message_list.Add( new CLobbyStartMatchRequest() );
			message_list.Add( new CLobbyStartMatchResponse( EMessageRequestID.Start, EStartMatchError.Not_Everyone_Ready ) );
		}

		private static void Build_Social_Message_Samples( List< CNetworkMessage > message_list )
		{
			message_list.Add( new CIgnorePlayerRequest( "PlayerToIgnore" ) );
			message_list.Add( new CIgnorePlayerResponse( EMessageRequestID.Start, (EPersistenceID) 10, EIgnorePlayerResult.Cannot_Ignore_Self ) );
			message_list.Add( new CUnignorePlayerRequest( "PlayerToUnignore" ) );
			message_list.Add( new CUnignorePlayerResponse( EMessageRequestID.Start, (EPersistenceID) 15, EUnignorePlayerResult.Persistence_Error ) );
		}
		
		private static void Build_Browse_Lobby_Message_Samples( List< CNetworkMessage > message_list )
		{
			message_list.Add( new CStartBrowseLobbyRequest( EGameModeType.Two_Players, ELobbyMemberType.Player, true ) );

			CLobbyConfig lobby_config = new CLobbyConfig();
			lobby_config.Initialize( "Pimps", EGameModeType.Two_Players, false, "BigPimpin" );

			CStartBrowseLobbyResponse browse_response = new CStartBrowseLobbyResponse( EMessageRequestID.Invalid, EStartBrowseResult.Success );
			for ( int i = 0; i < 10; i++ )
			{
				CLobbySummary lobby_summary = new CLobbySummary();
				lobby_summary.Initialize( ELobbyID.Invalid, lobby_config, DateTime.Now, EPersistenceID.Invalid, 2, 2 );
				browse_response.Add_Summary( lobby_summary );
			}

			message_list.Add( browse_response );
			message_list.Add( new CEndBrowseLobbyMessage() );
			message_list.Add( new CBrowseNextLobbySetRequest() );
			message_list.Add( new CBrowsePreviousLobbySetRequest() );

			CCursorBrowseLobbyResponse cursor_response = new CCursorBrowseLobbyResponse( EMessageRequestID.Invalid, ECursorBrowseResult.Success );
			for ( int i = 0; i < 10; i++ )
			{
				CLobbySummary lobby_summary = new CLobbySummary();
				lobby_summary.Initialize( ELobbyID.Invalid, lobby_config, DateTime.Now, EPersistenceID.Invalid, 2, 2 );
				cursor_response.Add_Summary( lobby_summary );
			}

			message_list.Add( cursor_response );

			CLobbySummary summary = new CLobbySummary();
			summary.Initialize( ELobbyID.Invalid, lobby_config, DateTime.Now, EPersistenceID.Invalid, 1, 4 );
			message_list.Add( new CBrowseLobbyAddRemoveMessage( summary, ELobbyID.Invalid ) );

			CLobbySummaryDelta summary_delta = new CLobbySummaryDelta();
			summary_delta.Initialize( ELobbyID.Invalid, 4, 1 );
			message_list.Add( new CBrowseLobbyDeltaMessage( summary_delta ) );
		}

		private static void Build_Match_Message_Samples( List< CNetworkMessage > message_list )
		{
			List< EPersistenceID > player_list = new List< EPersistenceID >();
			player_list.Add( (EPersistenceID) 2 );
			player_list.Add( (EPersistenceID) 3 );

			CMatchState match_state = new CMatchState( (EMatchInstanceID) 1, EGameModeType.Two_Players, 3 );
			match_state.Initialize_Match(player_list );

			CGameState game_state = new CGameState( EGameModeType.Two_Players );
			game_state.Initialize_Game( player_list );

			message_list.Add( new CJoinMatchSuccess( match_state, game_state ) );

			message_list.Add( new CMatchPlayerLeftMessage( (EPersistenceID) 5, EMatchRemovalReason.Player_Request ) );
			message_list.Add( new CMatchPlayerConnectionStateMessage( (EPersistenceID) 11, true ) );
			message_list.Add( new CLeaveMatchRequest() );
			message_list.Add( new CLeaveMatchResponse( (EMessageRequestID) 2, ELeaveMatchFailureError.Not_In_Match ) );

			message_list.Add( new CMatchTakeTurnRequest( new CPlayCardGameAction( new CCard( ECardColor.Green, ECardValue.Four ) ), new CDrawFromDiscardGameAction( ECardColor.Red ) ) );
			message_list.Add( new CMatchTakeTurnResponse( (EMessageRequestID) 5, EGameActionFailure.Card_Is_Not_In_Your_Hand ) );

			List< IObservedClonableDelta > deltas = new List< IObservedClonableDelta >();
			deltas.Add( new CCardCollection.CCardCollectionDelta( EGameSide.Side2, ECardColor.Blue, ECardValue.Eight ) );
			deltas.Add( new CDeck.CDeckDelta() );
			deltas.Add( new CDiscardPile.CDiscardPileDelta( ECardDelta.Add, ECardColor.White, ECardValue.Multiplier2 ) );
			deltas.Add( new CGameState.CGameStateDelta( 2 ) );
			deltas.Add( new CPlayerHand.CPlayerHandDelta( (EPersistenceID)6, ECardDelta.Remove, ECardColor.Yellow, ECardValue.Two ) );

			message_list.Add( new CMatchDeltaSetMessage( deltas ) );

			List< CAbstractMatchDelta > match_deltas = new List< CAbstractMatchDelta >();
			match_deltas.Add( new CSideMatchStats.CSideMatchStatsDelta( EGameSide.Side2, 132, 2, 3, 2 ) );

			message_list.Add( new CMatchStateDeltaMessage( match_deltas ) );

			message_list.Add( new CContinueMatchRequest( true ) );
			message_list.Add( new CContinueMatchResponse( (EMessageRequestID) 5, EContinueMatchFailure.Cannot_Change_Previous_Commitment ) );

			message_list.Add( new CMatchNewGameMessage( game_state ) );
		}
	}
}