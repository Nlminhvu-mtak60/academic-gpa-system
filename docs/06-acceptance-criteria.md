# 06 — Acceptance Criteria

> **Document ID**: SRS-AC-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Given-When-Then (GWT) and checklist

---

## 1. Authentication & Account Management

### AC-AUTH-01: Student Registration (US-AUTH-01)

**Scenario 1: Successful registration**
```
GIVEN   I am on the registration page
AND     I have not registered before
WHEN    I enter a valid email, password matching policy, and my name
AND     I click "Register"
THEN    the system creates my account in "unverified" status
AND     sends a verification email to the provided address
AND     displays "Registration successful. Please check your email to verify your account."
AND     I cannot log in until I verify my email
```

**Scenario 2: Duplicate email**
```
GIVEN   I am on the registration page
WHEN    I enter an email that already exists in the system
AND     I click "Register"
THEN    the system displays "An account with this email already exists."
AND     my password is not stored
```

**Scenario 3: Weak password**
```
GIVEN   I am on the registration page
WHEN    I enter a password that does not meet the password policy
THEN    the system shows real-time feedback indicating which criteria are not met
AND     the form cannot be submitted until all criteria are satisfied
```

**Checklist:**
- [ ] Email format validated (contains @ and valid domain)
- [ ] Email uniqueness checked (case-insensitive)
- [ ] Password validated against all policy criteria
- [ ] Confirm password matches password
- [ ] First name and last name validated (1–50 chars, no digits)
- [ ] Password hashed with bcrypt (cost factor ≥ 12) before storage
- [ ] Verification email sent with unique token (24h expiry)
- [ ] User role set to "Student"
- [ ] User IsActive set to false until email verified
- [ ] Success message does NOT auto-login the user

---

### AC-AUTH-03: Student Login (US-AUTH-03)

**Scenario 1: Successful login**
```
GIVEN   I am a registered student with a verified email
WHEN    I enter my correct email and password on the login page
AND     I click "Login"
THEN    the system returns a JWT access token (15 min expiry)
AND     a refresh token (7 day expiry)
AND     redirects me to the dashboard
AND     updates my last login timestamp
```

**Scenario 2: Wrong credentials**
```
GIVEN   I am on the login page
WHEN    I enter an incorrect email or password
THEN    the system displays "Invalid email or password."
AND     does NOT reveal whether the email exists
```

**Scenario 3: Unverified email**
```
GIVEN   I registered but did not verify my email
WHEN    I try to log in with correct credentials
THEN    the system displays "Please verify your email before logging in."
AND     offers to resend the verification email
```

**Scenario 4: Locked account**
```
GIVEN   my account has been locked by an admin
WHEN    I try to log in with correct credentials
THEN    the system displays "Your account has been locked. Please contact the administrator."
```

**Checklist:**
- [ ] Email lookup is case-insensitive
- [ ] Password verified against bcrypt hash
- [ ] JWT contains: sub (user ID), email, role, studentId
- [ ] Refresh token stored in database with expiry and IP
- [ ] Login attempt rate limited (5 per 15 min per IP)
- [ ] Failed attempts logged for security monitoring

---

### AC-AUTH-04: Google Login (US-AUTH-04)

**Scenario 1: First-time Google login (new account)**
```
GIVEN   I have a Google account
AND     my Google email is not registered in the system
WHEN    I click "Sign in with Google" and authorize the application
THEN    the system creates a new account with my Google email and name
AND     marks my email as verified (auto-verified via Google)
AND     links my Google ID to the account
AND     logs me in automatically
AND     redirects me to the dashboard
```

**Scenario 2: Returning Google login (existing account)**
```
GIVEN   I previously registered with the same email (via email or Google)
WHEN    I click "Sign in with Google"
THEN    the system links my Google ID (if not already linked)
AND     logs me in with existing account data
```

