using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace SVNMetaCommitCheck {
	public class LuaScriptCheck : IPreCommitFileCheck {
		private string luaGlobalVariables;
		public LuaScriptCheck() {
			luaGlobalVariables = File.ReadAllText("./LuaGlobalVariable.txt");
		}

		public bool Check(CommitFileInfo[] infos) {
			Debugger.Break();
			var collectLuaFiles = new List<string>();
			foreach (var item in infos) {
				if (item.CommitOperator == CommitOperatorType.Delete)
					continue;

				if (Path.GetExtension(item.FilePath) != ".lua") {
					continue;
				}

				var outputLuaFile = item.FilePath.Replace('/', '.');
				using (var process = new Process()) {
					process.StartInfo.FileName = Constant.SvnLookExecPath;
					process.StartInfo.CreateNoWindow = true;
					process.StartInfo.Arguments = $"cat -t {Constant.TransactionId} {Constant.RepoPath} {item.FilePath} > {outputLuaFile}";
				}

				collectLuaFiles.Add(outputLuaFile);
			}

			if (collectLuaFiles.Count == 0) {
				return true;
			}

			using (var process = new Process()) {
				process.StartInfo = new ProcessStartInfo {
					FileName = Constant.LuaCheckExecPath,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					CreateNoWindow = true,
					Arguments = "--max-code-line-length 120 --formatter default " + string.Join(" ", collectLuaFiles.ToArray()) +
						" --ignore 142 143 --globals " + luaGlobalVariables
				};
				process.Start();
				StringBuilder builder = new StringBuilder();
				while (!process.StandardOutput.EndOfStream) {
					var line = process.StandardOutput.ReadLine();
					if (line.EndsWith("OK")) {
						continue;
					}

					builder.AppendLine(line);
				}

				process.WaitForExit();

				if (builder.Length == 0) {
					return true;
				}

				Console.Error.WriteLine(builder.ToString());
				return false;
			}
		}
	}
}
