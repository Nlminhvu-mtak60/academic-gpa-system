import axiosInstance from './axiosInstance';

export interface ScoreDto {
  attendanceScore: number | null;
  continuousScore: number | null;
  finalExamScore: number | null;
  courseScore: number | null;
  letterGrade: string | null;
  gpa4Value: number | null;
  academicClassification?: string | null;
  isPass?: boolean | null;
  calculatedAt?: string | null;
}

export interface UpdateScoresRequest {
  attendanceScore: number | null;
  continuousScore: number | null;
  finalExamScore: number | null;
}

export interface ScoreAuditLogDto {
  fieldChanged: string;
  oldValue: string | null;
  newValue: string | null;
  changedAt: string;
}

export const scoreApi = {
  getScores: async (courseId: string) => {
    const response = await axiosInstance.get(`/v1/courses/${courseId}/scores`);
    return response.data; // Returns { success: true, data: ScoreDto }
  },

  updateScores: async (courseId: string, data: UpdateScoresRequest) => {
    const response = await axiosInstance.put(`/v1/courses/${courseId}/scores`, data);
    return response.data; // Returns { success: true, data: ScoreDto, message: string }
  },

  getScoreAuditLogs: async (courseId: string) => {
    const response = await axiosInstance.get(`/v1/courses/${courseId}/scores/audit`);
    return response.data; // Returns { success: true, data: ScoreAuditLogDto[] }
  }
};
