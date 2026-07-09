import axiosInstance from './axiosInstance';

export interface DashboardStudentDto {
  firstName: string;
  lastName: string;
  studentCode: string;
}

export interface DashboardPerformanceDto {
  currentSemesterGpa10: number | null;
  currentSemesterGpa4: number | null;
  currentAcademicYearGpa10: number | null;
  currentAcademicYearGpa4: number | null;
  cumulativeGpa10: number | null;
  cumulativeGpa4: number | null;
  totalCredits: number;
  passedCredits: number;
  failedCredits: number;
  currentAcademicYearName: string | null;
  currentSemesterName: string | null;
  classificationVn: string;
  totalCreditsCompleted: number;
  totalCreditsRequired: number;
}

export interface DashboardGoalProgressDto {
  targetGpa10: number | null;
  targetGpa4: number | null;
  isAchieved: boolean;
  requiredRemainingGpa: number | null;
}

export interface DashboardRecentCourseDto {
  id: string;
  courseCode: string;
  courseName: string;
  credits: number;
  courseScore: number | null;
  letterGrade: string | null;
}

export interface DashboardSummaryDto {
  student: DashboardStudentDto;
  performanceSummary: DashboardPerformanceDto;
  goalProgress: DashboardGoalProgressDto;
  recentCourses: DashboardRecentCourseDto[];
  unreadNotificationsCount: number;
}

export const dashboardApi = {
  getSummary: async () => {
    const response = await axiosInstance.get('/v1/dashboard/summary');
    return response.data;
  }
};
