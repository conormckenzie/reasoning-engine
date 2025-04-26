using System; // Removed Newtonsoft.Json
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization; // Added for System.Text.Json

namespace ReasoningEngine
{
    /// <summary>
    /// Represents a single range or point in a probability distribution.
    /// Using a struct for better serialization compatibility with System.Text.Json compared to tuples.
    /// </summary>
    public struct ProbabilityRange
    {
        // Use public init setters for easy construction and deserialization
        public double LowerBound { get; init; }
        public double UpperBound { get; init; }
        public double Probability { get; init; }

        // Optional: Add a constructor for convenience if needed elsewhere
        // public ProbabilityRange(double lower, double upper, double prob)
        // {
        //     LowerBound = lower;
        //     UpperBound = upper;
        //     Probability = prob;
        // }
    }

    /// <summary>
    /// Represents a probability distribution over a specified domain (Continuous, DiscreteInteger, or Truth).
    /// Handles uncertainty using an EPSILON tolerance and provides methods for adding points/ranges,
    /// querying probabilities (including interpolation near boundaries), and validation.
    /// </summary>
    public class ProbabilityDistribution
    {
        // Setters are now private to enforce validation via AddPoint/AddRange or JsonConstructor
        /// <summary>
        /// Gets the domain type (Continuous, DiscreteInteger, Truth) for this distribution.
        /// </summary>
        public DomainType DomainType { get; private set; }

        /// <summary>
        /// Gets the string representation of the DomainType. Ignored during serialization.
        /// </summary>
        [JsonIgnore] // Should not be serialized
        public string DomainType_StringRepresentation => DomainType.ToString();

        /// <summary>
        /// Gets the internal list of probability ranges or points defining the distribution.
        /// The list is kept sorted by LowerBound. Direct modification is discouraged; use AddPoint/AddRange.
        /// </summary>
        public List<ProbabilityRange> Distribution { get; private set; }
        private const double EPSILON = 1e-10; // For floating point comparisons

        /// <summary>
        /// Initializes a new, empty ProbabilityDistribution for the specified domain type.
        /// </summary>
        /// <param name="domainType">The domain type for this distribution.</param>
        public ProbabilityDistribution(DomainType domainType)
        {
            DomainType = domainType;
            Distribution = new List<ProbabilityRange>(); // Initialize with new type
        }

        // Constructor for JSON deserialization - performs validation on the loaded data
        [JsonConstructor]
        public ProbabilityDistribution(DomainType domainType, List<ProbabilityRange> distribution) // Changed parameter type
        {
            DomainType = domainType;
            Distribution = distribution ?? new List<ProbabilityRange>(); // Handle null input, use new type

            // Validate the entire loaded distribution state
            ValidateLoadedDistribution();
        }

        /// <summary>
        /// Adds a probability point for a specific value. Only valid for DiscreteInteger domains.
        /// </summary>
        /// <param name="value">The discrete integer value.</param>
        /// <param name="probability">The probability associated with the value (must be between 0 and 1).</param>
        /// <exception cref="ArgumentException">Thrown if probability is invalid or value is not an integer.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the domain is not DiscreteInteger, if the point already exists, or if total probability exceeds 1.</exception>
        public void AddPoint(double value, double probability)
        {
            if (probability < 0 || probability > 1)
                throw new ArgumentException("Probability must be between 0 and 1");

            if (DomainType == DomainType.DiscreteInteger && !IsInteger(value))
                throw new ArgumentException("Value must be an integer for DiscreteInteger domain");
            
            // Disallow AddPoint for Truth domain as per issues.md TODO #5/#12
            if (DomainType == DomainType.Truth)
                throw new InvalidOperationException("Use AddRange for Truth domain. Point probabilities can be approximated with narrow ranges.");

            // Only proceed for DiscreteInteger
            if (DomainType == DomainType.DiscreteInteger) 
            {
                // Check for duplicate points
                if (Distribution.Any(d => Math.Abs(d.LowerBound - value) < EPSILON))
                    throw new InvalidOperationException($"A probability is already defined for value {value}");
                
                // Insert sorted by LowerBound (value)
                int index = FindInsertionIndex(value);
                // Use ProbabilityRange struct
                Distribution.Insert(index, new ProbabilityRange { LowerBound = value, UpperBound = value, Probability = probability }); 
                ValidateTotalProbability();
            }
            else
            {
                throw new InvalidOperationException("Use AddRange for Continuous domain");
            }
        }

