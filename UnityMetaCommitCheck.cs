using System;
using System.IO;

namespace SVNMetaCommitCheck {
	public class UnityMetaCommitCheck : IPreCommitFileCheck {
		public bool Check(CommitFileInfo[] infos) {
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

				var filepath = info.FilePath + ".meta";
				if (Array.Find(infos, item => item.FilePath == filepath) == null) {
					Console.Error.WriteLine("提示：Meta文件不匹配; " + info.FilePath);
					return false;
				}
			}

			return true;
		}
	}
}
