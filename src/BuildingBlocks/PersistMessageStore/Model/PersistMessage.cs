using BuildingBlocks.Core.Model;

namespace BuildingBlocks.PersistMessageStore.Model
{
    public class PersistMessage
        (Guid id, string dataType, string data, MessageDeliveryType deliveryType) : IVersion
    {
        public Guid Id { get; set; } = id;
        public string DataType { get; set; } = dataType;
        public string Data { get; set; } = data;
        public int RetryCount { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public MessageDeliveryType MessageDeliveryType { get; set; } = deliveryType;
        public MessageStatus MessageStatus { get; set; } = MessageStatus.IN_PROGRESSING;
        public long Version { get; set; }

        public void ChangeState(MessageStatus messageStatus)
        {
            MessageStatus = messageStatus;
        }

        public void IncreaseRetry()
        {
            RetryCount++;
        }
    }
}
