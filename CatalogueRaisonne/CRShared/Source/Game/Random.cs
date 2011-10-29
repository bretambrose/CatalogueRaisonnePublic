using System;

namespace CRShared
{
	public static class CRandom
	{
		// Construction
		static CRandom() {}

		// Properties
		public static Random RNG { get { return m_RNG; } }

		// Fields
		private static Random m_RNG = new Random();
	}
}