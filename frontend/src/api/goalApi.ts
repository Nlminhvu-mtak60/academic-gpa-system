import axiosInstance from './axiosInstance';

export interface GoalDto {
  id: string;
  targetCumulativeGpa10: number;
  targetCumulativeGpa4: number;
  notes: string | null;
  isAchieved: boolean;
  isActive: boolean;
  createdAt: string;
}

export interface RequiredGpaDto {
  targetCumulativeGpa10: number;
  currentCumulativeGpa10: number;
  creditsCompleted: number;
  creditsRemaining: number;
  requiredRemainingGpa10: number;
  feasibility: string;
  message: string;
}

export interface SimulatedCourseInput {
  courseId: string;
  attendanceScore: number;
  continuousScore: number;
  finalExamScore: number;
}

export interface SimulationResultDto {
  simulatedSemesterGpa10: number | null;
  simulatedCumulativeGpa10: number | null;
  targetVariance: number | null;
}

export interface SetGoalRequest {
  targetCumulativeGpa10: number;
  notes?: string | null;
}

export interface SimulateScenarioRequest {
  simulatedCourses: SimulatedCourseInput[];
}

export const goalApi = {
  getGoals: async () => {
    const response = await axiosInstance.get('/v1/goals');
    return response.data; // Returns { success: true, data: GoalDto[] }
  },

  setGoal: async (data: SetGoalRequest) => {
    const response = await axiosInstance.post('/v1/goals', data);
    return response.data; // Returns { success: true, data: GoalDto }
  },

  getRequiredGpa: async () => {
    const response = await axiosInstance.get('/v1/goals/required-gpa');
    return response.data; // Returns { success: true, data: RequiredGpaDto }
  },

  simulateScenario: async (data: SimulateScenarioRequest) => {
    const response = await axiosInstance.post('/v1/goals/simulate', data);
    return response.data; // Returns { success: true, data: SimulationResultDto }
  }
};
