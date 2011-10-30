/*

	LoggingThread.cs

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
using System.Reflection;
using System.Threading;
using System.Diagnostics;

namespace CRShared
{
	public class CLoggingThread : CTimeKeeperThread
	{
		// Embedded classes
		private class CLogChannel
		{
			// Construction
			public CLogChannel( ELoggingChannel channel )
			{
				Channel = channel;
				m_FileName = null;
			}

			// Methods
			// Public interface
			public void Set_Filename( string filename )
			{
				if ( m_TextWriter != null )
				{
					throw new CApplicationException( "Internal logging error: channel file initialized multiple times" );
				}

				m_FileName = filename;

				foreach ( var message in m_QueuedMessages )
				{
					Log_Message( message );
				}
			}

			public void Flush()
			{
				if ( m_TextWriter != null )
				{
					m_TextWriter.Flush();
				}
			}

			public void Stop_Logging()
			{
				if ( m_TextWriter != null )
				{
					m_TextWriter.Flush();
					m_TextWriter.Close();
					m_TextWriter = null;
				}

				if ( m_FileStream != null )
				{
					m_FileStream.Close();
					m_FileStream = null;
				}
			}

			public void Log_Message( string text, int thread_id, DateTimeOffset time )
			{
				string final_log_string = Build_Final_Log_String( text, thread_id, time );

				if ( m_FileName == null )
				{
					m_QueuedMessages.Add( final_log_string );
					return;
				}

				Log_Message( final_log_string );
			}

			// Private interface
			private void Start_Logging()
			{
				string file_path_name = m_FileName + Process.GetCurrentProcess().Id.ToString() + ".log";

				m_FileStream = new FileStream( file_path_name, m_ShouldResetFile ? FileMode.Create : FileMode.Append, FileAccess.Write, FileShare.Read );
				m_ShouldResetFile = false;

				m_TextWriter = new StreamWriter( m_FileStream );
			}

			private void Log_Message( string text )
			{
				if ( m_TextWriter == null )
				{
					Start_Logging();
				}

				m_TextWriter.WriteLine( text );
			}

			private string Build_Final_Log_String( string text, int thread_id, DateTimeOffset time )
			{
				return String.Format( "[{0} ({1})]: {2}", time.ToUniversalTime().ToString( "yyyy-MM-dd HH:mm:ss.fff" ), thread_id, text );
			}

			// Properties
			public ELoggingChannel Channel { get; private set; }

			// Fields
			private List< string > m_QueuedMessages = new List< string >();
			private bool m_ShouldResetFile = true;
			private string m_FileName = null;

			private FileStream m_FileStream = null;
			private TextWriter m_TextWriter = null;
		}

		// Construction
		private CLoggingThread() :
			base()
		{
		}
		
		static CLoggingThread() {}
		
		// Methods
		// Public interface
		public static Thread Allocate_Thread()
		{
			return new Thread( Instance.Start );
		}
		
		// Protected interface and overrides
		protected override void Initialize()
		{
			base.Initialize();

			base.Initialize_Time_Keeper( TimeKeeperExecutionMode.Internal, TimeKeeperTimeUnit.RealTime, TimeKeeperAuthority.Master, LOGGING_SERVICE_INTERVAL, true, false );
			m_LoggingInterface = CLogFrameManager.Instance.LoggingInterface;	

			CGenericHandlerManager.Instance.Find_Handlers< CLogRequest >( Assembly.GetExecutingAssembly() );
		}

		protected override void Shutdown()
		{
			base.Shutdown();

			m_Channels.Apply( n => n.Value.Stop_Logging() );
			m_Channels.Clear();
		}
						
		protected override void Service( long current_time )
		{	
			base.Service( current_time );
					
			var incoming_frames = new List< COutboundLogFrame >();
			m_LoggingInterface.Receive( incoming_frames );
			incoming_frames.Apply( frame => Process_Incoming_Frame( frame ) );

			Flush_Streams();
		}

		// Private interface
		private void Process_Incoming_Frame( COutboundLogFrame frame )
		{
			foreach ( var request in frame.Requests )
			{
				if ( !CGenericHandlerManager.Instance.Try_Handle( request ) )
				{
					throw new CApplicationException( String.Format( "Logging request of type {0} does not have a handler", request.GetType().Name ) );
				}
			}
		}

		private void Flush_Streams()
		{
			m_Channels.Apply( channel_pair => channel_pair.Value.Flush() );
		}

		private CLogChannel Get_Or_Create_Log_Channel( ELoggingChannel channel )
		{
			CLogChannel log_channel = null;
			m_Channels.TryGetValue( channel, out log_channel );

			if ( log_channel == null )
			{
				log_channel = new CLogChannel( channel );
				m_Channels.Add( channel, log_channel );
			}

			return log_channel;
		}

		// Log request handlers
		[GenericHandler]
		private void Handle_Configure_Logging_Request( CConfigureLoggingRequest request )
		{
			if ( m_Initialized )
			{
				throw new CApplicationException( "Multiple logging configuration requests received" );
			}

			m_Initialized = true;
			m_LogFileDirectoryName = request.LogDirectory;

         // Determine whether the directory exists.
			if ( !Directory.Exists( m_LogFileDirectoryName ) ) 
			{
				// Try to create the directory.
				Directory.CreateDirectory( m_LogFileDirectoryName );
			}

			foreach ( ELoggingChannel channel_type in Enum.GetValues( typeof( ELoggingChannel ) ) )
			{
				if ( channel_type != ELoggingChannel.Invalid )
				{
					CLogChannel channel = Get_Or_Create_Log_Channel( channel_type );

					string filename = String.Format( @"{0}/{1}_{2}", m_LogFileDirectoryName, request.FilenamePrefix, channel_type.ToString() ); 
					channel.Set_Filename( filename );
				}
			}		
		}

		[GenericHandler]
		private void Handle_Log_Message_Request( CLogMessageRequest request )
		{
			CLogChannel channel = Get_Or_Create_Log_Channel( request.Channel );
			channel.Log_Message( request.Text, request.ThreadID, request.Time );
		}

		// Properties
		public static CLoggingThread Instance { get { return m_Instance; } }
		
		// Fields
		private static CLoggingThread m_Instance = new CLoggingThread();
		
		private const int LOGGING_SERVICE_INTERVAL = 1000;
		private ICrossThreadDataQueues< CInboundLogFrame, COutboundLogFrame > m_LoggingInterface = null;

		private Dictionary< ELoggingChannel, CLogChannel > m_Channels = new Dictionary< ELoggingChannel, CLogChannel >();
		private bool m_Initialized = false;
		private string m_LogFileDirectoryName = "";
	}
}
