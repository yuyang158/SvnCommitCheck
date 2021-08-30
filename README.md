# SvnCommitCheck
check svn commit on pre-commit hook

## 功能

1. 中文路径检查 ChineseAssetPathCheck.cs
2. Unity meta文件漏提检查 UnityMetaCommitCheck.cs
3. Lua代码提交时LuaCheck检查 LuaScriptCheck.cs
4. 提交日志检查 CommitLogCheck.cs

## 提交Log额外参数
1. --ignore-log 强制忽略Log检查
2. --ignore-lua-check 强制忽略Lua代码检查


## 依赖

[LuaCheck](https://github.com/mpeterv/luacheck)
