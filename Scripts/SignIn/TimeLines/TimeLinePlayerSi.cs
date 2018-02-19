using System;

namespace AssemblyCSharp
{
	public enum TimeLinePlayerSi
	{
		WaitingUserFillIn,
		SendForm,
		SendFormWaitReply,
		FormSent,
		SendingAborted,
		SendAgain,
		SendAgainWaitReply,
		SendAgainGotReply,
		AlreadyExists
	}
}

