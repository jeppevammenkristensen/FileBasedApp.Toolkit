# Foxtrot Implementation Plan

## Goal

Improve `SimpleExecRunner` so callers can clearly accept expected non-zero exit codes without writing a raw `Func<int, bool>`. While making that change, fix the related runner defect and ensure the solution validates every maintained project.

## Baseline

The health check was performed through Rider on 2026-08-29.

- `Source/FileBasedApp.Toolkit.slnx` builds successfully.
- Rider reports no solution-level inspection problems.
- All tests pass: 352 passed, 0 failed, 0 skipped.
  - `FileBasedApp.Toolkit.Tests`: 278 passed.
  - `FileBasedApp.Toolkit.CSharp.Tests`: 74 passed.
- The Recipes and Template projects build successfully when built directly.

## Work Item 1: Add an accepted-exit-code API

**Status:** In progress — implementation and focused unit coverage are complete; user documentation and final validation remain.

### Current implementation progress

- Added `WithAcceptedErrorCodes(int[] acceptedErrorCodes, bool failOnExistingErrorHandler = false)` as a fluent extension.
- Added `HandlesAllErrorCodes(bool failOnExistingErrorHandler = false)` as a fluent extension.
- Extended `WithExitCodeHandler` with optional conflict detection through `throwIfAlreadySet`.
- Added `SimpleExecRunResult` and `SimpleExecReadResult` result types.
- Added `RunWithExitCode`, `RunWithExitCodeAsync`, and `ReadEnhancedAsync` for returning the observed exit code, with captured output for reads.
- Added tests covering the three result-returning methods, custom-handler delegation, the default zero/non-zero policy, accepted-code behavior, null and empty input, and accept-all behavior.
- Verified all 82 `SimpleExecRunnerTest` tests pass.

### Intentional API decisions

- The public method is named `WithAcceptedErrorCodes` rather than `WithAcceptedExitCodes`.
- The accepted codes are supplied as an `int[]` rather than a `params int[]`.
- Passing a null or empty array is intentionally a no-op and preserves any existing exit-code handler.
- A non-empty accepted-code array replaces the existing handler by default. Callers can set `failOnExistingErrorHandler: true` to reject replacement.
- `HandlesAllErrorCodes` is intentionally included for callers that explicitly want to suppress default failure handling for every exit code.

### Remaining closure work

- Add or update the package README example for expected non-zero exit codes.
- Complete the final XML-documentation review.
- Update the changelog to use the final `WithAcceptedErrorCodes` name.
- Run final solution and affected separate-project validation.

### Problem

The current public API requires this for a command where exit code `1` is expected:

```csharp
.WithExitCodeHandler(code => code is 0 or 1)
```

That exposes SimpleExec's low-level “handled exit code” callback and makes every caller repeat the same predicate.

### Chosen public API

Add this fluent extension for `BaseSimpleExecRunner<TSelf>`:

```csharp
public TSelf WithAcceptedErrorCodes(
    int[] acceptedErrorCodes,
    bool failOnExistingErrorHandler = false)
```

Use it like this:

```csharp
await new SimpleExecRunner("git")
    .AddArguments("diff", "--quiet")
    .WithAcceptedErrorCodes([1])
    .RunAsync();
```

The method should have the following contract:

- Exit code `0` remains successful through SimpleExec's default behavior and does not need to be listed.
- Listed non-zero exit codes are treated as successful.
- Any other non-zero exit code keeps the existing behavior and throws.
- Duplicate exit codes have no effect.
- A null or empty array is a no-op and leaves the current exit-code handler unchanged.
- Listing `0` is harmless because SimpleExec already treats exit code `0` as successful.
- Calling `WithAcceptedErrorCodes` again with a non-empty array replaces the previous accepted-code policy rather than silently accumulating state.
- Keep `WithExitCodeHandler` as the advanced, backward-compatible API.
- By default, the last non-empty configuration call defines the active policy. Setting `failOnExistingErrorHandler: true` rejects replacement instead.

`HandlesAllErrorCodes` is included as an explicit opt-in for accepting every exit code.

### Implementation locations

- Main implementation:
  - `Source/FileBasedApp/FileBasedApp.Toolkit/SimpleExec/BaseSimpleExecRunnerExtensions.cs`
- Existing forwarding points that must continue to use the configured policy:
  - `Run` around lines 435-448.
  - `RunAsync` around lines 456-468.
  - `ReadAsync` around lines 477-489.
- Tests:
  - `Source/FileBasedApp/FileBasedApp.Toolkit.Tests/SimpleExec/SimpleExecRunnerTest.cs`
- User documentation:
  - `Source/FileBasedApp/FileBasedApp.Toolkit/README.md`
- Release notes:
  - `CHANGELOG.md`

### Test coverage

- [x] Configured codes are handled while unconfigured codes are not.
- [x] Multiple accepted codes work.
- [x] Duplicate codes do not change behavior.
- [x] Null and empty arrays preserve the existing handler, including when replacement rejection is enabled.
- [x] `WithAcceptedErrorCodes` replaces an existing custom handler by default.
- [x] Replacement can be rejected with `failOnExistingErrorHandler: true`.
- [x] `HandlesAllErrorCodes` handles zero, positive, negative, and maximum integer exit codes.
- [x] `HandlesAllErrorCodes` replacement and replacement rejection are covered.
- [x] Existing forwarding tests verify that `Run`, `RunAsync`, and `ReadAsync` pass their configured handler to SimpleExec.
- [x] Existing default-behavior tests verify that the handler is `null` when no policy is configured.

## Work Item 2: Fix strict secret validation

