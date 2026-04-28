namespace PotternMotching.Tests.ExternalPatterns;

using PotternMotching.TestExternalModels;

[AutoPatternFor(typeof(ExternalAddress))]
[AutoPatternFor(typeof(ExternalUserDto))]
[AutoPatternFor(typeof(ExternalCollectionsDto))]
[AutoPatternFor(typeof(ExternalWrappedUnknown))]
[AutoPatternFor(typeof(ExternalClassDto))]
[AutoPatternFor(typeof(ExternalJob))]
[AutoPatternFor(typeof(ExternalJobApplication))]
internal static class ExternalPatternMarkers;
