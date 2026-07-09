import axiosInstance from './axiosInstance';

export interface AcademicYearDto {
  id: string;
  yearName: string;
  startYear: number;
  endYear: number;
  status: string;
  isCurrent: boolean;
  completedCredits: number;
  yearGpa10: number;
  yearGpa4: number;
  isImported?: boolean;
  importedCredits?: number;
  importedGpa10?: number;
  importedGpa4?: number;
}

export interface CreateAcademicYearRequest {
  yearName: string;
  startYear: number;
  endYear: number;
}

export interface UpdateAcademicYearRequest {
  yearName: string;
  startYear: number;
  endYear: number;
  startDate?: string;
  endDate?: string;
}

export const academicYearApi = {
  getAcademicYears: async () => {
    const response = await axiosInstance.get('/v1/academic-years');
    return response.data; // Wraps { success: boolean, data: AcademicYearDto[], errors: any, message: string }
  },

  createAcademicYear: async (data: CreateAcademicYearRequest) => {
    const response = await axiosInstance.post('/v1/academic-years', data);
    return response.data;
  },

  importHistoricalYear: async (data: {
    yearName: string;
    startYear: number;
    endYear: number;
    importedCredits: number;
    importedGpa10: number;
    importedGpa4: number;
  }) => {
    const response = await axiosInstance.post('/v1/academic-years/import', data);
    return response.data;
  },

  updateAcademicYear: async (id: string, data: UpdateAcademicYearRequest) => {
    const response = await axiosInstance.put(`/v1/academic-years/${id}`, data);
    return response.data;
  },

  deleteAcademicYear: async (id: string) => {
    const response = await axiosInstance.delete(`/v1/academic-years/${id}`);
    return response.data;
  },

  setCurrentAcademicYear: async (id: string) => {
    const response = await axiosInstance.put(`/v1/academic-years/${id}/current`);
    return response.data;
  },
};
