using System;
using System.Collections.Generic;

namespace CRShared
{
	public sealed class EmptyHeapException : Exception
	{
		public EmptyHeapException() {}
		
		public override string ToString()
		{
			return "Heap is empty.  Cannot query elements in it.";
		}
	}
	
	public sealed class THeap< T > where T : IComparable< T >, new()
	{
		// Construction
		public THeap()
		{
			m_HeapVector = new List< T >();
			m_HeapVector.Add( new T() );	// dummy element in the 0th index
		}
		
		// Methods
		// Public interface		
		public T Peek_Top()
		{
			if ( Empty )
			{
				throw new EmptyHeapException();
			}
			
			return m_HeapVector[ 1 ];
		}

		public T Pop_Top()
		{
			if ( Empty )
			{
				throw new EmptyHeapException();
			}

			T top_spot = m_HeapVector[ 1 ];
			m_HeapVector[ 1 ] = m_HeapVector[ Size ];
			m_HeapVector.RemoveAt( Size );
			
			if ( Size > 1 )
			{
				Heapify( 1 );
			}
			
			return top_spot;
		}
		
		public void Add( T element )
		{
			m_HeapVector.Add( element );
	
			int index = Size;
			while ( true )
			{
				int parent = ParentIndex( index );
				if ( parent <= 0 || m_HeapVector[ parent ].CompareTo( m_HeapVector[ index ] ) < 0 )
				{
					break;
				}
				
				T temp = m_HeapVector[ parent ];
				m_HeapVector[ parent ] = m_HeapVector[ index ];
				m_HeapVector[ index ] = temp;
				
				index = parent;				
			}
		}
		
		// Private interface
		private void Heapify( int index )
		{
			int size = Size;
			while ( true )
			{
				int left = LeftIndex( index );
				int right = RightIndex( index );
				int smallest = index;
				
				if ( left <= size && m_HeapVector[ left ].CompareTo( m_HeapVector[ index ] ) < 0 )
				{
					smallest = left;
				}

				if ( right <= size && m_HeapVector[ right ].CompareTo( m_HeapVector[ smallest ] ) < 0 )
				{
					smallest = right;
				}
				
				if ( smallest == index )
				{
					break;
				}
				
				T temp = m_HeapVector[ smallest ];
				m_HeapVector[ smallest ] = m_HeapVector[ index ];
				m_HeapVector[ index ] = temp;
				
				index = smallest;
			}
		}
		
		private int ParentIndex( int index )
		{
			return index / 2;
		}
		
		private int LeftIndex( int index )
		{
			return index * 2;
		}
		
		private int RightIndex( int index )
		{
			return index * 2 + 1;
		}
		
		// Properties
		public bool Empty { get { return m_HeapVector.Count <= 1; } }
		public int Size { get { return m_HeapVector.Count - 1; } }
		
		// Fields
		private List< T > m_HeapVector = null;
	}
}
