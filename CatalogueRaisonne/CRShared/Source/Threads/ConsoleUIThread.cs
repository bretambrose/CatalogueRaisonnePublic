using System;
using System.Collections.Generic;

namespace CRShared
{
	public class CConsoleUIThread : CUIThreadBase
	{
		// Construction
		protected CConsoleUIThread() : 
			base() 
		{
			m_BaseInstance = this;
		}

		// Methods
		// Protected interface and overrides
		protected override void Service( long current_time )
		{
			base.Service( current_time );

			Read_Input();

			m_CompleteInputLines.Apply( input_line => Process_Console_Input( input_line ) );			
			m_CompleteInputLines.Clear();
		}

		protected virtual bool Process_Console_Input( string input_string )
		{
			if ( input_string.Length == 0 )
			{
				return false;
			}
			
			if ( input_string[ 0 ] == '/' )
			{
				CUIInputSlashCommandRequest request = new CUIInputSlashCommandRequest( input_string );
				Add_Input_Request( request );
				return true;
			}

			return false;
		}

		// Private interface
		private void Read_Input()
		{
			while ( Console.KeyAvailable )
			{
				ConsoleKeyInfo key_info = Console.ReadKey();
				if ( key_info.Key == ConsoleKey.Enter )
				{
					m_CompleteInputLines.Add( m_CurrentInputLine );
					Console.WriteLine( m_CurrentInputLine );
					m_CurrentInputLine = "";
				}
				else if ( key_info.Key == ConsoleKey.Backspace )
				{
					if ( m_CurrentInputLine.Length > 0 )
					{
						m_CurrentInputLine = m_CurrentInputLine.Substring( 0, m_CurrentInputLine.Length - 1 );
					}
				}
				else
				{
					m_CurrentInputLine += key_info.KeyChar;
				}
			}
		}

		// UI Notice Handlers
		[GenericHandler]
		private void Handle_Text_Output_Notification( CUITextOutputNotification notice )
		{
			CLog.Log( ELoggingChannel.UI, ELogLevel.Low, String.Format( "Console output: {0}", notice.Text ) );

			Console.WriteLine( notice.Text );
		}

		// Properties
		public static CConsoleUIThread BaseInstance { get { return m_BaseInstance; } }

		// Fields
		private List< string > m_CompleteInputLines = new List< string >();
		private string m_CurrentInputLine = "";

		private static CConsoleUIThread m_BaseInstance = null;

	}
}
