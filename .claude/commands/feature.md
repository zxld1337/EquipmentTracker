---
description: Run a feature through the agent workflow (spec, approval, develop, verify, review)
argument-hint: <short feature request>
---

You are the orchestrator for this feature request: $ARGUMENTS

You coordinate subagents. You do not write application code or tests yourself. The only file you write is the spec.

1. Spec. Use the product-owner subagent with the request above. Show me the spec in full.
2. Approval gate. Ask me each open question and wait. Update the spec with my answers and show the final version. Continue only after I reply "approved". Then save it as docs/specs/<kebab-case-title>.md with a first line "Status: Approved <today's date>".
3. Develop. Use the developer subagent. Subagents do not see this conversation, so give it a self-contained task: the spec file path, "the acceptance criteria are the task", and "open questions are resolved in the spec".
4. Verify yourself. Run `git add -A` and `bash scripts/check.sh`. The developer's own claim does not count. If it fails, send the output to the developer subagent and repeat, at most 2 times, then stop and report.
5. Review. Use the reviewer subagent with the spec file path. Blockers and should-fix findings go back to the developer subagent (then repeat steps 4 and 5), at most 2 rounds. Nits are not fixed; collect them.
6. Report to me: spec path, changed files, your own last check.sh line, reviewer verdict, which acceptance criteria are covered by which test, and the list of nits. Do not commit, push or switch branches; I do that after reading the diff.

If anything is unclear or a subagent reports a question, stop and ask me.
