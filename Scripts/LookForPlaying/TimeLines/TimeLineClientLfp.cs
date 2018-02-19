
using System;

namespace AssemblyCSharp
{
	public enum TimeLineClientLfp : int {

		RegisteredAsPlayerAsk,
		RegisteredAsPlayerWaitReply,
		RegisteredAsPlayerGotAnswer,
		RoomAvailableAsk,
		RoomAvailableWaitReply,
		RoomAvailableGotAnswer,
		MissingPlayersAsk,
		MissingPlayersWaitReply,
		MissingPlayersGotAnswer,
		ProceedToRegistrationAsPlayerAsk,
		ProceedToRegistrationAsPlayerWaitReply,
		ProceedToRegistrationAsPlayerGotAnswer,
		WaitingCommand
	}
}

