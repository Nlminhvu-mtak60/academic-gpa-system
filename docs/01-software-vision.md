# 01 — Software Vision

> **Document ID**: SRS-VISION-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review

---

## 1. Product Vision Statement

**For** university students in Vietnam  
**Who** need to accurately track, calculate, and plan their academic performance  
**The** Academic GPA Management System  
**Is a** web-based academic performance management platform  
**That** provides comprehensive GPA tracking, predictive planning, AI-powered advising, and transcript sharing  
**Unlike** basic GPA calculator apps or manual spreadsheet tracking  
**Our product** delivers a full lifecycle academic management experience — from score input to graduation planning — with institutional-grade accuracy, bilingual support, and intelligent recommendations.

---

## 2. Mission Statement

To empower Vietnamese university students with a professional, accurate, and intelligent tool that transforms academic data into actionable insights — enabling better academic decisions, improved performance, and clearer paths to graduation.

---

## 3. Strategic Objectives

### 3.1 Short-Term Objectives (0–6 months)

| ID | Objective | Measurable Target |
|----|-----------|-------------------|
| SO-01 | Deliver core GPA tracking functionality | 100% accuracy against Vietnamese grading standard |
| SO-02 | Achieve initial user adoption | 500 registered students within 3 months of launch |
| SO-03 | Maintain system reliability | 99.5% uptime from day one |
| SO-04 | Deliver bilingual experience | 100% UI coverage in both Vietnamese and English |

### 3.2 Mid-Term Objectives (6–12 months)

| ID | Objective | Measurable Target |
|----|-----------|-------------------|
| MO-01 | Establish AI advisory as a differentiator | 70% of active users engage with AI advisor at least once |
| MO-02 | Enable transcript sharing for job applications | 1,000+ shared transcript links generated |
| MO-03 | Expand admin capabilities | Admin panel used by 10+ university staff members |
| MO-04 | Achieve high user satisfaction | Net Promoter Score (NPS) ≥ 40 |

### 3.3 Long-Term Objectives (12–24 months)

| ID | Objective | Measurable Target |
|----|-----------|-------------------|
| LO-01 | Become the go-to academic tool for Vietnamese students | 10,000+ registered users across multiple universities |
| LO-02 | Support multiple grading systems | 3+ country/university grading standards supported |
| LO-03 | Enable institutional partnerships | 5+ university integrations (official data sync) |
| LO-04 | Build a sustainable platform | Revenue model (freemium or institutional licensing) |

---

## 4. Problem Statement

### 4.1 Current Pain Points

Vietnamese university students currently face these challenges:

| Pain Point | Impact | Severity |
|------------|--------|----------|
| **Manual GPA Calculation** | Students use spreadsheets or handheld calculators, leading to errors | 🔴 High |
| **Complex Grading Rules** | The Vietnamese 10-point system with component weights and rounding rules is confusing | 🔴 High |
| **No Predictive Tools** | Students cannot easily determine "What final exam score do I need?" | 🟡 Medium |
| **Fragmented Tracking** | Academic data spread across notebooks, Excel files, and university portals | 🔴 High |
| **No Personalized Guidance** | Limited access to academic advisors, especially outside office hours | 🟡 Medium |
| **No Shareable Transcript** | Students have no easy way to share academic records with employers | 🟡 Medium |
| **Language Barriers** | International students or English-focused programs lack Vietnamese tool alternatives | 🟢 Low |

### 4.2 Current Alternatives

| Alternative | Weakness |
|-------------|----------|
| University portals | Read-only, no planning features, poor UX |
| Spreadsheets (Excel/Google Sheets) | Error-prone, no standardization, no AI |
| Mobile GPA calculator apps | Too simple, no persistence, no multi-semester support |
| Manual calculation | Slow, error-prone, no trend analysis |

---

## 5. Target Audience

### 5.1 Primary Persona — "Minh" (Student)

| Attribute | Details |
|-----------|---------|
| **Name** | Nguyễn Văn Minh |
| **Age** | 20 |
| **Role** | 3rd-year Computer Science student |
| **University** | University of Information Technology (UIT) |
| **Tech Comfort** | High — daily smartphone and laptop user |
| **Pain Point** | Struggles to track GPA across 6 semesters, unsure if cumulative GPA will qualify for honors |
| **Goal** | Know exactly what grades he needs this semester to graduate with "Giỏi" (Good) classification |
| **Behavior** | Checks grades after each exam period, uses Google Sheets currently |

### 5.2 Secondary Persona — "Thảo" (Admin)

| Attribute | Details |
|-----------|---------|
| **Name** | Trần Thị Thảo |
| **Age** | 35 |
| **Role** | Academic affairs staff at a small private university |
| **Tech Comfort** | Moderate — uses email, Word, basic web apps |
| **Pain Point** | Needs to monitor student performance trends and send academic warnings |
| **Goal** | Quickly identify at-risk students and communicate with them |
| **Behavior** | Reviews student data weekly, sends batch notifications |

---

## 6. Value Proposition Canvas

### 6.1 Student Value Map

