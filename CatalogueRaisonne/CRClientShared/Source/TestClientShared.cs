using System;

using CRShared;

namespace CRClientShared
{
	[SlashCommand( "Test_Client_Shared_Internal" )]
	public class CTestClientSharedInternalSlashCommand : CSlashCommand
	{
		public CTestClientSharedInternalSlashCommand() {}

		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Test; } }
	}

	public class CTestClientShared
	{
		public static void Run_Internal_Tests()
		{
			CTestShared.Run_Internal_Tests();
		}

		[GenericHandler]
		private static void Handle_Test_Client_Shared_Internal( CTestClientSharedInternalSlashCommand command )
		{
			Run_Internal_Tests();
		}
	}
}