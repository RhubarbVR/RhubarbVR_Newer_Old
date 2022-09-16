using System;

namespace RhuEngine
{
	public static class RhuConsole
	{
		private static ConsoleColor _foregroundColor;
		public static ConsoleColor ForegroundColor
		{
			get => _foregroundColor;
			set {
				_foregroundColor = value;
				Console.ForegroundColor = value;
			}
		}
	}
}