using System;

namespace AssemblyCSharp
{
	public enum TLRoundF : int
	{
		Init,
		WaitingInfo,
		GotInfo,
		Preparation,
		PassiveBeginningOfTurn,
		PassiveWaitingOpponent,
		PassiveMovingOpponent,
		PassiveOpponentHasMoved,
		PassiveWaitingConsumers,
		PassiveMovingConsumers,
		PassiveConsumersHaveMoved,
		PassiveNextTurnWaitingUser,
		PassiveNextTurn,
		PassiveMovingBackConsumers,
		PassiveEndOfTurn,
		PassiveUpdatingScore,
		ActiveBeginningOfTurn,
		ActiveChoiceWaitingUser,
		ActiveChoiceMade,
		ActiveChoiceSubmitting,
		ActiveChoiceSubmitted,
		ActiveWaitingConsumers,
		ActiveMovingConsumers,
		ActiveConsumersHaveMoved,
		ActiveNextTurnWaitingUser,
		ActiveNextTurn,
		ActiveMovingBackConsumers,
		ActiveEndOfTurn,
		ActiveUpdatingScore,
		EndingGame,
		EndOfGame,
		WaitingGoCommand
	}
}

