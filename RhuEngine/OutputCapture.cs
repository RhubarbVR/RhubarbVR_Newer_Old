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

		private readonly TextWriter _stdOutWriter;

		public string[] consoleLines = new string[20];

		public int ConsoleLangth
		{
			get => consoleLines.Length;
			set => Array.Resize(ref consoleLines,value);
		}

		public string singleString = "null";

		private readonly object _lineLock = new();

		public int currentLine = 0;

		public override Encoding Encoding => Encoding.ASCII;

		// would crash when closing like this https://media.discordapp.net/attachments/805160377130156124/930605958344867890/unknown.png
		public bool DisableSingleString { get; set; }

		public void WriteText(string data) {
			lock (_lineLock) {
				_writer?.Write(data);
				foreach (var item in data.Split('\n')) {
					if (!string.IsNullOrWhiteSpace(item)) {
						for (var i = 0; i < consoleLines.Length - 1; i++) {
							consoleLines[i] = consoleLines[i + 1];
						}
						consoleLines[consoleLines.Length - 1] = item;
						currentLine++;
					}
					if (!DisableSingleString) {
						singleString = string.Join("\n", consoleLines);
					}
				}
			}
		}

		public OutputCapture() {
			RLog.Subscribe(LogCall);
			_stdOutWriter = Console.Out;
			Console.SetOut(this);
		}

		public void Start() {
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
			base.Dispose();
			_writer.Dispose();
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
