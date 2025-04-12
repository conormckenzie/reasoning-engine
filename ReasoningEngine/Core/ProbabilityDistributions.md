# Probability Distribution System: Conceptual Model and Specification

## Core Concepts and Design Rationale

### Measurement Uncertainty and Floating-Point Precision

The probability distribution system is designed with careful consideration for numerical stability and floating-point precision issues. Every numeric value in the system has an inherent uncertainty of ±EPSILON (where EPSILON = 1e-10). This fundamental design assumption acknowledges:

1. **Floating-Point Imprecision**: Direct equality comparisons with floating-point numbers are unreliable due to rounding errors.
2. **Measurement Uncertainty**: Real-world measurements always have some degree of uncertainty.
3. **Deterministic Behavior**: The system needs to make consistent, deterministic decisions even in the presence of tiny numerical differences.

### Range Design and Boundary Handling

Ranges in the system represent intervals `[LowerBound, UpperBound]` with specific properties:

1.  **Minimum Width**: All ranges must be at least `5 * EPSILON` wide (`UpperBound - LowerBound >= 5 * EPSILON`) to ensure they represent meaningful intervals.
2.  **Non-Overlapping & Minimum Gap**: Ranges cannot overlap. The `AddRange` method checks `lowerBound < d.UpperBound - EPSILON && upperBound > d.LowerBound + EPSILON` to prevent significant overlaps. Furthermore, for a `Continuous` distribution to be considered validly contiguous by `AreRangesContiguousAndValid`, the gap between the `UpperBound` of one range and the `LowerBound` of the next must be strictly greater than `EPSILON`.
3.  **Gap Ambiguity and Interpolation**: The system acknowledges that points falling within the small gap between two defined ranges might be ambiguously close to the boundaries of both ranges due to `EPSILON` tolerance.
    *   The `GetCoveringRanges` method identifies *all* ranges that a point might belong to, considering this tolerance (`point >= range.LowerBound - EPSILON && point <= range.UpperBound + EPSILON`). A point falling within the gap between range `i` (ending at `U_i`) and range `i+1` (starting at `L_{i+1}`) might be within `EPSILON` of *both* `U_i` and `L_{i+1}`, causing `GetCoveringRanges` to return both `i` and `i+1`.
    *   The `GetProbability(point)` method uses the result of `GetCoveringRanges`. If a point is covered by two ranges (indicating it falls in an ambiguous zone within the gap), it performs linear interpolation between the probabilities of those two ranges based on its position relative to the `UpperBound` of the lower range and the `LowerBound` of the upper range. This provides smoother probability transitions.

### Domain Types

The system supports three domain types:

1.  **Continuous**:
    *   Represents probabilities over a continuous range of real numbers.
    *   Defined using `AddRange`. `AddPoint` is not supported.
    *   Appropriate for physical measurements, real-valued parameters, etc.
2.  **DiscreteInteger**:
    *   Represents probabilities over a set of specific integer values.
    *   Defined using `AddPoint`. Values must be integers (within `EPSILON`). `AddRange` is not supported.
    *   Appropriate for counts, indices, or other integer-valued concepts.
