
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



static class BashRunner
{
	public static string Run(string commandLine) {
		StringBuilder errorBuilder = new();
		StringBuilder outputBuilder = new();
		var arguments = $"-c \"{commandLine}\"";
		using Process process = new() {
			StartInfo = new() {
				FileName = "bash",
				Arguments = arguments,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = false,
			}
		};
		process.Start();
		process.OutputDataReceived += (_, args) => outputBuilder.AppendLine(args.Data);
		process.BeginOutputReadLine();
		process.ErrorDataReceived += (_, args) => errorBuilder.AppendLine(args.Data);
		process.BeginErrorReadLine();
		if (!process.DoubleWaitForExit()) {
			var timeoutError = $@"Process timed out. Command line: bash {arguments}.
Output: {outputBuilder}
Error: {errorBuilder}";
			throw new(timeoutError);
		}
		if (process.ExitCode == 0) {
			return outputBuilder.ToString();
		}

		var error = $@"Could not execute process. Command line: bash {arguments}.
Output: {outputBuilder}
Error: {errorBuilder}";
		throw new(error);
	}

	//To work around https://github.com/dotnet/runtime/issues/27128
	static bool DoubleWaitForExit(this Process process) {
		var result = process.WaitForExit(500);
		if (result) {
			process.WaitForExit();
		}
		return result;
	}
}


static class LinuxClipboard
{
    static readonly bool _isWsl;

    static LinuxClipboard()
    {
        _isWsl = Environment.GetEnvironmentVariable("WSL_DISTRO_NAME") != null;
    }

    public static Task SetTextAsync(string text, CancellationToken _)
    {
        SetText(text);

        return Task.CompletedTask;
    }

    public static void SetText(string text)
    {
        var tempFileName = Path.GetTempFileName();
        File.WriteAllText(tempFileName, text);
        try
        {
            if (_isWsl)
            {
                BashRunner.Run($"cat {tempFileName} | clip.exe ");
            }
            else
            {
                BashRunner.Run($"cat {tempFileName} | xsel -i --clipboard ");
            }
        }
        finally
        {
            File.Delete(tempFileName);
        }
    }

    public static Task<string> GetTextAsync(CancellationToken _)
    {
        return Task.FromResult<string>(GetText());
    }

    public static string GetText()
    {
        var tempFileName = Path.GetTempFileName();
        try
        {
            if (_isWsl)
            {
                BashRunner.Run($"powershell.exe Get-Clipboard  > {tempFileName}");
            }
            else
            {
                BashRunner.Run($"xsel -o --clipboard  > {tempFileName}");
            }
            return File.ReadAllText(tempFileName);
        }
        finally
        {
            File.Delete(tempFileName);
        }
    }
}
