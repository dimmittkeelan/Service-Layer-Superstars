# Library Book Borrowing System — Design Document

**Team Name:** Service Layer Superstars  
**Course:** Cal State Fullerton — Backend Engineering (CPSC 449-03)  
**Team Members:** Keelan Dimmitt, Owin Rojas, Varnika Pullareddy

---

## 1. System Overview

The Library Book Borrowing System is a backend REST API built with ASP.NET Core (.NET 8). It allows a library to manage its book catalog, member registrations, and the full borrow/return lifecycle. The system is designed with correctness, scalability, and clean architecture as primary goals.

The system exposes three core resources — **Books**, **Members**, and **BorrowRecords** — through a set of RESTful endpoints. There is no UI or authentication layer; this is a pure backend service intended to be consumed by a frontend or another service.

Key engineering concerns addressed in this system:
- Strict business rule enforcement (e.g., copy availability, borrow limits)
- Safe concurrent access to shared resources (available copy counts)
- In-memory caching for read-heavy endpoints
- Consistent, client-friendly error responses
- Fully asynchronous I/O throughout

---

## 2. API Design

The API follows REST principles: nouns for resource names, proper HTTP verbs, and standard HTTP status codes. All request and response bodies are JSON.

### Base URL
```
/api
```

### Books
| Method | Route | Description |
|--------|-------|-------------|
| GET | /api/books | Get all books (supports search & pagination) |
| GET | /api/books/{id} | Get a single book by ID |
| POST | /api/books | Create a new book |
| PUT | /api/books/{id} | Update an existing book |
| DELETE | /api/books/{id} | Delete a book |

### Members
| Method | Route | Description |
|--------|-------|-------------|
| GET | /api/members | Get all members |
| GET | /api/members/{id} | Get a single member by ID |
| POST | /api/members | Create a new member |
| PUT | /api/members/{id} | Update an existing member |
| DELETE | /api/members/{id} | Delete a member |

### Borrow Records
| Method | Route | Description |
|--------|-------|-------------|
| POST | /api/borrowrecords/borrow | Borrow a book |
| POST | /api/borrowrecords/return | Return a book |
| GET | /api/borrowrecords | View all borrow records |
| GET | /api/borrowrecords/member/{memberId} | View borrow history for a member |

---

## 3. Architecture Explanation

The project is structured in three distinct layers, following the **Controller → Service → Repository** pattern.

```
HTTP Request
    │
    ▼
┌─────────────────────────────┐
│      Controller Layer        │  Handles routing, HTTP binding, status codes
│  BookController              │  No business logic lives here
│  MemberController            │
│  BorrowRecordController      │
└──────────────┬──────────────┘
               │ calls
               ▼
┌─────────────────────────────┐
│       Service Layer          │  Enforces all business rules
│  BookService                 │  Validates copy counts, borrow limits,
│  MemberService               │  handles caching, logging, concurrency
│  BorrowRecordService         │
└──────────────┬──────────────┘
               │ calls
               ▼
┌─────────────────────────────┐
│      Repository Layer        │  Data access only
│  BookRepository              │  Wraps EF Core operations
│  MemberRepository            │
│  BorrowRecordRepository      │
└──────────────┬──────────────┘
               │ calls
               ▼
┌─────────────────────────────┐
│    ApplicationDbContext      │  EF Core In-Memory Database
└─────────────────────────────┘
```

This separation ensures that each layer has a single responsibility. Controllers never touch the database directly; repositories never enforce business rules.

---

## 4. DTO Design

Data Transfer Objects (DTOs) are used for all API communication to decouple the API contract from the internal domain model.

### Request DTOs (used for create/update operations)
| DTO | Purpose |
|-----|---------|
| `CreateBookRequest` | Fields for creating a book (Title, Author, ISBN, TotalCopies, AvailableCopies) |
| `UpdateBookRequest` | All fields optional for partial updates |
| `CreateMemberRequest` | Fields for creating a member (FullName, Email) |
| `UpdateMemberRequest` | Partial update for member fields |
| `BorrowRequest` | BookId + MemberId to initiate a borrow |
| `ReturnRequest` | BorrowRecordId to process a return |

### Response DTOs (returned to API consumers)
| DTO | Purpose |
|-----|---------|
| `BookResponse` | Exposes Id, Title, Author, ISBN, TotalCopies, AvailableCopies |
| `MemberResponse` | Exposes Id, FullName, Email, MembershipDate |
| `BorrowRecordResponse` | Exposes Id, BookId, MemberId, BorrowDate, ReturnDate, Status |
| `PagedResponse<T>` | Wraps list results with pagination metadata (Page, PageSize, TotalCount, TotalPages) |
| `ErrorResponse` | Standard error wrapper: `{ "error": "message" }` |

