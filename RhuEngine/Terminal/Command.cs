﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhuEngine
{
	public abstract class Command
	{
		public CommandManager Manager { get; set; }

		public string ReadNextLine() {
			return Manager.ReadNextLine();
		}

		public string PasswordInput() {
			return Manager.PasswordInput();
		}

		public abstract Task RunCommand();

		public string[] args;

		public string FullCommand;

		public abstract string HelpMsg { get; }
	}
}
