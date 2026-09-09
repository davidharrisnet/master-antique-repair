# PizzaGo

PHASE 1 — Legacy Build
Stack: ASP.NET Framework 4.7.2, C#, WebForms, SQL Server (or SQLite is fine)

Functional requirements:
•	A single core domain with 3–4 related entities (e.g., a simple case/request tracking app: Requests → Assignees → Status History)
•	Basic CRUD for each entity
•	One approval/status-transition workflow with at least 3 states (e.g., Submitted → In Review → Closed)
•	A simple login/role check (hardcoded roles are fine)
•	One list/search view with filtering and pagination

Non-functional requirements:
•	Layered architecture (UI / business logic / data access clearly separated — no logic in code-behind)
•	Server-side input validation
•	Logging of workflow state changes (this becomes your audit trail in Phase 2)
•	A short README explaining the structure and how to run it.