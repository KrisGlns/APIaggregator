namespace APIaggregator.Models
{
    public class ApiSection<T> : BaseResult
    {
        public T? Data { get; set; }
    }
}
