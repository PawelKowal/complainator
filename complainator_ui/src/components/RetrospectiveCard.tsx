import type { FC } from "react";
import { useNavigate } from "react-router";
import { Box, Card, CardContent, Typography, List, ListItem, ListItemText } from "@mui/material";
import type { RetrospectiveListItem } from "../dto/RetrospectiveDto";

interface RetrospectiveCardProps {
  item: RetrospectiveListItem;
}

export const RetrospectiveCard: FC<RetrospectiveCardProps> = ({ item }) => {
  const navigate = useNavigate();
  const formattedDate = new Date(item.date).toLocaleDateString("pl-PL", {
    year: "numeric",
    month: "long",
    day: "numeric",
  });

  const handleClick = () => {
    navigate(`/retrospectives/${item.id}`);
  };

  return (
    <Card
      onClick={handleClick}
      sx={{
        cursor: "pointer",
        transition: "box-shadow 0.2s",
        "&:hover": {
          boxShadow: (theme) => theme.shadows[4],
        },
      }}
    >
      <CardContent>
        <Box sx={{ display: "flex", flexDirection: "column", gap: 1 }}>
          <Typography variant="h6" component="h2">
            {item.name}
          </Typography>
          <Typography variant="subtitle2" color="text.secondary">
            {formattedDate}
          </Typography>
          {item.acceptedSuggestions?.length > 0 && (
            <List dense sx={{ mt: 1 }}>
              {item.acceptedSuggestions.map((suggestion) => (
                <ListItem key={suggestion.id}>
                  <ListItemText primary={suggestion.suggestionText} />
                </ListItem>
              ))}
            </List>
          )}
        </Box>
      </CardContent>
    </Card>
  );
};
