using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RhuEngine
{
	public class CommandManager
	{
		internal readonly Type[] _commands;

		public CommandManager() {
			_commands = (from a in AppDomain.CurrentDomain.GetAssemblies()
						 from t in a.GetTypes()
						 where typeof(Command).IsAssignableFrom(t)
						 where t.IsClass && !t.IsAbstract
						 select t).ToArray();

		}

		public Engine Engine;

		public void Init(Engine engine) {
			Engine = engine;
		}

		public Semaphore semaphore = new Semaphore(1, 1);

		public bool WaitingForComand = false;

		public string ReadNextLine() {
			WaitingForComand = true;
			semaphore.WaitOne();
			return LastLine;
		}

		public string LastLine = string.Empty;

		public void RunComand(string line) {
			if (string.IsNullOrEmpty(line)) {
				return;
			}
			LastLine = line;
			if (WaitingForComand) {
				semaphore.Release();
			}
			var foundcomand = false;
			foreach (var item in _commands) {
				if (line.ToLower().StartsWith(item.Name.ToLower()+" ") || (line.ToLower() == item.Name.ToLower())) {
					foundcomand = true;
					var comand = (Command)Activator.CreateInstance(item);
					comand.args = line.Split(' ');
					comand.Manager = this;
					Task.Run(() => comand.RunCommand());
				}
			}
			if (!foundcomand) {
				Console.WriteLine($"{line} Is not a valid command run Help for available commands");
			}
		}
	}
}
