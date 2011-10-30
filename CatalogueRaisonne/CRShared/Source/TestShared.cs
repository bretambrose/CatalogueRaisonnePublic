/*

	TestShared.cs

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