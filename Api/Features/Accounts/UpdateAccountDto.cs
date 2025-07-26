// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace Api.Features.Accounts;

public class UpdateAccountDto
{
    public decimal? InterestRate { get; set; }
    public DateTime? ClosedDate { get; set; }
}