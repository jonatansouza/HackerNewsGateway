# HackerNews Gateway API

ASP.NET Core REST API that returns the best N stories from the [Hacker News API](https://github.com/HackerNews/API), sorted by score descending.

## How to Run

**Requirements:** .NET 10 SDK

```bash
cd HackerNewsGatewayApi
dotnet run
```

The API will be available at `https://localhost:7xxx` or `http://localhost:5xxx` (see console output).

## Endpoint

```
GET /stories/{n}
```

Returns the top `n` stories sorted by score descending.

**Example:**
```bash
curl https://localhost:7222/stories/5
```

**Response:**
```json
[
  {
    "title": "A uBlock Origin update was rejected from the Chrome Web Store",
    "uri": "https://github.com/uBlockOrigin/uBlock-issues/issues/745",
    "postedBy": "ismaildonmez",
    "time": "2019-10-12T13:43:01+00:00",
    "score": 1716,
    "commentCount": 572
  }
]
```

**Status codes:**
| Code | Reason |
|---|---|
| 200 | Success |
| 400 | `n` is zero or negative |
| 503 | Cache still warming up on cold start (retry after 10s) |

## Architecture

```
StorySyncWorker (BackgroundService)
  └── every 5 min: fetches 500 IDs + all story details in parallel
  └── sorts by score, writes to StoryCache

StoryCache (Singleton)
  └── ImmutableList<Story> — lock-free reads via Interlocked + Volatile

StoriesController
  └── GET /stories/{n} → cache.Take(n)  — zero I/O on request path
```

Key techniques:
- `Task.WhenAll` + `SemaphoreSlim(20)` for controlled parallel HTTP fetching
- `Interlocked.Exchange` + `Volatile.Read` for lock-free thread safety
- Pre-sorted cache — response is O(1) regardless of N

## Configuration

`appsettings.json`:
```json
{
  "HackerNews": {
    "SyncIntervalMinutes": 5
  }
}
```

## Assumptions

- The best stories list (up to 500) is small enough to fit entirely in memory (~500KB).
- Scores change slowly enough that a 5-minute refresh window is acceptable.
- Stories with missing fields (deleted/dead items) are silently skipped.
- No authentication is required on the gateway endpoint.

## Trade-offs & Known Limitations

**In-process background worker**
The sync worker runs inside the same process as the API. In a production Kubernetes environment, these would be separate workloads — a dedicated Deployment (or CronJob) for the worker writing to a shared distributed cache (Redis), and stateless API pods reading from it. The current in-memory approach is intentional for simplicity and to avoid infrastructure dependencies in this challenge.

**No retry policy**
If the Hacker News API is temporarily unavailable, the worker logs the error and continues serving the last successful cache. Adding a retry policy (e.g., Polly) would improve resilience but was omitted to keep the solution focused.

**Single instance only**
The in-memory cache is not shared across multiple instances. Horizontal scaling requires migrating to a distributed cache.

## Enhancements Given More Time

- Distributed cache (Redis) to support horizontal scaling
- Polly retry + circuit breaker on `HackerNewsClient`
- Background cache pre-warming on startup before accepting traffic
- Rate limiting on the gateway endpoint
- Structured logging with OpenTelemetry
- Unit and integration tests
