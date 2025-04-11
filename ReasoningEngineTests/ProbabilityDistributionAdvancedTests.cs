using ReasoningEngine;
using NUnit.Framework;

namespace ReasoningEngineTests {
    [TestFixture]
    public class ProbabilityDistributionAdvancedTests
    {
        private const double EPSILON = 1e-10;  // Example value, should match class constant

        [Test]
        public void TestEqualityNonTransitivity()
        {
            // Demonstrate that equality is not transitive with epsilon-based comparison
            double a = 1.0;
            double b = a + 0.6 * EPSILON;
            double c = b + 0.6 * EPSILON;

            Assert.Multiple(() =>
            {
                Assert.That(Math.Abs(a - b) < EPSILON, Is.True, "a should equal b");
                Assert.That(Math.Abs(b - c) < EPSILON, Is.True, "b should equal c");
                Assert.That(Math.Abs(a - c) < EPSILON, Is.False, "a should not equal c");
            });
        }

        [Test]
        public void TestMinimumRangeWidth()
        {
            var distribution = new ProbabilityDistribution(DomainType.Continuous);
            
            // Valid range (width > 5*EPSILON)
            Assert.DoesNotThrow(() => distribution.AddRange(0, 5.1 * EPSILON, 0.5));

            // Invalid range (width = 4.9*EPSILON)
            Assert.Throws<ArgumentException>(() => 
                distribution.AddRange(0, 4.9 * EPSILON, 0.5));
        }

        [Test]
        public void TestRangeEdgeCaseResolution()
        {
            var distribution = new ProbabilityDistribution(DomainType.Continuous);
            
            // Create two adjacent ranges with minimum gap (slightly larger than EPSILON)
            distribution.AddRange(0, 1, 0.5);
            distribution.AddRange(1 + 1.1 * EPSILON, 2, 0.5);  // Small gap to avoid ambiguity

            // Test points near first range's upper bound
            var boundaryPoint = 1.0;
            var slightlyBelow = boundaryPoint - 0.4 * EPSILON;  // Should be in first range
            var slightlyAbove = boundaryPoint + 0.4 * EPSILON;  // Should be in first range (closer to 1 than to 1+1.1*EPSILON)
            var midpoint = boundaryPoint + 0.55 * EPSILON;      // Should be in first range (closer to 1)
            var nearSecondRange = 1 + 0.9 * EPSILON;            // Should be covered by both ranges (within EPSILON of both boundaries)

            Assert.Multiple(() =>
            {
                var coveringBelow = distribution.GetCoveringRanges(slightlyBelow);
                Assert.That(coveringBelow, Has.Count.EqualTo(1), "Point below boundary count");
                Assert.That(coveringBelow, Is.EquivalentTo(new[] { 0 }), "Point below boundary index");

                var coveringAbove = distribution.GetCoveringRanges(slightlyAbove);
                Assert.That(coveringAbove, Has.Count.EqualTo(2), "Point slightly above boundary (within EPSILON of both) count");
                Assert.That(coveringAbove, Is.EquivalentTo(new[] { 0, 1 }), "Point slightly above boundary (within EPSILON of both) indices");
                
                var coveringMid = distribution.GetCoveringRanges(midpoint);
                 Assert.That(coveringMid, Has.Count.EqualTo(2), "Midpoint (within EPSILON of both) count");
                 Assert.That(coveringMid, Is.EquivalentTo(new[] { 0, 1 }), "Midpoint (within EPSILON of both) indices");

                var coveringNearSecond = distribution.GetCoveringRanges(nearSecondRange);
                 Assert.That(coveringNearSecond, Has.Count.EqualTo(2), "Point near second range boundary (within EPSILON of both) count");
                 Assert.That(coveringNearSecond, Is.EquivalentTo(new[] { 0, 1 }), "Point near second range boundary (within EPSILON of both) indices");
            });
        }

        [Test]
        public void TestNoTripleCoverage()
        {
            var distribution = new ProbabilityDistribution(DomainType.Continuous);
            
            // Create three consecutive ranges with required gaps
            distribution.AddRange(0, 1, 0.3);
            distribution.AddRange(1 + 2 * EPSILON, 2, 0.3);
            distribution.AddRange(2 + 2 * EPSILON, 3, 0.4);

            // Test points at various positions
            for (double x = 0; x <= 3; x += 0.1)
            {
                var coveringRanges = distribution.GetCoveringRanges(x);
                Assert.That(coveringRanges.Count, Is.LessThanOrEqualTo(2), 
                    $"Point {x} should not be covered by more than 2 ranges");
            }
        }