        /// <summary>
        /// Adds a probability range [lowerBound, upperBound] with a uniform probability density over that range.
        /// Only valid for Continuous or Truth domains.
        /// </summary>
        /// <param name="lowerBound">The lower bound of the range.</param>
        /// <param name="upperBound">The upper bound of the range.</param>
        /// <param name="probability">The probability associated with the range (must be between 0 and 1).</param>
        /// <exception cref="ArgumentException">Thrown if probability is invalid, range width is too small, or bounds are invalid for the Truth domain.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the domain is DiscreteInteger, if the range overlaps with an existing range, or if total probability exceeds 1.</exception>
        public void AddRange(double lowerBound, double upperBound, double probability)
        {
            // Basic validation
            if (probability < 0 || probability > 1)
                throw new ArgumentException("Probability must be between 0 and 1");

            // Width check must happen first
            if (upperBound - lowerBound < 5 * EPSILON)
                throw new ArgumentException("Range width must be at least 5*EPSILON");

            // Domain-specific validation
            switch (DomainType)
            {
                case DomainType.Truth:
                    if (lowerBound < 0 || upperBound > 1)
                        throw new ArgumentException("Truth domain bounds must be within [0,1]");
                    break;
                case DomainType.DiscreteInteger:
                    throw new InvalidOperationException("AddRange is only valid for Truth or Continuous domains");
                case DomainType.Continuous:
                    break;
            }

            // TODO: Relax this validation (See issues.md TODO #2)
            // TODO: Relax this validation (See issues.md TODO #2) - Temporarily commented out
            // // Check for too-close ranges (Prevents perfectly adjacent ranges)
            // if (Distribution.Any(d => 
            //     Math.Abs(lowerBound - d.UpperBound) < EPSILON || 
            //     Math.Abs(upperBound - d.LowerBound) < EPSILON))
            // {
            //     throw new InvalidOperationException("Range boundaries too close to existing range");
            // }

            // Check for overlapping ranges. 
            // Overlap occurs if the new range's start is before an existing range's end,
            // AND the new range's end is after that existing range's start.
            // Allow perfectly adjacent ranges (gap = 0).
            if (Distribution.Any(d => 
                lowerBound < d.UpperBound - EPSILON && upperBound > d.LowerBound + EPSILON))
            {
                throw new InvalidOperationException("New range overlaps with an existing range.");
            }

            // Insert sorted by LowerBound
            int index = FindInsertionIndex(lowerBound);
             // Use ProbabilityRange struct
            Distribution.Insert(index, new ProbabilityRange { LowerBound = lowerBound, UpperBound = upperBound, Probability = probability });
            ValidateTotalProbability();
        }

        /// <summary>
        /// Checks if the total probability defined in the distribution sums to 1 (within EPSILON).
        /// Note: This does NOT check if the ranges/points cover the entire domain without invalid gaps.
        /// Use AreRangesContiguousAndValid() for that check.
        /// </summary>
        /// <returns>True if total probability is approximately 1, false otherwise.</returns>
        public bool IsComplete()
        {
            if (Distribution.Count == 0)
                return false; // An empty distribution cannot sum to 1

            return Math.Abs(GetTotalProbability() - 1) < EPSILON;
        }

