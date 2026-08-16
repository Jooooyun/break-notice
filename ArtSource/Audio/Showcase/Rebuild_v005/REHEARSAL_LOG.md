# v005 Review Package Rehearsal Log

Date: `2026-08-16 KST`

Perceptual listening was unavailable to the rehearsal executors. No audio-quality PASS was inferred.

## Round 1 — NOT CLEAN

Zero-context execution found that Phase 1 and Phase 2 did not contain their own response forms, did not define where a verdict becomes locked, and required opening Phase 3 to find the full procedure. Phase 3 also lacked an exact document-entry order. Providing all three packages simultaneously allowed the same audio events to be correlated across phases even though candidate/reference identity remained hidden.

Fixes:

- added a self-contained README and response form to Phase 1;
- added a self-contained README and response form to Phase 2;
- defined `sent to the current Codex task` as the immutable answer lock;
- defined MP4 as the first review surface and WAV as the same audio's lossless fallback;
- added exact Phase 3 entry and diagnosis order;
- fixed actual delivery to sequential release: Phase 1 response lock, then Phase 2 response lock, then Phase 3 deblind.

## Round 2 — CLEAN

The executor received only Phase 1, completed the documented path, wrote and sent a response lock, and reported no blocking stall, guess, or misread. It then received only Phase 2 and repeated the process without a stall. Finally, after both simulated locks, it received Phase 3 and followed the root README through deblind, composite, five-stem diagnosis, checklist, final response, provenance, and evidence.

Technical rehearsal evidence:

- Phase 1 and Phase 2 ZIP extraction and response paths completed.
- Phase 3 mapping and seven evidence JSON files parsed.
- Diagnostic PNG decoded at `2380×1360 RGBA`.
- Two composites and ten stems decoded as `48 kHz / 24-bit PCM / stereo / 1.350 s`.
- Six blind MP4 files technically decoded.
- No automated or rehearsal result granted perceptual approval.

A non-blocking repository-versus-ZIP evidence-path wording issue was corrected after this clean run.
