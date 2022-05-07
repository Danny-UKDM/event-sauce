namespace EventSauceApi.Models.Response;

public sealed record EventResponse(What What, When When, Where Where, Who Who);

public sealed record What(string Detail, RiskFactor RiskFactor);

public sealed record When(DateTime OccurredOn, DateTime CreatedOn);

public sealed record Where(string Area, string Sector, string Domain);

public sealed record Who(IEnumerable<Person> People);

public sealed record Person(string Name, string EmailAddress);


