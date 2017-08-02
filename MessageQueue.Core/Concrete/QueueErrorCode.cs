namespace MessageQueue.Core.Concrete
{
    /// <summary>
    /// Queue exception error codes.
    /// </summary>
    public enum QueueErrorCode : ushort
    {
        // Instantiation related
        FailedToInstantiateInboundFaFMq,
        FailedToInstantiateOutboundFaFMq,
        FailedToInstantiateInboundRaRMq,
        FailedToInstantiateOutboundRaRMq,

        // Initialization related
        FailedToInitializeMessageQueue,

        // Configuration related
        NotSupportedConfigurationParameters,
        MissingRequiredConfigurationParameter,
        InvalidValueForConfigurationParameter,
        ParameterNotApplicationInCurrentConfiguration,
        ParameterRequiredInCurrentConfiguration,
        FailedToExtractConstantFields,
        GeneralConfigurationParsingError,

        // Serialization related
        FailedToSerializeObjectIntoJsonString,
        FailedToSerializeObjectIntoJsonBytes,
        FailedToDeserializeJsonString,
        FailedToDeserializeJsonBytes,
        FailedToSerializeMessage,
        FailedToDeserializeMessage,

        // Message related
        FailedToReceiveMessage,
        FailedToSendMessage,
        FailedToReceiveRequestMessage,
        FailedToReceiveResponseMessage,
        FailedToSendResponseMessage,

        // Queue related
        MessageQueueIsNotInitialized,
        FailedToCreateMessageQueue,
        FailedToStartReceivingMessage,
        FailedToStartReceivingRequest,
        FailedToStopReceivingMessage,
        FailedToStopReceivingRequest,
        FailedToStipReceivingRequest,
        FailedToCheckQueueExistence,
        FailedToCheckExchangeExistence,
        FailedToCheckQueueHasMessage,
        FailedToCreateExchange,
        QueueDoesNotExist,
        ExchangeDoesNotExist,
        AcknowledgmentIsNotConfiguredForQueue,
        FailedToAcknowledgeMessage,
        FailedToAbandonMessageAcknowledgment,
        MessageReturnedFromQueue,

        // ZeroMq related
        InvalidZeroMqSocketType,
        FailedToCreateZeroMqSocket,

        // ServiceBus related
        MissingNamespaceAddressInConfiguration,
    }
}
