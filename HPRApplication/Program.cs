using System;

namespace RelayHolePuncher
{
	internal class Program
	{
		static void Main(string[] args) {
			var server = new LiteNetLibService();
			server.Initialize();
			server.StartUpdateLoop();
		}
	}
}
