using Insurance.Domain;
using Insurance.Persistence;

namespace Insurance.Business;

public class AgentService
{
    private readonly IRepository<Agent> _agents;
    private readonly IRepository<Policy> _policies;

    public AgentService(IRepository<Agent> agents, IRepository<Policy> policies)
    {
        _agents = agents;
        _policies = policies;
    }

    public Agent Create(Agent agent)
    {
        var errors = agent.Validate();
        if (errors.Any())
            throw new ValidationException(errors);

        _agents.Add(agent);
        return agent;
    }

    public IEnumerable<Agent> GetAll() => _agents.GetAll();

    public void Update(Agent agent)
    {
        var errors = agent.Validate();
        if (errors.Any())
            throw new ValidationException(errors);

        _agents.Update(agent);
    }

    public IEnumerable<Policy> GetPolicies(Guid agentId) =>
        _policies.GetAll().Where(p => p.AgentId == agentId).ToList();

    public void AssignToPolicy(Guid agentId, Guid policyId)
    {
        var agent = _agents.GetById(agentId);
        var policy = _policies.GetById(policyId);

        var errors = new List<string>();
        if (agent == null) errors.Add("Агента не знайдено.");
        if (policy == null) errors.Add("Поліс не знайдено.");
        if (errors.Any())
            throw new ValidationException(errors);

        policy!.AgentId = agent!.Id;
        _policies.Update(policy);
    }

    public void UnassignFromPolicy(Guid policyId)
    {
        var policy = _policies.GetById(policyId)
            ?? throw new ValidationException(new List<string> { "Поліс не знайдено." });

        if (policy.AgentId == null)
            throw new ValidationException(new List<string> { "До цього поліса агента не прикріплено." });

        policy.AgentId = null;
        _policies.Update(policy);
    }

    // Відкріплює агента від усіх його полісів, повертає кількість полісів
    public int UnassignAll(Guid agentId)
    {
        var assigned = GetPolicies(agentId).ToList();

        foreach (var policy in assigned)
        {
            policy.AgentId = null;
            _policies.Update(policy);
        }

        return assigned.Count;
    }
    
    public bool Delete(Guid id, bool unassignPolicies = false)
    {
        if (GetPolicies(id).Any())
        {
            if (!unassignPolicies)
                throw new ValidationException(new List<string>
                {
                    "Неможливо видалити агента, якому призначені поліси."
                });

            UnassignAll(id);
        }

        return _agents.Delete(id);
    }
}