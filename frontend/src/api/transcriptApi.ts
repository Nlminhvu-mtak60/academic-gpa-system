import axiosInstance from './axiosInstance';
import { TranscriptImportPreviewResult } from '../components/TranscriptImportModal';

export const transcriptApi = {
  parseTranscript: async (
    semesterId: string,
    file: File,
    sourceType: string
  ): Promise<{ success: boolean; message?: string; data?: TranscriptImportPreviewResult }> => {
    try {
      const formData = new FormData();
      formData.append('file', file);
      formData.append('sourceType', sourceType);

      const response = await axiosInstance.post(
        `/v1/semesters/${semesterId}/transcript/parse`,
        formData,
        {
          headers: {
            'Content-Type': 'multipart/form-data',
          },
        }
      );
      return response.data;
    } catch (error: any) {
      console.error('Error parsing transcript:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Failed to parse transcript',
      };
    }
  },

  confirmImport: async (
    semesterId: string,
    payload: { sourceType: string; courses: any[] }
  ): Promise<{ success: boolean; message?: string; data?: { batchId: string } }> => {
    try {
      const response = await axiosInstance.post(
        `/v1/semesters/${semesterId}/transcript/confirm`,
        payload
      );
      return response.data;
    } catch (error: any) {
      console.error('Error confirming import:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Failed to confirm import',
      };
    }
  },
};
