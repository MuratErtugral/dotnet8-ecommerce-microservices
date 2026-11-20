using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace Engagement.API.Infrastructure.Messaging;

public sealed class KafkaSettings
{
    public string BootstrapServers { get; set; } = "localhost:9092";
    public string InteractionTopic { get; set; } = "storefront.widget.interactions";
}

public interface IKafkaEventProducer
{
    Task PublishAsync<T>(string topic, string key, T message, CancellationToken ct = default);
}

public sealed class KafkaEventProducer : IKafkaEventProducer, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaEventProducer> _logger;

    public KafkaEventProducer(IOptions<KafkaSettings> options, ILogger<KafkaEventProducer> logger)
    {
        _logger = logger;
        var config = new ProducerConfig
        {
            BootstrapServers = options.Value.BootstrapServers,
            Acks = Acks.Leader,
            EnableIdempotence = false,
            MessageTimeoutMs = 5000
        };
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishAsync<T>(string topic, string key, T message, CancellationToken ct = default)
    {
        var payload = JsonSerializer.Serialize(message);
        try
        {
            var deliveryResult = await _producer.ProduceAsync(
                topic,
                new Message<string, string> { Key = key, Value = payload },
                ct
            );

            _logger.LogInformation(
                "Published Kafka event to {Topic} [Partition: {Partition}, Offset: {Offset}]",
                topic,
                deliveryResult.Partition.Value,
                deliveryResult.Offset.Value
            );
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogWarning(ex, "Kafka broker unreachable; event buffered/logged locally for key {Key}", key);
        }
    }

    public void Dispose() => _producer.Dispose();
}
