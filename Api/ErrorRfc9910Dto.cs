namespace Api
{
    public class ErrorRfc9910Dto
    {
        public string Type { get; set; } = default!;
        public string Title { get; set; } = default!;
        public int Status { get; set; } = default;
        public string TraceId { get; set; } = default!;
    }
}
