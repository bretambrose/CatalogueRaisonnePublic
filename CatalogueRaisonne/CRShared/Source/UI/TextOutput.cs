/*

	TextOutput.cs

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

namespace CRShared
{
	public enum ETextOutputCategory
	{
		Default,

		Error,
		Chat,
		Request_Result,
		Game
	}

	public class CTextOutputConfig
	{
		public CTextOutputConfig( ETextOutputCategory category )
		{
			Category = category;
		}

		public ETextOutputCategory Category { get; private set; }
	}

	public class CUITextOutputNotification : CUILogicNotification
	{
		public CUITextOutputNotification( CTextOutputConfig config, string text )
		{
			Config = config;
			Text = text;
		}

		public CUITextOutputNotification( ETextOutputCategory category, string text )
		{
			Config = new CTextOutputConfig( category );
			Text = text;
		}

		public CUITextOutputNotification( string text )
		{
			Config = new CTextOutputConfig( ETextOutputCategory.Default );
			Text = text;
		}

		public CTextOutputConfig Config { get; private set; }
		public string Text { get; private set; }
	}
}