3.  **Truth**:
    *   Represents a probability value itself as a continuous variable within the range `[0, 1]`. This allows representing uncertainty about a probability (e.g., P(Heads) is likely around 0.5 but could be between 0.48 and 0.52).
    *   Defined using `AddRange`. Range boundaries must be within `[0, 1]`.
    *   `AddPoint` is currently allowed but its validation (`value` must be near 0 or 1) is inconsistent with the continuous [0,1] nature. Using `AddRange` with a very narrow width is the recommended way to represent near-point probabilities for this domain (See TODO #12 in issues.md).

## Core Methods

1.  **`AddPoint(value, probability)`**:
    *   Supported for `DiscreteInteger` domain.
    *   Value must be an integer (within `EPSILON`).
    *   Currently also allowed for `Truth` but validation is inconsistent (See TODO #12).
    *   Checks for duplicate points (within `EPSILON`).
    *   Validates probability is in [0, 1] and total probability doesn't exceed 1.
    *   Inserts point maintaining sorted order.
2.  **`AddRange(lowerBound, upperBound, probability)`**:
    *   Supported for `Continuous` and `Truth` domains.
    *   Validates probability is in [0, 1] and total probability doesn't exceed 1.
    *   Validates range width (`>= 5 * EPSILON`).
    *   Validates domain bounds for `Truth` ([0, 1]).
    *   Checks for significant overlaps with existing ranges (`lowerBound < d.UpperBound - EPSILON && upperBound > d.LowerBound + EPSILON`). Allows adjacent ranges.
    *   Inserts range maintaining sorted order by `LowerBound`.
3.  **`GetProbability(value)`**:
    *   Returns probability for a specific value.
    *   For `DiscreteInteger`, finds the matching point (within `EPSILON`).
    *   For `Continuous` and `Truth`:
        *   Uses `GetCoveringRanges` to find relevant ranges.
        *   If 1 range covers the point, returns its probability.
        *   If 2 ranges cover the point (boundary/gap ambiguity), returns linearly interpolated probability.
        *   If 0 ranges cover the point, returns 0.
4.  **`GetProbability(lowerBound, upperBound)`**:
    *   Returns total probability within the specified bounds.
    *   For `DiscreteInteger`/`Truth`, sums probabilities of points within the bounds (using `EPSILON` tolerance).
    *   For `Continuous`, currently sums probabilities of ranges that *exactly* match the query bounds (within `EPSILON`). Could be extended for partial overlaps.
5.  **`GetCoveringRanges(point)`**:
    *   Helper method used by `GetProbability(value)`.
    *   Returns indices of all ranges `[L, U]` where the point `p` satisfies `p >= L - EPSILON && p <= U + EPSILON`.
    *   Crucially, returns *two* indices if `p` is within `EPSILON` of both the `UpperBound` of one range and the `LowerBound` of the next range, enabling interpolation.
6.  **`IsComplete()`**:
    *   Checks only if the sum of all defined probabilities is approximately 1 (within `EPSILON`).
    *   Does *not* check for full domain coverage or gap sizes.
7.  **`AreRangesContiguousAndValid()`**:
    *   Checks if defined ranges/points are spaced correctly according to domain rules.
    *   For `Continuous`, requires gaps between ranges to be `> EPSILON`.
    *   For `DiscreteInteger`, requires points to be consecutive integers.
    *   For `Truth`, always returns true (no specific contiguity requirement enforced beyond non-overlap).
8.  **`GetQuantization()` / `GetQuantizationWithProbabilities()`**:
    *   Return read-only lists of the defined ranges/points (and their probabilities), sorted by `LowerBound`.

## Key Properties/Invariants

1.  **Total Probability**: Sum of probabilities in `Distribution` must not exceed 1 (within `EPSILON`).
2.  **Non-Overlapping Ranges**: Defined ranges do not significantly overlap (checked by `AddRange`).
3.  **Minimum Gap (Continuous)**: For a distribution to be considered contiguous via `AreRangesContiguousAndValid`, gaps between ranges must be `> EPSILON`.
4.  **Domain Constraints**: Points/ranges must satisfy basic domain type constraints (e.g., integers for `DiscreteInteger`, bounds for `Truth`).
5.  **Sorted Order**: The internal `Distribution` list is always kept sorted by `LowerBound`.
6.  **Boundary Interpolation**: `GetProbability(value)` uses linear interpolation for points falling within `EPSILON` of boundaries between two ranges in `Continuous` or `Truth` domains.

## Future Considerations

As precision requirements evolve, several approaches could be considered:

1. **Higher Precision Types**: For extremely small EPSILON values, specialized numeric types like `decimal` or arbitrary-precision libraries could be employed.
2. **Adaptive Precision**: EPSILON could be made domain-dependent or automatically adjusted based on the scale of values.
3. **Interval Arithmetic**: Explicitly tracking uncertainty bounds rather than using a fixed EPSILON.

These considerations ensure the system can adapt to increasing precision needs while maintaining its core design principles.
