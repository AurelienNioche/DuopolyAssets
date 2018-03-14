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

