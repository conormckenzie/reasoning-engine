# Probability Distribution System: Conceptual Model and Specification

## Core Concepts and Design Rationale

### Measurement Uncertainty and Floating-Point Precision

The probability distribution system is designed with careful consideration for numerical stability and floating-point precision issues. Every numeric value in the system has an inherent uncertainty of ±EPSILON (where EPSILON = 1e-10). This fundamental design assumption acknowledges:

1. **Floating-Point Imprecision**: Direct equality comparisons with floating-point numbers are unreliable due to rounding errors.
2. **Measurement Uncertainty**: Real-world measurements always have some degree of uncertainty.
3. **Deterministic Behavior**: The system needs to make consistent, deterministic decisions even in the presence of tiny numerical differences.

### Range Design and Gap Requirements

Ranges in the system are closed intervals [a,b] with specific properties:

1. **Minimum Width**: All ranges must be at least 5*EPSILON wide to ensure they represent meaningful intervals rather than effectively single points.

2. **Separation Between Ranges**: Adjacent ranges must have a small gap (> EPSILON) between them. This design decision serves several critical purposes:
   - **Prevents Excessive Ambiguity**: Ensures that the region where a point could be assigned to multiple ranges is minimized
   - **Enables Deterministic Assignment**: When a point falls in an ambiguous region (within EPSILON of multiple range boundaries), explicit tie-breaking rules determine which range it belongs to:
     * The range with the closest boundary gets priority
     * If distances are equal, the range with the lower index is chosen
   - **Supports Clear Reasoning**: Makes the boundaries between different probability ranges explicit and auditable

3. **Completeness Criteria**: A distribution is considered "complete" when:
   - The total probability sums to 1 (within EPSILON)
   - For continuous domains, gaps between ranges are no larger than 1.9*EPSILON
   
   This gap threshold (1.9*EPSILON) ensures that no point can be "lost" between ranges, since any point in such a gap would be within EPSILON of at least one range boundary and thus assigned to that range.

4. **Boundary Ambiguity Handling**: With gaps between EPSILON and 1.9*EPSILON, there will be points that are within EPSILON of both adjacent range boundaries. For example:
   - If Range 1 ends at position b and Range 2 starts at position c
   - And if c - b = 1.5*EPSILON
   - Then a point at position b + 0.8*EPSILON would be within EPSILON of both range boundaries
   
   This ambiguity is handled explicitly through the tie-breaking rules mentioned above, ensuring deterministic behavior even in these edge cases.

### Domain Types

The system supports three domain types, each with specific characteristics:

1. **Continuous**: 
   - Uses ranges to represent continuous intervals
   - Appropriate for physical measurements, real-valued parameters, etc.

2. **DiscreteInteger**: 
   - Uses single points that must be integers (within EPSILON)
   - Appropriate for counts, indices, or other integer-valued concepts

3. **Truth**: 
   - Uses ranges or points, all values must be in [0,1]
   - Appropriate for representing degrees of truth, confidence levels, or probabilities

## Required Operations

1. **AddPoint(value, probability)**:
   - For DiscreteInteger and Truth domains only
   - Value must satisfy domain constraints
   - No duplicate points (within EPSILON)
   - Probability must be in [0,1]

2. **AddRange(lowerBound, upperBound, probability)**:
   - For Continuous and Truth domains only
   - Range must be at least 5*EPSILON wide
   - Must satisfy domain constraints
   - No overlaps with existing ranges
   - Adjacent ranges must have small gap (> EPSILON) to prevent ambiguity
   - Probability must be in [0,1]

3. **GetProbability(point)**:
   - Returns probability for the given point
   - For discrete domains: exact match within EPSILON
   - For continuous domains: point must be within a range (considering EPSILON)

4. **GetProbability(lowerBound, upperBound)**:
   - Returns probability for the given range
   - For discrete domains: sum of probabilities of contained points
   - For continuous domains: sum of probabilities of overlapping ranges

5. **GetContainingRange(point)**:
   - Returns index of range containing the point
   - If point could belong to multiple ranges due to uncertainty, choose range with closest boundary
   - If distances are equal, prefer lower index range
   - Throw if point not in any range

6. **IsComplete()**:
   - Verifies distribution is properly defined
   - All probabilities must sum to 1 (within EPSILON)
   - For discrete domains: no gaps in integer sequence
   - For continuous domains: gaps between ranges must be no larger than 1.9*EPSILON

7. **GetQuantization()**:
   - Returns how the distribution is divided into ranges
   - For discrete domains: list of points
   - For continuous domains: list of ranges
   - Results ordered by position on number line

## Properties/Invariants

1. **Total Probability**: Must never exceed 1 (within EPSILON)
2. **Range Separation**: Ranges must not overlap (considering EPSILON uncertainty)
3. **Boundary Clarity**: Adjacent ranges must have small gap to prevent excessive ambiguity
4. **Domain Constraints**: Points/ranges must satisfy domain type constraints
5. **Deterministic Assignment**: Every point is assigned to exactly one range through consistent tie-breaking rules
6. **Uncertainty Handling**: All numeric comparisons account for ±EPSILON uncertainty

## Future Considerations

As precision requirements evolve, several approaches could be considered:

1. **Higher Precision Types**: For extremely small EPSILON values, specialized numeric types like `decimal` or arbitrary-precision libraries could be employed.
2. **Adaptive Precision**: EPSILON could be made domain-dependent or automatically adjusted based on the scale of values.
3. **Interval Arithmetic**: Explicitly tracking uncertainty bounds rather than using a fixed EPSILON.

These considerations ensure the system can adapt to increasing precision needs while maintaining its core design principles.
