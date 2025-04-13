using NUnit.Framework;
using ReasoningEngine;
using System;

namespace ReasoningEngineTests
{
    [TestFixture]
    public class ProbabilityDistributionTests
    {
        [Test]
        public void TestGetQuantization_DiscreteInteger()
        {
            var distribution = new ProbabilityDistribution(DomainType.DiscreteInteger);
            distribution.AddPoint(1, 0.3);
            distribution.AddPoint(2, 0.3);
            distribution.AddPoint(3, 0.4);

            var quantization = distribution.GetQuantization();
            Assert.That(quantization, Has.Count.EqualTo(3));
            
            // Points should be ordered
            Assert.Multiple(() =>
            {
                Assert.That(quantization[0], Is.EqualTo((1.0, 1.0)));
                Assert.That(quantization[1], Is.EqualTo((2.0, 2.0)));
                Assert.That(quantization[2], Is.EqualTo((3.0, 3.0)));
            });
        }

        [Test]
        public void TestGetQuantization_Truth()
        {
            var distribution = new ProbabilityDistribution(DomainType.Truth);
            // Use AddRange with narrow ranges to represent points for Truth domain
            double epsilon = 1e-10; // Match the EPSILON constant in ProbabilityDistribution
            double rangeWidth = 5 * epsilon; // Minimum allowed width
            distribution.AddRange(0.0, rangeWidth, 0.4); // Represent point 0.0
            distribution.AddRange(1.0 - rangeWidth, 1.0, 0.6); // Represent point 1.0

            var quantization = distribution.GetQuantization();
            Assert.That(quantization, Has.Count.EqualTo(2));
            
            // Assertions check for the ranges now
            Assert.Multiple(() =>
            {
                Assert.That(quantization[0], Is.EqualTo((0.0, rangeWidth)));
                Assert.That(quantization[1], Is.EqualTo((1.0 - rangeWidth, 1.0)));
            });
        }

        [Test]
        public void TestGetQuantization_Continuous()
        {
            var distribution = new ProbabilityDistribution(DomainType.Continuous);
            // Add ranges with the required gap (> EPSILON) between them
            double epsilon = 1e-10; // Match the EPSILON constant in ProbabilityDistribution
            distribution.AddRange(0.0, 1.0, 0.3);
            distribution.AddRange(1.0 + 2 * epsilon, 2.0, 0.3);
            distribution.AddRange(2.0 + 2 * epsilon, 3.0, 0.4);

            var quantization = distribution.GetQuantization();
            Assert.That(quantization, Has.Count.EqualTo(3));
            
            Assert.Multiple(() =>
            {
                Assert.That(quantization[0], Is.EqualTo((0.0, 1.0)));
                Assert.That(quantization[1], Is.EqualTo((1.0 + 2 * epsilon, 2.0)));
                Assert.That(quantization[2], Is.EqualTo((2.0 + 2 * epsilon, 3.0)));
            });
        }

        [Test]
        public void TestGetQuantizationWithProbabilities_DiscreteInteger()
        {
            var distribution = new ProbabilityDistribution(DomainType.DiscreteInteger);
            distribution.AddPoint(1, 0.3);
            distribution.AddPoint(2, 0.3);
            distribution.AddPoint(3, 0.4);

            var quantization = distribution.GetQuantizationWithProbabilities();
            Assert.That(quantization, Has.Count.EqualTo(3));
            
            Assert.Multiple(() =>
            {
                Assert.That(quantization[0], Is.EqualTo((1.0, 1.0, 0.3)));
                Assert.That(quantization[1], Is.EqualTo((2.0, 2.0, 0.3)));
                Assert.That(quantization[2], Is.EqualTo((3.0, 3.0, 0.4)));
            });
        }

        [Test]
        public void TestGetQuantizationWithProbabilities_Truth()
        {
            var distribution = new ProbabilityDistribution(DomainType.Truth);
            // Use AddRange with narrow ranges to represent points for Truth domain
            double epsilon = 1e-10; // Match the EPSILON constant in ProbabilityDistribution
            double rangeWidth = 5 * epsilon; // Minimum allowed width
            distribution.AddRange(0.0, rangeWidth, 0.4); // Represent point 0.0
            distribution.AddRange(1.0 - rangeWidth, 1.0, 0.6); // Represent point 1.0

            var quantization = distribution.GetQuantizationWithProbabilities();
            Assert.That(quantization, Has.Count.EqualTo(2));
            
            // Assertions check for the ranges and probabilities now
            Assert.Multiple(() =>
            {
                Assert.That(quantization[0], Is.EqualTo((0.0, rangeWidth, 0.4)));
                Assert.That(quantization[1], Is.EqualTo((1.0 - rangeWidth, 1.0, 0.6)));
            });
        }

        [Test]
        public void TestGetQuantizationWithProbabilities_Continuous()
        {
            var distribution = new ProbabilityDistribution(DomainType.Continuous);
            // Add ranges with the required gap (> EPSILON) between them
            double epsilon = 1e-10; // Match the EPSILON constant in ProbabilityDistribution
            distribution.AddRange(0.0, 1.0, 0.3);
            distribution.AddRange(1.0 + 2 * epsilon, 2.0, 0.3);
            distribution.AddRange(2.0 + 2 * epsilon, 3.0, 0.4);

            var quantization = distribution.GetQuantizationWithProbabilities();
            Assert.That(quantization, Has.Count.EqualTo(3));
            
            Assert.Multiple(() =>
            {
                Assert.That(quantization[0], Is.EqualTo((0.0, 1.0, 0.3)));
                Assert.That(quantization[1], Is.EqualTo((1.0 + 2 * epsilon, 2.0, 0.3)));
                Assert.That(quantization[2], Is.EqualTo((2.0 + 2 * epsilon, 3.0, 0.4)));
            });
        }

