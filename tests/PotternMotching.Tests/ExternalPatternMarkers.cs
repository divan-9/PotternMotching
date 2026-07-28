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
[AutoPatternFor(typeof(ExternalJobBoard))]
[AutoPatternFor(typeof(ExternalContent))]
[AutoPatternFor(typeof(ExternalGenericBox<ExternalAddress>))]
[AutoPatternFor(typeof(ExternalGenericEnvelope<ExternalAddress>))]
[AutoPatternFor(typeof(ExternalNullableCollection))]
[AutoPatternFor(typeof(ExternalNullableElements))]
[AutoPatternFor(typeof(ExternalNullableSet))]
[AutoPatternFor(typeof(ExternalNullableScalar))]
[AutoPatternFor(typeof(ExternalMediaOptions))]
[AutoPatternFor(typeof(ExternalFieldOptions))]
internal static class ExternalPatternMarkers;
