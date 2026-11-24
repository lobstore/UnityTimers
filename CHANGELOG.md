# Changelog

All notable changes are documented here.

## [Unreleased]

### Added
- XML documentation for public timer types and methods.
- Structured API contract for AI agents.
- Russian human-readable manual and Quick Start PDF.

### Changed
- Timer pause/resume now synchronizes registration with TimerManager.
- TimerManager uses a mutation-safe update snapshot and prevents duplicate registrations.
- All timer implementations consistently use the supplied deltaTime.
- FrequencyTimer preserves accumulated time and catches up all elapsed tick intervals.
- PlayerLoop bootstrap is idempotent and editor-only APIs are conditionally compiled.
- PlayerLoop utilities handle empty trees, nested systems, and invalid insertion indices safely.

### Fixed
- CountdownTimer.Progress handles zero-duration timers without NaN.
- IntervalTimer validates its interval and no longer risks an infinite loop for zero intervals.
- StopwatchTimer and IntervalTimer no longer read Time.deltaTime directly.
- Timer disposal deregisters and stops active timers.
