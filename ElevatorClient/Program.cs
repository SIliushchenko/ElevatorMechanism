using System.Net.Sockets;
using System.Text;
using Elevator.Common.Models;
using Newtonsoft.Json;

namespace ElevatorClient
{
    internal class Program
    {
        static async Task Main()
        {
            await StartSender();
        }

        static async Task StartSender()
        {
            using TcpClient client = new TcpClient("127.0.0.1", 8888);
            await using NetworkStream stream = client.GetStream();
            var messageObject = new ElevatorCall
            {
                Floor = 13,
                Direction = Direction.Down,
                SelectedFloor = 2
            };
            string jsonMessage = JsonConvert.SerializeObject(messageObject);
            byte[] data = Encoding.UTF8.GetBytes(jsonMessage);
            await stream.WriteAsync(data, 0, data.Length);

            var messageObject2 = new ElevatorCall
            {
                Floor = 13,
                Direction = Direction.Up,
                SelectedFloor = 14
            };
            string jsonMessage2 = JsonConvert.SerializeObject(messageObject2);
            byte[] data2 = Encoding.UTF8.GetBytes(jsonMessage2);
            await stream.WriteAsync(data2, 0, data2.Length);
        }
    }


}