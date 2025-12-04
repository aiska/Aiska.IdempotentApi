namespace Aiska.IdempotentApi.SampleMvc
{
    public class DataTransfer
    {
        public string? IssuerName { get; set; }
 
        public string? AquirerName { get; set; }

        public decimal Money { get; set; }
        
        public DateTime? Timestamp { get; set; }

    }
}
