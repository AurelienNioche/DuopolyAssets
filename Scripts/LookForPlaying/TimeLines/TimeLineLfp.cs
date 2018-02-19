using System;

namespace AssemblyCSharp
{
	public enum TimeLineLfp : int {
		RegisteredAsPlayerAsk,
		RegisteredAsPlayerWaitReply,
		RegisteredAsPlayerGotAnswer,
		RoomAvailableAsk,
		RoomAvailableWaitReply,
		WaitingForUserToParticipate,
		GotUserParticipation,
		ProceedToRegistrationAsPlayerAsk,
		ProceedToRegistrationAsPlayerWaitReply,
		GotRegistrationAsPlayer,
		MissingPlayersAsk,
		MissingPlayersWaitReply,
		MissingPlayersGotAnswer,
		PrepareNewScene,
		ComeBackHome,
		WaitEndAnimationQuitScene,
		EndAnimationQuitScene,
		Dead
	}
}

