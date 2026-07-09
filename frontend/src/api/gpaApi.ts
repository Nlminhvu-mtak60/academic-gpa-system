import axiosInstance from './axiosInstance';

export interface SemesterGpaDto {
  semesterId: string;
  gpa10: number | null;
  gpa4: number | null;
  totalCredits: number;
  passedCredits: number;
  failedCredits: number;
}

export interface AcademicYearGpaDto {
  academicYearId: string;
  gpa10: number | null;
  gpa4: number | null;
  totalCredits: number;
  passedCredits: number;
  failedCredits: number;
}

export interface CumulativeGpaDto {
  cumulativeGpa10: number | null;
  cumulativeGpa4: number | null;
  totalCreditsCompleted: number;
  totalCreditsRequired: number;
  completionPercentage: number;
}

export interface GpaClassificationDto {
  cumulativeGpa10: number | null;
  classificationEn: string;
  classificationVn: string;
  minimumThresholdGpa10: number;
}

export const gpaApi = {
  getSemesterGpa: async (semesterId: string) => {
    const response = await axiosInstance.get(`/v1/gpa/semester/${semesterId}`);
    return response.data; // Returns { success: true, data: SemesterGpaDto }
  },

  getAcademicYearGpa: async (yearId: string) => {
    const response = await axiosInstance.get(`/v1/gpa/academic-year/${yearId}`);
    return response.data; // Returns { success: true, data: AcademicYearGpaDto }
  },

  getCumulativeGpa: async () => {
    const response = await axiosInstance.get(`/v1/gpa/cumulative`);
    return response.data; // Returns { success: true, data: CumulativeGpaDto }
  },

  getGpaClassification: async () => {
    const response = await axiosInstance.get(`/v1/gpa/classification`);
    return response.data; // Returns { success: true, data: GpaClassificationDto }
  }
};
