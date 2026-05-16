import axiosClient from "./axiosClient";

export const FeedbackStatus = {
  New: 1,
  UnderReview: 2,
  Planned: 3,
  InProgress: 4,
  Completed: 5,
  Declined: 6,
  Duplicate: 7,
} as const;

export type FeedbackStatus =
  (typeof FeedbackStatus)[keyof typeof FeedbackStatus];

export interface FeedbackStatusInfo {
  id: number;
  name: string;
  displayName: string;
  color: string;
}

export interface StatusChangeResponse {
  oldStatus: string;
  newStatus: string;
  changedBy: string;
  reason?: string;
  timestamp: string;
}

export interface FeedbackResponse {
  id: string;
  title: string;
  description: string;
  categoryId: number;
  categoryName?: string;
  categoryIcon?: string;
  statusInfo?: FeedbackStatusInfo;
  statusHistory: StatusChangeResponse[];
  sentiment?: string;
  suggestedCategoryName?: string;
  hasDuplicates: boolean;
  authorName: string;
  voteCount: number;
  hasVoted: boolean;
  createdAt: string;
  updatedAt?: string;
  completedAt?: string;
  attachmentUrls: string[];
}

export interface CreateFeedbackRequest {
  title: string;
  description: string;
  categoryId: number;
  userId: string;
  authorName: string;
}

export interface ChangeStatusRequest {
  newStatus: FeedbackStatus;
  changedBy: string;
  reason?: string;
}

export interface VoteRequest {
  userId: string;
}

export interface CategoryInfo {
  id: number;
  name: string;
  displayName: string;
  icon: string;
  description: string;
}

export const feedbackService = {
  createFeedback: async (
    data: CreateFeedbackRequest,
  ): Promise<FeedbackResponse> => {
    const response = await axiosClient.post<FeedbackResponse>(
      "/feedback",
      data,
    );
    return response.data;
  },

  getFeedbackById: async (id: string): Promise<FeedbackResponse> => {
    const response = await axiosClient.get<FeedbackResponse>(`/feedback/${id}`);
    return response.data;
  },

  getFeedbacksByCategory: async (
    categoryId: string,
  ): Promise<FeedbackResponse[]> => {
    const response = await axiosClient.get<FeedbackResponse[]>(
      `/feedback/category/${categoryId}`,
    );
    return response.data;
  },

  getFeedbacksByStatus: async (status: number): Promise<FeedbackResponse[]> => {
    const response = await axiosClient.get<FeedbackResponse[]>(
      `/feedback/status/${status}`,
    );
    return response.data;
  },

  getTopFeedbacks: async (count = 10): Promise<FeedbackResponse[]> => {
    const response = await axiosClient.get<FeedbackResponse[]>(
      `/feedback/top?count=${count}`,
    );
    return response.data;
  },

  changeFeedbackStatus: async (
    id: string,
    data: ChangeStatusRequest,
  ): Promise<FeedbackResponse> => {
    const response = await axiosClient.post<FeedbackResponse>(
      `/feedback/${id}/status`,
      data,
    );
    return response.data;
  },

  vote: async (id: string, data: VoteRequest): Promise<FeedbackResponse> => {
    const response = await axiosClient.post<FeedbackResponse>(
      `/feedback/${id}/vote`,
      data,
    );
    return response.data;
  },

  getStatuses: async (): Promise<FeedbackStatusInfo[]> => {
    const response =
      await axiosClient.get<FeedbackStatusInfo[]>(`/feedback/statuses`);
    return response.data;
  },

  getCategories: async (): Promise<CategoryInfo[]> => {
    const response =
      await axiosClient.get<CategoryInfo[]>(`/feedback/categories`);
    return response.data;
  },
};
