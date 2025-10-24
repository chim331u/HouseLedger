# VaultLocal Requirements Document

## 1. Introduction

**VaultLocal** is an internal secure storage component designed to manage application secrets locally in a controlled, auditable, and encrypted manner. It functions as an independent module callable by other components of the main application, with its own isolated SQLite database for data persistence and versioning.

## 2. Objectives

- Provide a consistent and secure local vault for storing and retrieving secrets.
- Ensure data confidentiality, integrity, and traceability.
- Enable controlled evolution through phased implementation.
- Maintain flexibility for integration and testing without external dependencies.

## 3. Scope

This document defines the functional, security, and non-functional requirements for the VaultLocal project, to be implemented and released in incremental, consistent phases.

VaultLocal is **not an external API service** in its first phase but a callable internal component. A minimal API interface may be introduced in Phase 4 for extended use.

## 4. Architecture Overview

VaultLocal is a .NET 8 component providing secure data storage and retrieval using AES-256-GCM encryption.  
It maintains a dedicated SQLite database to store encrypted secrets, their metadata, and version history.  
The module exposes interfaces for secret CRUD operations, role-based access control, audit logging, and encrypted export/import.

**Core architectural principles:**
- Isolation from the main application database.
- Transparent encryption/decryption layer.
- Deterministic version history.
- Extensible governance and auditing capabilities.
- Configurable operational modes (persistent, memory-only for testing).

---

## 5. Functional Requirements by Phase

### Phase 1 – Core Vault

| ID | Title | Description | Actors | Notes |
|----|--------|--------------|--------|-------|
| **UC1** | Store new secret | The main application module stores a secret by name; the vault encrypts it and saves it in its DB. | Internal system component | Adds CreatedBy tag |
| **UC2** | Retrieve secret | A module requests a secret by name; the vault decrypts and returns it. | Internal system component | Checks TTL validity |
| **UC3** | Update secret | An existing secret is updated; the previous version is preserved in history. | Internal system component | Increments version counter |
| **UC4** | Delete secret | Removes a secret (optionally marks as deleted). | Internal system component | Adds audit log entry (in Phase 2) |
| **UC5** | List secrets | Returns list of all stored secret names and metadata (CreatedBy, version, TTL). | Internal system component | For management/debug |
| **UC6** | Check secret expiration | Vault automatically marks or rejects expired secrets. | System process | TTL enforcement |

**Deliverables:**
- Secret repository using SQLite + Dapper.
- AES-256-GCM encryption/decryption service.
- Versioning and history tracking.
- TTL expiration logic.
- Core service interface callable from the main app.

---

### Phase 2 – Governance & Security

| ID | Title | Description | Actors | Notes |
|----|--------|--------------|--------|-------|
| **UC7** | Assign role to user | Assigns a user a specific role (Reader/Writer/Admin). | Admin | Stored in vault DB |
| **UC8** | Enforce access control | Vault verifies if user can read/write/delete based on role. | Vault service | Applied to all UC1–UC5 |
| **UC9** | Record audit trail | Every operation (read/write/delete) generates an audit entry. | Vault service | Linked to AuditService |
| **UC10** | Validate data integrity | Periodically verify that encrypted values match stored checksums. | System process | Detects tampering or corruption |

**Deliverables:**
- RBAC system integrated with vault operations.
- Audit logging service.
- Data integrity validation job.
- Security event tracking and reporting.

---

### Phase 3 – Operational & Reliability

| ID | Title | Description | Actors | Notes |
|----|--------|--------------|--------|-------|
| **UC11** | Export secrets archive | Vault exports all secrets into an encrypted JSON archive. | Admin | Password-protected archive |
| **UC12** | Import secrets archive | Vault imports a previously exported archive. | Admin | Merges or replaces existing secrets |
| **UC13** | Backup vault DB | A scheduled job backs up the vault SQLite database. | System process | Backup frequency configurable |
| **UC14** | Restore vault DB | Restores from a backup file. | Admin | Optional integrity validation |
| **UC15** | Health check | Provides a vault status report (DB access, encryption key loaded, version). | Monitoring system | Returns structured status |

**Deliverables:**
- Encrypted import/export functionality.
- Backup and restore utility.
- Health diagnostics endpoint or callable method.

---

### Phase 4 – Optional API Exposure

| ID | Title | Description | Actors | Notes |
|----|--------|--------------|--------|-------|
| **UC16** | Remote store/retrieve secrets | Expose secret CRUD operations via REST endpoints. | External module | Optional minimal API |
| **UC17** | Remote admin operations | Export/import/health endpoints for external tools. | Admin user | Requires authentication |

**Deliverables:**
- Minimal REST API interface (optional).
- Authentication/authorization layer.
- HTTPS and secure configuration enforcement.

---

## 6. Security Model (Summary)

**Core Security Objectives:**
- Protect data confidentiality, integrity, and traceability.
- Limit secret exposure to in-memory operations only.
- Provide auditable, role-restricted access to sensitive information.

### Security Layers per Phase

| Phase | Security Mechanisms | Description |
|-------|---------------------|-------------|
| **Phase 1** | AES-256-GCM encryption, isolated DB, TTL, CreatedBy tagging | Base encryption and controlled storage |
| **Phase 2** | RBAC, audit trail, integrity validation | Enforced access control and traceability |
| **Phase 3** | Encrypted export/import, backup validation, health diagnostics | Secure lifecycle management |
| **Phase 4** | API authentication, HTTPS | Secure external exposure |

### Master Key Management

- **Development mode:** auto-generated, in-memory key.
- **Production mode:** key provided via environment variable or secured file.
- **Encryption:** AES-256-GCM with random IV per secret.
- **Integrity:** SHA-256 checksum stored per version.

### PCI DSS Alignment (Practical)

| PCI DSS Principle | VaultLocal Implementation | Status |
|--------------------|----------------------------|---------|
| Data encryption | AES-256-GCM for all stored secrets | ✔ |
| Key management | Master key rotation (future) | 🕓 Partial |
| Access control | Role-based permissions | ✔ |
| Audit trails | Operation logging | ✔ |
| Data separation | Dedicated vault database | ✔ |
| Key storage | External key source (production) | 🕓 Partial |

---

## 7. Non-Functional Requirements

| Category | Requirement |
|-----------|--------------|
| **Performance** | Retrieve secrets in <10ms average under local load. |
| **Scalability** | Designed for local use; not intended for distributed scaling. |
| **Reliability** | SQLite transactions ensure ACID properties. |
| **Security** | Encryption keys never persisted; all secrets encrypted at rest. |
| **Maintainability** | Clean separation between repository, encryption, and service layers. |
| **Observability** | Logging integrated with .NET ILogger. |
| **Testability** | Memory-only mode for unit tests. |

---

## 8. Future Enhancements (Out of Scope for Now)

- Key rotation and re-encryption of data.
- Integration with HSM or TPM.
- External vault synchronization (HashiCorp, Azure Key Vault, etc.).
- Multi-instance clustering or replication.

---

**End of Document**
