# memory/ — BREAK NOTICE brain

Purpose: this folder is the durable memory for BREAK NOTICE. Conversations forget; this folder does not.

## File map

| File | What | Write rule |
|---|---|---|
| `DECISIONS.md` | Confirmed decisions | Append-only; supersede, never rewrite |
| `OPEN-QUESTIONS.md` | Unresolved items awaiting evidence or a decision | Close with the resolving finding/decision |
| `SESSION-LOG.md` | What happened per working session | Append, dated |
| `PRODUCT-TRUTH.md` | What the product actually does | Evidence + date only |
| `goal/chr00-animation-sync.md` | Current CHR00 animation/gameplay synchronization goal | Keep the next leaf and done-check current |
| `knowledge/unity-fbx-binary-replacement.md` | Safe FBX binary-replacement/import procedure and failure signatures | Update only with verified importer/runtime evidence |

## Operating principles

1. Record decisions and verified findings in-session.
2. Distinguish user-confirmed decisions from AI proposals.
3. Label claims: confirmed / observed / assumed / hearsay / unknown.
4. Product claims require evidence in `PRODUCT-TRUTH.md`.
5. Open questions are registered, never silently forgotten.
