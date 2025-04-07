using System;
using System.Collections.Generic;
using System.Linq;

namespace ReasoningEngine
{
    public class ProbabilityDistribution
    {
        public DomainType DomainType { get; private set; }
        // Add string representation property
        public string DomainType_StringRepresentation => DomainType.ToString();
        private List<(double LowerBound, double UpperBound, double Probability)> Distribution { get; set; }
        private const double EPSILON = 1e-10; // For floating point comparisons

        public ProbabilityDistribution(DomainType domainType)
        {
            DomainType = domainType;
            Distribution = new List<(double, double, double)>();
        }

        public void AddPoint(double value, double probability)
        {
            if (probability < 0 || probability > 1)
                throw new ArgumentException("Probability must be between 0 and 1");

            if (DomainType == DomainType.DiscreteInteger && !IsInteger(value))
                throw new ArgumentException("Value must be an integer for DiscreteInteger domain");
            
            if (DomainType == DomainType.Truth && (value < 0 || value > 1))
                throw new ArgumentException("Value must be between 0 and 1 for Truth domain");

            if (DomainType == DomainType.DiscreteInteger || DomainType == DomainType.Truth)
            {
                // Check for duplicate points
                if (Distribution.Any(d => Math.Abs(d.LowerBound - value) < EPSILON))
                    throw new InvalidOperationException($"A probability is already defined for value {value}");
                
                // Insert sorted by LowerBound (value)
                int index = FindInsertionIndex(value);
                Distribution.Insert(index, (value, value, probability)); 
                ValidateTotalProbability();
            }
            else
            {
                throw new InvalidOperationException("Use AddRange for Continuous domain");
            }
        }

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
            // Check for too-close ranges (Prevents perfectly adjacent ranges)
            if (Distribution.Any(d => 
                Math.Abs(lowerBound - d.UpperBound) < EPSILON || 
                Math.Abs(upperBound - d.LowerBound) < EPSILON))
            {
                throw new InvalidOperationException("Range boundaries too close to existing range");
            }

            // Check for overlapping ranges. 
            // We need lowerBound > d.UpperBound + EPSILON OR upperBound < d.LowerBound - EPSILON
            // So, overlap occurs if !(lowerBound > d.UpperBound + EPSILON || upperBound < d.LowerBound - EPSILON)
            // which simplifies to (lowerBound <= d.UpperBound + EPSILON && upperBound >= d.LowerBound - EPSILON)
            // This check prevents ranges from having gaps <= EPSILON, enforcing a minimum gap > EPSILON.
            if (Distribution.Any(d => 
                lowerBound <= d.UpperBound + EPSILON && upperBound >= d.LowerBound - EPSILON))
            {
                throw new InvalidOperationException("New range overlaps or is too close to an existing range (gap must be > EPSILON)");
            }

            // Note: The previous "too close" check was redundant with the modified overlap check above.

            // Insert sorted by LowerBound
            int index = FindInsertionIndex(lowerBound);
            Distribution.Insert(index, (lowerBound, upperBound, probability)); 
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
        /// Gets the probability for a specific value.
        /// For Continuous domains, if the value falls within EPSILON of two range boundaries (ambiguous point),
        /// this method currently returns the probability of the first range identified by GetCoveringRanges.
        /// TODO: Implement smoothing/interpolation for ambiguous points (TODO #4).
        /// </summary>
        /// <param name="value">The value to query.</param>
        /// <returns>The probability associated with the value, or 0 if it falls outside defined ranges/points.</returns>
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
                return Distribution[coveringRangesIndices[0]].Probability;
            }
            else // Count is 2 (or potentially more if ranges weren't added correctly, though AddRange prevents overlap)
            {
                // Point is ambiguous, falling within EPSILON of two boundaries.
                // TODO: Implement smoothing/interpolation logic here (TODO #4).
                // For now, return the probability of the first range found (lower LowerBound due to sorted list).
                // Consider adding a warning here as well.
                // DebugWriter.DebugWriteLine("#AMBIGUOUS#", $"Ambiguous point {value} covered by ranges {coveringRangesIndices[0]} and {coveringRangesIndices[1]}");
                return Distribution[coveringRangesIndices[0]].Probability; 
            }
        }

        public double GetProbability(double lowerBound, double upperBound)
        {
            if (lowerBound >= upperBound)
                throw new ArgumentException("Upper bound must be greater than lower bound");

            switch (DomainType)
            {
                case DomainType.Truth:
                case DomainType.DiscreteInteger:
                    return Distribution
                        .Where(d => d.LowerBound >= lowerBound - EPSILON && d.UpperBound <= upperBound + EPSILON)
                        .Sum(d => d.Probability);

                case DomainType.Continuous:
                    // For now, only handle cases where query range exactly matches stored ranges
                    // Could be extended to handle partial overlaps if needed
                    return Distribution
                        .Where(d => Math.Abs(d.LowerBound - lowerBound) < EPSILON && 
                                  Math.Abs(d.UpperBound - upperBound) < EPSILON)
                        .Sum(d => d.Probability);

                default:
                    throw new InvalidOperationException("Unknown domain type");
            }
        }

        public IReadOnlyList<(double LowerBound, double UpperBound, double Probability)> GetDistribution()
        {
            return Distribution.AsReadOnly();
        }

        private double GetTotalProbability()
        {
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
            var result = Distribution
                .Select(d => (d.LowerBound, d.UpperBound))
                .OrderBy(range => range.LowerBound)
                .ToList();

            return result.AsReadOnly();
        }

        public IReadOnlyList<(double Start, double End, double Probability)> GetQuantizationWithProbabilities()
        {
            var result = Distribution
                .Select(d => (d.LowerBound, d.UpperBound, d.Probability))
                .OrderBy(range => range.LowerBound)
                .ToList();

            return result.AsReadOnly();
        }

        // Removed GetContainingRange as its single-assignment logic is less useful than GetCoveringRanges for smoothing.

        /// <summary>
        /// Returns a list of indices of ranges that might contain the given point,
        /// taking into account measurement uncertainty.
        /// </summary>
        public List<int> GetCoveringRanges(double point)
        {
            var coveringRanges = new List<int>();

            for (int i = 0; i < Distribution.Count; i++)
            {
                var range = Distribution[i];
                
                // Check if point might be in this range
                if (point - EPSILON <= range.UpperBound + EPSILON && 
                    point + EPSILON >= range.LowerBound - EPSILON)
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
            // Perform binary search to find the index of the first element >= lowerBound
            int index = Distribution.BinarySearch(0, Distribution.Count, (lowerBound, 0, 0), Comparer<(double LowerBound, double UpperBound, double Probability)>.Create((x, y) => x.LowerBound.CompareTo(y.LowerBound)));

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
