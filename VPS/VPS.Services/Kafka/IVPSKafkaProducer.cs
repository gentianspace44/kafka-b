using Confluent.Kafka;

namespace VPS.Services.Kafka
{
    public interface IVpsKafkaProducer
    {
        Task<DeliveryResult<Null, string>?> Produce(string topic, string message);
    }
}
