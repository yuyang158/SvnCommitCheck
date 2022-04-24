namespace SVNMetaCommitCheck {
	public static class Constant {
#if LINUX_PATH
		public static readonly string SvnLookExecPath = @"/usr/bin/svnlook";
		public static readonly string LuaCheckExecPath = @"/usr/bin/luacheck";
#else
		public static readonly string SvnLookExecPath = @"C:\Program Files\VisualSVN Server\bin\svnlook.exe";
		public static readonly string LuaCheckExecPath = @"C:\Program Files\luacheck\luacheck.exe";
#endif
		public static string RepoPath { get; set; }
		public static string TransactionId { get; set; }
	}
}
