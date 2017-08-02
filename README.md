# About

Message Queues are very crucial part of many applications specially Enterprise and Real Time applications. There are many message broker frameworks available in the market e.g. `RabbitMq`, `ZeroMq`, `ServiceBus` and so on. Each framework has different features, implementation and client(s) to interact with the framework.

It is very common that in a single application you use more than one message broker. For example, `ZeroMq` is in-memory messaging queue and it is very efficient because of this reason; so, it can be used for logging purpose in a separate thread. Or the scenario could be using free message broker in **Development Environment** and paid one in **Production Environment**. So there are many possibilities using more than one message broker in a single application.

The challenge is that you need to learn and maintain different clients and to also need to define some abstraction to easily switch between different message brokers. This is the reason **Dynamic Queue** is born for.

Dynamic Queue (this framework) is an abstraction which is independent from any specific message broker implementation, hides the details of any message broker client, common interfaces to interact and the flexibility to switch the message broker without updating and compiling the single line of code (yes, this is true).

# Communication Patterns

We can generally define the communication patterns as follow:
- Fire and forget [FaF] (Push-Pull, Producer-Consumer)
- Request and response [RaR] (Remote procedure call, Server-Client)
- Publisher and subscriber [PaS]

Dynamic Queue framework defines abstraction around these communication patterns. It further divides the interfaces as inbound and outbound. Please see the below table for details:

| Pattern | Inbound-Interface | Outbound-Interface | Async |
| --- | --- | --- | --- |
| FaF | IInboundFaFMq`<TMessage>` | IOutboundFaFMq`<TMessage>` | :white_check_mark: |
| RaR | IInboundRaRMq`<TRequest, TResponse>` | IOutboundRaRMq`<TResponse, TRequest>` | :x: |
| PaS | *Not Implemented* :x: | *Not Implemented* :x: | :x: |

# Architecture

Dynamic Queue framework architecture has been designed with loosely coupled modules, interface-based dependencies, flexible configuration, seamless serialization and easy to extend. Below are further details:

### Exception Handling
All the exceptions from the framework are thrown as `QueueException` with error code defined in `QueueErrorCode` enum and message.

### Logging
Logging is optional and if logger is passed while creating the instance of any type (inbound or outbound), then the logging will be done. There are two interfaces define in `MessageQueue.Log.Core` `dll` as `IQueueLogger` and `IQueueLoggerAsync`. The default logger using NLog is defined in `**MessageQueue.Log.NLog**` and the usage can be seen in Samples (please see the Samples section below).

### Serialization
Serialization is seamless and is done using `Newtonsoft`.

### Configuration
The configuration can be stored in any type of store or configuration file. There is an interface named as `IQueueConfigurationProvider` in `MessageQueue.CofigurationProvider.Core` `dll` and the default implementation which retrieves configuration from AppSettings is also defined in `MessageQueue.CofigurationProvider.Core`. If you want to define your custom configuration provider, then simply implement `IQueueConfigurationProvider`.

# Message Brokers
As of now, following message brokers have been implemented:

| Message Broker | FaF | RaR | PaS |
| --- | --- | --- | --- |
| [ZeroMq](http://zeromq.org/) | :white_check_mark: | :white_check_mark: | :x: |
| [RabbitMq](https://www.rabbitmq.com/) | :white_check_mark: | :white_check_mark: | :x: |
| [ServiceBus](https://azure.microsoft.com/en-us/services/service-bus/) | :white_check_mark: | :x: | :x: |

# Samples
*In-progress*

# Nuget
Yet to come. I will try to create nuget package soon.

## Appreciation
Like it? Wants to apricate? Please go ahead!!! [![paypal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=AEAML5T4W4NXJ)
