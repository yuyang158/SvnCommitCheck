namespace SVNMetaCommitCheck {
	public static class Constant {
		public static readonly string SvnLookExecPath = @"C:\Program Files\VisualSVN Server\bin\svnlook.exe";
		public static readonly string LuaCheckExecPath = @"C:\Program Files\luacheck\luacheck.exe";
		public static string RepoPath { get; set; }
		public static string TransactionId { get; set; }
	}
}
