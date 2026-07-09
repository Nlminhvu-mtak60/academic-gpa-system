import axiosInstance from './axiosInstance';

export interface FinalScorePredictionDto {
  attendanceScore: number;
  continuousScore: number;
  targetGrade: string;
  targetScoreThreshold: number;
  requiredFinalExamScore: number;
  feasibility: string;
  advice: string;
}

export interface ScenarioPredictionDto {
  targetGrade: string;
  requiredScore: number;
  feasibility: string;
}

export interface PredictFinalScoreRequest {
  attendanceScore: number;
  continuousScore: number;
  targetGrade: string;
}

export interface GetPredictionScenariosRequest {
  attendanceScore: number;
  continuousScore: number;
}

export const predictionApi = {
  predictFinalScore: async (data: PredictFinalScoreRequest) => {
    const response = await axiosInstance.post('/v1/prediction/final-score', data);
    return response.data; // Returns { success: true, data: FinalScorePredictionDto }
  },

  getPredictionScenarios: async (data: GetPredictionScenariosRequest) => {
    const response = await axiosInstance.post('/v1/prediction/scenarios', data);
    return response.data; // Returns { success: true, data: ScenarioPredictionDto[] }
  }
};
