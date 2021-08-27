using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace SVNMetaCommitCheck {
	class Program {
		static int Main(string[] args) {
			string transactionId = args[1];
			string repoPath = args[0];

			Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

			List<IPreCommitLogCheck> logChecks = new List<IPreCommitLogCheck>();
			List<IPreCommitFileCheck> fileChecks = new List<IPreCommitFileCheck>();
			var types = Assembly.GetExecutingAssembly().GetTypes();
			foreach (var type in types) {
				if (typeof(IPreCommitLogCheck).IsAssignableFrom(type) && !type.IsInterface) {
					logChecks.Add(Activator.CreateInstance(type) as IPreCommitLogCheck);
				}
			}
			foreach (var type in types) {
				if (typeof(IPreCommitFileCheck).IsAssignableFrom(type) && !type.IsInterface) {
					fileChecks.Add(Activator.CreateInstance(type) as IPreCommitFileCheck);
				}
			}

			using (var process = new Process()) {
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.FileName = Constant.SvnLookExecPath;
				process.StartInfo.Arguments = string.Format("log -t {0} \"{1}\"", transactionId, repoPath);
				process.Start();
				string content = process.StandardOutput.ReadToEnd();

				foreach (var logCheck in logChecks) {
					if (!logCheck.Check(content)) {
						return 1;
					}
				}

				process.WaitForExit();
			}

			using (var process = new Process()) {
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.FileName = Constant.SvnLookExecPath;
				process.StartInfo.Arguments = string.Format("changed -t {0} \"{1}\"", transactionId, repoPath);
				process.Start();

				List<CommitFileInfo> infos = new List<CommitFileInfo>();
				while (!process.StandardOutput.EndOfStream) {
					var line = process.StandardOutput.ReadLine();
					if (string.IsNullOrEmpty(line)) {
						continue;
					}

					infos.Add(new CommitFileInfo(line));
				}

				foreach (var fileCheck in fileChecks) {
					if (!fileCheck.Check(infos.ToArray())) {
						return 1;
					}
				}

				process.WaitForExit();
			}

			return 0;
		}
	}
}
