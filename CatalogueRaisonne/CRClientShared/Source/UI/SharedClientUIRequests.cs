/*

	SharedClientUIRequests.cs

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