Database entities (e.g., `Book`, `Member`, `BorrowRecord`) are never serialized and returned directly. This protects internal schema details and allows the API contract to evolve independently.

---

## 5. Validation Strategy

Validation is implemented at multiple levels:

### Controller Level
- Required field checks using `[Required]` data annotations on request DTOs
- `[EmailAddress]` annotation on Member email
- `[Range]` annotations on copy count fields
- Invalid model states return a structured `{ "error": "..." }` response via the global `InvalidModelStateResponseFactory` override in `Program.cs`

### Service Level
Business rules enforced in the service layer include:
- `TotalCopies` must be > 0
- `AvailableCopies` must be ≥ 0 and ≤ `TotalCopies`
- A book can only be borrowed if `AvailableCopies > 0`
- A member cannot borrow more than **5 books** at a time
- A member cannot borrow the same book twice without returning it first
- A return request fails if the borrow record doesn't exist or is already in `Returned` status

### Database Level
- The `AvailableCopies` column is annotated with `[ConcurrencyCheck]`, meaning EF Core generates an optimistic concurrency check on UPDATE statements. This prevents two concurrent transactions from both decrementing from the same value.

---

## 6. Error Handling Approach

All errors are returned in a consistent format:

```json
{
  "error": "Descriptive message here"
}
```

### Global Middleware
`ExceptionHandlingMiddleware` catches all unhandled exceptions and returns:
- `400 Bad Request` for `ArgumentException`
- `409 Conflict` for `DbUpdateConcurrencyException` (concurrent borrow conflict)
- `500 Internal Server Error` for all other exceptions

Stack traces are never exposed to clients.

### HTTP Status Code Usage
| Code | When Used |
|------|-----------|
| 200 OK | Successful GET, PUT, DELETE |
| 201 Created | Successful POST (resource created) |
| 400 Bad Request | Validation failures, bad input |
| 404 Not Found | Resource does not exist |
| 409 Conflict | Concurrency conflict on borrow |
| 500 Internal Server Error | Unexpected server-side errors |

---

## 7. Concurrency Handling

### The Problem
Two users simultaneously attempt to borrow the last available copy of a book. Without concurrency control, both could read `AvailableCopies = 1`, both pass the availability check, and both decrement — leaving `AvailableCopies = -1`.

### Our Solution: Optimistic Concurrency via `[ConcurrencyCheck]`
The `Book` model's `AvailableCopies` property is decorated with `[ConcurrencyCheck]`. When EF Core generates the `UPDATE` SQL, it includes the original `AvailableCopies` value in the `WHERE` clause:

```sql
UPDATE Books
SET AvailableCopies = @newValue
WHERE Id = @id AND AvailableCopies = @originalValue
```

If two transactions race:
1. **Transaction A** reads `AvailableCopies = 1`, decrements to 0, and saves successfully.
2. **Transaction B** reads `AvailableCopies = 1` (same original value), tries to save — EF Core detects that the row has already changed and throws a `DbUpdateConcurrencyException`.
3. The middleware catches this and returns `409 Conflict` to the second caller.

**Result:** `AvailableCopies` never goes below 0, and only one borrow succeeds.

---

## 8. Caching Strategy

### Why Caching Improves Performance
Database queries — even fast in-memory ones — have overhead: EF Core query compilation, object materialization, and context switching. For read-heavy endpoints like "get all books," serving the response from an in-memory cache eliminates all of that overhead. As the number of concurrent readers increases, caching dramatically reduces latency and CPU load.

### What We Cached and Why

| Cached Data | Cache Key | TTL | Reason |
|-------------|-----------|-----|--------|
| All books list | `books:all` | 5 min | High read traffic; rarely changes |
| Individual book | `books:{id}` | 5 min | Frequent lookups by ID |
| All borrow records | `borrow-records:all` | 5 min | Admin-level view; stable between borrows |
| Member borrow history | `borrow-records:member:{id}` | 5 min | Frequently viewed; changes only on borrow/return |

We use `IMemoryCache` (ASP.NET Core's built-in in-memory cache), registered as a singleton in `Program.cs` via `builder.Services.AddMemoryCache()`.

### Cache Invalidation
When a write operation occurs (create, update, delete book, or any borrow/return action), the relevant cache keys are explicitly removed:
- Updating or deleting a book removes `books:all` and `books:{id}`
- Borrowing or returning removes `books:all`, `books:{id}`, `borrow-records:all`, and the member's history cache

This ensures that **the first request after a write always fetches fresh data from the database**, and subsequent reads are served from cache until the next write.
