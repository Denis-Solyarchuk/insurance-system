using Insurance.Domain;
using Insurance.Persistence;

namespace Insurance.Business;

public class PaymentService
{
    private readonly IRepository<Payment> _payments;
    private readonly IRepository<Policy> _policies;
    private readonly IRepository<Claim> _claims;

    public PaymentService(
        IRepository<Payment> payments,
        IRepository<Policy> policies,
        IRepository<Claim> claims)
    {
        _payments = payments;
        _policies = policies;
        _claims = claims;
    }

    public Payment Register(Payment payment)
    {
        var policy = _policies.GetById(payment.PolicyId)
            ?? throw new ValidationException(new List<string> { "Поліс не знайдено." });

        var errors = payment.Validate();
        var existing = _payments.GetAll().Where(p => p.PolicyId == policy.Id).ToList();

        if (payment.Type == PaymentType.Contribution)
        {
            var contributed = existing
                .Where(p => p.Type == PaymentType.Contribution)
                .Sum(p => p.Amount);

            if (contributed + payment.Amount > policy.Cost)
                errors.Add($"Сума внесків перевищує вартість поліса ({policy.Cost:N2}).");
        }
        else
        {
            // Виплата можлива лише в межах затверджених або вже виплачених подій
            var approved = _claims.GetAll()
                .Where(c => c.PolicyId == policy.Id
                         && (c.Status == ClaimStatus.Approved || c.Status == ClaimStatus.Paid))
                .Sum(c => c.PayoutAmount);

            var paidOut = existing
                .Where(p => p.Type == PaymentType.Payout)
                .Sum(p => p.Amount);

            if (paidOut + payment.Amount > approved)
                errors.Add($"Сума виплат перевищує суму затверджених подій ({approved:N2}).");
        }

        if (errors.Any())
            throw new ValidationException(errors);

        _payments.Add(payment);
        return payment;
    }

    public IEnumerable<Payment> GetByPolicy(Guid policyId) =>
        _payments.GetAll().Where(p => p.PolicyId == policyId).OrderBy(p => p.Date).ToList();

    public decimal GetTotal(PaymentType type) =>
        _payments.GetAll().Where(p => p.Type == type).Sum(p => p.Amount);

    // Баланс за платежами: внески мінус виплати
    public decimal GetBalance() => GetTotal(PaymentType.Contribution) - GetTotal(PaymentType.Payout);
}