# 12 — Validation Strategy

> **Document ID**: ARC-BE-VAL-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Validation pipelines and FluentValidation rules

---

## 1. Validation Strategy

The backend enforces a strict validation strategy: all incoming request payloads are validated for syntax correctness and range boundaries before they are processed by use case handlers.

---

## 2. Validation Flow Pipeline

Validation runs automatically in the MediatR pipeline using a custom `ValidationBehavior`:

```
MediatR Dispatches Command
   └── ValidationBehavior intercepts Command
          └── scans assembly for registered `AbstractValidator<T>`
                 └── runs FluentValidation checks
                        ├── Success: Passes execution to Handler
                        └── Failure: Aborts execution and throws `ValidationException`
```

---

## 3. Core Validation Rules

### 3.1 Registration Validator (`RegisterUserCommandValidator`)
*   `Email`: Must not be empty, must be a valid email format, and must not exist in the database (verified using an asynchronous DB check).
*   `Password`: Minimum 8 characters. Must contain at least one uppercase letter, one lowercase letter, one number, and one special character.
*   `FirstName` / `LastName`: Must not contain numbers or special characters.

### 3.2 Academic Structure Validators
*   `CreateAcademicYearCommandValidator`: `StartYear` must be $\le$ `EndYear`.
*   `CreateCourseCommandValidator`: `CourseCredits` must be an integer between 1 and 6. `CourseCode` must be alphanumeric.
*   `UpdateScoresCommandValidator`: Component scores (`AttendanceScore`, `ContinuousScore`, `FinalExamScore`) must be decimals between `0.0` and `10.0` with up to one decimal place.

---

## 4. Asynchronous Database Validations

Validation rules that require database checks (e.g. verifying email uniqueness or confirming that a retake points to a valid original course) are executed asynchronously inside the validators using injected database interfaces:

```csharp
RuleFor(x => x.Email)
    .MustAsync(BeUniqueEmailAsync)
    .WithMessage("Email address is already registered.");
```

---

*End of Document — Validation Strategy*
