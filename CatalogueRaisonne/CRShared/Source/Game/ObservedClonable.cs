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