**Status:** Completed — implemented and tested on 2026-08-29.

### Original problem

`AddSecrets(bool strict, params string[] secrets)` had two problems:

1. It throws every time `strict` is `true`, even when no secret is unmatched.
2. It calculates `Secrets.Except(secrets)` instead of checking whether the supplied secrets already occur in `Arguments`.

### Required behavior

- With `strict: false`, append the supplied secrets as today.
- With `strict: true`, compare every supplied secret against `Arguments`.
- Throw only when one or more supplied secrets are not present in `Arguments`.
- Include only the unmatched values in the exception message.
- If all supplied secrets match arguments, append them and return the runner.
- Match using `StringComparison.OrdinalIgnoreCase`, consistent with the confirmed SimpleExec redaction implementation.

### Tests added

- [x] Strict mode succeeds when every secret exists in `Arguments`.
- [x] Case-insensitive substring matching reflects SimpleExec's redaction behavior.
- [x] Strict mode throws when one secret is absent without mutating the configured secrets.
- [x] The exception identifies the absent secret.
- [x] Strict mode with a mixture of matched and unmatched secrets reports only unmatched values.
- [x] Non-strict mode continues to accept values that are not arguments.
- [x] Duplicate secrets are stored once.
- [x] Null-element validation continues to throw without mutating the configured secrets.

## Work Item 3: Decide how JSON parsing handles stderr

### Problem

`ReadAndParseJson` in `BaseSimpleExecRunnerExtensions.cs:49-61` throws whenever stderr contains text. Stderr output does not necessarily mean the process failed; some successful commands write warnings or progress there. Exit-code handling should determine process success.

### Recommended change

Make the behavior explicit instead of silently conflating stderr with failure:

1. Keep the existing overload for source compatibility.
2. Add an overload or option named `failOnStandardError`.
3. Have the existing overload preserve its current strict behavior.
4. Allow callers to set `failOnStandardError: false`, in which case JSON is parsed from stdout after the exit-code policy accepts the process result.
5. Document that stderr is discarded by the JSON-returning helper. If callers need it, they should use `ReadAsync` directly.

### Tests to add

- Valid JSON with empty stderr deserializes successfully.
- Valid JSON with stderr throws in strict mode.
- Valid JSON with stderr deserializes when `failOnStandardError` is `false`.
- Invalid JSON still produces the JSON deserialization exception.
- A null deserialization result still produces the existing `InvalidOperationException`.

## Closed Finding: Projects outside the main solution

**Status:** Closed — intentional.

`Source/FileBasedApp.Toolkit.slnx:7-19` excludes these projects:

- `Source/FileBasedApp/FileBasedApp.Toolkit.Recipes/FileBasedApp.Toolkit.Recipes.csproj`
- `Source/FileBasedApp/Templates/FileBasedAppTemplates.csproj`

This is the intended repository structure. Do not add these projects to `FileBasedApp.Toolkit.slnx`. Continue validating them separately when changes affect them.

## Implementation Order

1. [x] Fix strict `AddSecrets` behavior and add its tests.
2. [x] Implement the accepted-error-code API and its direct tests.
3. [ ] Update the SimpleExecRunner README examples and complete XML documentation.
4. [ ] Add the optional stderr policy to `ReadAndParseJson` and test it.
5. [ ] Build and test the main solution, plus any intentionally separate projects affected by the changes.
6. [x] Update `CHANGELOG.md` with the current public API additions and behavior fixes.

## Definition of Done

- [x] `WithAcceptedErrorCodes` is implemented with the chosen contract above.
- [x] The default exit-code behavior remains unchanged.
- [x] Strict `AddSecrets` validation is fixed and tested.
- [ ] JSON stderr behavior is explicit and tested.
- [ ] Intentionally separate projects are built directly when affected.
- [ ] Public XML documentation explains accepted versus handled exit codes.
- [ ] The package README contains an expected-non-zero-exit-code example.
- [x] The changelog describes the current API additions and fixes.
- [ ] Rider reports no new problems.
- [ ] The complete solution builds successfully.
- [ ] All tests pass.

## Work Log

### 2026-08-29

- Initialized the Foxtrot plan.
- Completed the Rider solution health check.
- Converted the findings into implementation-ready work items for manual implementation.
- Completed Work Item 2: fixed strict `AddSecrets` validation and added coverage for matching, failures, non-strict behavior, duplicates, and null elements.
- Verified all 63 `SimpleExecRunnerTest` tests pass and the Rider solution build succeeds without reported problems.
- Began Work Item 1 by adding `AcceptedErrorCodes`, `HandlesAllErrorCodes`, and optional conflict detection to `WithExitCodeHandler`.
- Added `SimpleExecRunResult`, `SimpleExecReadResult`, `RunWithExitCode`, `RunWithExitCodeAsync`, and `ReadEnhancedAsync`.
- Added focused tests for the three result-returning methods, configured-handler delegation, and default exit-code handling.
- Verified all 70 `SimpleExecRunnerTest` tests pass. Full solution and separate-project validation remain pending for the completed Foxtrot work.
- Updated `CHANGELOG.md` with the current public API additions and the strict `AddSecrets` fix.
- Finalized the intentional API decisions: `WithAcceptedErrorCodes` uses an array, null and empty arrays are no-ops, and `HandlesAllErrorCodes` remains available as an explicit opt-in.
- Added direct coverage for `WithAcceptedErrorCodes` and `HandlesAllErrorCodes`, including null and empty input and handler-replacement behavior.
- Verified all 82 `SimpleExecRunnerTest` tests pass.