**Checklist:**
- [ ] Google OAuth 2.0 authorization code flow implemented
- [ ] Google access token exchanged for user profile (email, name, avatar)
- [ ] Account creation skips email verification
- [ ] Existing email accounts linked to Google (not duplicated)
- [ ] Google avatar URL stored if no existing avatar

---

### AC-AUTH-05: Forgot Password (US-AUTH-05)

**Scenario 1: Existing email**
```
GIVEN   I am on the forgot password page
WHEN    I enter my registered email and click "Send Reset Link"
THEN    the system sends a password reset email with a unique token (1h expiry)
AND     displays "If an account exists with this email, a reset link has been sent."
```

**Scenario 2: Non-existing email**
```
GIVEN   I enter an email that is NOT in the system
WHEN    I click "Send Reset Link"
THEN    the system displays the SAME message: "If an account exists with this email, a reset link has been sent."
AND     no email is sent
```

**Checklist:**
- [ ] Same success message regardless of email existence (anti-enumeration)
- [ ] Reset token is cryptographically random (≥ 32 bytes)
- [ ] Token expires after 1 hour
- [ ] Rate limited: 3 requests per hour per email
- [ ] Previous unused reset tokens invalidated when new one is generated

---

## 2. Course & Score Management

### AC-COURSE-01: Add Course (US-COURSE-01)

**Scenario 1: Successfully add a course**
```
GIVEN   I am on the courses page for a specific semester
WHEN    I click "Add Course" and enter:
        - Course Code: "CS101"
        - Course Name: "Introduction to Programming"
        - Credits: 3
AND     I click "Save"
THEN    the course appears in the semester's course list
AND     scores are empty (no grade yet)
AND     semester credit count is updated
```

**Scenario 2: Add retake course**
```
GIVEN   I previously took "CS101" in Semester 1 with a failing grade
WHEN    I add "CS101" to Semester 2 and mark it as a "retake"
AND     select the original course as the retake reference
THEN    the course is created with IsRetake = true
AND     the original course reference is stored
AND     GPA calculation uses only the best score between original and retake
```

**Checklist:**
- [ ] Course code: 1–20 characters validated
- [ ] Course name: 1–200 characters validated
- [ ] Credits: integer 1–6 validated
- [ ] Semester belongs to the authenticated student
- [ ] Empty score record created automatically
- [ ] Retake course references original course correctly

---

### AC-COURSE-05: Input Component Scores (US-COURSE-05) ⭐ CRITICAL

**Scenario 1: Input all three scores**
```
GIVEN   I have a course "CS101" with no scores entered
WHEN    I enter:
        - Attendance: 8.0
        - Continuous: 7.0
        - Final: 6.5
AND     I click "Save"
THEN    the system rounds:
        - Attendance: 8.0 (no change)
        - Continuous: 7.0 (no change)
        - Final: 6.5 (no change)
AND     calculates: 8.0×0.1 + 7.0×0.3 + 6.5×0.6 = 6.8
AND     displays: Course Score = 6.8, Grade = C+, GPA-4 = 2.5
AND     semester GPA is recalculated
AND     cumulative GPA is recalculated
```

**Scenario 2: Rounding applied**
```
GIVEN   I enter scores with values that require rounding
WHEN    I enter:
        - Attendance: 7.3 (rounds to 7.5)
        - Continuous: 8.7 (rounds to 8.5)
        - Final: 5.2 (rounds to 5.0)
THEN    the system stores rounded values
AND     calculates: 7.5×0.1 + 8.5×0.3 + 5.0×0.6 = 6.3
AND     displays: Course Score = 6.3, Grade = C, GPA-4 = 2.0
```

**Scenario 3: Partial score entry**
```
GIVEN   I enter only Attendance and Continuous scores (Final not yet taken)
WHEN    I save
THEN    the system stores the entered scores
AND     course score is NOT calculated (displayed as "–")
AND     the course does NOT count toward semester GPA
```

**Scenario 4: Score update with audit**
```
GIVEN   I previously entered Final = 6.5
WHEN    I update Final to 7.0
THEN    the system creates an audit log entry: {field: "FinalExamScore", old: "6.5", new: "7.0", timestamp}
AND     recalculates the course score
AND     recalculates semester and cumulative GPA
```

