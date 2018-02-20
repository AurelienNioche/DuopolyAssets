using System;

namespace AssemblyCSharp
{
	public enum TLClientF {
		WaitCommand,
		TutorialDone,
		TutorialDoneWaitReply,
		GotTutorialDone,
		Init, 
		InitWaitReply,
		GotInit,
		PassiveOpponentChoice,
		PassiveOpponentChoiceWaitReply,
		GotPassiveOpponentChoice,
		PassiveConsumerChoices,
		PassiveConsumerChoicesWaitReply,
		GotPassiveConsumerChoices,
		ActiveChoiceRecording,
		ActiveChoiceRecordingWaitReply,
		GotActiveChoiceRecording,
		ActiveConsumerChoices,
		ActiveConsumerChoicesWaitReply,
		GotActiveConsumerChoices,
		SubmitTutorialProgression,
		SubmitTutorialProgressionWaitReply,
		GotSubmitTutorialProgression
	}
}

