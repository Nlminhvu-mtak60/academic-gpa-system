# 04 - Environment Configuration Guide (Phase 11)

This document maps environment configuration settings, variables schemas, and security parameters across Development, Testing, and Production environments.

---

## 1. Environment Configurations Matrix

| Config Parameter | Development | Testing | Production |
|---|---|---|---|
| **ASPNETCORE_ENVIRONMENT** | `Development` | `Testing` | `Production` |
| **Database Server** | LocalDB / SQL Express | InMemory Provider | Containerized MS SQL Server |
| **DB Port** | `Local` | `N/A` | `1433` (isolated internally) |
| **CORS Policy** | Allows all `*` | Allows all `*` | Specific Domain mappings |
| **JWT Key length** | 128+ bits | 128+ bits | 256+ bits (securely stored) |
| **Access Token Lifetime** | 15 Minutes | 15 Minutes | 15 Minutes |
| **Refresh Token Lifetime** | 7 Days | 7 Days | 7 Days |
| **Logging Destinations** | Console | Debug Output | Console + Rolling Log Files |

---

## 2. Production Environment Variables Schema

Production configurations should be loaded via system environment variables. The Docker Compose file automatically reads and maps these:

```bash
# ==============================================================================
# Database Variables Configuration
# ==============================================================================
DB_PASSWORD=ProdDbPasswordReplacedHere!      # Strong password for SA DB access

# ==============================================================================
# Security Configurations
# ==============================================================================
JWT_SECRET=ProdJWTSecretEnsureItIsMinimum32CharsAndExtremelySecure999!
JWT_ISSUER=gpa-api-server
JWT_AUDIENCE=gpa-client-app

# ==============================================================================
# AI Advisors API Configuration
# ==============================================================================
AI_SERVICE_API_KEY=SharedApiGatewayCommunicationSecretKeyGoesHere!
```

---

## 3. JWT Secret Key Policy
The production JWT Secret key **must** exceed 32 characters (256 bits) to satisfy HMAC-SHA256 signature verification policies. Ensure this key is rotated at regular enterprise compliance intervals.
