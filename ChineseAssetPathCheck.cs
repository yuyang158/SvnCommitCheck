using System;
using System.Text.RegularExpressions;

namespace SVNMetaCommitCheck {
	class ChineseAssetPathCheck : IPreCommitFileCheck {
		private static bool HasChinese(string str) {
			return Regex.IsMatch(str, @"[\u4e00-\u9fa5]");
		}
		public bool Check(CommitFileInfo[] infos, string log) {
			foreach (var info in infos) {
				if (info.CommitOperator != CommitOperatorType.Add) {
					continue;
				}

				if (!info.FilePath.Contains("/Assets/")) {
					continue;
				}

				if (HasChinese(info.FilePath)) {
					Console.Error.WriteLine("路径中不可包含中文 : " + info.FilePath);
					return false;
				}
			}
			return true;
		}
	}
}
