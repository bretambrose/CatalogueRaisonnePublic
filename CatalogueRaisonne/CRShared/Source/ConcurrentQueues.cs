using System;
using System.Collections.Generic;

namespace CRShared
{
	public class CLockedQueue< T >
	{
		// Construction
		public CLockedQueue()
		{
		}
		
		// Methods
		// Public interface
		public void Add( T item )
		{
			lock( m_Lock )
			{
				m_Queue.Enqueue( item );
			}
		}
		
		public void Take_All( ICollection< T > dest_collection )
		{
			lock( m_Lock )
			{
				m_Queue.ShallowCopy( dest_collection );
				m_Queue.Clear();
			}
		}
		
		// Fields
		private Queue< T > m_Queue = new Queue< T >();
		private object m_Lock = new object();
		
	}
}

