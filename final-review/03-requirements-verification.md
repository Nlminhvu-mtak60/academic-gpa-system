# Requirements Verification — Business Logic Mapping

This document verifies the exact implementation locations of core academic rules and validation constraints within the codebase.

---

## 1. Course Score Calculation

### Business Rule
$$\text{Course Score} = (\text{Attendance} \times 0.1) + (\text{Continuous} \times 0.3) + (\text{Final} \times 0.6)$$
- Grade components are first rounded to the nearest `0.5` increment.
- The resulting Course Score is rounded to exactly one decimal place.

### Verification Matrix
- **Entity Definition**: [Score.cs](file:///d:/aiiii/backend/src/AcademicGPA.Domain/Entities/Score.cs#L36) defines structural elements, properties, and validation checks.
- **Calculator Logic**: [GpaCalculator.cs](file:///d:/aiiii/backend/src/AcademicGPA.Infrastructure/Services/GpaCalculator.cs#L26) executes the component rounding and calculates the weighted course grade.
- **Unit Tests**: [ScoreTests.cs](file:///d:/aiiii/backend/tests/AcademicGPA.UnitTests/ScoreTests.cs#L15) and [GpaCalculatorExtendedTests.cs](file:///d:/aiiii/backend/tests/AcademicGPA.UnitTests/GpaCalculatorExtendedTests.cs#L56) verify weighted values and rounding behaviors.

---

## 2. Grade Conversions

### Business Rule
10-Point Course Scores map to standard Vietnamese Letter Grades and 4.0 Scale GPAs:
- $[9.0 - 10.0]$: `A+` / `4.0`
- $[8.5 - 8.9]$: `A` / `3.8`
- $[8.0 - 8.4]$: `B+` / `3.5`
- $[7.0 - 7.9]$: `B` / `3.0`
- $[6.5 - 6.9]$: `C+` / `2.5`
- $[5.5 - 6.4]$: `C` / `2.0`
- $[5.0 - 5.4]$: `D+` / `1.5`
- $[4.0 - 4.9]$: `D` / `1.0`
- $< 4.0$: `F` / `0.0`

### Verification Matrix
- **Conversion Map**: [GpaCalculator.cs](file:///d:/aiiii/backend/src/AcademicGPA.Infrastructure/Services/GpaCalculator.cs) contains static conversion definitions.
- **Unit Tests**: [GpaCalculatorExtendedTests.cs](file:///d:/aiiii/backend/tests/AcademicGPA.UnitTests/GpaCalculatorExtendedTests.cs) tests bounds checking and scale conversions.

---

## 3. Retake Logic

### Business Rule
When a student retakes a course:
- Both attempts are preserved in the historical grade log.
- Only the attempt with the highest score is counted towards the cumulative GPA.

### Verification Matrix
- **Filtering Logic**: [GpaCalculator.cs](file:///d:/aiiii/backend/src/AcademicGPA.Infrastructure/Services/GpaCalculator.cs) implements key filtering logic that groups grades by subject and selects the maximum score.
- **Integration Tests**: [GpaCalculationIntegrationTests.cs](file:///d:/aiiii/backend/tests/AcademicGPA.UnitTests/GpaCalculationIntegrationTests.cs) simulates retake inputs and confirms correct GPA recalculation.

---

## 4. Final Exam Prediction Logic

### Business Rule
When a student wants to achieve a certain overall course score target (e.g. 8.0, 9.0, or custom):
$$\text{Required Final} = \frac{\text{Target Score} - (\text{Attendance} \times 0.1) - (\text{Continuous} \times 0.3)}{0.6}$$
- Rounded to the nearest `0.5` increment.
- Checked for feasibility ($0 \le \text{Required Final} \le 10.0$).

### Verification Matrix
- **Calculation Logic**: [PredictionService.cs](file:///d:/aiiii/backend/src/AcademicGPA.Infrastructure/Services/PredictionService.cs#L87) calculates required grades and outputs feasibility status.
- **Unit Tests**: [PredictionServiceTests.cs](file:///d:/aiiii/backend/tests/AcademicGPA.UnitTests/PredictionServiceTests.cs) validates targets and edge cases.
