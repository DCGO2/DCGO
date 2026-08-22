Cherry-pick series (not format-patch)

Android custom work is thousands of binary/LFS files. Storing `git format-patch` here would duplicate that into `tools/`.

`dcgo-patch.ps1 rebase` instead cherry-picks every commit after `config.json` `baselineCommit` from `customBranch` onto `-Onto`.

Small text diffs for official C# files live in `../patches/files/`.