        [Test]
        public void TestRangeOrdering()
        {
            var distribution = new ProbabilityDistribution(DomainType.Continuous);
            
            // Add ranges in non-sequential order with required gaps
            distribution.AddRange(2 + 2 * EPSILON, 3, 0.3);
            distribution.AddRange(0, 1, 0.3);
            distribution.AddRange(1 + 2 * EPSILON, 2, 0.4);

            var ranges = distribution.GetQuantization();
            
            // Verify ranges are ordered correctly
            Assert.Multiple(() =>
            {
                Assert.That(ranges[0], Is.EqualTo((0.0, 1.0)));
                Assert.That(ranges[1], Is.EqualTo((1.0 + 2 * EPSILON, 2.0)));
                Assert.That(ranges[2], Is.EqualTo((2.0 + 2 * EPSILON, 3.0)));
            });
        }

        [Test]
        public void TestDomainBoundValidation()
        {
            var truthDistribution = new ProbabilityDistribution(DomainType.Truth);
            
            // Valid truth domain ranges with required gap
            Assert.DoesNotThrow(() => truthDistribution.AddRange(0, 0.5, 0.5));
            Assert.DoesNotThrow(() => truthDistribution.AddRange(0.5 + 2 * EPSILON, 1, 0.5));

            // Create a new distribution for testing invalid ranges
            var truthDistribution2 = new ProbabilityDistribution(DomainType.Truth);
            
            // Invalid truth domain ranges
            Assert.Throws<ArgumentException>(() => 
                truthDistribution2.AddRange(-0.1, 0.5, 0.5), "Should not allow range below 0");
            Assert.Throws<ArgumentException>(() => 
                truthDistribution2.AddRange(0.5, 1.1, 0.5), "Should not allow range above 1");
        }

        [Test]
        public void TestCoveringRangesForPointsInGap() // Renamed test
        {
            var distribution = new ProbabilityDistribution(DomainType.Continuous);

            // Create two ranges with small gap
            distribution.AddRange(0, 1, 0.5);
            distribution.AddRange(1 + 2 * EPSILON, 2, 0.5);

            // Test points in the gap
            var gapPoint1 = 1 + 0.5 * EPSILON;     // Should be in first range (closer to 1)
            var gapPoint2 = 1 + 1.5 * EPSILON;     // Should be in second range (closer to 1+2*EPSILON)
            var gapMidpoint = 1 + EPSILON;         // Equidistant point in gap, should be covered by both

            Assert.Multiple(() =>
            {
                var covering1 = distribution.GetCoveringRanges(gapPoint1);
                 Assert.That(covering1, Has.Count.EqualTo(1), "Gap point 1 (only within EPSILON of range 0) count"); 
                 Assert.That(covering1, Is.EquivalentTo(new[] { 0 }), "Gap point 1 (only within EPSILON of range 0) index"); 

                var covering2 = distribution.GetCoveringRanges(gapPoint2);
                 Assert.That(covering2, Has.Count.EqualTo(1), "Gap point 2 (only within EPSILON of range 1) count"); 
                 Assert.That(covering2, Is.EquivalentTo(new[] { 1 }), "Gap point 2 (only within EPSILON of range 1) index"); 

                var coveringMid = distribution.GetCoveringRanges(gapMidpoint);
                 Assert.That(coveringMid, Has.Count.EqualTo(2), "Gap midpoint (within EPSILON of both) count"); // Equidistant, covered by both
                 Assert.That(coveringMid, Is.EquivalentTo(new[] { 0, 1 }), "Gap midpoint (within EPSILON of both) indices"); 
            });
        }

        [Test]
        public void TestDiscreteIntegerDomainValidation()
        {
            var discreteDistribution = new ProbabilityDistribution(DomainType.DiscreteInteger);

            // Test valid discrete integer points
            Assert.DoesNotThrow(() => discreteDistribution.AddPoint(1, 0.3));
            Assert.DoesNotThrow(() => discreteDistribution.AddPoint(2, 0.3));

            // Test points near integers
            Assert.DoesNotThrow(() => 
                discreteDistribution.AddPoint(3 + 0.1 * EPSILON, 0.4), 
                "Should accept points very close to integers");

            // Test invalid non-integer points
            Assert.Throws<ArgumentException>(() => 
                discreteDistribution.AddPoint(1.5, 0.5), 
                "Should not accept points far from integers");
        }
    }
}
