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

		public bool Check(CommitFileInfo[] infos, string log) {
			if (log.Contains("--ignore-lua-check")) {
				return true;
			}
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
					process.StartInfo.UseShellExecute = false;
					process.StartInfo.RedirectStandardOutput = true;
					process.StartInfo.CreateNoWindow = true;
					process.StartInfo.Arguments = $"cat -t {Constant.TransactionId} {Constant.RepoPath}" +
						$" {item.FilePath}";
					process.Start();

					using (var stream = new FileStream(outputLuaFile, FileMode.OpenOrCreate))
					using (var luaWriter = new StreamWriter(stream)) {
						while (!process.StandardOutput.EndOfStream) {
							luaWriter.WriteLine(process.StandardOutput.ReadLine());
						}
					}

					process.WaitForExit();
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
				string finalLine = "";
				StringBuilder builder = new StringBuilder();
				while (!process.StandardOutput.EndOfStream) {
					var line = process.StandardOutput.ReadLine();
					builder.AppendLine(line);
					finalLine = line;
				}

				process.WaitForExit();

				foreach (var tmpLuaFile in collectLuaFiles) {
					File.Delete(tmpLuaFile);
				}

				if (builder.Length == 0) {
					return true;
				}

				if (string.IsNullOrEmpty(finalLine) || finalLine.StartsWith("Total: 0 warnings / 0 errors")) {
					return true;
				}

				Console.Error.WriteLine(builder.ToString());
				return false;
			}
		}
	}
}
