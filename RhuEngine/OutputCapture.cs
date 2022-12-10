using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

using RhuEngine.Linker;

namespace RhuEngine
{
	public sealed class OutputCapture : TextWriter, IDisposable
	{
		public string LogsPath = null;

		private StreamWriter _writer = null;

		public string InGameConsole = null;

		private TextWriter _stdOutWriter;

		public override Encoding Encoding => Encoding.ASCII;

		public event Action TextEdied;

		public void WriteText(string data) {
			var consoleColor = RhuConsole.ForegroundColor;
			if (consoleColor == ConsoleColor.Gray) {
				consoleColor = ConsoleColor.White;
			}
			InGameConsole += $"[color={consoleColor}]{data.Replace("info", "[color=Blue]info[/color]").Replace("error", "[color=Red]error[/color]").Replace("diagnostic", "[color=MidnightBlue]diagnostic[/color]").Replace("warn", "[color=yellow]warn[/color]")}";
			var splitLines = InGameConsole.Split('\n');
			InGameConsole = string.Join("\n", splitLines.Skip(Math.Max(0, splitLines.Length - 30)));
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

		public new void Dispose() {
			RLog.Unsubscribe(LogCall);
			_writer.Close();
			_writer.Dispose();
			_writer = null;
			base.Dispose();
		}

		override public void Write(string output) {
			if (string.IsNullOrEmpty(LogsPath)) {
				output = " ";
			}
			// Capture the output and also send it to StdOut
			WriteText(output);
			_stdOutWriter.Write(output);
		}

		override public void WriteLine(string output) {
			if(string.IsNullOrEmpty(LogsPath)) {
				output = " ";
			}
			WriteText(output + "\n");
			_stdOutWriter.WriteLine(output);
		}
	}
}
