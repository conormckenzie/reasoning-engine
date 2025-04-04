KNOWN ISSUES:

- `issues.md` is not an ideal place to put known issues (this is a quick-and-dirty solution in lieu of proper issue tracking)
- Tests:
    - Unit test `TestLoadEdgeThatDoesNotExist` assumes node 9999999999999999 has no edges
    - Other unit tests MAY have unspecified underlying assumptions
- Instructions for usage and contribution to this repo are lacking

## TODO: Probability Distribution & Data Loading Refinements (from chat 2025-04-04)

Context: Issues identified during scenario data population and review of probability handling.

1.  **`ProbabilityDistribution`: Sorted Inserts:** Modify `AddPoint` and `AddRange` to insert new elements while maintaining sorted order by `LowerBound`. (Improves consistency and enables deterministic tie-breaking).
2.  **`ProbabilityDistribution`: Relax `AddRange` Validation:** Remove the "too close" check (`Math.Abs(lowerBound - d.UpperBound) < EPSILON || ...`) to allow defining contiguous ranges necessary for `IsComplete`. Keep the significant overlap check.
3.  **`ProbabilityDistribution`: Deterministic `GetContainingRange`:** Ensure tie-breaking for points equidistant from boundaries is deterministic by relying on the list being sorted by `LowerBound` (as per point 1). The existing `ThenBy(r => r.Index)` should then suffice.
4.  **`ProbabilityDistribution`: Interpolation (Medium Term):** Consider modifying `GetProbability` (or adding a new method) to interpolate probability values for points near range boundaries to avoid discrete jumps in output probability.
5.  **`PopulateWeatherScenarioData`: Rerun:** After fixes 1-3 (and potentially 4) are implemented, rerun the scenario populator (`dotnet run --project ReasoningEngine/ReasoningEngine.csproj --run-scenario weather --verbosity Minimal`) to generate correct data files. (Note: Script was already partially fixed to call SaveNode).
6.  **`GraphFileManager`: `LoadNode` Fix:** Corrected `LoadNode` to deserialize based on `NodeType` in JSON. (DONE - 2025-04-04)
7.  **`GraphFileManager`: Verbosity Fix:** Adjusted `DebugWriter` calls for file operations to use `VerbosityLevel.Detailed`. (DONE - 2025-04-04)
8.  **`CommandProcessor`: `edit_node` Behavior:** Review and potentially fix the `edit_node` command, which currently resets the distribution when editing a SIMO node. It should likely preserve the distribution unless the node type/interpretation changes.
