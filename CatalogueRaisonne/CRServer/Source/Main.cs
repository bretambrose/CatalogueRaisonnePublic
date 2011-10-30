/*

	Main.cs

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
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using CRShared;

namespace CRServer
{
	class CLostCitiesServer
	{
		static void Main(string[] args)
		{
			try
			{
				Initialize();			
			}
			catch ( Exception e )
			{
				Console.WriteLine( "Initialization Error: " + e.Message );
				Console.WriteLine( "Hit any key to terminate process." );
				while ( !Console.KeyAvailable ) {;}
				return;
			}
			
			try
			{
				Main_Loop();
			}
			finally
			{	
				Shutdown();
			}

			Console.WriteLine( "Server Finished.  Hit any key to terminate process." );
			while ( !Console.KeyAvailable )
			{
				;
			}
		}
		
		static void Initialize()
		{
			CServerResource.Initialize_Resources();

			CServerLogicalThread.Load_Settings();
			CServerLogicalThread.Find_Slash_Commands();

			CLog.Configure( "Server" );

			CNetworkMessageSerializationManager.Register_Assembly( Assembly.GetAssembly( typeof( CNetworkMessage ) ) );
			CNetworkMessageSerializationManager.Build_Serialization_Objects();

			m_AuxiliaryThreads.Add( CServerNetworkThread.Allocate_Thread() );
			m_AuxiliaryThreads.Add( CServerConsoleUIThread.Allocate_Thread() );
			m_AuxiliaryThreads.Add( CLoggingThread.Allocate_Thread() );
			m_AuxiliaryThreads.Add( CPersistenceThread.Allocate_Thread() );				
		}
		
		private static void Shutdown()
		{
			// Wait for all threads to exit
			bool ready_to_exit = false;
			while ( !ready_to_exit )
			{
				ready_to_exit = true;

				Thread.Sleep( 500 );

				m_AuxiliaryThreads.Aggregate( ready_to_exit, ( x, t ) => x && !t.IsAlive );
			}

			if ( CLog.Had_Fatal_Exception() )
			{
				CLog.Write_Crash_File();
				Console.WriteLine( String.Format( "Fatal Exception: See {0}/{1} for details", CLog.Get_Crash_Directory(), CLog.Get_Crash_Filename() ) );
			}
		}
		
		private static void Main_Loop()
		{
			m_AuxiliaryThreads.Apply( t => t.Start() );

			CServerLogicalThread.Start_Main_Thread();
		}

		// Fields
		private static List< Thread > m_AuxiliaryThreads = new List< Thread >();

	}
}
