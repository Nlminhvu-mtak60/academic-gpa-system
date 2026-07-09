import axiosInstance from './axiosInstance';

export interface GpaTrendDto {
  semesterId: string;
  semesterName: string;
  yearName: string;
  gpa10: number | null;
  gpa4: number | null;
  cumulativeGpa10: number | null;
  cumulativeGpa4: number | null;
}

export interface GradeDistributionDto {
  aplus: number;
  a: number;
  bplus: number;
  b: number;
  cplus: number;
  c: number;
  dplus: number;
  d: number;
  f: number;
}

export interface CreditProgressDto {
  completedCredits: number;
  failedCredits: number;
  inProgressCredits: number;
  totalRequiredCredits: number;
  remainingCredits: number;
}

export interface StrengthWeaknessCourseDto {
  courseCode: string;
  courseName: string;
  score: number;
  letterGrade: string;
}

export interface StrengthsWeaknessesDto {
  strongestCourses: StrengthWeaknessCourseDto[];
  weakestCourses: StrengthWeaknessCourseDto[];
}

export const statisticsApi = {
  getGpaTrend: async () => {
    const response = await axiosInstance.get('/v1/statistics/gpa-trend');
    return response.data;
  },

  getGradeDistribution: async () => {
    const response = await axiosInstance.get('/v1/statistics/grade-distribution');
    return response.data;
  },

  getCreditProgress: async () => {
    const response = await axiosInstance.get('/v1/statistics/credit-progress');
    return response.data;
  },

  getStrengthsWeaknesses: async () => {
    const response = await axiosInstance.get('/v1/statistics/strengths-weaknesses');
    return response.data;
  }
};