        /// <summary>
        /// Checks if the defined ranges or points form a valid, contiguous sequence according to domain rules.
        /// - For Continuous: Checks if gaps between sorted ranges are all strictly greater than EPSILON.
        /// - For DiscreteInteger: Checks if sorted points are consecutive integers (within EPSILON).
        /// - For Truth: Returns true (no contiguity requirement beyond non-overlap enforced by AddRange/AddPoint).
        /// Note: This does NOT check if the total probability sums to 1. Use IsComplete() for that check.
        /// </summary>
        /// <returns>True if ranges/points are contiguous and validly spaced, false otherwise.</returns>
        public bool AreRangesContiguousAndValid()
        {
             if (Distribution.Count <= 1) // Single point/range is always contiguous
                return true;

            // Distribution is already sorted by LowerBound due to insertion logic
            var ranges = Distribution; // No need to OrderBy again

            switch (DomainType)
            {
                 case DomainType.Truth:
                     // For Truth domain, AddRange/AddPoint already enforce non-overlap.
                     // Contiguity isn't strictly required unless it aims to cover [0,1] fully,
                     // which would be checked by IsComplete() and the range bounds themselves.
                     // We consider any validly added set of non-overlapping ranges/points as "contiguous and valid" here.
                     return true; 

                case DomainType.DiscreteInteger:
                    // Check if values are consecutive integers
                    for (int i = 1; i < ranges.Count; i++)
                    {
                        // Check if difference is approximately 1
                        if (Math.Abs(ranges[i].LowerBound - ranges[i - 1].LowerBound - 1) > EPSILON)
                            return false;
                    }
                    return true; // Passed all checks

                case DomainType.Continuous:
                    // Check for gaps between ranges
                    // Gaps must be strictly greater than EPSILON
                    for (int i = 1; i < ranges.Count; i++)
                    {
                        double gap = ranges[i].LowerBound - ranges[i - 1].UpperBound;
                        if (gap <= EPSILON) // Gap must be > EPSILON
                            return false; 
                    }
                    return true; // Passed all checks

                default:
                    throw new InvalidOperationException("Unknown domain type");
            }
        }

