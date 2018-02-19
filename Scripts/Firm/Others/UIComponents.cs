using System;

namespace AssemblyCSharp
{

	public class Turn {

		public static string player = "TurnPlayer";
		public static string opponent = "TurnOpponent";
		public static string consumers1 = "TurnConsumers1";
		public static string consumers2 = "TurnConsumers2";
		public static string none = "none";

		public static string[] GetArray() {
			string[] array = {
				player, 
				opponent,
				consumers1,
				consumers2
			};
			return array;
		}
	}

	public class ButtonNames {

		public static string left = "ButtonLeft";
		public static string right = "ButtonRight";
		public static string plus = "ButtonPlus";
		public static string minus = "ButtonMinus";
		public static string yes ="ButtonYes";
		public static string cashRegister = "ButtonCashRegister";
		public static string menu = "ButtonMenu";
		public static string position = "PositionButton";
	}

	public class Bool {
		public static string visible = "Visible";
		public static string glow = "Glow";
	}

	public class Trigger {

		public static string staticState = "Static";
		public static string appear = "Appear";
		public static string disappear = "Disappear";
		public static string addScore = "AddScore";
		public static string floatScore = "FloatScore";
		public static string floatConsumerAndCurrentScore = "FloatConsumerAndCurrentScore";
		public static string glowCurrentScore = "GlowCurrentScore";
	}
}

