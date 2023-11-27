using FluentAssertions;
using NUnit.Framework;
using ReleaseRetainer.Models;
using ReleaseRetainer.Test.Builders;

namespace ReleaseRetainer.Test.UnitTests;

[TestFixture]
public class RetainReleaseOptionsTests
{
    private static readonly ReleaseRetainOptionsBuilder ReleaseRetainOptionsBuilder = new();

    private static IEnumerable<TestCaseData> NullableCollectionsTestsCases()
    {
        yield return new TestCaseData(
            () =>
            {
                var options = ReleaseRetainOptionsBuilder.CreateRandom().Build();
                options.Deployments = null;
            },

            $@"{nameof(ReleaseRetainOptions.Deployments)} list cannot be null. (Parameter '{nameof(ReleaseRetainOptions.Deployments)}')")
        { TestName = "NullDeployments" };
        yield return new TestCaseData(
            () =>
            {
                var options = ReleaseRetainOptionsBuilder.CreateRandom().Build();
                options.Environments = null;
            },
            $@"{nameof(ReleaseRetainOptions.Environments)} list cannot be null. (Parameter '{nameof(ReleaseRetainOptions.Environments)}')")
        { TestName = "NullEnvironments" };
        yield return new TestCaseData(
            () =>
            {
                var options = ReleaseRetainOptionsBuilder.CreateRandom().Build();
                options.Projects = null;
            },
            $@"{nameof(ReleaseRetainOptions.Projects)} list cannot be null. (Parameter '{nameof(ReleaseRetainOptions.Projects)}')")
        { TestName = "NullProjects" };
        yield return new TestCaseData(
            () =>
            {
                var options = ReleaseRetainOptionsBuilder.CreateRandom().Build();
                options.Releases = null;
            },
            $@"{nameof(ReleaseRetainOptions.Releases)} list cannot be null. (Parameter '{nameof(ReleaseRetainOptions.Releases)}')")
        { TestName = "NullReleases" };
    }

    [Test]
    [TestCase(-1, TestName = "NumOfReleasesToKeepIsLessThanZero")]
    [TestCase(0, TestName = "NumOfReleasesToKeepIsZero")]
    public void ThrowsArgumentException_WhenNumOfReleasesToKeepIsLessOrEqualsToZero(int numOfReleasesToKeep)
    {
        // Arrange
        const string expectedExceptionMessage = $@"{nameof(ReleaseRetainOptions.NumOfReleasesToKeep)} must be greater than zero. (Parameter '{nameof(ReleaseRetainOptions.NumOfReleasesToKeep)}')";
        var options = ReleaseRetainOptionsBuilder.CreateRandom().Build();

        // Act
        // Assert
        var exception = Assert.Throws<ArgumentException>(() => options.NumOfReleasesToKeep = numOfReleasesToKeep);

        exception.Message.Should().Be(expectedExceptionMessage);
    }

    [Test]
    [TestCaseSource(nameof(NullableCollectionsTestsCases))]
    public void ThrowsArgumentException_WhenExpectedCollectionsAreNull(Action tDelegate, string expectedExceptionMessage)
    {
        // Act
        // Assert
        var exception = Assert.Throws<ArgumentNullException>(tDelegate.Invoke);

        exception.Message.Should().Be(expectedExceptionMessage);
    }
}