        /// <summary>
        /// Validates the state of the Distribution list after it has been loaded (e.g., by deserialization).
        /// Checks for sorting, overlaps, domain consistency, and total probability.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the loaded state is invalid.</exception>
        private void ValidateLoadedDistribution()
        {
            // 1. Check Total Probability
            double totalProbability = GetTotalProbability(); // Use existing helper
            if (totalProbability > 1 + EPSILON)
                throw new InvalidOperationException($"Loaded distribution is invalid: Total probability {totalProbability} exceeds 1.");

            // 2. Check Sorting and Overlaps/Gaps/Consistency
            if (Distribution.Count > 1)
            {
                for (int i = 1; i < Distribution.Count; i++)
                {
                    // Access properties via the ProbabilityRange struct
                    var prev = Distribution[i - 1];
                    var curr = Distribution[i];

                    // Check sorting (should be guaranteed by FindInsertionIndex if built incrementally, but check for loaded data)
                    if (curr.LowerBound < prev.LowerBound - EPSILON)
                        throw new InvalidOperationException($"Loaded distribution is invalid: List is not sorted by LowerBound at index {i}. Prev={prev.LowerBound}, Curr={curr.LowerBound}.");

                    // Check for significant overlaps (same logic as AddRange)
                    if (curr.LowerBound < prev.UpperBound - EPSILON)
                        throw new InvalidOperationException($"Loaded distribution is invalid: Overlap detected between index {i-1} (ends {prev.UpperBound}) and {i} (starts {curr.LowerBound}).");

                    // Check domain-specific rules
                    switch (DomainType)
                    {
                        case DomainType.DiscreteInteger:
                            if (!IsInteger(prev.LowerBound) || !IsInteger(curr.LowerBound))
                                throw new InvalidOperationException($"Loaded distribution is invalid: Non-integer value found at index {i} for DiscreteInteger domain.");
                            // Optional: Could re-check consecutive integers here if needed, but AreRangesContiguousAndValid does it.
                            break;
                        case DomainType.Truth:
                            if (prev.LowerBound < 0 || prev.UpperBound > 1 || curr.LowerBound < 0 || curr.UpperBound > 1)
                                throw new InvalidOperationException($"Loaded distribution is invalid: Bounds outside [0,1] found at index {i} for Truth domain.");
                            break;
                        case DomainType.Continuous:
                            // Optional: Could re-check gap > EPSILON here if needed, but AreRangesContiguousAndValid does it.
                            break;
                    }
                }
            }

            // 3. Check individual range/point validity (e.g., width, domain bounds for single entries)
            foreach (var item in Distribution) // item is now ProbabilityRange
            {
                 if (item.Probability < 0 || item.Probability > 1)
                     throw new InvalidOperationException($"Loaded distribution is invalid: Probability {item.Probability} outside [0,1] found for range [{item.LowerBound},{item.UpperBound}].");

                 if (DomainType == DomainType.DiscreteInteger)
                 {
                     if (!IsInteger(item.LowerBound)) // UpperBound is same as LowerBound for points
                         throw new InvalidOperationException($"Loaded distribution is invalid: Non-integer value {item.LowerBound} found for DiscreteInteger domain.");
                 }
                 else // Continuous or Truth (Ranges)
                 {
                     // Check width using struct properties
                     if (item.UpperBound - item.LowerBound < 5 * EPSILON)
                         throw new InvalidOperationException($"Loaded distribution is invalid: Range width too small for [{item.LowerBound},{item.UpperBound}].");
                     // Check Truth bounds using struct properties
                     if (DomainType == DomainType.Truth && (item.LowerBound < 0 || item.UpperBound > 1))
                         throw new InvalidOperationException($"Loaded distribution is invalid: Bounds outside [0,1] found for Truth domain range [{item.LowerBound},{item.UpperBound}].");
                 }
            }

            // If all checks pass, the loaded distribution is considered valid.
        }


        /// <summary>
        /// Gets the probability for a specific value.
        /// - For DiscreteInteger domains, returns the probability of the specific point if defined, otherwise 0.
        /// - For Continuous/Truth domains:
        ///   - If the value falls clearly within one defined range (more than EPSILON from any boundary of any other range), returns that range's probability.
        ///   - If the value falls within EPSILON of two or more boundaries between two defined ranges (ambiguous point), 
        ///     this method returns a linearly interpolated probability between the probabilities of the two adjacent ranges.
        ///     A warning is logged via DebugWriter when interpolation occurs.
        ///   - If the value falls outside all defined ranges (more than EPSILON from any boundary), returns 0.
        /// </summary>
        /// <param name="value">The value to query.</param>
        /// <returns>The probability associated with the value, potentially interpolated for Continuous domains near boundaries, or 0 if undefined.</returns>
        public double GetProbability(double value)
        {
            var coveringRangesIndices = GetCoveringRanges(value);

            if (coveringRangesIndices.Count == 0)
            {
                // Point is not covered by any range (considering EPSILON tolerance)
                // This might happen in an incomplete distribution. Return 0 probability.
                // Callers can check IsComplete() and AreRangesContiguousAndValid() if full coverage is expected.
                return 0;
            }
            else if (coveringRangesIndices.Count == 1)
            {
                // Point clearly falls within one range
                // Access Probability via struct property
                return Distribution[coveringRangesIndices[0]].Probability;
            }
            else // Count is 2 (Point is ambiguous, falling within EPSILON of two boundaries)
            {
                // Implement linear interpolation (TODO #4)
                int index1 = coveringRangesIndices[0];
                int index2 = coveringRangesIndices[1];
                // Access ranges as ProbabilityRange structs
                var range1 = Distribution[index1];
                var range2 = Distribution[index2];

                // Ensure range1 is the lower range if indices weren't guaranteed sorted by GetCoveringRanges (though they should be)
                // Compare struct properties
                if (range1.LowerBound > range2.LowerBound)
                {
                    (range1, range2) = (range2, range1); // Swap if needed
                    (index1, index2) = (index2, index1);
                }

                // Access struct properties
                double zoneStart = range1.UpperBound;
                double zoneEnd = range2.LowerBound;
                double zoneWidth = zoneEnd - zoneStart;

                // Log a warning about ambiguity and interpolation
                // Access struct properties for logging
                DebugUtils.DebugWriter.DebugWriteLine("#PROB01#", // Corrected Debug ID format 
                    $"Ambiguous point {value} between range {index1} [{range1.LowerBound},{range1.UpperBound}] (P={range1.Probability}) and range {index2} [{range2.LowerBound},{range2.UpperBound}] (P={range2.Probability}). Interpolating.");

                if (zoneWidth <= EPSILON) // Should not happen with gap > EPSILON, but handle defensively
                {
                    // If gap is negligible, arbitrarily return the average or one of the probabilities.
                    // Returning the probability of the range the point is closer to might be slightly better.
                     double dist1 = Math.Abs(value - zoneStart);
                     double dist2 = Math.Abs(value - zoneEnd);
                     // Access struct properties
                     return (dist1 <= dist2) ? range1.Probability : range2.Probability;
                }

                double positionInZone = value - zoneStart;
                double t = positionInZone / zoneWidth;

                // Clamp t to [0, 1] to handle cases where 'value' might be slightly outside the strict gap due to EPSILON checks
                t = Math.Max(0.0, Math.Min(1.0, t)); 

                // Access struct properties for interpolation
                double interpolatedProbability = range1.Probability * (1.0 - t) + range2.Probability * t;

                return interpolatedProbability;
            }
        }

