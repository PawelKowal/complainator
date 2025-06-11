import type { FC } from "react";
import { Box, Card, CardContent, Skeleton, Stack } from "@mui/material";

export const LoadingSkeleton: FC = () => {
  return (
    <Stack spacing={2}>
      {[1, 2, 3].map((index) => (
        <Card key={index}>
          <CardContent>
            <Box sx={{ display: "flex", flexDirection: "column", gap: 1 }}>
              <Skeleton variant="text" width="60%" height={32} />
              <Skeleton variant="text" width="30%" height={24} />
              <Box sx={{ mt: 2 }}>
                <Skeleton variant="text" width="100%" height={24} />
                <Skeleton variant="text" width="90%" height={24} />
                <Skeleton variant="text" width="80%" height={24} />
              </Box>
            </Box>
          </CardContent>
        </Card>
      ))}
    </Stack>
  );
};
