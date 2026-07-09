# Google OAuth 2.0 Integration Setup Guide

This guide walks through configuring Google Sign-In for the Academic GPA Management System.

---

## 1. Creating a Google Cloud Project
1. Open the [Google Cloud Console](https://console.cloud.google.com/).
2. Click on the project dropdown at the top navigation bar and select **New Project**.
3. Enter a descriptive project name (e.g. `academic-gpa-management`) and click **Create**.

---

## 2. Configuring the OAuth Consent Screen
Before creating credentials, you must configure the consent screen that users see when signing in:
1. Navigate to **APIs & Services** > **OAuth consent screen** from the left navigation menu.
2. Select **External** user type and click **Create**.
3. Provide the required App Information:
   - **App name**: Academic GPA Management
   - **User support email**: Your support email address.
   - **Developer contact information**: Your developer email address.
4. Click **Save and Continue**.
5. Under **Scopes**, click **Add or Remove Scopes**, select `.../auth/userinfo.email` and `.../auth/userinfo.profile`, then click **Save and Continue**.
6. Under **Test users**, add the email addresses of accounts that will test the application during development.
7. Click **Save and Continue**, review the summary, and click **Back to Dashboard**.

---

## 3. Creating OAuth 2.0 Credentials
1. Go to **APIs & Services** > **Credentials**.
2. Click **+ Create Credentials** at the top and select **OAuth client ID**.
3. Select **Web application** as the Application type.
4. Name the credential (e.g. `Academic GPA Web Client`).
5. Configure the URIs based on your environment:

### Localhost Development Configuration
- **Authorized JavaScript origins**:
  - `http://localhost:5173` (Vite development server)
- **Authorized redirect URIs**:
  - `http://localhost:5173` (Since the GIS popup callback returns the credential directly to the client origin, we do not require a separate server-side callback path for client-side popups).

### Production Configuration
- **Authorized JavaScript origins**:
  - `https://your-production-domain.com`
- **Authorized redirect URIs**:
  - `https://your-production-domain.com`

6. Click **Create** to generate your client ID and client secret. Copy these credentials immediately.

---

## 4. Application Configuration

### Backend Setup (`appsettings.json` / `appsettings.Production.json`)
Add the credentials under the `Google` block:
```json
"Google": {
  "ClientId": "YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com",
  "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
}
```

### Frontend Setup (`.env`)
Create or edit the `.env` file in the root of the `frontend` directory:
```env
VITE_GOOGLE_CLIENT_ID=YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com
VITE_API_URL=http://localhost:5046
```
