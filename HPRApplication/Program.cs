using System;

namespace RelayHolePuncher
{
	internal sealed class Program
	{
		static async Task Main(string[] args) {
			if (args is null) {
				throw new ArgumentNullException(nameof(args));
			}
			var server = new LiteNetLibService();
			server.Initialize();
			await server.StartUpdateLoop();
		}
	}
}