**Checklist:**
- [ ] Each score validated: 0.0–10.0, step 0.1
- [ ] Attendance and Continuous rounded to nearest 0.5
- [ ] Final Exam rounded to nearest 0.5
- [ ] Course score rounded to 1 decimal place
- [ ] Letter grade assigned per conversion table
- [ ] GPA-4 value assigned per conversion table
- [ ] Audit log entry created for each changed field
- [ ] Semester GPA (10 and 4 scale) recalculated
- [ ] Cumulative GPA recalculated
- [ ] Boundary conditions tested: 0.0, 3.9, 4.0, 8.9, 9.0, 10.0

---

## 3. GPA Calculation

### AC-GPA-04: Cumulative GPA (US-GPA-04)

**Scenario 1: Multiple semesters**
```
GIVEN   I have:
        Semester 1: CS101 (3cr, 8.5), MATH101 (4cr, 7.2)
        Semester 2: CS201 (3cr, 9.0), ENG101 (2cr, 6.8)
WHEN    I view cumulative GPA
THEN    the system calculates:
        = (8.5×3 + 7.2×4 + 9.0×3 + 6.8×2) / (3+4+3+2)
        = (25.5 + 28.8 + 27.0 + 13.6) / 12
        = 94.9 / 12
        = 7.91
AND     displays: Cumulative GPA₁₀ = 7.91
```

**Scenario 2: With retake course**
```
GIVEN   CS101 in Semester 1 scored 3.5 (F)
AND     CS101 retake in Semester 2 scored 7.0 (B)
WHEN    I view cumulative GPA
THEN    CS101 uses score 7.0 (best attempt)
AND     credits counted once (3, not 6)
```

**Scenario 3: No graded courses**
```
GIVEN   I have semesters but no courses with complete scores
WHEN    I view cumulative GPA
THEN    the system displays "–" (not 0.0)
```

---

## 4. Goal Planner

### AC-GOAL-02: Calculate Required GPA (US-GOAL-02)

**Scenario 1: Achievable goal**
```
GIVEN   my current cumulative GPA is 7.50 with 60 total credits
AND     my target GPA is 8.00
AND     I plan to take 15 credits next semester
WHEN    I view the required GPA
THEN    the system calculates:
        Required = (8.00 × 75 − 7.50 × 60) / 15
        = (600 − 450) / 15
        = 150 / 15
        = 10.00
AND     displays: "You need a semester GPA of 10.00 to reach your goal."
```

**Scenario 2: Impossible goal**
```
GIVEN   the calculated required GPA exceeds 10.0
WHEN    I view the required GPA
THEN    the system displays: "This goal cannot be achieved with 15 credits. Consider taking more credits or adjusting your goal."
```

---

## 5. Final Exam Prediction

### AC-PREDICT-01: Calculate Required Final Score (US-PREDICT-01)

**Scenario 1: Achievable target**
```
GIVEN   my Attendance = 8.0 and Continuous = 7.0
AND     I want to achieve grade B (minimum course score 7.0)
WHEN    I request prediction
THEN    Required Final = (7.0 − 8.0×0.1 − 7.0×0.3) / 0.6
        = (7.0 − 0.8 − 2.1) / 0.6
        = 4.1 / 0.6
        = 6.83 → rounded to 7.0
AND     displays: "You need at least 7.0 on the final exam to get a B."
```

**Scenario 2: Impossible target**
```
GIVEN   my Attendance = 5.0 and Continuous = 5.0
AND     I want to achieve grade A+ (minimum 9.0)
WHEN    I request prediction
THEN    Required Final = (9.0 − 0.5 − 1.5) / 0.6 = 7.0 / 0.6 = 11.67
AND     displays: "Impossible — you cannot achieve A+ with your current scores."
```

