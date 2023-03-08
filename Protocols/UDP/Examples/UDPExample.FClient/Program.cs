using NSL.Logger;
using UDPExample;

ConsoleLogger logger = new ConsoleLogger();

new SenderExample(logger);

while (true)
{
    Thread.Sleep(100);
}