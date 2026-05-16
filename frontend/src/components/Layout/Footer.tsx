import { Box, Typography, Container } from "@mui/material";

export default function Footer() {
  return (
    <Box sx={{ bgcolor: "grey.100", py: 3, mt: "auto" }}>
      <Container maxWidth="md">
        <Typography variant="body2" color="text.secondary" align="center">
          © {new Date().getFullYear()} FeedbackBoard. Built with Azure & React.
        </Typography>
      </Container>
    </Box>
  );
}
