import { AppBar, Toolbar, Typography, Button, Box } from "@mui/material";
import FeedbackIcon from "@mui/icons-material/Feedback";

export default function Header() {
  return (
    <AppBar position="sticky">
      <Toolbar>
        <FeedbackIcon sx={{ mr: 1 }} />
        <Typography variant="h6" sx={{ flexGrow: 1 }}>
          FeedbackBoard
        </Typography>
        <Box>
          <Button color="inherit">Home</Button>
          <Button color="inherit">About</Button>
        </Box>
      </Toolbar>
    </AppBar>
  );
}
