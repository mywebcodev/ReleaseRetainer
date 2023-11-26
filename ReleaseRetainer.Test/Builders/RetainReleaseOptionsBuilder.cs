﻿using ReleaseRetainer.Entities;
using ReleaseRetainer.Models;
using ReleaseRetainer.Test.Helpers;

namespace ReleaseRetainer.Test.Builders;

using Environment = Entities.Environment;

public class RetainReleaseOptionsBuilder : GenericBuilder<RetainReleaseOptions>
{
    public override RetainReleaseOptionsBuilder CreateRandom()
    {
        Instance.Releases = new List<Release> {new ReleaseBuilder().CreateRandom().Build()};
        Instance.Environments = new List<Environment> {new EnvironmentBuilder().CreateRandom().Build()};
        Instance.Projects = new List<Project> {new ProjectBuilder().CreateRandom().Build()};
        Instance.Releases = new List<Release> {new ReleaseBuilder().CreateRandom().Build()};
        Instance.NumOfReleasesToKeep = TestDataGenerator.GetRandomNumber(1, 24);

        return this;
    }
}