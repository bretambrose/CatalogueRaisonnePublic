using System;

using CRShared;

namespace CRShared
{
	public class CApplicationException : Exception
	{
		// Construction
		public CApplicationException( string error_message )
		{
			ErrorMessage = error_message;
		}

		public CApplicationException()
		{
			ErrorMessage = "";
		}
		
		// Methods
		public override string ToString() { return string.Format( "Application Exception - Message({0})", ErrorMessage ); }
		
		// Properties
		public string ErrorMessage { get; private set; }
	}

	public class CQuitException : Exception
	{
		public CQuitException() {}
	}
}