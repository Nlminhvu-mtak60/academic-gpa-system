import axiosInstance from './axiosInstance';

export interface SemesterDto {
  id: string;
  semesterName: string;
  sortOrder: number;
  completedCredits: number;
  semesterGpa10: number;
  semesterGpa4: number;
  isImported?: boolean;
  importedCredits?: number;
  importedGpa10?: number;
  importedGpa4?: number;
}

export interface CreateSemesterRequest {
  semesterName: string;
}

export interface UpdateSemesterRequest {
  semesterName: string;
  importedCredits?: number;
  importedGpa10?: number;
  importedGpa4?: number;
}

export const semesterApi = {
  getSemesters: async (yearId: string) => {
    const response = await axiosInstance.get(`/v1/academic-years/${yearId}/semesters`);
    return response.data;
  },

  createSemester: async (yearId: string, data: CreateSemesterRequest) => {
    const response = await axiosInstance.post(`/v1/academic-years/${yearId}/semesters`, data);
    return response.data;
  },

  importHistoricalSemester: async (
    yearId: string,
    data: {
      semesterName: string;
      importedCredits: number;
      importedGpa10: number;
      importedGpa4: number;
    }
  ) => {
    const response = await axiosInstance.post(`/v1/academic-years/${yearId}/semesters/import`, data);
    return response.data;
  },

  updateSemester: async (id: string, data: UpdateSemesterRequest) => {
    const response = await axiosInstance.put(`/v1/semesters/${id}`, data);
    return response.data;
  },

  deleteSemester: async (id: string) => {
    const response = await axiosInstance.delete(`/v1/semesters/${id}`);
    return response.data;
  },
};
