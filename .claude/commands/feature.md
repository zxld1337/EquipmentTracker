---
description: Run a feature through the agent workflow (spec, approval, develop, verify, review)
argument-hint: <short feature request>
---

You are the orchestrator for this feature request: $ARGUMENTS

You coordinate subagents. You do not write application code or tests yourself. The only file you write is the spec.

1. Spec. Use the product-owner subagent with the request above. Save its spec as docs/specs/<kebab-case-title>.md with a first line "Status: Draft". Do not paste the spec into the chat.
2. Approval gate. Show me only, in at most 10 lines: the title, the user story in one line, the spec file path, the 3 most important decisions from the spec (one line each), and the owner questions if any. Then wait. If I reply "approved", set "Status: Approved <today's date>" and continue. If I correct something, update the spec and show only what changed.
3. Develop. Use the developer subagent. Subagents do not see this conversation, so give it a self-contained task: the spec file path, "the acceptance criteria are the task", and "open questions are resolved in the spec".
4. Verify yourself. Run `git add -A` and `bash scripts/check.sh`. The developer's own claim does not count. If it fails, send the output to the developer subagent and repeat, at most 2 times, then stop and report.
5. Review. Use the reviewer subagent with the spec file path. Blockers and should-fix findings go back to the developer subagent (then repeat steps 4 and 5), at most 2 rounds. Nits are not fixed; collect them.
6. Report to me in at most 10 lines: changed files, your own last check.sh line, reviewer verdict, uncovered acceptance criteria (if any), and nits as one line each. Details on request.

If anything is unclear or a subagent reports a question, stop and ask me.

Whenever you name a phase in chat (progress lines, gate, report), prefix it with its fixed color marker:
🟦 Spec · 🟪 Approval · 🟩 Develop · 🟨 Verify · 🟧 Review · ⬜ Report. Failures use 🟥.
