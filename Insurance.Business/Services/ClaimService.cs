using Insurance.Domain;
using Insurance.Persistence;

namespace Insurance.Business;

public class ClaimService
{
    private readonly IRepository<Claim> _claims;
    private readonly IRepository<Policy> _policies;

    public ClaimService(IRepository<Claim> claims, IRepository<Policy> policies)
    {
        _claims = claims;
        _policies = policies;
    }

    public Claim Create(Claim claim)
    {
        var policy = _policies.GetById(claim.PolicyId)
                     ?? throw new ValidationException(new List<string> { "Поліса не знайдено." });

        var errors = claim.Validate();

        if (policy.Status != PolicyStatus.Active)
            errors.Add("Страхову подію можна створити лише для активного поліса.");

        if (claim.Date < policy.StartDate || claim.Date > policy.EndDate)
            errors.Add("Дата події виходить за межі дії поліса.");

        if (claim.PayoutAmount > policy.CoverageAmount)
            errors.Add("Сума виплати перевищує суму покриття.");

        if (errors.Any())
            throw new ValidationException(errors);

        _claims.Add(claim);
        return claim;
    }

    public IEnumerable<Claim> GetAll() => _claims.GetAll();

    public IEnumerable<Claim> GetByPolicy(Guid policyId) =>
        _claims.GetAll().Where(c => c.PolicyId == policyId);
}