```
╔═══════════════════════════════════════════════════════════╗
║                   VALUE PROPOSITION                       ║
╠═══════════════════════════════════════════════════════════╣
║                                                           ║
║  GAINS CREATED                 PAINS RELIEVED             ║
║  ─────────────                 ───────────────             ║
║  • Accurate GPA at a glance    • No more manual errors    ║
║  • Know exactly what you need  • No more spreadsheets     ║
║  • AI study guidance 24/7      • No rounding confusion    ║
║  • Shareable transcript        • No fragmented data       ║
║  • Visual progress tracking    • No guessing on finals    ║
║  • Goal-based motivation       • No language barriers     ║
║                                                           ║
║  PRODUCTS & SERVICES                                      ║
║  ──────────────────                                       ║
║  • GPA Calculator (10 & 4 scale)                          ║
║  • Score Component Manager                                ║
║  • Semester/Year Organizer                                ║
║  • Goal Planner & What-If Simulator                       ║
║  • Final Exam Score Predictor                             ║
║  • AI Academic Advisor (Chat)                             ║
║  • Shareable Transcript Generator                         ║
║  • Performance Analytics Dashboard                        ║
║                                                           ║
╚═══════════════════════════════════════════════════════════╝
```

---

## 7. Success Metrics (KPIs)

### 7.1 Product Metrics

| Metric | Definition | Target (6 months) | Target (12 months) |
|--------|-----------|-------------------|---------------------|
| **MAU** | Monthly Active Users | 300 | 2,000 |
| **DAU/MAU Ratio** | Daily engagement ratio | ≥ 20% | ≥ 30% |
| **Courses Tracked** | Total courses entered across all users | 5,000 | 50,000 |
| **AI Sessions** | Monthly AI advisor conversations | 200 | 3,000 |
| **Transcript Shares** | Monthly shared transcript links | 50 | 500 |

### 7.2 Technical Metrics

| Metric | Definition | Target |
|--------|-----------|--------|
| **Uptime** | System availability | ≥ 99.5% |
| **API Latency (p95)** | 95th percentile response time | ≤ 200ms |
| **Error Rate** | Server-side errors / total requests | ≤ 0.1% |
| **Page Load Time** | Time to interactive on 4G | ≤ 2.5s |
| **Test Coverage** | Business logic code coverage | ≥ 80% |

### 7.3 Business Metrics

| Metric | Definition | Target |
|--------|-----------|--------|
| **NPS** | Net Promoter Score | ≥ 40 |
| **CSAT** | Customer Satisfaction (survey) | ≥ 4.2/5.0 |
| **Churn Rate** | Monthly user churn | ≤ 5% |
| **Support Tickets** | Tickets per 100 users per month | ≤ 3 |

---

## 8. Competitive Landscape

| Competitor | Type | Strengths | Weaknesses | Our Advantage |
|------------|------|-----------|------------|---------------|
| University Portals | Institutional | Official data | Read-only, no planning | Full CRUD + planning |
| GPA Calculator Apps | Mobile | Quick, simple | No persistence, basic | Multi-semester, persistent |
| Google Sheets | Spreadsheet | Flexible | Manual, error-prone | Automated, standardized |
| MyStudyLife | App | Timetable + tasks | No Vietnamese grading | Localized grading rules |
| Scholaro | Web | Multi-country GPA | Calculator only, no tracking | Full lifecycle management |

---

## 9. Scope Boundaries

### 9.1 In Scope (Version 1.0)

- Student registration and authentication (email + Google)
- Profile management with preferences
- Academic year and semester management
- Course CRUD with component score input
- Automated GPA calculation (10-scale and 4-scale)
- Semester, academic year, and cumulative GPA
- Statistics and performance analytics
- Goal planner with what-if simulator
- Final exam score predictor
- AI academic advisor (chat-based)
- In-app notification system
- Transcript sharing via link
- Admin panel (student management, notifications, statistics)
- Dark/Light theme
- Vietnamese/English interface
- Responsive web design

### 9.2 Explicitly Out of Scope (Version 1.0)

| Feature | Reason | Planned For |
|---------|--------|-------------|
| Mobile native app (iOS/Android) | Web-first strategy | v2.0+ |
| University API integration | Requires institutional partnerships | v2.0+ |
| Course catalog/database | Each university differs | v2.0+ |
| Timetable/scheduling | Not core to GPA management | v3.0+ |
| Social features (study groups) | Complexity vs. value | v3.0+ |
| Payment/subscription | Free for v1.0 | v2.0+ |
| PDF transcript with university branding | Requires institutional sign-off | v1.5 |
| Offline mode | Web-first | v2.0+ |
| Multi-university admin | Single-tenant for v1.0 | v2.0+ |

---

## 10. Stakeholders

| Stakeholder | Role | Interest | Influence |
|-------------|------|----------|-----------|
| Product Owner | Decision maker | Feature prioritization | 🔴 High |
| Development Team | Builders | Technical feasibility | 🔴 High |
| Students (End Users) | Primary users | Usability, accuracy | 🔴 High |
| University Staff (Admins) | Secondary users | Monitoring, efficiency | 🟡 Medium |
| University Management | Potential sponsors | ROI, adoption | 🟡 Medium |
| QA Team | Quality assurance | Test coverage, defects | 🟡 Medium |

---

*End of Document — Software Vision*
