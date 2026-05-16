import { useEffect } from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  IconButton,
  Typography,
  Box,
  Chip,
  Button,
  Divider,
  Stepper,
  Step,
  StepLabel,
  CircularProgress,
} from "@mui/material";
import CloseIcon from "@mui/icons-material/Close";
import ThumbUpIcon from "@mui/icons-material/ThumbUp";
import ThumbUpOutlinedIcon from "@mui/icons-material/ThumbUpOutlined";
import toast from "react-hot-toast";
import { useVote } from "../hooks/useFeedback";
import { getConnection } from "../../../api/signalR";
import StatusBadge from "../../../components/StatusBadge";
import type { FeedbackStatusInfo } from "../../../api/feedbackService";
import { useFeedbackById } from "../hooks/useFeedback";

interface FeedbackDetailModalProps {
  feedbackId: string | null;
  onClose: () => void;
}

export default function FeedbackDetailModal({
  feedbackId,
  onClose,
}: FeedbackDetailModalProps) {
  const { data: feedback, isLoading } = useFeedbackById(feedbackId);

  const voteMutation = useVote();

  // Слухаємо SignalR оновлення
  useEffect(() => {
    if (!feedbackId) return;

    const connection = getConnection();

    const handleVoteUpdate = (data: {
      feedbackId: string;
      voteCount: number;
      hasVoted: boolean;
    }) => {
      if (data.feedbackId === feedbackId) {
        toast.success(`Votes updated: ${data.voteCount}`);
      }
    };

    const handleStatusChange = (data: {
      feedbackId: string;
      status: FeedbackStatusInfo;
    }) => {
      if (data.feedbackId === feedbackId) {
        toast(`Status changed to "${data.status.displayName}"`, {
          icon: "🔄",
          duration: 4000,
        });
      }
    };

    connection.on("VoteUpdated", handleVoteUpdate);
    connection.on("StatusChanged", handleStatusChange);

    return () => {
      connection.off("VoteUpdated", handleVoteUpdate);
      connection.off("StatusChanged", handleStatusChange);
    };
  }, [feedbackId]);

  const handleVote = () => {
    if (!feedback) return;
    voteMutation.mutate({
      id: feedback.id,
      request: { userId: "user-001" },
    });
  };

  const statusOrder = [
    "New",
    "UnderReview",
    "Planned",
    "InProgress",
    "Completed",
  ];
  const currentStep = feedback?.statusInfo?.name
    ? statusOrder.indexOf(feedback.statusInfo.name)
    : -1;

  return (
    <Dialog open={!!feedbackId} onClose={onClose} maxWidth="md" fullWidth>
      {isLoading ? (
        <DialogContent
          sx={{ display: "flex", justifyContent: "center", py: 6 }}
        >
          <CircularProgress />
        </DialogContent>
      ) : feedback ? (
        <>
          <DialogTitle
            sx={{
              display: "flex",
              justifyContent: "space-between",
              alignItems: "center",
            }}
          >
            <Box>
              <Typography variant="h5">{feedback.title}</Typography>
              {feedback.statusInfo && (
                <Box sx={{ mt: 1 }}>
                  <StatusBadge status={feedback.statusInfo} />
                </Box>
              )}
            </Box>
            <IconButton onClick={onClose}>
              <CloseIcon />
            </IconButton>
          </DialogTitle>

          <DialogContent dividers>
            {/* Category */}
            {feedback.categoryName && (
              <Chip
                label={feedback.categoryName}
                size="small"
                variant="outlined"
                icon={<span>{feedback.categoryIcon}</span>}
                sx={{ mb: 2 }}
              />
            )}

            {/* Description */}
            <Typography variant="body1" sx={{ mb: 3, whiteSpace: "pre-wrap" }}>
              {feedback.description}
            </Typography>

            {/* AI analisys (if exists) */}
            {feedback.sentiment && (
              <Box sx={{ mb: 2, p: 2, bgcolor: "grey.50", borderRadius: 1 }}>
                <Typography
                  variant="subtitle2"
                  color="text.secondary"
                  gutterBottom
                >
                  AI Analysis
                </Typography>
                <Typography variant="body2">
                  Sentiment: <strong>{feedback.sentiment}</strong>
                </Typography>
                {feedback.suggestedCategoryName && (
                  <Typography variant="body2">
                    Suggested Category:{" "}
                    <strong>{feedback.suggestedCategoryName}</strong>
                  </Typography>
                )}
                {feedback.hasDuplicates && (
                  <Typography variant="body2" color="warning.main">
                    Possible duplicates detected
                  </Typography>
                )}
              </Box>
            )}

            {/* Voting */}
            <Box sx={{ display: "flex", alignItems: "center", mb: 3 }}>
              <Button
                variant={feedback.hasVoted ? "contained" : "outlined"}
                startIcon={
                  feedback.hasVoted ? <ThumbUpIcon /> : <ThumbUpOutlinedIcon />
                }
                onClick={handleVote}
                disabled={voteMutation.isPending}
                size="small"
              >
                {feedback.voteCount}{" "}
                {feedback.voteCount === 1 ? "Vote" : "Votes"}
              </Button>
            </Box>

            <Divider sx={{ mb: 3 }} />

            {/* Status tracking */}
            <Typography variant="subtitle2" gutterBottom>
              Status Progress
            </Typography>
            <Stepper
              activeStep={currentStep >= 0 ? currentStep : 0}
              alternativeLabel
              sx={{ mb: 3 }}
            >
              {statusOrder.map((status) => (
                <Step key={status}>
                  <StepLabel>{status}</StepLabel>
                </Step>
              ))}
            </Stepper>

            {/* Changes history */}
            <Typography variant="subtitle2" gutterBottom>
              History
            </Typography>
            {feedback.statusHistory.length > 0 ? (
              feedback.statusHistory.map((change, index) => (
                <Box
                  key={index}
                  sx={{
                    display: "flex",
                    gap: 2,
                    py: 0.5,
                    fontSize: "0.875rem",
                  }}
                >
                  <Typography
                    variant="body2"
                    color="text.secondary"
                    sx={{ minWidth: 160 }}
                  >
                    {new Date(change.timestamp).toLocaleString()}
                  </Typography>
                  <Typography variant="body2">
                    <strong>{change.oldStatus}</strong> →{" "}
                    <strong>{change.newStatus}</strong>
                    {change.reason && ` — ${change.reason}`}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    by {change.changedBy}
                  </Typography>
                </Box>
              ))
            ) : (
              <Typography variant="body2" color="text.secondary">
                No status changes yet
              </Typography>
            )}

            {/* Info */}
            <Divider sx={{ my: 2 }} />
            <Box
              sx={{
                display: "flex",
                gap: 3,
                fontSize: "0.875rem",
                color: "text.secondary",
                flexWrap: "wrap",
              }}
            >
              <Typography variant="body2">
                Author: {feedback.authorName}
              </Typography>
              <Typography variant="body2">
                Created: {new Date(feedback.createdAt).toLocaleDateString()}
              </Typography>
              {feedback.updatedAt && (
                <Typography variant="body2">
                  Updated: {new Date(feedback.updatedAt).toLocaleDateString()}
                </Typography>
              )}
              {feedback.completedAt && (
                <Typography variant="body2">
                  Completed:{" "}
                  {new Date(feedback.completedAt).toLocaleDateString()}
                </Typography>
              )}
            </Box>
          </DialogContent>
        </>
      ) : (
        <DialogContent>
          <Typography
            color="text.secondary"
            sx={{ textAlign: "center", py: 4 }}
          >
            Feedback not found
          </Typography>
        </DialogContent>
      )}
    </Dialog>
  );
}
