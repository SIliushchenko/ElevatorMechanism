using Elevator.Common.Models;

namespace ElevatorReceiver.Models
{
    public class Elevator
    {
        public ElevatorState State { get; set; } = ElevatorState.Idle;
        public ElevatorDoorState DoorState { get; set; } = ElevatorDoorState.Closed;
        public ElevatorCallType ElevatorCallType { get; set; }
        public int CurrentFloor { get; set; } = 1;
        public List<Passenger> Passengers { get; set; } = new();

        public override string ToString()
        {
            return State switch
            {
                ElevatorState.Idle => "The elevator is idle.",
                ElevatorState.Up => "The elevator is moving up.",
                ElevatorState.Down => "The elevator is moving down.",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
