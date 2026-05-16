import { Backdrop, CircularProgress } from "@mui/material";

interface LoadingOverlayProps {
  loading: boolean;
}

export default function LoadingOverlay({ loading }: LoadingOverlayProps) {
  return (
    <Backdrop
      sx={{
        color: "#fff",
        zIndex: (theme) => theme.zIndex.drawer + 10,
        backgroundColor: "rgba(0, 0, 0, 0.3)",
        backdropFilter: "blur(2px)",
      }}
      open={loading}
    >
      <CircularProgress color="inherit" />
    </Backdrop>
  );
}
