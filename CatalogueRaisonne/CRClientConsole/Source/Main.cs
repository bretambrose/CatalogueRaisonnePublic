using System;
using System.Net;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using CRShared;
using LCClient;
using CRClientShared;

namespace CRClientConsole
{
	
	class CClientConsole
	{
		
		// Methods	
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
					
			Console.WriteLine( "Client Finished.  Hit any key to terminate process." );
			while ( !Console.KeyAvailable )
			{
				;
			}
		}
		
		private static void Initialize()
		{
			CClientResource.Initialize_Resources();

			CClientLogicalThread.Load_Settings();
			CClientLogicalThread.Find_Slash_Commands();

			CLog.Configure( "Client" );

			CNetworkMessageSerializationManager.Register_Assembly( Assembly.GetAssembly( typeof( CNetworkMessage ) ) );
			CNetworkMessageSerializationManager.Build_Serialization_Objects();
													
			m_AuxiliaryThreads.Add( CClientNetworkThread.Allocate_Thread() );
			m_AuxiliaryThreads.Add( CClientConsoleUIThread.Allocate_Thread() );
			m_AuxiliaryThreads.Add( CLoggingThread.Allocate_Thread() );

			CClientLogicalThread.Instance.Add_UI_Notification( new CUIScreenStateNotification( EUIScreenState.Main_Menu ) );	
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

			Thread.Sleep( 2000 );

			if ( CLog.Had_Fatal_Exception() )
			{
				CLog.Write_Crash_File();
				Console.WriteLine( String.Format( "Fatal Exception: See {0}/{1} for details", CLog.Get_Crash_Directory(), CLog.Get_Crash_Filename() ) );
			}
		}
		
		private static void Main_Loop()
		{
			foreach ( var thread in m_AuxiliaryThreads )
			{
				thread.Start();
			}

			CClientLogicalThread.Start_Main_Thread();
		}

		private static List< Thread > m_AuxiliaryThreads = new List< Thread >();
	}
}