        /// <summary>
        /// Gets the total probability defined within the specified bounds [lowerBound, upperBound].
        /// For DiscreteInteger/Truth domains, sums probabilities of points within the bounds (inclusive, using EPSILON tolerance).
        /// For Continuous domains, currently only sums probabilities of ranges that exactly match the query bounds (within EPSILON).
        /// </summary>
        /// <param name="lowerBound">The lower bound of the query range.</param>
        /// <param name="upperBound">The upper bound of the query range.</param>
        /// <returns>The total probability within the specified bounds.</returns>
        /// <exception cref="ArgumentException">Thrown if lowerBound >= upperBound.</exception>
        /// <exception cref="InvalidOperationException">Thrown for unknown domain types.</exception>
        public double GetProbability(double lowerBound, double upperBound)
        {
            if (lowerBound >= upperBound)
                throw new ArgumentException("Upper bound must be greater than lower bound");

            switch (DomainType)
            {
                case DomainType.Truth:
                case DomainType.DiscreteInteger:
                    // Access struct properties in LINQ query
                    return Distribution
                        .Where(d => d.LowerBound >= lowerBound - EPSILON && d.UpperBound <= upperBound + EPSILON)
                        .Sum(d => d.Probability);

                case DomainType.Continuous:
                    // For now, only handle cases where query range exactly matches stored ranges
                    // Could be extended to handle partial overlaps if needed
                    // Access struct properties in LINQ query
                    return Distribution
                        .Where(d => Math.Abs(d.LowerBound - lowerBound) < EPSILON && 
                                  Math.Abs(d.UpperBound - upperBound) < EPSILON)
                        .Sum(d => d.Probability);

                default:
                    throw new InvalidOperationException("Unknown domain type");
            }
        }

        // Consider if this method is still needed, or if callers can use the public Distribution property directly.
        // If kept, update return type to reflect the internal structure.
        /// <summary>
        /// Gets a read-only view of the internal distribution list.
        /// </summary>
        /// <returns>A read-only list of ProbabilityRange structs.</returns>
        public IReadOnlyList<ProbabilityRange> GetDistribution()
        {
            return Distribution.AsReadOnly(); // Return list of structs
        }

