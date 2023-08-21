using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElevatorConsoleApp
{
    public class Elevator
    {
        private bool[] floorReady;
        public int CurrentFloor = 1;
        public int MovingToFloor = 1;
        private int topfloor;
        public ElevatorStatus Status = ElevatorStatus.STOPPED;
        public int PassengerCount = 0;
        public int PassengerLimit = 5;

        public Elevator(int NumberOfFloors = 0)
        {
            floorReady = new bool[NumberOfFloors + 1];
            topfloor = NumberOfFloors;
        }

        private void Stop(int floor)
        {
            PassengerCount = 0;
            Status = ElevatorStatus.STOPPED;
            CurrentFloor = floor;
            floorReady[floor] = false;
        }

        private void Descend(int floor)
        {
            Status = ElevatorStatus.DOWN;

            for (int i = CurrentFloor; i >= 1; i--)
            {
                Thread.Sleep(2000); //Pretend the elevator takes 2 seconds to move between floors
                CurrentFloor = i - 1;

                if (floorReady[i])
                    Stop(floor);
                else
                    continue;
            }

            Status = ElevatorStatus.STOPPED;
        }

        private void Ascend(int floor)
        {
            Status = ElevatorStatus.UP;

            for (int i = CurrentFloor; i <= topfloor; i++)
            {
                Thread.Sleep(2000); //Pretend the elevator takes 2 seconds to move between floors
                CurrentFloor = i - 1;

                if (floorReady[i])
                    Stop(floor);
                else
                    continue;
            }

            Status = ElevatorStatus.STOPPED;
        }

        void Stay()
        {
            Console.WriteLine("Elevator is already on this floor");
        }

        public void GoToFloor(int floor)
        {
            if (floor > topfloor)
            {
                Console.WriteLine("We only have {0} floors", topfloor);
                return;
            }

            MovingToFloor = floor;
            floorReady[floor] = true;

            switch (Status)
            {
                case ElevatorStatus.DOWN:
                    Descend(floor);
                    break;

                case ElevatorStatus.STOPPED:
                    if (CurrentFloor < floor)
                        Ascend(floor);
                    else if (CurrentFloor == floor)
                        Stay();
                    else
                        Descend(floor);
                    break;

                case ElevatorStatus.UP:
                    Ascend(floor);
                    break;

                default:
                    break;
            }
        }

        
    }
}
