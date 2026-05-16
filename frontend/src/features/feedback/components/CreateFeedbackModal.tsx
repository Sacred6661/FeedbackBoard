import { useState } from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Button,
  MenuItem,
  Alert,
  CircularProgress,
} from "@mui/material";
import { useCategories, useCreateFeedback } from "../hooks/useFeedback";

interface CreateFeedbackModalProps {
  open: boolean;
  onClose: () => void;
}

export default function CreateFeedbackModal({
  open,
  onClose,
}: CreateFeedbackModalProps) {
  const { data: categories } = useCategories();
  const createMutation = useCreateFeedback();

  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [categoryId, setCategoryId] = useState<number>(0);
  const [userId, setUserId] = useState("user-001");
  const [authorName, setAuthorName] = useState("John Doe");
  const [error, setError] = useState("");

  const handleSubmit = async () => {
    setError("");

    if (!title.trim()) {
      setError("Title is required");
      return;
    }
    if (!description.trim()) {
      setError("Description is required");
      return;
    }
    if (categoryId <= 0) {
      setError("Please select a category");
      return;
    }

    try {
      await createMutation.mutateAsync({
        title: title.trim(),
        description: description.trim(),
        categoryId,
        userId,
        authorName,
      });

      setTitle("");
      setDescription("");
      setCategoryId(0);
      onClose();
    } catch {
      setError("Failed to create feedback. Please try again.");
    }
  };

  const handleClose = () => {
    if (!createMutation.isPending) {
      setError("");
      onClose();
    }
  };

  return (
    <Dialog
      open={open}
      onClose={handleClose}
      maxWidth="sm"
      fullWidth
      disableScrollLock
      sx={{
        "& .MuiDialogContent-root": {
          overflow: "hidden", // ← Запобігає внутрішньому скролу
        },
      }}
    >
      <DialogTitle>Create New Feedback</DialogTitle>
      <DialogContent>
        {error && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {error}
          </Alert>
        )}

        <TextField
          fullWidth
          label="Title"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          margin="normal"
          placeholder="Brief summary of your idea or issue"
          disabled={createMutation.isPending}
          autoFocus
          autoComplete="off" //  Prevents browser prompts
        />

        <TextField
          fullWidth
          label="Description"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          margin="normal"
          multiline
          rows={4}
          placeholder="Describe your feedback in detail"
          disabled={createMutation.isPending}
          autoComplete="off" //  Prevents browser prompts
          sx={{
            "& .MuiInputBase-root": {
              overflow: "hidden", // ← Запобігає скролу всередині поля
            },
          }}
        />

        <TextField
          fullWidth
          select
          label="Category"
          value={categoryId}
          onChange={(e) => setCategoryId(Number(e.target.value))}
          margin="normal"
          disabled={createMutation.isPending}
          slotProps={{
            select: {
              MenuProps: {
                disableScrollLock: true,
                slotProps: {
                  paper: {
                    style: {
                      maxHeight: 300,
                    },
                  },
                },
              },
            },
          }}
        >
          <MenuItem value={0} disabled>
            Select a category
          </MenuItem>
          {categories?.map((cat) => (
            <MenuItem key={cat.id} value={cat.id}>
              {cat.icon} {cat.displayName}
            </MenuItem>
          ))}
        </TextField>

        <TextField
          fullWidth
          label="Your Name"
          value={authorName}
          onChange={(e) => setAuthorName(e.target.value)}
          margin="normal"
          disabled={createMutation.isPending}
        />

        <TextField
          fullWidth
          label="User ID"
          value={userId}
          onChange={(e) => setUserId(e.target.value)}
          margin="normal"
          helperText="Temporary — will be replaced by authentication"
          disabled={createMutation.isPending}
        />
      </DialogContent>
      <DialogActions>
        <Button onClick={handleClose} disabled={createMutation.isPending}>
          Cancel
        </Button>
        <Button
          onClick={handleSubmit}
          variant="contained"
          disabled={createMutation.isPending}
          startIcon={
            createMutation.isPending ? (
              <CircularProgress size={20} />
            ) : undefined
          }
        >
          Submit Feedback
        </Button>
      </DialogActions>
    </Dialog>
  );
}
