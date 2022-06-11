namespace RabbitMQ.EventBus.Helper
{
    public class EnumEventBusErrorMessage
    {
        public EnumEventBusErrorMessage(string message)
        {
            this.message = message;
        }

        public string message { get; set; }

        public static EnumEventBusErrorMessage ConnectionProblem = new EnumEventBusErrorMessage("Bağlantı hatası.");
        public static EnumEventBusErrorMessage ConnectionFail = new EnumEventBusErrorMessage("Bağlantı oluşturulamadı.");
        public static EnumEventBusErrorMessage ConnectionBlocked = new EnumEventBusErrorMessage("Bağlantı engellendi. Yeniden bağlanmaya çalışıyor.");
        public static EnumEventBusErrorMessage ConnectionClosed = new EnumEventBusErrorMessage("Bağlantı kapatıldı. Yeniden bağlanmaya çalışıyor.");
        public static EnumEventBusErrorMessage NoValidConnectionToCreateModel = new EnumEventBusErrorMessage("Model oluşturabilmek için geçerli bir bağlantı bulunamadı.");
        public static EnumEventBusErrorMessage TheResourcesUsedDeleted = new EnumEventBusErrorMessage("Kullanılan kaynaklar silindi.");
    }
}