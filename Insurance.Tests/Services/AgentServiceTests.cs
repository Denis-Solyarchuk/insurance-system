using Insurance.Business;
using Insurance.Domain;
using Xunit;

namespace Insurance.Tests;

public class AgentServiceTests
{
    private readonly InMemoryRepository<Agent> _agents = new(a => a.Id);
    private readonly InMemoryRepository<Policy> _policies = new(p => p.Id);
    private readonly AgentService _service;

    public AgentServiceTests()
    {
        _service = new AgentService(_agents, _policies);
    }

    private Agent AddAgent()
    {
        var agent = new Agent { Name = "Петренко", CommissionPercent = 10m };
        _agents.Add(agent);
        return agent;
    }

    private Policy AddPolicy(Guid? agentId = null)
    {
        var policy = new Policy
        {
            Number = Guid.NewGuid().ToString(),
            Type = PolicyType.Auto,
            CoverageAmount = 100_000m,
            Cost = 5_000m,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddYears(1),
            ClientId = Guid.NewGuid(),
            AgentId = agentId
        };
        _policies.Add(policy);
        return policy;
    }

    [Fact]
    public void Delete_AgentWithPolicies_ThrowsByDefault()
    {
        var agent = AddAgent();
        AddPolicy(agent.Id);

        Assert.Throws<ValidationException>(() => _service.Delete(agent.Id));
    }

    [Fact]
    public void Delete_WithUnassign_UnassignsPoliciesAndRemovesAgent()
    {
        var agent = AddAgent();
        var policy = AddPolicy(agent.Id);

        _service.Delete(agent.Id, unassignPolicies: true);

        Assert.Null(_agents.GetById(agent.Id));
        Assert.Null(_policies.GetById(policy.Id)!.AgentId);
    }

    [Fact]
    public void UnassignFromPolicy_ClearsAgent()
    {
        var agent = AddAgent();
        var policy = AddPolicy(agent.Id);

        _service.UnassignFromPolicy(policy.Id);

        Assert.Empty(_service.GetPolicies(agent.Id));
    }

    [Fact]
    public void UnassignFromPolicy_WithoutAgent_ThrowsValidationException()
    {
        var policy = AddPolicy();

        Assert.Throws<ValidationException>(() => _service.UnassignFromPolicy(policy.Id));
    }
}