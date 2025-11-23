[◀ Back to Documentation Home](../README.md)

# 🔐 SECURITY Components

## Crypto
**Namespace:** `Nglib.SECURITY.CRYPTO`

Cryptography tools and token management for securing data and authentications.

**Main Features:**
- **AES256 Encryption**: Encryption/decryption of binary data and streams
- **JWT Tokens**: Creation and validation of JWT tokens with HMAC-SHA256 signature
- **Key Management**: Flexible configuration of encryption keys and initialization vectors
- **Hashing**: Support for SHA256, SHA1, SHA512, MD5 algorithms
- **Random Generation**: Creation of cryptographically secure random bytes

**Main Classes:**
- `CryptoTools`: AES encryption/decryption with async support
- `TokenJwtTools`: JWT token generation and validation
- `CryptoOption`: Encryption parameters configuration
- `CryptoCoreTools`: Base utilities for cryptography

## Identity  
**Namespace:** `Nglib.SECURITY.IDENTITY`

User identity management, claims and multi-tenant systems.

**Main Features:**
- **Claims Management**: Claims manipulation in ClaimsIdentity
- **Multi-tenant**: Interface for tenant-related objects
- **User abstractions**: Base interface to represent users
- **Authentication helpers**: Authentication status verification

**Main Classes:**
- `ClaimsIdentityTools`: Extensions for claims manipulation
- `IUser`: Base interface for users
- `ITenantObject`: Interface for multi-tenant objects