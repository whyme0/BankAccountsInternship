// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PreferConcreteValueOverDefault
namespace Api.Features.Accounts;

public class ClientDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
}