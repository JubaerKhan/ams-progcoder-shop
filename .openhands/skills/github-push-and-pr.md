---
name: github-push-and-pr
type: knowledge
version: 1.0.0
agent: CodeActAgent
triggers:
- git push
- pull request
- open a PR
- permission denied
- 403
- Resource not accessible
---

# Pushing and opening a pull request

AMS assigns your branch and gives OpenHands the credential. The remote is
already configured. You do not need to set it up.

## The whole procedure

```bash
git add <the files you changed>
git commit -m "type: imperative summary"
git push -u origin "$(git branch --show-current)"
```

Then open the PR with the `create_pr` tool, targeting the branch the task names.
If the task does not name one, target the repository's default branch.

Commit only the files you changed. Never `git add -A` — the sandbox leaves build
output and scratch files behind.

## If the push fails, stop

A push failure means the credential is wrong. You cannot fix that from inside
the sandbox. **Report the exact error text and stop.** AMS will replace the
token and start you again.

These errors all mean the same thing:

```
remote: Permission to <owner>/<repo>.git denied to <user>.
fatal: ... The requested URL returned error: 403
Resource not accessible by personal access token
```

The usual cause is a **fine-grained** token (`github_pat_…`). Fine-grained
tokens only reach repositories owned by the token's owner, or organisations that
granted them access. They cannot write to another user's personal repository,
even when that user has added you as a collaborator with write permission. The
fix is a **classic** token (`ghp_…`) with the `repo` scope, and only a human
outside the sandbox can issue one.

Note that `GET /repos/{owner}/{repo}` reports `"permissions": {"push": true}`
for a collaborator regardless of what the *token* can do. A successful read
proves nothing about writes.

## Never do these

Each was attempted on a previous run. Together they consumed 16 of 69 steps and
the push still failed:

- Rewriting `origin` to embed the token: `git remote set-url origin
  "https://${GITHUB_TOKEN}@github.com/..."`. The credential helper already does
  this. It changes nothing.
- Retrying the push with different usernames — `x-access-token`, `oauth2`,
  `token`, the account name. The username is not the problem.
- Reading token scopes from response headers, or calling `/user` to see who you
  are. It will not tell you why the write was refused.
- Searching for a fork to push to instead. Do not silently change the target.
- Probing `host.docker.internal` or scanning localhost ports for something that
  might hold a better credential.

One failed push, one clear report. That turns a wasted run into a two-minute
fix.
