# Santander Hacker News API

ASP.NET Core REST API that retrieves the best stories from Hacker News and returns the top N stories ordered by score.

## Assignment

The API retrieves story IDs from the Hacker News Best Stories endpoint and fetches the corresponding story details.

The endpoint returns the requested number of stories sorted by score in descending order.

### Hacker News APIs Used

- Best Stories:
  https://hacker-news.firebaseio.com/v0/beststories.json

- Story Details:
  https://hacker-news.firebaseio.com/v0/item/{id}.json

## API Endpoint

```http
GET /api/stories/best/{n}