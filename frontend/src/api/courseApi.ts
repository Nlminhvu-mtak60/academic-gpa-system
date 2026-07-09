import axiosInstance from './axiosInstance';
import { ScoreDto } from './scoreApi';

export interface CourseDto {
  id: string;
  courseCode: string;
  courseName: string;
  credits: number;
  isRetake: boolean;
  originalCourseId: string | null;
  score?: ScoreDto | null;
}

export interface CreateCourseRequest {
  courseCode: string;
  courseName: string;
  credits: number;
  isRetake: boolean;
  originalCourseId?: string | null;
}

export interface UpdateCourseRequest {
  courseCode: string;
  courseName: string;
  credits: number;
  isRetake: boolean;
  originalCourseId?: string | null;
}

export const courseApi = {
  getCourses: async (semesterId: string) => {
    const response = await axiosInstance.get(`/v1/semesters/${semesterId}/courses`);
    return response.data; // Returns { success: true, data: CourseDto[] }
  },

  createCourse: async (semesterId: string, data: CreateCourseRequest) => {
    const response = await axiosInstance.post(`/v1/semesters/${semesterId}/courses`, data);
    return response.data;
  },

  updateCourse: async (id: string, data: UpdateCourseRequest) => {
    const response = await axiosInstance.put(`/v1/courses/${id}`, data);
    return response.data;
  },

  deleteCourse: async (id: string) => {
    const response = await axiosInstance.delete(`/v1/courses/${id}`);
    return response.data;
  },

  getEligibleOriginalCourses: async (courseCode: string) => {
    const response = await axiosInstance.get(`/v1/courses/eligible-originals`, {
      params: { courseCode }
    });
    return response.data; // Returns { success: true, data: CourseDto[] }
  }
};
