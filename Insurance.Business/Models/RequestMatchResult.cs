using Insurance.Domain;

namespace Insurance.Business;

public record RequestMatchResult(Request Request, IReadOnlyList<Policy> Matches, decimal EstimatedCost);