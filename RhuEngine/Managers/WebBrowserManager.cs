using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using SharedModels;

using StereoKit;

using Xilium.CefGlue;
using Xilium.CefGlue.Platform.Windows;
using Xilium.CefGlue.Platform;
using RhuEngine.WebBrowser;

namespace RhuEngine.WebBrowser
{
	class RenderProcessHandler : CefRenderProcessHandler
	{
		internal static bool DumpProcessMessages { get; private set; } = true;

		public RenderProcessHandler() {
			//MessageRouter = new CefMessageRouterRendererSide(new CefMessageRouterConfig());
		}

		//internal CefMessageRouterRendererSide MessageRouter { get; private set; }

		protected override void OnContextCreated(CefBrowser browser, CefFrame frame, CefV8Context context) {
			//MessageRouter.OnContextCreated(browser, frame, context);

			// MessageRouter.OnContextCreated doesn't capture CefV8Context immediately,
			// so we able to release it immediately in this call.
			context.Dispose();
		}

		protected override void OnContextReleased(CefBrowser browser, CefFrame frame, CefV8Context context) {
			// MessageRouter.OnContextReleased releases captured CefV8Context (if have).
			//MessageRouter.OnContextReleased(browser, frame, context);

			// Release CefV8Context.
			context.Dispose();
		}

		protected override bool OnProcessMessageReceived(CefBrowser browser, CefFrame frame, CefProcessId sourceProcess, CefProcessMessage message) {
			if (DumpProcessMessages) {
				Log.Info("Render::OnProcessMessageReceived: SourceProcess={0}", sourceProcess);
				Log.Info("Message Name={0} IsValid={1} IsReadOnly={2}", message.Name, message.IsValid, message.IsReadOnly);
				var arguments = message.Arguments;
				for (var i = 0; i < arguments.Count; i++) {
					var type = arguments.GetValueType(i);
					object value;
					switch (type) {
						case CefValueType.Null:
							value = null;
							break;
						case CefValueType.String:
							value = arguments.GetString(i);
							break;
						case CefValueType.Int:
							value = arguments.GetInt(i);
							break;
						case CefValueType.Double:
							value = arguments.GetDouble(i);
							break;
						case CefValueType.Bool:
							value = arguments.GetBool(i);
							break;
						default:
							value = null;
							break;
					}

					Log.Info("  [{0}] ({1}) = {2}", i, type, value);
				}
			}

			//var handled = MessageRouter.OnProcessMessageReceived(browser, sourceProcess, message);
			//if (handled) return true;

			if (message.Name == "myMessage2") {
				return true;
			}

			// Sending renderer->renderer is not supported.
			//var message2 = CefProcessMessage.Create("myMessage2");
			//frame.SendProcessMessage(CefProcessId.Renderer, message2);
			//Console.WriteLine("Sending myMessage2 to renderer process = {0}");

			var message3 = CefProcessMessage.Create("myMessage3");
			frame.SendProcessMessage(CefProcessId.Browser, message3);
			Log.Info("Sending myMessage3 to browser process");

			return false;
		}
	}


	public sealed class BrowserProcessHandler : CefBrowserProcessHandler
	{
		protected override void OnBeforeChildProcessLaunch(CefCommandLine commandLine) {
			Console.WriteLine("AppendExtraCommandLineSwitches: {0}", commandLine);
			Console.WriteLine(" Program == {0}", commandLine.GetProgram());

			// .NET in Windows treat assemblies as native images, so no any magic required.
			// Mono on any platform usually located far away from entry assembly, so we want prepare command line to call it correctly.
			if (Type.GetType("Mono.Runtime") != null) {
				if (!commandLine.HasSwitch("cefglue")) {
					var path = new Uri(Assembly.GetEntryAssembly().CodeBase).LocalPath;
					commandLine.SetProgram(path);

					var mono = CefRuntime.Platform == CefRuntimePlatform.Linux ? "/usr/bin/mono" : @"C:\Program Files\Mono-2.10.8\bin\monow.exe";
					commandLine.PrependArgument(mono);

					commandLine.AppendSwitch("cefglue", "w");
				}
			}

			Console.WriteLine("  -> {0}", commandLine);
		}
	}
}

namespace RhuEngine.Managers
{


	public class WebBrowserManager : CefApp, IManager
	{
		private Engine _engine;
		protected bool MultiThreadedMessageLoop { get; private set; }

		public void Dispose() {
			CefRuntime.Shutdown();
		}
		private readonly CefBrowserProcessHandler _browserProcessHandler = new BrowserProcessHandler();
		private readonly CefRenderProcessHandler _renderProcessHandler = new RenderProcessHandler();

		protected override void OnBeforeCommandLineProcessing(string processType, CefCommandLine commandLine) {
			Console.WriteLine("OnBeforeCommandLineProcessing: {0} {1}", processType, commandLine);

			// TODO: currently on linux platform location of locales and pack files are determined
			// incorrectly (relative to main module instead of libcef.so module).
			// Once issue http://code.google.com/p/chromiumembedded/issues/detail?id=668 will be resolved
			// this code can be removed.
			if (CefRuntime.Platform == CefRuntimePlatform.Linux) {
				var path = new Uri(Assembly.GetEntryAssembly().CodeBase).LocalPath;
				path = Path.GetDirectoryName(path);

				commandLine.AppendSwitch("resources-dir-path", path);
				commandLine.AppendSwitch("locales-dir-path", Path.Combine(path, "locales"));
			}
		}

		protected override CefBrowserProcessHandler GetBrowserProcessHandler() {
			return _browserProcessHandler;
		}

		protected override CefRenderProcessHandler GetRenderProcessHandler() {
			return _renderProcessHandler;
		}

		public class RwebClient: CefClient
		{

		}

		public void Init(Engine engine) {
			_engine = engine;
			var settings = new CefSettings {
				MultiThreadedMessageLoop = MultiThreadedMessageLoop = CefRuntime.Platform == CefRuntimePlatform.Windows,
				LogSeverity = CefLogSeverity.Default,
				LogFile = "cef.log",
				ResourcesDirPath = System.IO.Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetEntryAssembly().CodeBase).LocalPath),
				RemoteDebuggingPort = 20480,
				NoSandbox = true
			};

			var args = new string[0];
			var argv = args;
			if (CefRuntime.Platform != CefRuntimePlatform.Windows) {
				argv = new string[args.Length + 1];
				Array.Copy(args, 0, argv, 1, args.Length);
				argv[0] = "-";
			}

			var mainArgs = new CefMainArgs(argv);
			var exitCode = CefRuntime.ExecuteProcess(mainArgs, this, IntPtr.Zero);
			if (exitCode != -1) {
				Log.Err("CefRuntime.ExecuteProcess() returns {0}", exitCode);
				return;
			}
			CefRuntime.Initialize(mainArgs, settings, this, IntPtr.Zero);
			Log.Info("CefRuntime Started");
			Task.Run(CefRuntime.RunMessageLoop);
			var client = new RwebClient();
			CefBrowserHost.CreateBrowser(CefWindowInfo.Create(), client, new CefBrowserSettings { WebGL = CefState.Enabled, RemoteFonts = CefState.Enabled, JavaScript = CefState.Enabled }, "https://google.com");
		}

		public void Step() {
		}
	}
}
