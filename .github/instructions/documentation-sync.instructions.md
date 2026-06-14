---
description: "Use when creating, updating, or modifying any file, feature, component, entity, service, or configuration in this project. Enforces documentation-first awareness: consult README.md before building, and keep it in sync when intentional changes diverge from it."
applyTo: "**"
---

# Documentation-First Development

## Before Creating or Updating Anything

Always read `README.md` before implementing a new feature, entity, service, endpoint, component, or configuration change.

- Use it to understand intended design decisions, technology choices, and architecture patterns already planned for the project.
- If the documentation describes what you are about to build, follow it — do not invent a different approach.
- If the documentation is silent on the topic, proceed with the best-fit approach given the established patterns in the codebase.

## Documentation Is Not an Absolute Source of Truth

`README.md` reflects *intent*, not *law*. You may intentionally deviate when:

- A better pattern or library has been chosen during implementation.
- A planned feature is out of scope for the current task.
- A technology listed in the docs has been replaced by a more suitable alternative.

When you intentionally create something that differs from what README.md describes, **update README.md** to reflect the actual state of the project. Keep the doc honest.

## What to Update in README.md

| Change Made | What to Update |
|---|---|
| New entity, service, or component added | Add it to the relevant section (stack table, architecture overview, repo structure) |
| Technology replaced or removed | Update the technology stack table and any references |
| New API endpoint created | Add it to the "Key API Endpoints Reference" section |
| Architecture decision changed | Update the architecture overview section |
| Scope changed (in or out) | Update Section 2 (Scope & Boundaries) |

## Workflow Summary

1. **Read** `README.md` for context on what is planned.
2. **Build** according to the documented design, or deviate with good reason.
3. **Update** `README.md` if the implementation differs from or extends what was documented.