        private double GetTotalProbability()
        {
            // Access struct property
            return Distribution.Sum(d => d.Probability);
        }

        private void ValidateTotalProbability()
        {
            double total = GetTotalProbability();
            if (total > 1 + EPSILON)
                throw new InvalidOperationException("Total probability cannot exceed 1");
        }

        private bool IsInteger(double value) => Math.Abs(value % 1) < EPSILON;

        /// <summary>
        /// Returns a description of how the probability distribution is quantized.
        /// For discrete domains (Truth and DiscreteInteger), returns the set of points with non-zero probability.
        /// For continuous domains, returns the ranges over which the probability density is defined.
        /// The returned values are ordered by position on the number line.
        /// </summary>
        public IReadOnlyList<(double Start, double End)> GetQuantization()
        {
            // Select from struct properties
            var result = Distribution
                .Select(d => (d.LowerBound, d.UpperBound))
                .OrderBy(range => range.LowerBound) // OrderBy still works on the tuple created by Select
                .ToList();

            return result.AsReadOnly();
        }

        /// <summary>
        /// Returns a description of how the probability distribution is quantized, including probabilities.
        /// Similar to GetQuantization, but includes the probability for each point/range.
        /// The returned values are ordered by position on the number line.
        /// </summary>
        /// <returns>A read-only list of tuples representing the quantized ranges/points and their probabilities.</returns>
        public IReadOnlyList<(double Start, double End, double Probability)> GetQuantizationWithProbabilities()
        {
             // Select from struct properties
            var result = Distribution
                .Select(d => (d.LowerBound, d.UpperBound, d.Probability))
                .OrderBy(range => range.LowerBound) // OrderBy still works on the tuple created by Select
                .ToList();

            return result.AsReadOnly();
        }

        // Removed GetContainingRange as its single-assignment logic is less useful than GetCoveringRanges for smoothing.

        /// <summary>
        /// Returns a list of indices of ranges that cover the given point, considering boundary ambiguity.
        /// A range `[L, U]` is considered covering if the point `p` satisfies `p >= L - EPSILON && p <= U + EPSILON`.
        /// This means a point within EPSILON of two ranges' boundaries will be covered by both,
        /// enabling interpolation in GetProbability.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <returns>A list of indices of the covering ranges.</returns>
        public List<int> GetCoveringRanges(double point)
        {
            var coveringRanges = new List<int>();

            for (int i = 0; i < Distribution.Count; i++)
            {
                // Access struct properties
                var range = Distribution[i];
                
                // Check if point might be in this range (Original logic restored again)
                // Access struct properties
                if (point <= range.UpperBound + EPSILON && 
                    point >= range.LowerBound - EPSILON)
                {
                    coveringRanges.Add(i);
                }
            }

            return coveringRanges;
        }

        /// <summary>
        /// Finds the correct index to insert a new element to maintain sorted order by LowerBound.
        /// Uses BinarySearch for efficiency.
        /// </summary>
        private int FindInsertionIndex(double lowerBound)
        {
            // Perform binary search using ProbabilityRange and comparing LowerBound
            // Create a dummy ProbabilityRange just for the comparison key
            var searchKey = new ProbabilityRange { LowerBound = lowerBound }; 
            int index = Distribution.BinarySearch(searchKey, Comparer<ProbabilityRange>.Create((x, y) => x.LowerBound.CompareTo(y.LowerBound)));

            // If BinarySearch returns a non-negative value, it's the index of an exact match (or where it would be).
            // If it returns a negative value, it's the bitwise complement of the index of the first element larger than the item.
            if (index < 0)
            {
                index = ~index; // Bitwise complement gives the insertion point
            }
            // Handle cases where multiple items might have the same LowerBound (though unlikely for ranges)
            // Ensure we insert *after* any existing items with the same LowerBound for stability if needed,
            // but for simple insertion, this index is correct.

            return index;
        }
    }
}
