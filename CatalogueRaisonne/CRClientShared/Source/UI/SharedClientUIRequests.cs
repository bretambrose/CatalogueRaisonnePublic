using System;

using CRShared;

namespace CRClientShared
{
	public class CUIInputChatRequest : CUIInputRequest
	{
		// Construction
		public CUIInputChatRequest( string input_string ) :
			base( EUIInputRequest.Chat )
		{
			Text = input_string;						
		}
		
		// Methods	
		
		// Properties
		public string Text { get; private set; }
		
		// Fields
	}
}