**Scenario 3: Guaranteed target**
```
GIVEN   my Attendance = 10.0 and Continuous = 10.0
AND     I want to achieve grade D (minimum 4.0)
WHEN    I request prediction
THEN    Required Final = (4.0 − 1.0 − 3.0) / 0.6 = 0.0 / 0.6 = 0.0
AND     displays: "Guaranteed — you will achieve at least grade D regardless of your final exam score."
```

---

## 6. Admin Operations

### AC-ADMIN-04: Lock Student Account (US-ADMIN-04)

**Scenario 1: Successful lock**
```
GIVEN   I am logged in as an admin
AND     I am viewing a student's details
WHEN    I click "Lock Account" and provide a reason: "Academic policy violation"
AND     I confirm the action
THEN    the student's account is marked as locked (IsActive = false)
AND     the lock reason and timestamp are recorded
AND     all student's refresh tokens are revoked
AND     the student is immediately logged out from all sessions
AND     the student list shows the account as "Locked"
```

**Scenario 2: Student attempts login while locked**
```
GIVEN   my account has been locked by an admin
WHEN    I try to log in
THEN    I see: "Your account has been locked. Please contact the administrator."
AND     I cannot access any authenticated endpoints
```

### AC-ADMIN-06: Reset Student Password (US-ADMIN-06)

**Scenario 1: Successful reset**
```
GIVEN   I am an admin viewing a student's profile
WHEN    I click "Reset Password" and confirm
THEN    a temporary password is generated (12 chars, mixed)
AND     the student's password is updated to the temporary password
AND     all refresh tokens are revoked
AND     the temporary password is sent via email to the student
AND     the student is forced to change password on next login
```

**Checklist:**
- [ ] Temporary password meets complexity requirements
- [ ] Student's old password is irreversibly replaced
- [ ] Email sent with temporary credentials
- [ ] Force-change-password flag set on account
- [ ] Temporary password expires after 72 hours if unused

---

## 7. Transcript Sharing

### AC-TRANSCRIPT-02: Generate Share Link (US-TRANSCRIPT-02)

**Scenario 1: Create share link**
```
GIVEN   I am on my transcript page
WHEN    I click "Share Transcript"
AND     select expiry: "30 days"
AND     click "Generate Link"
THEN    the system creates a share record with:
        - Unique UUID v4 token
        - Expiry date = now + 30 days
AND     displays the shareable URL
AND     provides a "Copy Link" button
```

**Scenario 2: Access shared link (as visitor)**
```
GIVEN   I have a valid share link
WHEN    I open the link in a browser (no login required)
THEN    I see the student's transcript in read-only format
AND     the view count is incremented
AND     the page displays "Shared on [date]" and "Valid until [date]"
```

**Scenario 3: Expired link**
```
GIVEN   I have a share link that has expired
WHEN    I try to access it
THEN    I see: "This shared transcript has expired."
```

---

## 8. UI Preferences

### AC-PREF-01: Dark/Light Mode (US-PREF-01)

```
GIVEN   I am logged in and using light mode
WHEN    I click the theme toggle
THEN    the interface immediately switches to dark mode
AND     the preference is saved to my profile (server-side)
AND     on next login, dark mode is applied automatically
```

**Checklist:**
- [ ] Theme changes without page reload
- [ ] All components properly styled in both modes
- [ ] Color contrast meets WCAG AA (≥ 4.5:1)
- [ ] Default theme follows system preference (prefers-color-scheme)

### AC-PREF-02: Language Switcher (US-PREF-02)

```
GIVEN   the interface is in Vietnamese
WHEN    I switch to English
THEN    all UI labels, buttons, menus, and messages change to English
AND     date formats change from dd/MM/yyyy to MM/dd/yyyy
AND     number formats change (decimal separator: comma → period)
AND     the preference is saved to my profile
```

**Checklist:**
- [ ] 100% of UI strings translated
- [ ] No hardcoded text in components
- [ ] Validation error messages translated
- [ ] Date and number formatting locale-aware
- [ ] Language persists across sessions

---

*End of Document — Acceptance Criteria*
