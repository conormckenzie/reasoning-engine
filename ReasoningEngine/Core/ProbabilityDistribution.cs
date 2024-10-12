using System;
using System.Collections.Generic;

namespace ReasoningEngine
{
    public enum DomainType
    {
        Continuous,
        DiscreteInteger,
        Truth
    }

    public class ProbabilityDistribution
    {
        public DomainType DomainType { get; private set; }
        private List<(double LowerBound, double UpperBound, double Probability)> Distribution { get; set; }

        public ProbabilityDistribution(DomainType domainType)
        {
            DomainType = domainType;
            Distribution = new List<(double, double, double)>();
        }

        public void AddPoint(double value, double probability)
        {
            if (DomainType == DomainType.DiscreteInteger && !IsInteger(value))
                throw new ArgumentException("Value must be an integer for DiscreteInteger domain");
            
            if (DomainType == DomainType.Truth && (value < 0 || value > 1))
                throw new ArgumentException("Value must be between 0 and 1 for Truth domain");

            if (DomainType == DomainType.DiscreteInteger || DomainType == DomainType.Truth)
            {
                Distribution.Add((value, value, probability));
            }
            else
            {
                throw new InvalidOperationException("Use AddRange for Continuous domain");
            }
        }

        public void AddRange(double lowerBound, double upperBound, double probability)
        {
            if (DomainType != DomainType.Continuous)
                throw new InvalidOperationException("AddRange is only valid for Continuous domain");

            if (lowerBound >= upperBound)
                throw new ArgumentException("Upper bound must be greater than lower bound");

            Distribution.Add((lowerBound, upperBound, probability));
        }

        public IReadOnlyList<(double LowerBound, double UpperBound, double Probability)> GetDistribution()
        {
            return Distribution.AsReadOnly();
        }

        private bool IsInteger(double value) => Math.Abs(value % 1) < double.Epsilon;
    }
}