using System;

namespace AssemblyCSharp
{
	public enum TLClientF {
		WaitCommand,
		Init, 
		InitWaitReply,
		GotInit,
		PassiveOpponentChoice,
		PassiveOpponentChoiceWaitReply,
		GotPassiveOpponentChoice,
		ActiveChoiceRecording,
		ActiveChoiceRecordingWaitReply,
		GotActiveChoiceRecording,
		SubmitTutorialProgression,
		SubmitTutorialProgressionWaitReply,
		GotSubmitTutorialProgression
	}
}

