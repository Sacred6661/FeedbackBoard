import {
  Card,
  CardContent,
  CardActionArea,
  Typography,
  Box,
  Chip,
  keyframes,
} from "@mui/material";
import ThumbUpIcon from "@mui/icons-material/ThumbUp";
import { useState, useEffect } from "react";
import type { FeedbackResponse } from "../api/feedbackService";
import StatusBadge from "./StatusBadge";

const voteAnimation = keyframes`
  0% { transform: scale(1); }
  50% { transform: scale(1.3); color: #1976d2; }
  100% { transform: scale(1); }
`;

interface FeedbackCardProps {
  feedback: FeedbackResponse;
  onClick: () => void;
}

export default function FeedbackCard({ feedback, onClick }: FeedbackCardProps) {
  const [voteAnimating, setVoteAnimating] = useState(false);

  useEffect(() => {
    // animation when the voteCount changes
    requestAnimationFrame(() => {
      setVoteAnimating(true);
    });

    const timer = setTimeout(() => {
      setVoteAnimating(false);
    }, 600);

    return () => clearTimeout(timer);
  }, [feedback.voteCount]);

  return (
    <Card
      sx={{
        mb: 2,
        transition: "box-shadow 0.3s ease",
        "&:hover": { boxShadow: 4 },
      }}
    >
      <CardActionArea onClick={onClick}>
        <CardContent>
          <Box
            sx={{
              display: "flex",
              justifyContent: "space-between",
              alignItems: "center",
              mb: 1,
            }}
          >
            <Typography variant="h6" noWrap sx={{ flex: 1 }}>
              {feedback.title}
            </Typography>
            {feedback.statusInfo && (
              <StatusBadge status={feedback.statusInfo} />
            )}
          </Box>
          <Typography
            variant="body2"
            color="text.secondary"
            sx={{
              overflow: "hidden",
              textOverflow: "ellipsis",
              display: "-webkit-box",
              WebkitLineClamp: 2,
              WebkitBoxOrient: "vertical",
              mb: 1,
            }}
          >
            {feedback.description}
          </Typography>
          <Box sx={{ display: "flex", alignItems: "center", gap: 2 }}>
            <Box sx={{ display: "flex", alignItems: "center" }}>
              <ThumbUpIcon
                fontSize="small"
                sx={{
                  mr: 0.5,
                  animation: voteAnimating
                    ? `${voteAnimation} 0.3s ease-in-out 2`
                    : "none",
                }}
              />
              <Typography
                variant="body2"
                sx={{
                  fontWeight: voteAnimating ? 700 : 400,
                  transition: "font-weight 0.3s ease",
                }}
              >
                {feedback.voteCount}
              </Typography>
            </Box>
            {feedback.categoryName && (
              <Chip
                label={feedback.categoryName}
                size="small"
                variant="outlined"
                icon={<span>{feedback.categoryIcon}</span>}
              />
            )}
            <Typography
              variant="caption"
              color="text.secondary"
              sx={{ ml: "auto" }}
            >
              {new Date(feedback.createdAt).toLocaleDateString()}
            </Typography>
          </Box>
        </CardContent>
      </CardActionArea>
    </Card>
  );
}
