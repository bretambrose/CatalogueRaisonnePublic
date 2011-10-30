/*

	LoggingInterface.cs

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
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace CRShared
{
	public enum ELoggingChannel 
	{
		Invalid,

		Logic,
		Error,
		Network,
		Game,
		UI,
		Persistence,
		Chat,
		Lobby
	}

	public enum ELogLevel : long
	{
		None,
		Low,
		Medium,
		High,
		Very_High
	}

	public static class CLog
	{
		// Embedded classes
		public class CThreadedException
		{
			public CThreadedException( Exception exception, int thread_id )
			{
				CrashException = exception;
				ThreadID = thread_id;
			}

			public Exception CrashException { get; private set; }
			public int ThreadID { get; private set; }
		}

		// Construction
		static CLog() {}

		// Methods
		// Public interface
		[Conditional("ENABLE_LOGGING")]
		public static void Log( ELoggingChannel channel, ELogLevel log_level, string log_text )
		{
			if ( log_level > Get_Log_Level() || log_level <= ELogLevel.None )
			{
				return;
			}

			if ( Frame == null )
			{
				Frame = new COutboundLogFrame();
			}

			Frame.Add_Request( new CLogMessageRequest( channel, log_text ) );
		}

		[Conditional("ENABLE_LOGGING")]
		public static void Configure( string filename_prefix )
		{
			m_FileNamePrefix = filename_prefix;

			if ( Frame == null )
			{
				Frame = new COutboundLogFrame();
			}

			Frame.Add_Request( new CConfigureLoggingRequest( filename_prefix, m_LogDirectory ) );

			Cleanup_Old_Logs();
		}

		[Conditional("ENABLE_LOGGING")]
		public static void Flush_Frame()
		{
			if ( Frame != null && !Frame.Empty )
			{
				CLogFrameManager.Instance.PublicInterface.Send( Frame );
				Frame = null;
			}
		}

		public static string Get_Crash_Filename()
		{
			// using a GUID to come up with what is essentially a random number is kinda dumb I know
			return m_FileNamePrefix + "_Crash_" + Math.Abs( m_CrashGUID.GetHashCode() ).ToString() + ".txt";
		}

		public static string Get_Crash_Directory()
		{
			return m_CrashDirectory;
		}

		public static void Log_Fatal_Exception( Exception e )
		{
			lock( m_LockObject )
			{
				m_Exceptions.Add( new CThreadedException( e, Thread.CurrentThread.ManagedThreadId ) );			
			}
		}

		public static bool Had_Fatal_Exception()
		{
			lock( m_LockObject )
			{
				return m_Exceptions.Count > 0;
			}
		}

		public static string ThreadID_To_Possible_Thread_Name( int thread_id )
		{
			return thread_id.ToString();
		}

		public static void Write_Crash_File()
		{
			// Doesn't need to be thread-safe because all but the main thread are gone by the time this is reached
			string file_path_name = Get_Crash_Directory() + @"/" + Get_Crash_Filename();

			if ( !Directory.Exists( Get_Crash_Directory() ) ) 
			{
				// Try to create the directory.
				Directory.CreateDirectory( Get_Crash_Directory() );
			}

			using ( FileStream fs = new FileStream( file_path_name, FileMode.Create, FileAccess.Write, FileShare.Read ) )
			{
				using ( TextWriter tw = new StreamWriter( fs ) )
				{
					foreach ( var threaded_exception in m_Exceptions )
					{
						tw.WriteLine( String.Format( "** Fatal Exception in thread {0}: ", ThreadID_To_Possible_Thread_Name( threaded_exception.ThreadID ) ) );
						tw.WriteLine( threaded_exception.CrashException.ToString() );
						tw.WriteLine( "Stack Trace:" );
						tw.WriteLine( threaded_exception.CrashException.StackTrace.ToString() ); 
						tw.WriteLine();
					}

					// append all process log files to crash file here
					string current_path = Directory.GetCurrentDirectory();
					Directory.SetCurrentDirectory( current_path + @"\\" + m_LogDirectory );

					string[] file_names  = Directory.GetFiles( @"./" );
					string process_suffix = String.Format( "{0}.log", Process.GetCurrentProcess().Id.ToString() );
					foreach( string file_name in file_names )
					{
						if ( file_name.EndsWith( process_suffix ) && file_name.StartsWith( @"./" + m_FileNamePrefix ) )
						{
							bool success = false;

							for ( uint i = 0; i < MAX_APPEND_TRIES; i++ )
							{
								try
								{
									using ( FileStream log_file = new FileStream( file_name, FileMode.Open, FileAccess.Read ) )
									{
										using ( TextReader tr = new StreamReader( log_file ) )
										{
											tw.WriteLine( "***********************************************************************" );
											tw.WriteLine();
											tw.WriteLine( "LogFile: " + file_name.Substring( 2 ) );
											tw.WriteLine();

											string log_line = null;
											for ( log_line = tr.ReadLine(); log_line != null; log_line = tr.ReadLine() )
											{
												tw.WriteLine( log_line );
											}

											tw.WriteLine();
											success = true;
										}
									}
								}
								catch ( Exception )
								{
									tw.WriteLine( String.Format( "@@ Failed to read log file due to file lock #{0} @@", i + 1 ) );
									Thread.Sleep( 500 );
								}

								if ( success )
								{
									break;
								}
							}
						}
					}

					Directory.SetCurrentDirectory( current_path );

					tw.Flush();
					tw.Close();
				}
			}			
		}

		public static void Set_Log_Level( ELogLevel log_level )
		{
			Interlocked.Exchange( ref m_LogLevel, (long) log_level );
		}

		public static ELogLevel Get_Log_Level()
		{
			return (ELogLevel) Interlocked.Read( ref m_LogLevel );
		}

		// Private interface
		[Conditional("ENABLE_LOGGING")]
		private static void Cleanup_Old_Logs()
		{
			using ( Mutex file_global_lock = new Mutex( false, m_FileNamePrefix + "LogCleanup" ) )
			{
				string current_path = Directory.GetCurrentDirectory();
				try
				{
					Directory.SetCurrentDirectory( current_path + @"\\" + m_LogDirectory );

					string[] file_names  = Directory.GetFiles( @"./" );
					foreach( string file_name in file_names )
					{
						if ( file_name.EndsWith( ".log" ) && file_name.StartsWith( @"./" + m_FileNamePrefix ) )
						{
							try
							{
								File.Delete( file_name );
							}
							catch	// locked/read-only files are currently in use; just ignore the exception since we only want to clean up old files
							{
							}
						}
					}
				}
				catch ( DirectoryNotFoundException )		// this is ok; it just means it's the first time one of the services has been run
				{
				}
				finally
				{
					Directory.SetCurrentDirectory( current_path );
				}
			}
		}

		// Slash command handlers
		[GenericHandler]
		private static void Handle_Set_Log_Level_Command( CSetLogLevelSlashCommand command )
		{
			Set_Log_Level( command.Level );
		}

		// Properties
		private static COutboundLogFrame Frame { get { return m_Frame; } set { m_Frame = value; } }

		// Fields
		[ThreadStatic]
		private static COutboundLogFrame m_Frame = null;

		private static string m_FileNamePrefix = "";
		private static object m_LockObject = new object();
		private static List< CThreadedException > m_Exceptions = new List< CThreadedException >();

#if DEBUG
		private static long m_LogLevel = (long) ELogLevel.High;
#else
		private static long m_LogLevel = (long) ELogLevel.Low;
#endif

		private static string m_LogDirectory = "Logs";
		private static string m_CrashDirectory = "Crashes";
		private static Guid m_CrashGUID = Guid.NewGuid();

		private const uint MAX_APPEND_TRIES = 20;
	}
}
