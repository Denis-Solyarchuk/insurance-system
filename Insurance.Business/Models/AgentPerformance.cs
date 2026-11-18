using Insurance.Domain;

namespace Insurance.Business;

public record AgentPerformance(Agent Agent, int PoliciesSold, decimal Commission);