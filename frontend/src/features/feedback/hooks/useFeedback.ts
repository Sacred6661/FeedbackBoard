import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import toast from "react-hot-toast";
import { feedbackService } from "../../../api/feedbackService";
import { queryKeys } from "../../../api/queryKeys";
import type {
  CreateFeedbackRequest,
  ChangeStatusRequest,
  VoteRequest,
} from "../../../api/feedbackService";

export const useTopFeedbacks = (count = 10) => {
  return useQuery({
    queryKey: queryKeys.feedbacks.top(count),
    queryFn: () => feedbackService.getTopFeedbacks(count),
  });
};

export const useFeedbackById = (feedbackId?: string | null) => {
  return useQuery({
    queryKey: ["feedback", feedbackId],
    queryFn: () => feedbackService.getFeedbackById(feedbackId as string),
    enabled: !!feedbackId,
  });
};

export const useCategories = () => {
  return useQuery({
    queryKey: queryKeys.metadata.categories,
    queryFn: feedbackService.getCategories,
    staleTime: 1000 * 60 * 60,
  });
};

export const useCreateFeedback = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CreateFeedbackRequest) =>
      feedbackService.createFeedback(request),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.feedbacks.all });
      toast.success(`"${data.title}" created successfully!`);
    },
    onError: () => {
      toast.error("Failed to create feedback. Please try again.");
    },
  });
};

export const useVote = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, request }: { id: string; request: VoteRequest }) =>
      feedbackService.vote(id, request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.feedbacks.all });
      toast.success("Vote recorded!");
    },
    onError: () => {
      toast.error("Failed to vote. Please try again.");
    },
  });
};

export const useChangeStatus = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      id,
      request,
    }: {
      id: string;
      request: ChangeStatusRequest;
    }) => feedbackService.changeFeedbackStatus(id, request),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.feedbacks.all });
      toast.success(`Status changed to "${data.statusInfo?.displayName}"`);
    },
    onError: () => {
      toast.error("Failed to change status.");
    },
  });
};
