using System;

namespace SVNMetaCommitCheck {
	public class CommitLogCheck : IPreCommitLogCheck {
		public bool Check(string log) {
			if (log.Length < 4) {
				Console.Error.WriteLine("提示:日志文字数量不足");
				return false;
			}

			if (!log.StartsWith("#")) {
				Console.Error.WriteLine("提示:日志文字需位单号开头:#100 ");
				return false;
			}

			return true;
		}
	}
}
