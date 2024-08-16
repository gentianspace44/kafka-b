namespace VPS.Services.Kafka
{
    public interface IVpsKafkaConsumer
    {
        Task<bool> ConsumeAndProcessTransaction(string message);
    }
}
