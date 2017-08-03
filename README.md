# About

Message Queues are very crucial part of many applications specially Enterprise and Real Time applications. There are many message broker frameworks available in the market e.g. `RabbitMq`, `ZeroMq`, `ServiceBus` and so on. Each framework has different features, implementation and client(s) to interact with the framework.

It is very common that in a single application you use more than one message broker. For example, `ZeroMq` is in-memory messaging queue and it is very efficient because of this reason; so, it can be used for logging purpose in a separate thread. Or the scenario could be using free message broker in **Development Environment** and paid one in **Production Environment**. So there are many possibilities using more than one message broker in a single application.

The challenge is that you need to learn and maintain different clients, handle seriliazation (binary, json etc.), exception handling, logging, thread safety and also need to define some abstraction to easily switch between different message brokers. So you spend good amount of time on **HOW TO DO** rahter than **WHAT TO DO**. This is the reason **Dynamic Queue** is born for. You just focus on the main stream of work and the rest is on **Dynamic Queue**. Sounds good! Right? Please continue reading....

Dynamic Queue (this framework) is an abstraction which is independent from any specific message broker implementation, hides the details of any message broker client, seamless serilization, multi-threading, common interfaces to interact and the flexibility to switch the message broker without updating and compiling the single line of code (yes, this is true).

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
| PaS | *Not Implemented*  :x: | *Not Implemented*  :x: | :x: |

# Architecture

Dynamic Queue framework architecture has been designed with loosely coupled modules, interface-based dependencies, flexible configuration, seamless serialization and easy to extend. Below are further details:

### Technology
The framework has built using `C# 6.0` and `.net 4.5.2`.

### Exception Handling
All the exceptions from the framework are thrown as `QueueException` with error code defined in `QueueErrorCode` enum and message.

