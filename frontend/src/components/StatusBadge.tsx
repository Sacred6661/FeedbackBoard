import { Chip, keyframes } from "@mui/material";
import type { FeedbackStatusInfo } from "../api/feedbackService";
import { useState, useEffect } from "react";

const pulseAnimation = keyframes`
  0% { transform: scale(1); }
  50% { transform: scale(1.1); }
  100% { transform: scale(1); }
`;

interface StatusBadgeProps {
  status: FeedbackStatusInfo;
}

export default function StatusBadge({ status }: StatusBadgeProps) {
  const [animating, setAnimating] = useState(false);
  const [currentStatus, setCurrentStatus] = useState(status);

  useEffect(() => {
    if (status.id !== currentStatus.id) {
      // use requestAnimationFrame for smooth updates
      requestAnimationFrame(() => {
        setAnimating(true);
        setCurrentStatus(status);
      });

      const timer = setTimeout(() => {
        setAnimating(false);
      }, 600);

      return () => clearTimeout(timer);
    }
  }, [status, currentStatus.id]);

  return (
    <Chip
      label={currentStatus.displayName}
      size="small"
      sx={{
        backgroundColor: currentStatus.color,
        color: "#fff",
        fontWeight: 600,
        animation: animating ? `${pulseAnimation} 0.3s ease-in-out 2` : "none",
        transition: "background-color 0.3s ease",
      }}
    />
  );
}
