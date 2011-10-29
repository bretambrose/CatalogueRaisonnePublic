using System;

namespace CRShared
{
	[SlashCommand( "Test_Shared_Internal" )]
	public class CTestSharedInternalSlashCommand : CSlashCommand
	{
		public CTestSharedInternalSlashCommand() {}

		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Test; } }
	}

	public class CTestShared
	{
		public static void Run_Internal_Tests()
		{
			CSerializationTester.Test_Basic_Functionality();
			CSerializationTester.Test_Serialize_All_Messages();
		}

		[GenericHandler]
		private static void Handle_Test_Shared_Internal( CTestSharedInternalSlashCommand command )
		{
			Run_Internal_Tests();
		}
	}
}