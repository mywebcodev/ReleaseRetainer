namespace ReleaseRetainer.Test.Expectations;

internal static class LogExpectations
{
    internal static string CreateExpectedRetainedReleaseLogMessage(string releaseId, string envId)
    {
       return $"'{releaseId}' kept because it was most recently deployed to '{envId}'";
    }
}