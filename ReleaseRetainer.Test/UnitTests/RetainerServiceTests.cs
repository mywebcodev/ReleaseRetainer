using NSubstitute;
using NUnit.Framework;
using ReleaseRetainer.Strategies;
using ReleaseRetainer.Test.Builders;

namespace ReleaseRetainer.Test.UnitTests;

[TestFixture]
public class RetainerServiceTests
{
    private static readonly ReleaseRetainOptionsBuilder ReleaseRetainOptionsBuilder = new();
    private IReleaseRetentionStrategy _releaseRetentionStrategy;
    private RetainerService _systemUnderTest;

    [SetUp]
    public void SetUp()
    {
        _releaseRetentionStrategy = Substitute.For<IReleaseRetentionStrategy>();
        _systemUnderTest = new RetainerService(_releaseRetentionStrategy);
    }

    [Test]
    public void RetainReleases_ShouldCallReleaseRetentionStrategyRetainReleases()
    {
        // Arrange
        var options = ReleaseRetainOptionsBuilder.CreateRandom().Build();

        // Act
        _systemUnderTest.RetainReleases(options);

        // Assert
        _releaseRetentionStrategy.Received(1).RetainReleases(options);
    }
}