/*

	ServerConsoleUIThread.cs

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
using System.Reflection;
using System.Threading;

using CRShared;

namespace CRServer
{
	public class CServerConsoleUIThread : CConsoleUIThread
	{
		static CServerConsoleUIThread() {}
		protected CServerConsoleUIThread() : base() {}

		// Methods
		// Public interface
		public static Thread Allocate_Thread()
		{
			return new Thread( Instance.Start );
		}

		// Protected interface and overrides
		protected override void Initialize()
		{
			base.Initialize();

			Register_Generic_Handlers();
		}

		// Private interface
		private void Register_Generic_Handlers()
		{
			CGenericHandlerManager.Instance.Find_Handlers< CUILogicNotification >( Assembly.GetAssembly( typeof( CUILogicNotification ) ) );
			CGenericHandlerManager.Instance.Find_Handlers< CUILogicNotification >( Assembly.GetExecutingAssembly() );
		}

		// Properties
		public static CServerConsoleUIThread Instance { get { return m_Instance; } }

		// Fields
		private static CServerConsoleUIThread m_Instance = new CServerConsoleUIThread();
	}
}

