using System.Net.Sockets;
using System.Net;
using System.Text;
using Elevator = ElevatorReceiver.Models.Elevator;

namespace ElevatorReceiver
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var elevatorManager = new ElevatorManager(new Models.Elevator());
            await elevatorManager.StartReceivingElevatorCallsAsync();
        }
    }
}