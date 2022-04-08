using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace SVNMetaCommitCheck {
	class Program {
		private static bool CheckAttributeExist(Type typeToCheck, Type attributeType) {
			var attributes = typeToCheck.GetCustomAttributes(false);
			foreach (var attr in attributes) {
				if(attr.GetType() == attributeType) {
					return true;
				}
			}
			return false;
		}

		static int Main(string[] args) {

			Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

			List<IPreCommitLogCheck> logChecks = new List<IPreCommitLogCheck>();
			List<IPreCommitFileCheck> fileChecks = new List<IPreCommitFileCheck>();
			var types = Assembly.GetExecutingAssembly().GetTypes();
			foreach (var type in types) {
				if (typeof(IPreCommitLogCheck).IsAssignableFrom(type) && !type.IsInterface && !CheckAttributeExist(type, typeof(CheckIgnoreAttribute))) {
					logChecks.Add(Activator.CreateInstance(type) as IPreCommitLogCheck);
				}
			}
			foreach (var type in types) {
				if (typeof(IPreCommitFileCheck).IsAssignableFrom(type) && !type.IsInterface && !CheckAttributeExist(type, typeof(CheckIgnoreAttribute))) {
					fileChecks.Add(Activator.CreateInstance(type) as IPreCommitFileCheck);
				}
			}
			string transactionId = args[1];
			string repoPath = args[0];
			Constant.TransactionId = transactionId;
			Constant.RepoPath = repoPath;

			string log;
			using (var process = new Process()) {
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.FileName = Constant.SvnLookExecPath;
				process.StartInfo.Arguments = string.Format("log -t {0} \"{1}\"", transactionId, repoPath);
				process.Start();
				log = process.StandardOutput.ReadToEnd();

				if (!log.Contains("--ignore-log")) {
					foreach (var logCheck in logChecks) {
						if (!logCheck.Check(log)) {
							return 1;
						}
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
					if (!fileCheck.Check(infos.ToArray(), log)) {
						return 1;
					}
				}

				process.WaitForExit();
			}

			return 0;
		}
	}
}
