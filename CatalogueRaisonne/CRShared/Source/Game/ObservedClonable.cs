/*

	ObservedClonable.cs

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
	public enum EGameStateClonePermission
	{
		Full,
		Hidden
	}

	public interface ISimpleClonable< T >
	{
		T Clone();
	}

	public interface IObservedClonable
	{
		IObservedClonable Clone( EGameStateClonePermission permission );
	}

	public enum EGameStateDelta
	{
		CardCollection,
		Deck,
		DiscardPile,
		GameState,
		PlayerHand
	}

	public interface IObservedClonableDelta
	{
		IObservedClonableDelta Clone( EPersistenceID player_id, bool admin );

		void Apply( CGameState game_state );

		EGameStateDelta ObservableDeltaType { get; }
	}
}