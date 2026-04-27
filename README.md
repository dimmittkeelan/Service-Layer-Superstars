# Assignment: Library Book Borrowing System (Backend API)

Backend Library API Project built with ASP.NET Core

**Team:** Service Layer Superstars
**Course:** Cal State Fullerton — Backend Engineering (CPSC 449-03)

## Tested Environment

This project was tested on .NET 8.0

## Team Members

- Keelan Dimmitt
- Owin Rojas
- Varnika Pullareddy

## Project Overview

A backend REST API for a Library Book Borrowing System built with ASP.NET Core. The system supports managing books, library members, and borrowing records with a focus on clean architecture, correctness, and real-world backend engineering practices.

This project includes no UI, No authentication, backend only.

## Resources

The system is built around three core resources:

- **Book** — tracks titles, authors, ISBNs, and copy availability
- **Member** — tracks library members and their membership info
- **BorrowRecord** — tracks who borrowed what, when, and the return status

## Architecture

The project follows a multi-layer architecture:

- **Controller Layer** — handles routing and HTTP concerns
- **Service Layer** — enforces business logic and rules
- **Repository Layer** — handles all data access

DTOs are used for all API input and output. Dependencies are injected via constructor injection registered in `Program.cs`.

### Source Code
This GitHub repository containing the complete ASP.NET Core Web API project with visible commits from all team members.
