# Security Policy

## Supported Versions

Security updates are provided for the following FASTER versions:

| Version | Supported Status   |
| ------- | ------------------ |
| < 1.6x  | :x:                |
| 1.7x    | :x:                |
| 1.8x    | :warning:          |
| 1.9x    | :heavy_check_mark: |

:x: : Not supported
:warning: : Critical security updates only
:heavy_check_mark: : Fully supported 


## Reporting a Vulnerability

Please do not report security vulnerabilities through public GitHub issues.

Use GitHub's **Report a vulnerability** feature on the FASTER repository to submit security issues privately.

Please provide enough information for the maintainers to reproduce and assess the issue, including the affected version, vulnerability details, reproduction steps, proof of concept, and potential impact.


## Steam Password Storage

Versions prior to 1.7 used a weaker cipher than the one introduced in 1.8, which uses AES-based encryption.

FASTER 1.9.9.0 introduced Windows DPAPI for Steam password storage.

Users running earlier versions should update to a supported release.