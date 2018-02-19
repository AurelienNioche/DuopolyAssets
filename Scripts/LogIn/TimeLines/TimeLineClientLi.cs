
using System;

namespace AssemblyCSharp
{
	public enum TimeLineClientLi : int {

		WaitingUserIdentification,
		AskConnection,
		AskConnectionWaitReply,
		GotConnection,
		ErrorIdentification,
		WaitingCommand
	}
}

