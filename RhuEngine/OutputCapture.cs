using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using RhuEngine.Linker;

namespace RhuEngine
{
	public class OutputCapture : TextWriter, IDisposable
	{
		public string LogsPath = null;

		private StreamWriter _writer = null;

		public string InGameConsole = null;

		private TextWriter _stdOutWriter;


		public int currentLine = 0;

		public override Encoding Encoding => Encoding.ASCII;

		public event Action TextEdied;

		public int AmountOfNewLines;

		public void RemoveNewLines(int amount) {
			if(amount < 0) {
				return;
			}
			var lastIndex = 0;
			for (var i = 0; i < amount; i++) {
				lastIndex = InGameConsole.IndexOf('\n', lastIndex) + 1;
				if(lastIndex == -1) {
					InGameConsole = null;
					AmountOfNewLines = 0;
					break;
				}
			}
			InGameConsole = InGameConsole.Substring(lastIndex);
			AmountOfNewLines -= amount;
		}

		public void WriteText(string data) {
			var amountOfnewLines = data.Count((car) => car == '\n');
			AmountOfNewLines += amountOfnewLines;
			var consoleColor = RhuConsole.ForegroundColor;
			if(consoleColor == ConsoleColor.Gray) {
				consoleColor = ConsoleColor.White;
			}
			InGameConsole += $"<color{consoleColor}>{data.Replace("info", "<colorBlue>info</color>").Replace("error", "<colorRed>error</color>").Replace("diagnostic", "<colorMidnightBlue>diagnostic</color>").Replace("warn", "<coloryellow>warn</color>")}</clearstyle>";
			RemoveNewLines(AmountOfNewLines - 35);
			_writer?.Write(data);
			TextEdied?.Invoke();
		}

		public void Start() {
			RLog.Subscribe(LogCall);
			_stdOutWriter = Console.Out;
			Console.SetOut(this);
			Directory.CreateDirectory(LogsPath);
			_writer = new StreamWriter(LogsPath + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".txt") {
				AutoFlush = true
			};
		}

		public void LogCall(LogLevel level, string text) {
			WriteText(text);
		}

		public new void Dispose() 
		{
			RLog.Unsubscribe(LogCall);
			_writer.Close();
			_writer.Dispose();
			_writer = null;
			base.Dispose();
		}

		override public void Write(string output) {
			// Capture the output and also send it to StdOut
			WriteText(output);
			_stdOutWriter.Write(output);
		}

		override public void WriteLine(string output) {
			WriteText(output+"\n");
			_stdOutWriter.WriteLine(output);
		}
	}
}
