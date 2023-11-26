using FluentAssertions;
using NUnit.Framework;
using ReleaseRetainer.Models;
using ReleaseRetainer.Test.Builders;

namespace ReleaseRetainer.Test.UnitTests;

[TestFixture]
public class RetainReleaseOptionsTests
{
    private static readonly RetainReleaseOptionsBuilder RetainReleaseOptionsBuilder = new();

    private static IEnumerable<TestCaseData> NullableCollectionsTestsCases()
    {
        yield return new TestCaseData(
            () =>
            {
                var options = RetainReleaseOptionsBuilder.CreateRandom().Build();
                options.Deployments = null;
            },

            $@"{nameof(RetainReleaseOptions.Deployments)} list cannot be null. (Parameter '{nameof(RetainReleaseOptions.Deployments)}')")
        { TestName = "NullDeployments" };
        yield return new TestCaseData(
            () =>
            {
                var options = RetainReleaseOptionsBuilder.CreateRandom().Build();
                options.Environments = null;
            },
            $@"{nameof(RetainReleaseOptions.Environments)} list cannot be null. (Parameter '{nameof(RetainReleaseOptions.Environments)}')")
        { TestName = "NullEnvironments" };
        yield return new TestCaseData(
            () =>
            {
                var options = RetainReleaseOptionsBuilder.CreateRandom().Build();
                options.Projects = null;
            },
            $@"{nameof(RetainReleaseOptions.Projects)} list cannot be null. (Parameter '{nameof(RetainReleaseOptions.Projects)}')")
        { TestName = "NullProjects" };
        yield return new TestCaseData(
            () =>
            {
                var options = RetainReleaseOptionsBuilder.CreateRandom().Build();
                options.Releases = null;
            },
            $@"{nameof(RetainReleaseOptions.Releases)} list cannot be null. (Parameter '{nameof(RetainReleaseOptions.Releases)}')")
        { TestName = "NullReleases" };
    }

    [Test]
    [TestCase(-1, TestName = "NumOfReleasesToKeepIsLessThanZero")]
    [TestCase(0, TestName = "NumOfReleasesToKeepIsZero")]
    public void ThrowsArgumentException_WhenNumOfReleasesToKeepIsLessOrEqualsToZero(int numOfReleasesToKeep)
    {
        // Arrange
        const string expectedExceptionMessage = $@"{nameof(RetainReleaseOptions.NumOfReleasesToKeep)} must be greater than zero. (Parameter '{nameof(RetainReleaseOptions.NumOfReleasesToKeep)}')";
        var options = RetainReleaseOptionsBuilder.CreateRandom().Build();

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