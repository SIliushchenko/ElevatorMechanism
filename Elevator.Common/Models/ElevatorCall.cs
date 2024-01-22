namespace Elevator.Common.Models
{
    public class ElevatorCall
    {
        public int Floor { get; set; }
        public int SelectedFloor { get; set; }
        public Direction Direction { get; set; }
    }
}
