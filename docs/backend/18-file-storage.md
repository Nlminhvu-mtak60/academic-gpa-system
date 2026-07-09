# 18 — File Storage Design

> **Document ID**: ARC-BE-FILE-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: File storage interfaces and file upload security rules

---

## 1. Document Purpose

This document defines the file storage abstraction layer, security validation rules, and storage adapters for the application.

---

## 2. File Storage Abstraction (`IFileStorageService`)

To decouple the application from physical file systems, file uploads are managed using a storage interface:

*   **Methods**:
    *   `SaveFileAsync(Stream fileStream, string fileName, string contentType)`: Saves a file and returns its public URL path.
    *   `DeleteFileAsync(string fileUrl)`: Deletes an existing file.

---

## 3. Storage Providers

*   **Local Disk Storage (`LocalFileStorageService`)**: Saves files to the server's local disk (e.g. inside a `/wwwroot/uploads` folder). Used in development and staging environments.
*   **Cloud Storage (`BlobFileStorageService`)**: Saves files to cloud storage services (Azure Blob Storage or AWS S3). Used in production environments.

---

## 4. File Upload Security & Validation Rules

To protect the server from security exploits (such as shell script execution or disk space exhaustion), all file uploads must pass validation checks:

1.  **File Size Limits**: Restricted to a maximum of **2MB**. Larger uploads are rejected with a `400 Bad Request` response.
2.  **Allowed File Extensions**: Only image formats are permitted: `.png`, `.jpg`, `.jpeg`, `.webp`. Content type headers are verified against a whitelist.
3.  **Unique File Names**: Uploaded files are renamed using GUIDs (e.g., `avatar-a62fb33f.png`) to prevent name collisions and overwrite issues.
4.  **Virus Scanning (Production)**: In production environments, files are scanned for malware before being saved to storage.

---

*End of Document — File Storage Design*
