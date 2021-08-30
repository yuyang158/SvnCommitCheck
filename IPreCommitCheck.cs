using System;

namespace SVNMetaCommitCheck {
	[Flags]
	public enum CommitOperatorType {
		Add = 1,
		Modify = 2,
		Delete = 4,
		Property = 8
	}

	public class CommitFileInfo {
		public CommitOperatorType CommitOperator { get; private set; }
		public string FilePath { get; private set; }

		public CommitFileInfo(string commitLog) {
			char type = commitLog[0];
			switch (type) {
				case 'A':
					CommitOperator = CommitOperatorType.Add;
					break;
				case 'D':
					CommitOperator = CommitOperatorType.Delete;
					break;
				case 'U':
					CommitOperator = CommitOperatorType.Modify;
					break;
				default:
					CommitOperator = CommitOperatorType.Property;
					break;
			}

			commitLog = commitLog.Substring(1);
			FilePath = commitLog.TrimStart();
		}
	}

	public interface IPreCommitFileCheck {
		bool Check(CommitFileInfo[] infos, string log);
	}

	public interface IPreCommitLogCheck {
		bool Check(string log);
	}
}
