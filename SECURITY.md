# Security review and hardening notes

This document records the security boundaries enforced by the authentication plugin and the regression cases that should be checked before a release.

## Changes in this branch

- Validate release versions before using them and pass the value to shell commands through environment variables instead of interpolating it into shell source.
- Restrict Discord member join, leave, and role-update synchronization to the configured Discord server. Events from other servers must not change Jellyfin accounts or permissions.
- Check Discord OAuth token and user endpoint HTTP status codes before parsing response bodies.
- Bound avatar downloads to 8 MiB and use response-header streaming to avoid buffering an unbounded response.
- Carry the one-time exchange code in the URL fragment, which browsers do not send in HTTP requests, instead of the query string. The completion page still redeems the code through a POST; the code remains one-time-use and expires after five minutes.

## Security regression checklist

Before release, verify the following manually or with automated tests:

1. A malformed or non-SemVer release version fails before any release, branch, or manifest operation.
2. Version and changelog values containing shell metacharacters are treated as data and cannot execute commands.
3. A linked user leaving a Discord server other than the configured server does not disable their Jellyfin account.
4. Role updates and joins from a different server do not change Jellyfin permissions or assign configured default roles.
5. Discord token and user API error responses fail authentication without creating a Jellyfin session.
6. Missing, mismatched, expired, and reused OAuth state/exchange values fail closed.
7. Exchange codes cannot be redeemed more than once and expire after five minutes.
8. Avatar downloads above 8 MiB, unsuccessful HTTP responses, and unsupported content types are rejected.
9. Only Jellyfin administrators can access the plugin configuration endpoint and its secrets.
10. Removing the configured Discord admin role revokes Jellyfin administrator, content-deletion, and all-folder permissions.
11. No bot token, client secret, OAuth access token, or Jellyfin access token appears in logs.

## Operational guidance

- Keep the bot token and OAuth client secret private. Rotate either credential immediately if it appears in logs, issue reports, screenshots, or source control.
- The plugin relies on Jellyfin's configuration API for its admin-only configuration UI. Confirm the installed Jellyfin version enforces the `RequiresElevation` policy and administrator authorization as expected.
- This source review is not a substitute for dependency scanning, build/test execution, or a runtime security assessment.
