using System.Text;
using Insurance.Business;
using Insurance.Domain;
using Insurance.Persistence;
using Insurance.UI;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

// ---------- Збирання залежностей ----------
var dataDir = Path.Combine(AppContext.BaseDirectory, "data");

var clientRepo  = new JsonRepository<Client>(Path.Combine(dataDir, "clients.json"), c => c.Id);
var policyRepo  = new JsonRepository<Policy>(Path.Combine(dataDir, "policies.json"), p => p.Id);
var claimRepo   = new JsonRepository<Claim>(Path.Combine(dataDir, "claims.json"), c => c.Id);
var agentRepo   = new JsonRepository<Agent>(Path.Combine(dataDir, "agents.json"), a => a.Id);
var queryRepo   = new JsonRepository<SavedQuery>(Path.Combine(dataDir, "queries.json"), q => q.Id);
var paymentRepo = new JsonRepository<Payment>(Path.Combine(dataDir, "payments.json"), p => p.Id);
var requestRepo = new JsonRepository<Request>(Path.Combine(dataDir, "requests.json"), r => r.Id);

var clientService  = new ClientService(clientRepo, policyRepo);
var policyService  = new PolicyService(policyRepo, clientRepo);
var claimService   = new ClaimService(claimRepo, policyRepo, clientRepo);
var agentService   = new AgentService(agentRepo, policyRepo);
var queryService   = new SavedQueryService(queryRepo, clientRepo);
var statsService   = new StatisticsService(policyRepo, claimRepo, agentRepo);
var paymentService = new PaymentService(paymentRepo, policyRepo, claimRepo);
var requestService = new RequestService(requestRepo, clientRepo, policyRepo, policyService);

var clientMenu  = new ClientMenu(clientService);
var policyMenu  = new PolicyMenu(policyService, clientService, agentService);
var claimMenu   = new ClaimMenu(claimService, policyService);
var agentMenu   = new AgentMenu(agentService, policyService);
var queryMenu   = new QueryMenu(queryService, clientService);
var paymentMenu = new PaymentMenu(paymentService, policyService);
var requestMenu = new RequestMenu(requestService, clientService);
var statsMenu   = new StatisticsMenu(statsService);

// ---------- Головне меню ----------
while (true)
{
    Console.WriteLine("\n=== Страхова компанія ===");
    Console.WriteLine("1. Клієнти");
    Console.WriteLine("2. Поліси");
    Console.WriteLine("3. Страхові події");
    Console.WriteLine("4. Агенти");
    Console.WriteLine("5. Запити клієнтів (збережені)");
    Console.WriteLine("6. Платежі");
    Console.WriteLine("7. Заявки на страхування");
    Console.WriteLine("8. Статистика");
    Console.WriteLine("0. Вихід");
    Console.Write("> ");

    switch (Console.ReadLine()?.Trim())
    {
        case "1": clientMenu.Run(); break;
        case "2": policyMenu.Run(); break;
        case "3": claimMenu.Run(); break;
        case "4": agentMenu.Run(); break;
        case "5": queryMenu.Run(); break;
        case "6": paymentMenu.Run(); break;
        case "7": requestMenu.Run(); break;
        case "8": statsMenu.Run(); break;
        case "0": return;
        default: Console.WriteLine("Невідомий пункт меню."); break;
    }
}