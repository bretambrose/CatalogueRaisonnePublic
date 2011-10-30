/*

	ConsoleUIThread.cs

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
