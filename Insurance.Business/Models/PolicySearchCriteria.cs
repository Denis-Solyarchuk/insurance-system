using Insurance.Domain;

namespace Insurance.Business;

public class PolicySearchCriteria
{
    public PolicyType? Type { get; set; }
    public Guid? ClientId { get; set; }
    public PolicyStatus? Status { get; set; }
    public decimal? MinCost { get; set; }
    public decimal? MaxCost { get; set; }
}