### Logging
Logging is optional and if logger is passed while creating the instance of any type (inbound or outbound), then the logging will be done. There are two interfaces define in `MessageQueue.Log.Core` `dll` as `IQueueLogger` and `IQueueLoggerAsync`. The default logger using NLog is defined in `MessageQueue.Log.NLog` and the usage can be seen in Samples (please see the [Samples](#samples) section below).

### Serialization
Serialization is seamless and is done using `Newtonsoft`.

### Configuration
The configuration can be stored in any type of store or configuration file. There is an interface named as `IQueueConfigurationProvider` in `MessageQueue.CofigurationProvider.Core` `dll` and the default implementation which retrieves configuration from AppSettings is also defined in `MessageQueue.CofigurationProvider.Core`. If you want to define your custom configuration provider, then simply implement `IQueueConfigurationProvider`.

> Creation of any interface implementation is dyamic and is based on the fully qualified class name and assembly name.

### Thread Safety
All the implementations of message brokers (some by default from the message broker and others managed by Dynamic Queue) are thread safe.

### The Queue Factory
There is a class named as `MessagingQueueFactory` which is responsible to create any kind of interface implementation. This class is available in `MessageQueue.Core` dll.

# Message Brokers
As of now, following message brokers have been implemented:

| Message Broker | FaF | RaR | PaS |
| --- | --- | --- | --- |
| [ZeroMq](http://zeromq.org/) | :white_check_mark: | :white_check_mark: | :x: |
| [RabbitMq](https://www.rabbitmq.com/) | :white_check_mark: | :white_check_mark: | :x: |
| [ServiceBus](https://azure.microsoft.com/en-us/services/service-bus/) | :white_check_mark: | :x: | :x: |

### Configuration Parameters


##### ZeroMq - Configuration

| Name | Description | Example | Required |
| --- | --- | --- | --- |
| Address | The address of the queue (no server as it is in-memory) | `>tcp://localhost:5551` |  :white_check_mark: |
| Implementation | The relevant implementaion (inbound or outbound) | `MessageQueue.ZeroMq...` |  :white_check_mark: |

##### ZeroMq - Interface Implementation

| Pattern | Inbound-Interface | Outbound-Interface  |
| --- | --- | --- |
| FaF |``MessageQueue.ZeroMq.Concrete.Inbound.ZmqInboundFaF`1, MessageQueue.ZeroMq`` |``MessageQueue.ZeroMq.Concrete.Outbound.ZmqOutboundFaF`1, MessageQueue.ZeroMq`` |
| RaR |``MessageQueue.ZeroMq.Concrete.Inbound.ZmqInboundRaR`2, MessageQueue.ZeroMq`` |``MessageQueue.ZeroMq.Concrete.Outbound.ZmqOutboundRaR`2, MessageQueue.ZeroMq`` |
----

##### RabbitMq - Configuration

| Name | Description | Example | Required |
| --- | --- | --- | --- |
| Address | The address of the RabbitMq server | `localhost` |  :white_check_mark: |
| Implementation | The relevant implementaion (inbound or outbound) |`MessageQueue.RabbitMq...` |  :white_check_mark: |
| QueueName | The queue name | `MyQueue` |  :white_check_mark: |
| UserName | Username to connect with server | `guest` |  :white_check_mark: |
| Password | Password to connect with server | `guest` |  :white_check_mark: |
| Port | The port on which server is listening | `1234` |  :x: |
| Acknowledgment | The message acknowledgment setting (inbound only) | `true` OR `false` |  :x: |
| MaxConcurrentReceiveCallback | The max number of concurrent calls to the receive handler | `5` |  :x: |
| Acknowledgment | The message acknowledgment setting (inbound only) | `true` OR `false` |  :x: |
| ExchangeName | The exchange name | `MyExchange` |  :x: |
| RoutingKey | The routing key | `Key1` |  :x: |
| ConnectionTimeoutInMinutes | The connection timeout in minutes | `2` |  :x: |

##### RabbitMq - Interface Implementation

| Pattern | Inbound-Interface | Outbound-Interface  |
| --- | --- | --- |
| FaF |``MessageQueue.RabbitMq.Concrete.Inbound.RmqInboundFaF`1, MessageQueue.RabbitMq`` |``MessageQueue.RabbitMq.Concrete.Outbound.RmqOutboundFaF`1, MessageQueue.RabbitMq`` |
| RaR |``MessageQueue.RabbitMq.Concrete.Inbound.RmqInboundRaR`2, MessageQueue.RabbitMq`` |``MessageQueue.RabbitMq.Concrete.Outbound.RmqOutboundRaR`2, MessageQueue.RabbitMq`` |
----

##### ServiceBus - Configuration

| Name | Description | Example | Required |
| --- | --- | --- | --- |
| Address | The servicebus endpoint address | `Endpoint` |  :white_check_mark: |
| Implementation | The relevant implementaion (inbound or outbound) |`MessageQueue.ServiceBus...` |  :white_check_mark: |
| QueueName | The queue name | `MyQueue` |  :white_check_mark: |
| Acknowledgment | The message acknowledgment setting (inbound only) | `true` OR `false` |  :x: |
| MaxConcurrentReceiveCallback | The max number of concurrent calls to the receive handler | `5` |  :x: |
| Acknowledgment | The message acknowledgment setting (inbound only) | `true` OR `false` |  :x: |

##### ServiceBus - Interface Implementation

| Pattern | Inbound-Interface | Outbound-Interface  |
| --- | --- | --- |
| FaF |``MessageQueue.ServiceBus.Concrete.Inbound.SbInboundFaF`1, MessageQueue.ServiceBus`` |``MessageQueue.ServiceBus.Concrete.Outbound.SbOutboundFaF`1, MessageQueue.ServiceBus`` |
| RaR | *Not Implemented* | *Not Implemented* |


> The framework does not create the queues. The only scenario is in RaR pattern where it needs to create queue for the responses.

# Setup
If you want to run the code in `Visual Studio` or any other `.Net IDE`, just download the source code, restore the nuget packages, update the configuration and you are good to go. Please see the section [Samples](#samples) below for details.

# Samples
In the Test folder, there are four projects (console application) which consumes Dynamic Queue for each supported communication pattern and message broker. For FaF patter, `MessageQueue.Sender` [ [code snippet](https://github.com/muhamad-ahsan/dynamic-queue/blob/master/Tests/MessageQueue.Sender/Program.cs) ] and `MessageQueue.Receiver`[ [code snippet](https://github.com/muhamad-ahsan/dynamic-queue/blob/master/Tests/MessageQueue.Receiver/Program.cs) ] test projects are configured and for RaR pattern, `MessageQueue.RaR.Server` [ [code snippet](https://github.com/muhamad-ahsan/dynamic-queue/blob/master/Tests/MessageQueue.RaR.Server/Program.cs) ] and `MessageQueue.RaR.Client` [ [code snippet](https://github.com/muhamad-ahsan/dynamic-queue/blob/master/Tests/MessageQueue.RaR.Client/Program.cs) ] test projects are configured.

### FaF Samples

| ZeroMq | RabbitMq | ServiceBus |
| --- | --- | --- |
| :white_check_mark: | :white_check_mark: | :white_check_mark: |

### RaR Samples

| ZeroMq | RabbitMq | ServiceBus |
| --- | --- | --- |
| :white_check_mark: | :white_check_mark: | :x: *Not Implemented* |

# Nuget
Yet to come. I will try to create nuget package soon.

# Appreciation
Like it? Wants to apricate? Please go ahead!!! [![paypal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=AEAML5T4W4NXJ)
