namespace APIaggregator.Models
{
    public abstract class BaseResult
    {
        public ApiStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
