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
