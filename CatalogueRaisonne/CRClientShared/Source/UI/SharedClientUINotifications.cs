using System;

using CRShared;

namespace CRClientShared
{
	public enum EUIScreenState
	{
		Startup,
		Main_Menu,
		Chat_Idle,
		Lobby,
		Match_Idle
	}

	public class CUIScreenStateNotification : CUILogicNotification
	{
		public CUIScreenStateNotification( EUIScreenState screen_state )
		{
			ScreenState = screen_state;
		}

		public EUIScreenState ScreenState { get; private set; }
	}
}