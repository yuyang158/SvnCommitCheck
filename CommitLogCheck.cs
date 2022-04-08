using System;

namespace SVNMetaCommitCheck {
	[CheckIgnore]
	public class CommitLogCheck : IPreCommitLogCheck {
		public bool Check(string log) {
			if (log.Length < 4) {
				Console.Error.WriteLine("Error: log string count >= 5.");
				return false;
			}

			if (!log.StartsWith("#")) {
				Console.Error.WriteLine("Error: log string need start with: #100 ");
				return false;
			}

			return true;
		}
	}
}
