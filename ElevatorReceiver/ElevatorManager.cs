using System.Net.Sockets;
using System.Net;
using System.Text;
using Elevator.Common.Models;
using Elevator.Common.Types;
using ElevatorReceiver.Models;
using Newtonsoft.Json;
using static System.Console;


namespace ElevatorReceiver
{
    public class ElevatorManager
    {
        private readonly Models.Elevator _elevator;
        private readonly ConcurrentList<ElevatorCall> _elevatorCalls = new();

        public ElevatorManager(Models.Elevator elevator)
        {
            _elevator = elevator ?? throw new ArgumentNullException(nameof(elevator));
        }


        public async Task StartReceivingElevatorCallsAsync()
        {
            var listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8888);
            listener.Start();

            Console.WriteLine(_elevator);

            try
            {
                while (true)
                {
                    var client = await listener.AcceptTcpClientAsync();
                    _ = HandleElevatorCallAsync(client);
                }
            }
            finally
            {
                listener.Stop();
            }
        }

        private async Task HandleElevatorCallAsync(TcpClient client)
        {
            try
            {
                await using NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                while (true)
                {
                    int bytesRead;
                    try
                    {
                        bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reading from client: {ex.Message}");
                        break;
                    }

                    if (bytesRead == 0)
                    {
                        break;
                    }

                    string jsonMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    var receivedMessage = JsonConvert.DeserializeObject<ElevatorCall>(jsonMessage)!;
                    _elevatorCalls.Add(receivedMessage);

                    if (_elevator.State == ElevatorState.Idle)
                    {
                        _elevator.State = ElevatorState.Processing;
                        _ = RunElevatorAsync();
                    }
                }

            }
            finally
            {
                client.Close();
            }
        }

        private async Task RunElevatorAsync()
        {
            while (_elevator.Passengers.Any() || _elevatorCalls.Any())
            {
                UpdateElevatorCallType();

                var callFloor = GetCurrentCallFloor();

                if (callFloor == _elevator.CurrentFloor)
                {
                    ProcessElevatorArrival();
                    continue;
                }

                await MoveElevatorToCallFloorAsync(callFloor);

                ProcessElevatorCalls(callFloor);

                await Task.Delay(1000);
            }

            _elevator.State = ElevatorState.Idle;
            WriteLine(_elevator);
        }

        private void UpdateElevatorCallType()
        {
            _elevator.ElevatorCallType = _elevator.Passengers.Any() ? ElevatorCallType.In : ElevatorCallType.Out;
        }

        private int GetCurrentCallFloor()
        {
            return _elevator.ElevatorCallType == ElevatorCallType.In
                ? GetNearestSelectedFloor()
                : _elevatorCalls.First().Floor;
        }

        private void ProcessElevatorArrival()
        {
            _elevator.DoorState = ElevatorDoorState.Opened;
            WriteLine("The elevator door is opening.");

            var newPassenger = new Passenger
            {
                Direction = _elevatorCalls[0].Direction,
                SelectedFloor = _elevatorCalls[0].SelectedFloor
            };

            _elevator.Passengers.Add(newPassenger);
            _elevatorCalls.RemoveAt(0);
            WriteLine("The passenger got into the elevator.");

            if (_elevator.DoorState == ElevatorDoorState.Opened)
            {
                _elevator.DoorState = ElevatorDoorState.Closed;
                WriteLine("The elevator door is closing.");
            }
        }

        private async Task MoveElevatorToCallFloorAsync(int callFloor)
        {
            var direction = callFloor > _elevator.CurrentFloor ? Direction.Up : Direction.Down;

            for (int i = _elevator.CurrentFloor; (direction > 0 && i <= callFloor) || (direction < 0 && i >= callFloor); i += (int)direction)
            {
                _elevator.CurrentFloor = i;
                _elevator.State = direction == Direction.Up ? ElevatorState.Up : ElevatorState.Down;
                WriteLine(_elevator);
                WriteLine($"The elevator is on the {i} floor.");
                ProcessElevatorCalls(i);

                await Task.Delay(1000);
            }
        }

        private void ProcessElevatorCalls(int floor)
        {
            var relevantCalls = _elevatorCalls
                .Where(x => (_elevator.ElevatorCallType == ElevatorCallType.In && x.Floor == floor && (int)x.Direction == (int)_elevator.State) ||
                            (_elevator.ElevatorCallType == ElevatorCallType.Out && x.Floor == floor))
                .ToList();

            if (relevantCalls.Any())
            {
                _elevator.DoorState = ElevatorDoorState.Opened;
                WriteLine("The elevator door is opening.");

                var passengers = relevantCalls.Select(x => new Passenger
                {
                    Direction = x.Direction,
                    SelectedFloor = x.SelectedFloor
                }).ToList();

                _elevator.Passengers.AddRange(passengers);
                WriteLine($"{passengers.Count} person/persons got into the elevator.");

                _elevatorCalls.RemoveAll(x => (_elevator.ElevatorCallType == ElevatorCallType.In && x.Floor == floor && (int)x.Direction == (int)_elevator.State) ||
                                              (_elevator.ElevatorCallType == ElevatorCallType.Out && x.Floor == floor));
            }

            var passengersToExit = _elevator.Passengers.Where(x => x.SelectedFloor == floor).ToList();
            if (passengersToExit.Any())
            {
                if (_elevator.DoorState == ElevatorDoorState.Closed)
                {
                    _elevator.DoorState = ElevatorDoorState.Opened;
                    WriteLine("The elevator door is opening.");
                    foreach (var passengerToExit in passengersToExit)
                    {
                        _elevator.Passengers.Remove(passengerToExit);
                    }
                    WriteLine($"{passengersToExit.Count} person/persons got out of the elevator.");
                }
            }

            if (_elevator.DoorState == ElevatorDoorState.Opened)
            {
                _elevator.DoorState = ElevatorDoorState.Closed;
                WriteLine("The elevator door is closing.");
            }
        }

        private int GetNearestSelectedFloor()
        {
            int nearestSelectedFloor = 0;
            int differenceBetweenFloors = 0;
            foreach (var passenger in _elevator.Passengers)
            {
                int currentDifferenceBetweenFloors = Math.Abs(_elevator.CurrentFloor - passenger.SelectedFloor);
                if (currentDifferenceBetweenFloors == 1)
                {
                    return passenger.SelectedFloor;
                }
                if (differenceBetweenFloors == 0 || differenceBetweenFloors > currentDifferenceBetweenFloors)
                {
                    differenceBetweenFloors = currentDifferenceBetweenFloors;
                    nearestSelectedFloor = passenger.SelectedFloor;
                }
            }

            return nearestSelectedFloor;
        }
    }
}
