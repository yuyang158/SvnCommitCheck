using System;
using System.IO;

namespace SVNMetaCommitCheck {
	public class UnityMetaCommitCheck : IPreCommitFileCheck {
		public bool Check(CommitFileInfo[] infos, string log) {
			foreach (var info in infos) {
				if (info.CommitOperator == CommitOperatorType.Modify) {
					continue;
				}

				if (!info.FilePath.Contains("/Assets/")) {
					continue;
				}

				if (Path.GetExtension(info.FilePath) == ".meta") {
					continue;
				}

				string metaFilepath;
				if(info.FilePath.EndsWith("/")) {
					metaFilepath = info.FilePath.Substring(0, info.FilePath.Length - 1) + ".meta";
				}
				else {
					metaFilepath = info.FilePath + ".meta";
				}
				if (Array.Find(infos, item => item.FilePath == metaFilepath) == null) {
					Console.Error.WriteLine("Error： Meta file not match: " + info.FilePath);
					return false;
				}
			}

			return true;
		}
	}
}
