// HomePage.tsx
import { useState } from "react";
import { Box, Button, Typography } from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import { useTopFeedbacks } from "../features/feedback/hooks/useFeedback";
import FeedbackCard from "../components/FeedbackCard";
import CreateFeedbackModal from "../features/feedback/components/CreateFeedbackModal";
import FeedbackDetailModal from "../features/feedback/components/FeedbackDetailModal";
import LoadingOverlay from "../components/LoadingOverlay";

export default function HomePage() {
  const { data: feedbacks, isLoading } = useTopFeedbacks(20);
  const [createOpen, setCreateOpen] = useState(false);
  const [selectedFeedbackId, setSelectedFeedbackId] = useState<string | null>(
    null,
  );

  return (
    <Box>
      <Box
        sx={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          mb: 3,
        }}
      >
        <Typography variant="h4">Feedback</Typography>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => setCreateOpen(true)}
        >
          New Feedback
        </Button>
      </Box>

      <LoadingOverlay loading={isLoading} />

      {feedbacks?.map((feedback) => (
        <FeedbackCard
          key={feedback.id}
          feedback={feedback}
          onClick={() => setSelectedFeedbackId(feedback.id)}
        />
      ))}

      {feedbacks?.length === 0 && !isLoading && (
        <Typography color="text.secondary" sx={{ textAlign: "center", py: 4 }}>
          No feedback yet. Be the first to share your idea!
        </Typography>
      )}

      <CreateFeedbackModal
        open={createOpen}
        onClose={() => setCreateOpen(false)}
      />

      <FeedbackDetailModal
        feedbackId={selectedFeedbackId}
        onClose={() => setSelectedFeedbackId(null)}
      />
    </Box>
  );
}
