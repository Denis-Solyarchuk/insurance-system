using Insurance.Domain;

namespace Insurance.Business;

public record PolicyTypeReport(PolicyType Type, int Active, int Finished, decimal AverageCost);