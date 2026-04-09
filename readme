Library Book Borrowing System (Backend API)
Due Dates
Milestone 1 (Team Setup): April 11
Final Submission: April 26

Group Size
3–5 students per group. Students must form groups using the “People → Groups” section in Canvas.

Objective
In this project, your team will design and build a backend system for a Library Book Borrowing System using ASP.NET Core.

This project is intended to help you apply core backend engineering concepts, including:

REST API design
Multi-layer architecture
Data Transfer Objects (DTOs)
Dependency Injection (DI)
Validation
Error handling
Asynchronous programming
Concurrency handling
Performance and caching
The goal is to build a system that is not only functional, but also clean, well-structured, and designed with scalability and correctness in mind.

Team Identity Requirement
This is a group project. Each group must choose a unique team name representing your team as a startup company.

The name should be professional and appropriate. It will be used in your submission, documentation, and final presentation.

Students must form groups using the “People → Groups” section in Canvas. The Group name should be your company name.

System Overview
You will build a backend API that supports:

Managing books
Managing members
Borrowing and returning books
Core Requirements
Resources (Required)
Your system must include the following three resources:

Book
Id
Title
Author
ISBN
TotalCopies
AvailableCopies
Member
Id
FullName
Email
MembershipDate
BorrowRecord
Id
BookId
MemberId
BorrowDate
ReturnDate
Status (Borrowed / Returned)
Functional Requirements
Book APIs
Create a book
Get all books
Get book by id
Update book
Delete book
Rules:

Title, Author, and ISBN are required
TotalCopies must be greater than 0
AvailableCopies must be greater than or equal to 0
AvailableCopies must not exceed TotalCopies
Member APIs
Create a member
Get all members
Get member by id
Update member
Delete member
Rules:

FullName is required
Email is required and must be valid
Borrowing APIs
Borrow a book
Return a book
View all borrow records
View borrow history for a member
Rules:

A book can only be borrowed if AvailableCopies > 0
Borrowing a book decreases AvailableCopies
Returning a book increases AvailableCopies
A member cannot return a book that was not borrowed
(Optional) Prevent duplicate borrowing without returning
Architecture Requirements
Your project must implement multi-layer architecture with the following layers:

Controller layer (API)
Service layer (business logic)
Repository layer (data access)
Requirements:

Controllers must not contain business logic
Services must enforce business rules
Repositories must handle database access only
DTO Requirements
You must use DTOs for API communication.

Requirements:

Use request DTOs for create/update operations
Use response DTOs for API responses
Do not expose database entities directly through the API
Dependency Injection Requirements
You must use Dependency Injection:

Register services and repositories in Program.cs
Use constructor injection
Do not instantiate dependencies using “new” inside controllers
Validation Requirements
Validation must be implemented at multiple levels:

Controller level:

Required fields
Data types
Basic validation
Service level:

Business rules
Borrow/return logic
Database level:

Basic constraints where applicable
Error Handling Requirements
You must implement consistent error handling.

Requirements:

Use correct HTTP status codes (200, 201, 400, 404, 409, 500)
Implement global exception handling (middleware or equivalent)
Return a consistent error response format:
{
"error": "Message here"
}
Do not expose stack traces to clients
Async Programming Requirements
You must use asynchronous programming for database operations.

Examples include:

ToListAsync()
FindAsync()
SaveChangesAsync()
Controllers, services, and repositories should use async/await where appropriate.

Concurrency Requirement
Your system must correctly handle the following scenario:

Two users attempt to borrow the last available copy of a book at the same time.

Requirements:

Only one request should succeed
The other request should fail safely
AvailableCopies must never go below 0
Performance and Caching Requirement
You must implement basic caching.

Requirements:

Cache at least one read-heavy endpoint (e.g., get all books or get book by id)
Use in-memory caching (IMemoryCache or equivalent)
Expected behavior:

First request retrieves data from the database
Subsequent requests return cached data
Additional requirement:

Invalidate cache when data changes (update or delete operations)
In your document, explain:

Why caching improves performance
What you chose to cache and why
API Design Requirements
Your API must follow REST principles:

Use nouns for resources (books, members)
Use proper HTTP methods
Use appropriate status codes
Examples:

GET /api/books
POST /api/books
GET /api/books/{id}
Deliverables
1. Source Code (Required)
GitHub repository link
Complete ASP.NET Core Web API project
Proper layered structure (Controller, Service, Repository)
Requirement:
Each team member must contribute code to the repository with visible commits.

2. Design Document (Required) (2–4 pages)
Include the following sections:

System overview
API design (list of endpoints)
Architecture explanation
DTO design
Validation strategy
Error handling approach
Concurrency handling explanation
Caching strategy
3. API Endpoint List (Required)
Provide a list of all endpoints with HTTP methods and routes.

4. Team Contribution Summary (Required)
Include a section in your document listing each member and their contributions.

Example format:

Team Name: [Your Team Name]

Member 1 – Name

Contribution details
Member 2 – Name

Contribution details
All contributions must be clearly described and reflect actual work done.

5. Demo / Presentation (Required)
Options: Video recording. 

Each group must demonstrate:

Project structure
Borrow flow
Return flow
Validation example
Error handling example
Concurrency scenario
Caching behavior
Grading Rubric
Category	Points
API Design (REST)	10
Architecture (Layered Design)	15
DTO Usage	10
Dependency Injection	10
Validation	10
Error Handling	10
Async Implementation	10
Concurrency Handling	10
Caching and Performance	10
Code Quality and Structure	5
Documentation	5
Demo / Presentation	5
Total	100
Project Timeline
Milestone 1 (April 11)
Submit:

Team name
Team members
Final Submission (April 26)
Submit: Email to mfirdouse@fullerton.edu. Include you class, Company Team

GitHub repository link
Design document
API endpoint list
Contribution summary
Important Guidelines
Keep the system simple
Focus on correctness and clean design
Do not build a UI
Do not implement authentication
Do not over-engineer
Optional Features (Bonus)
Search books
Pagination
Limit number of books per member
Logging
Final Note
This project is designed to simulate a real backend system. Focus on:

clean architecture
correct behavior
handling edge cases
thinking like a backend engineer