/*

	LogRequests.cs

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

namespace CRShared
{
	public class CLogMessageRequest : CLogRequest
	{
		// Construction
		public CLogMessageRequest( ELoggingChannel channel, string text ) :
			base()
		{
			Channel = channel;
			Text = text;
			ThreadID = Thread.CurrentThread.ManagedThreadId;
			Time = DateTimeOffset.Now;
		}

		// Properties
		public ELoggingChannel Channel { get; private set; }
		public string Text { get; private set; }
		public int ThreadID { get; private set; }
		public DateTimeOffset Time { get; private set; }
	}

	public class CConfigureLoggingRequest : CLogRequest
	{
		// Construction
		public CConfigureLoggingRequest( string filename_prefix, string log_directory ) :
			base()
		{
			FilenamePrefix = filename_prefix;
			LogDirectory = log_directory;
		}

		// Properties
		public string FilenamePrefix { get; private set; }
		public string LogDirectory { get; private set; }
	}
}