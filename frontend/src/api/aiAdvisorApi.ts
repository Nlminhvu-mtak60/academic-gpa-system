import axiosInstance from './axiosInstance';

export interface ConversationDto {
  id: string;
  title: string;
  createdAt: string;
  updatedAt: string;
}

export interface ConversationMessageDto {
  role: 'user' | 'assistant';
  content: string;
  createdAt: string;
}

export interface PaginationDto {
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

export interface ConversationListResponse {
  success: boolean;
  data: ConversationDto[];
  pagination: PaginationDto;
}

export interface SendMessageResponse {
  success: boolean;
  data: {
    reply: string;
    createdAt: string;
  };
}

export const aiAdvisorApi = {
  getConversations: async (page = 1, pageSize = 20): Promise<ConversationListResponse> => {
    const response = await axiosInstance.get('/v1/ai/conversations', {
      params: { page, pageSize },
    });
    return response.data;
  },

  startConversation: async (title: string): Promise<{ success: boolean; data: ConversationDto }> => {
    const response = await axiosInstance.post('/v1/ai/conversations', { title });
    return response.data;
  },

  deleteConversation: async (id: string): Promise<void> => {
    await axiosInstance.delete(`/v1/ai/conversations/${id}`);
  },

  getMessages: async (id: string): Promise<{ success: boolean; data: ConversationMessageDto[] }> => {
    const response = await axiosInstance.get(`/v1/ai/conversations/${id}/messages`);
    return response.data;
  },

  sendMessage: async (id: string, message: string): Promise<SendMessageResponse> => {
    const response = await axiosInstance.post(`/v1/ai/conversations/${id}/messages`, { message });
    return response.data;
  },
};
export default aiAdvisorApi;
