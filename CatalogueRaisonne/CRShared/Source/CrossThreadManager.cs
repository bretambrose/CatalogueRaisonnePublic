/*

	CrossThreadManager.cs

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
	public interface ICrossThreadDataQueues< T1, T2 >
	{
		void Send( T1 item );
		void Receive( ICollection< T2 > destination ); 
	}
	
	public class CCrossThreadManagerClass< T1, T2 >
	{
		// Embedded types
		private class CQueuedDataManager<TWrite, TRead> : ICrossThreadDataQueues< TWrite, TRead >
		{
			public CQueuedDataManager( CLockedQueue< TWrite > write_queue, CLockedQueue< TRead > read_queue )
			{
				m_ReadQueue = read_queue;
				m_WriteQueue = write_queue;
			}

			public void Send( TWrite item ) { m_WriteQueue.Add( item ); }
			public void Receive( ICollection< TRead > destination ) { m_ReadQueue.Take_All( destination ); }
			
			private CLockedQueue< TRead > m_ReadQueue;
			private CLockedQueue< TWrite > m_WriteQueue;
		}
		
		// Construction
		public CCrossThreadManagerClass() {}		
		
		// Methods
			
		// Properties
		protected ICrossThreadDataQueues< T1, T2 > Create_InterfaceA() { return new CQueuedDataManager< T1, T2 >( m_DataA, m_DataB ); }
		protected ICrossThreadDataQueues< T2, T1 > Create_InterfaceB() { return new CQueuedDataManager< T2, T1 >( m_DataB, m_DataA ); } 
		
		// Fields		
		private CLockedQueue< T1 > m_DataA = new CLockedQueue< T1 >();
		private CLockedQueue< T2 > m_DataB = new CLockedQueue< T2 >();
	}
}