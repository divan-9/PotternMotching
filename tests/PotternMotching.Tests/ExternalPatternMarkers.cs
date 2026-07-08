namespace PotternMotching.Tests.ExternalPatterns;

using PotternMotching.TestExternalModels;

[AutoPatternFor(typeof(ExternalAddress))]
[AutoPatternFor(typeof(ExternalUserDto))]
[AutoPatternFor(typeof(ExternalCollectionsDto))]
[AutoPatternFor(typeof(ExternalWrappedUnknown))]
[AutoPatternFor(typeof(ExternalClassDto))]
[AutoPatternFor(typeof(ExternalJob))]
[AutoPatternFor(typeof(ExternalJobApplication))]
[AutoPatternFor(typeof(ExternalCompany))]
[AutoPatternFor(typeof(ExternalContent))]
[AutoPatternFor(typeof(ExternalGenericBox<ExternalAddress>))]
[AutoPatternFor(typeof(ExternalGenericEnvelope<ExternalAddress>))]
[AutoPatternFor(typeof(ExternalStringFragmentTemplate))]
[AutoPatternFor(typeof(ExternalBannerFragmentTemplate))]
[AutoPatternFor(typeof(ExternalImpressionRule))]
[AutoPatternFor(typeof(ExternalNullableCollection))]
[AutoPatternFor(typeof(ExternalNullableElements))]
[AutoPatternFor(typeof(ExternalNullableSet))]
internal static class ExternalPatternMarkers;
