# Security Policy

## Supported Versions

| Version | Supported          |
| ------- | ------------------ |
| 1.x.x   | :white_check_mark: |
| < 1.0   | :x:                |

## Reporting a Vulnerability

We take the security of AutoMender seriously. If you believe you have found a security vulnerability, please report it to us as described below.

**Please do not report security vulnerabilities through public GitHub issues.**

### How to Report

1. **Email**: Send details to the repository owner via GitHub
2. **Include**:
   - Type of issue (e.g., buffer overflow, SQL injection, cross-site scripting, etc.)
   - Full paths of source file(s) related to the issue
   - Location of the affected source code (tag/branch/commit or direct URL)
   - Any special configuration required to reproduce the issue
   - Step-by-step instructions to reproduce the issue
   - Proof-of-concept or exploit code (if possible)
   - Impact of the issue, including how an attacker might exploit it

### Response Timeline

- **Initial Response**: Within 48 hours
- **Status Update**: Within 7 days
- **Resolution Target**: Within 30 days for critical issues

## Security Measures

AutoMender implements the following security measures:

### Authentication & Authorization
- JWT-based authentication for API endpoints (planned)
- Role-based access control for administrative functions (planned)

### Data Protection
- All sensitive configuration stored in environment variables
- No credentials stored in source code
- API keys validated before use

### Network Security
- CORS configured for specific trusted origins only
- HTTPS recommended for production deployment
- Input validation on all API endpoints

### AI Agent Security
- Sandboxed code execution environment
- Limited file system access
- Audit logging of all AI-generated fixes

## Security Best Practices for Deployment

1. **Environment Variables**: Never commit API keys or secrets
2. **HTTPS**: Always use TLS in production
3. **Firewall**: Restrict access to RabbitMQ management interface
4. **Updates**: Keep all dependencies up to date
5. **Monitoring**: Enable audit logging for all operations

## Acknowledgments

We appreciate responsible disclosure from security researchers.
