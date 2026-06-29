# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

Commonly used commands for the project:

- `npm run dev`: Start the development server.
- `npm run build`: Build the project (includes type checking).
- `npm run lint`: Lint the code using ESLint.

## High-Level Architecture

This project is a React-based frontend application built with Vite and TypeScript.

- **Frontend**: Uses React 19, TypeScript, and Vite.
- **Styling**: Uses Tailwind CSS.
- **State Management**: Uses Zustand for global state, specifically authentication management.
- **Data Fetching**: Uses @tanstack/react-query for managing server state.
- **Routing**: Uses React Router v7.
- **Components**: UI is built with Ant Design.
- **Architecture**:
  - `src/api`: Axios client configuration with interceptors for authentication.
  - `src/store/auth`: Zustand store for authentication state.
  - `src/components`: Reusable UI components.
  - `src/pages`: Page-level components associated with specific routes.
  - `src/layout`: Main layout wrappers (e.g., MainLayout).
