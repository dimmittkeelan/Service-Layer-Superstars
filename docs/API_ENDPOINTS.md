# API Endpoint Reference

**Team:** Service Layer Superstars  
**Base URL:** `https://localhost:{port}/api`

---

## Books

### GET /api/books
Returns a paginated list of all books. Supports optional search and pagination query parameters.

**Query Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `search` | string | null | Filter by title, author, or ISBN (case-insensitive) |
| `page` | int | 1 | Page number (1-indexed) |
| `pageSize` | int | 10 | Results per page (max 100) |

**Response: 200 OK**
```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "title": "Clean Code",
      "author": "Robert C. Martin",
      "isbn": "978-0132350884",
      "totalCopies": 5,
      "availableCopies": 3
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 10,
  "totalPages": 1
}
```

---

### GET /api/books/{id}
Returns a single book by its GUID.

**Path Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `id` | Guid | Book ID |

**Response: 200 OK**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "title": "Clean Code",
  "author": "Robert C. Martin",
  "isbn": "978-0132350884",
  "totalCopies": 5,
  "availableCopies": 3
}
```

**Response: 404 Not Found**
```json
{ "error": "Book not found." }
```

---

### POST /api/books
Creates a new book.

**Request Body:**
```json
{
  "title": "Clean Code",
  "author": "Robert C. Martin",
  "isbn": "978-0132350884",
  "totalCopies": 5,
  "availableCopies": 5
}
```

**Validation Rules:**
- `title` — required, non-empty string
- `author` — required, non-empty string
- `isbn` — required, non-empty string
- `totalCopies` — required, must be > 0
- `availableCopies` — required, must be ≥ 0 and ≤ `totalCopies`

**Response: 201 Created** — Returns the created book.

**Response: 400 Bad Request**
```json
{ "error": "TotalCopies must be greater than 0." }
```

---

### PUT /api/books/{id}
Updates an existing book. All fields are optional (partial update supported).

**Request Body:**
```json
{
  "title": "Clean Code (Updated Edition)",
  "totalCopies": 10,
  "availableCopies": 8
}
```

**Response: 200 OK** — Returns the updated book.

**Response: 404 Not Found**
```json
{ "error": "Book not found." }
```

---

### DELETE /api/books/{id}
Deletes a book by its GUID.

**Response: 200 OK** — Returns the deleted book.

**Response: 404 Not Found**
```json
{ "error": "Book not found." }
```

---

## Members

### GET /api/members
Returns a list of all members.

**Response: 200 OK**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "fullName": "Jane Doe",
    "email": "jane.doe@example.com",
    "membershipDate": "2025-01-15T00:00:00Z"
  }
]
```

---

### GET /api/members/{id}
Returns a single member by their GUID.

**Response: 200 OK** — Returns the member.

**Response: 404 Not Found**
```json
{ "error": "Member not found." }
```

---

### POST /api/members
Creates a new library member.

**Request Body:**
```json
{
  "fullName": "Jane Doe",
  "email": "jane.doe@example.com"
}
```

**Validation Rules:**
- `fullName` — required, non-empty string
- `email` — required, must be a valid email address

**Response: 201 Created** — Returns the created member.

**Response: 400 Bad Request**
```json
{ "error": "Email is required." }
```

---

### PUT /api/members/{id}
Updates an existing member.

**Request Body:**
```json
{
  "fullName": "Jane Smith",
  "email": "jane.smith@example.com"
}
```

**Response: 200 OK** — Returns the updated member.

**Response: 404 Not Found**
```json
{ "error": "Member not found." }
```

---

### DELETE /api/members/{id}
Deletes a member by their GUID.

**Response: 200 OK** — Returns the deleted member.

**Response: 404 Not Found**
```json
{ "error": "Member not found." }
```

---

## Borrow Records

### POST /api/borrowrecords/borrow
Borrows a book for a member.

**Request Body:**
```json
{
  "bookId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "memberId": "1fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Business Rules:**
- Book must exist
- Member must exist
- `AvailableCopies` must be > 0
- Member must not already have an active borrow for the same book
- Member must not exceed the 5 active borrow limit

**Response: 201 Created**
```json
{
  "id": "9fa85f64-5717-4562-b3fc-2c963f66afa6",
  "bookId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "memberId": "1fa85f64-5717-4562-b3fc-2c963f66afa6",
  "borrowDate": "2026-04-26T10:30:00Z",
  "returnDate": null,
  "status": "Borrowed"
}
```

**Response: 400 Bad Request**
```json
{ "error": "No copies of this book are currently available." }
```

**Response: 409 Conflict**
```json
{ "error": "A concurrency conflict occurred. Please try again." }
```

---

### POST /api/borrowrecords/return
Returns a borrowed book.

**Request Body:**
```json
{
  "borrowRecordId": "9fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Business Rules:**
- BorrowRecord must exist
- BorrowRecord status must be `"Borrowed"` (cannot return an already-returned book)

**Response: 200 OK**
```json
{
  "id": "9fa85f64-5717-4562-b3fc-2c963f66afa6",
  "bookId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "memberId": "1fa85f64-5717-4562-b3fc-2c963f66afa6",
  "borrowDate": "2026-04-26T10:30:00Z",
  "returnDate": "2026-04-30T14:00:00Z",
  "status": "Returned"
}
```

**Response: 400 Bad Request**
```json
{ "error": "This book was not borrowed or has already been returned." }
```

---

### GET /api/borrowrecords
Returns all borrow records in the system.

**Response: 200 OK** — Returns an array of `BorrowRecordResponse` objects.

---

### GET /api/borrowrecords/member/{memberId}
Returns the full borrow history for a specific member.

**Path Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `memberId` | Guid | Member ID |

**Response: 200 OK** — Returns an array of `BorrowRecordResponse` objects for the given member.

**Response: 404 Not Found**
```json
{ "error": "Member not found." }
```

---

## Error Response Format

All errors across the API use this consistent format:
```json
{
  "error": "Human-readable error message here."
}
```

## HTTP Status Code Summary

| Code | Meaning |
|------|---------|
| 200 OK | Successful read, update, or delete |
| 201 Created | Resource successfully created |
| 400 Bad Request | Validation failure or invalid input |
| 404 Not Found | Requested resource does not exist |
| 409 Conflict | Concurrent modification conflict (on borrow) |
| 500 Internal Server Error | Unexpected server-side error |
