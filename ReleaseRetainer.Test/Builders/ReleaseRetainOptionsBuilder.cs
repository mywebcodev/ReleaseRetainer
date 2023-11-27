using ReleaseRetainer.Dtos;
using ReleaseRetainer.Models;
using ReleaseRetainer.Test.Helpers;

namespace ReleaseRetainer.Test.Builders;

public class ReleaseRetainOptionsBuilder : GenericBuilder<ReleaseRetainOptions>
{
    public override ReleaseRetainOptionsBuilder CreateRandom()
    {
        Instance.Releases = new List<ReleaseDto> {new ReleaseBuilder().CreateRandom().Build()};
        Instance.Environments = new List<EnvironmentDto> {new EnvironmentBuilder().CreateRandom().Build()};
        Instance.Projects = new List<ProjectDto> {new ProjectBuilder().CreateRandom().Build()};
        Instance.Releases = new List<ReleaseDto> {new ReleaseBuilder().CreateRandom().Build()};
        Instance.NumOfReleasesToKeep = TestDataGenerator.GetRandomNumber(1, 24);

        return this;
    }
}