        [Test]
        public void TestGetQuantization_EmptyDistribution()
        {
            var distribution = new ProbabilityDistribution(DomainType.DiscreteInteger);
            var quantization = distribution.GetQuantization();
            Assert.That(quantization, Is.Empty);
        }

        [Test]
        public void TestGetQuantizationWithProbabilities_EmptyDistribution()
        {
            var distribution = new ProbabilityDistribution(DomainType.DiscreteInteger);
            var quantization = distribution.GetQuantizationWithProbabilities();
            Assert.That(quantization, Is.Empty);
        }

        [Test]
        public void TestGetQuantization_OrderPreservation()
        {
            var distribution = new ProbabilityDistribution(DomainType.DiscreteInteger);
            // Add points in non-sequential order
            distribution.AddPoint(3, 0.3);
            distribution.AddPoint(1, 0.3);
            distribution.AddPoint(2, 0.4);

            var quantization = distribution.GetQuantization();
            Assert.That(quantization, Has.Count.EqualTo(3));
            
            // Should be ordered regardless of insertion order
            Assert.Multiple(() =>
            {
                Assert.That(quantization[0], Is.EqualTo((1.0, 1.0)));
                Assert.That(quantization[1], Is.EqualTo((2.0, 2.0)));
                Assert.That(quantization[2], Is.EqualTo((3.0, 3.0)));
            });
        }

        [Test]
        public void TestAddPoint_NegativeProbability()
        {
            var distribution = new ProbabilityDistribution(DomainType.DiscreteInteger);
            Assert.Throws<ArgumentException>(() => distribution.AddPoint(1, -0.1));
        }

        [Test]
        public void TestAddPoint_ProbabilityGreaterThanOne()
        {
            var distribution = new ProbabilityDistribution(DomainType.DiscreteInteger);
            Assert.Throws<ArgumentException>(() => distribution.AddPoint(1, 1.1));
        }

        [Test]
        public void TestAddPoint_DuplicatePoint()
        {
            var distribution = new ProbabilityDistribution(DomainType.DiscreteInteger);
            distribution.AddPoint(1, 0.5);
            Assert.Throws<InvalidOperationException>(() => distribution.AddPoint(1, 0.3));
        }

        [Test]
        public void TestAddRange_NegativeProbability()
        {
            var distribution = new ProbabilityDistribution(DomainType.Continuous);
            Assert.Throws<ArgumentException>(() => distribution.AddRange(0, 1, -0.1));
        }

        [Test]
        public void TestAddRange_ProbabilityGreaterThanOne()
        {
            var distribution = new ProbabilityDistribution(DomainType.Continuous);
            Assert.Throws<ArgumentException>(() => distribution.AddRange(0, 1, 1.1));
        }

        [Test]
        public void TestAddRange_InvalidBounds()
        {
            var distribution = new ProbabilityDistribution(DomainType.Continuous);
            Assert.Throws<ArgumentException>(() => distribution.AddRange(1, 0, 0.5));
        }

        [Test]
        public void TestAddRange_OverlappingRanges()
        {
            var distribution = new ProbabilityDistribution(DomainType.Continuous);
            distribution.AddRange(0, 2, 0.5);
            Assert.Throws<InvalidOperationException>(() => distribution.AddRange(1, 3, 0.5));
        }

        [Test]
        public void TestAddPoint_NonIntegerValueForDiscreteInteger()
        {
            var distribution = new ProbabilityDistribution(DomainType.DiscreteInteger);
            Assert.Throws<ArgumentException>(() => distribution.AddPoint(1.5, 0.5));
        }

        [Test]
        public void TestAddPoint_OutOfRangeValueForTruth() // Test name is now slightly misleading, but reflects original intent
        {
            // This test now verifies that AddPoint is disallowed for Truth domain,
            // regardless of the value provided, as per issues.md TODO #5/#12.
            var distribution = new ProbabilityDistribution(DomainType.Truth);
            // Expect InvalidOperationException because AddPoint is disallowed for Truth
            Assert.Throws<InvalidOperationException>(() => distribution.AddPoint(0.5, 0.5)); 
            Assert.Throws<InvalidOperationException>(() => distribution.AddPoint(1.5, 0.5)); // Also check original out-of-range case
        }

        [Test]
        public void TestAddPoint_InvalidDomainTypeForAddRange()
        {
            var distribution = new ProbabilityDistribution(DomainType.DiscreteInteger);
            Assert.Throws<InvalidOperationException>(() => distribution.AddRange(0, 1, 0.5));
        }

        [Test]
        public void TestAddRange_InvalidDomainTypeForAddPoint()
        {
            var distribution = new ProbabilityDistribution(DomainType.Continuous);
            Assert.Throws<InvalidOperationException>(() => distribution.AddPoint(1, 0.5));
        }

        [Test]
        public void TestTotalProbabilityExceedsOne()
        {
            var distribution = new ProbabilityDistribution(DomainType.DiscreteInteger);
            distribution.AddPoint(1, 0.6);
            Assert.Throws<InvalidOperationException>(() => distribution.AddPoint(2, 0.5));
        }
    }
}
