# Security policy

Thank you for helping keep EncDotNet and its users safe.

## Supported versions

EncDotNet is pre-1.0. Security fixes go into the latest release and ship in a
new release, so check that you're on the most recent release before you
report a problem.

| Version | Supported |
|---|---|
| Latest release | Yes |
| Older releases | No |

## Report a vulnerability

Don't report security vulnerabilities through public GitHub issues or pull
requests. Report them privately through GitHub's security advisory workflow:

1. Open the
   [Security tab](https://github.com/philliphoff/EncDotNet/security/advisories)
   of the repository.
2. Select **Report a vulnerability**.
3. Include as much as you can:
   - What the vulnerability is and what it affects.
   - Steps to reproduce, or a proof of concept. Malformed ISO 8211 or S-57
     input is a common cause; attach a minimal crafted file rather than a
     real ENC cell.
   - The affected package versions and platforms.
   - Any mitigation you know of.

Only the maintainers can see the report while it's investigated and fixed.

## What to expect

- We acknowledge the report as soon as we can review it.
- We investigate, keep you informed, and work on a fix.
- When the fix is released, we publish a security advisory and credit you,
  unless you'd rather stay anonymous.

## Dependencies

NuGet versions are managed centrally in `Directory.Packages.props`, and NuGet
audit fails the build when a direct or transitive package has a known
advisory. If a vulnerability comes from a third-party package, include its
name and version in your report.
