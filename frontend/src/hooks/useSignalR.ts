import { useEffect } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { startConnection, getConnection, stopConnection } from "../api/signalR";
import { queryKeys } from "../api/queryKeys";
import type {
  FeedbackResponse,
  FeedbackStatusInfo,
} from "../api/feedbackService";

interface VoteUpdateData {
  feedbackId: string;
  voteCount: number;
  hasVoted: boolean;
}

interface StatusChangeData {
  feedbackId: string;
  status: FeedbackStatusInfo;
}

export const useSignalR = () => {
  const queryClient = useQueryClient();

  useEffect(() => {
    startConnection();
    const connection = getConnection();

    // Listening voice update
    connection.on("VoteUpdated", (data: VoteUpdateData) => {
      // Оновлюємо кеш для топ-фідбеків
      queryClient.setQueriesData<FeedbackResponse[]>(
        { queryKey: queryKeys.feedbacks.top(20) },
        (oldData) => {
          if (!oldData) return oldData;
          return oldData.map((feedback) =>
            feedback.id === data.feedbackId
              ? {
                  ...feedback,
                  voteCount: data.voteCount,
                  hasVoted: data.hasVoted,
                }
              : feedback,
          );
        },
      );

      // update cache for one feedback
      queryClient.setQueryData<FeedbackResponse>(
        queryKeys.feedbacks.byId(data.feedbackId),
        (oldData) => {
          if (!oldData) return oldData;
          return {
            ...oldData,
            voteCount: data.voteCount,
            hasVoted: data.hasVoted,
          };
        },
      );
    });

    // listening status update
    connection.on("StatusChanged", (data: StatusChangeData) => {
      // update all feedback list
      queryClient.setQueriesData<FeedbackResponse[]>(
        { queryKey: queryKeys.feedbacks.all },
        (oldData) => {
          if (!oldData) return oldData;
          return oldData.map((feedback) =>
            feedback.id === data.feedbackId
              ? { ...feedback, statusInfo: data.status }
              : feedback,
          );
        },
      );

      // update one feedback
      queryClient.setQueryData<FeedbackResponse>(
        queryKeys.feedbacks.byId(data.feedbackId),
        (oldData) => {
          if (!oldData) return oldData;
          return { ...oldData, statusInfo: data.status };
        },
      );
    });

    return () => {
      stopConnection();
    };
  }, [queryClient]);
};
