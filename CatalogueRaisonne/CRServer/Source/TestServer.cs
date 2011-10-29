using System;

using CRShared;

namespace CRServer
{
	[SlashCommand( "Test_Server_Internal" )]
	public class CTestServerInternalSlashCommand : CSlashCommand
	{
		public CTestServerInternalSlashCommand() {}

		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Test; } }
	}

	public class CTestServer
	{
		public static void Run_Internal_Tests()
		{
			CTestShared.Run_Internal_Tests();
		}

		[GenericHandler]
		private static void Handle_Test_Server_Internal( CTestServerInternalSlashCommand command )
		{
			Run_Internal_Tests();
		}
	}
}