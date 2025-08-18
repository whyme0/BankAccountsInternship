// ReSharper disable PreferConcreteValueOverDefault
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength
namespace Api.Models;

public class InboxConsumed
{
    public Guid Id { get; set; }
    public DateTime ProcessedAt { get; set; }
    public string Handler { get; set; } = default!;
}