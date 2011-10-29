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