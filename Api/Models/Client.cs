namespace Api.Models
{
    // Данная сущность выступает скорее в роли заглушки, нежели чем дейтсивтельная сущность бизнес логики.
    // В дальнейшем вероятней всего необходимо будет Client по ролям
    public class Client
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public ICollection<Account> Accounts { get; set; } = new List<Account>();
    }
}
