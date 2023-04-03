using NSL.Logger;
using UDPExample;

ConsoleLogger logger = new ConsoleLogger();

Thread.Sleep(3000);
new SenderExample(logger);

while (true)
{
    Thread.Sleep(100);
}