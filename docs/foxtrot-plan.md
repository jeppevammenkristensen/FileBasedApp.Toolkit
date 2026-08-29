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

### Problem

The current public API requires this for a command where exit code `1` is expected:

```csharp
.WithExitCodeHandler(code => code is 0 or 1)
```

That exposes SimpleExec's low-level “handled exit code” callback and makes every caller repeat the same predicate.

### Recommended public API

Add this fluent method to `BaseSimpleExecRunner<TSelf>`:

```csharp
public TSelf WithAcceptedExitCodes(params int[] exitCodes)
```

Use it like this:

```csharp
await new SimpleExecRunner("git")
    .AddArguments("diff", "--quiet")
    .WithAcceptedExitCodes(1)
    .RunAsync();
```

The method should have the following contract:

- Exit code `0` remains successful through SimpleExec's default behavior and does not need to be listed.
- Listed non-zero exit codes are treated as successful.
- Any other non-zero exit code keeps the existing behavior and throws.
- Duplicate exit codes have no effect.
- An empty list clears the generated handler and restores the default behavior where only `0` succeeds.
- Listing `0` is harmless but should be ignored when constructing the non-zero accepted-code set.
- Calling `WithAcceptedExitCodes` again replaces the previous accepted-code policy rather than silently accumulating state.
- Keep `WithExitCodeHandler` as the advanced, backward-compatible API.
- If both configuration methods are called, the last call should define the active policy. Document this explicitly.

Do not add `AcceptAnyExitCode()` in the first implementation. Requiring explicit codes makes accidental failure suppression less likely. It can be added later when there is a concrete use case.

### Implementation locations

- Main implementation:
  - `Source/FileBasedApp/FileBasedApp.Toolkit/SimpleExec/BaseSimpleExecRunner.cs`
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

### Tests to add

- `WithAcceptedExitCodes(1)` produces a handler that marks `1` as handled but not `2`; exit code `0` remains successful through SimpleExec's default behavior.
- Multiple accepted codes work.
- Duplicate codes do not change behavior.
- An empty list clears the handler and restores the default policy.
- A second call replaces the first accepted-code list.
- Calling `WithExitCodeHandler` after `WithAcceptedExitCodes` replaces the generated policy.
- Calling `WithAcceptedExitCodes` after `WithExitCodeHandler` replaces the custom policy.
- The generated handler is forwarded by `Run`, `RunAsync`, and `ReadAsync`.
- Existing default-behavior tests continue to verify that the handler is `null` when no policy is configured.

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
2. Add `WithAcceptedExitCodes` and its tests.
3. Update the SimpleExecRunner README examples and XML documentation.
4. Add the optional stderr policy to `ReadAndParseJson` and test it.
5. Build and test the main solution, plus any intentionally separate projects affected by the changes.
6. Update `CHANGELOG.md` with the public API addition and behavior fixes.

## Definition of Done

- [ ] `WithAcceptedExitCodes` is implemented with the contract above.
- [ ] The default exit-code behavior remains unchanged.
- [x] Strict `AddSecrets` validation is fixed and tested.
- [ ] JSON stderr behavior is explicit and tested.
- [ ] Intentionally separate projects are built directly when affected.
- [ ] Public XML documentation explains accepted versus handled exit codes.
- [ ] The package README contains an expected-non-zero-exit-code example.
- [ ] The changelog describes the new API and fixes.
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
