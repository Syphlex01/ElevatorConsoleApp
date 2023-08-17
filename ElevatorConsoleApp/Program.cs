using System;
using System.Threading;
using static Elevator.Elevator;

namespace Elevator
{
    public class Program
    {
        private const string QUIT = "quit";

        public static void Main(string[] args)
        {
        Start:
            int floor;
            int elevators;
            string floorAmount;
            string elevatorAmount;
            Elevator elevator;
            Elevator[] elevatorsList;
            List<int> Queue = new List<int>();

            Console.WriteLine("How many floors are in the building?");
            floorAmount = Console.ReadLine();

            if (int.TryParse(floorAmount, out floor) && floor > 0)
            {
                elevator = new Elevator(floor);
            }
            else
            {
                DisplayError();
                goto Start;
            }

            Console.WriteLine("How many elevators does the building have? (Each elevator can handle 5 people at a time)");
            elevatorAmount = Console.ReadLine();

            if (int.TryParse(elevatorAmount, out elevators) && elevators > 0)
            {
                elevatorsList = new Elevator[elevators];

                for (int i = 0; i < elevators; i++)
                {
                    elevatorsList[i] = new Elevator(floor);
                }
            }
            else
            {
                DisplayError();
                goto Start;
            }

            string selectedFloor = string.Empty;
            int currentFloor = 0;
            int amountOfPeople = 0;
            bool isValid = false;

            while (selectedFloor != QUIT)
            {
                UpdateStatuses(elevatorsList);

                while (isValid == false)
                {
                    Console.WriteLine("Which floor are you on?");
                    string passangerFloor = Console.ReadLine();

                    if (!int.TryParse(passangerFloor, out currentFloor) && currentFloor > 0)
                    {
                        Console.WriteLine("There is no such floor, Please try again");
                    }
                    else
                    {
                        isValid = true;
                    }
                }

                Console.WriteLine("Which floor you would like to go to");
                selectedFloor = Console.ReadLine();

                if (int.TryParse(selectedFloor, out floor))
                {
                    //Doing queue on first come due to time constraint
                    Queue.Add(floor);
                    elevator = CheckElevator(elevatorsList, currentFloor, floor);
                    
                    isValid = false;

                    while (isValid == false)
                    {
                        Console.WriteLine("How many people are getting on?");
                        string peopleGettingOn = Console.ReadLine();

                        if (!int.TryParse(peopleGettingOn, out amountOfPeople))
                        {
                            Console.WriteLine("Please enter a valid number of people");
                        }
                        else if (elevator.PassangerCount + amountOfPeople <= elevator.PassangerLimit)
                        {
                            elevator.PassangerCount += amountOfPeople;
                            isValid = true;
                        }
                        else
                        {
                            Console.WriteLine("Weight limit exceeded");
                        }
                    }

                    if (elevator != null)
                    {
                        if (elevator.Status == ElevatorStatus.STOPPED && elevator.CurrentFloor != currentFloor)
                        {
                            //Elevator needs to move to your floor to pick you up
                            floor = currentFloor;
                        }

                        Task task = Task.Run(() =>
                        {
                            elevator.GoToFloor(floor);
                            Queue.RemoveAt(0);
                        }
                        );
                    }
                    else
                    {
                        Queue.Add(floor);
                    }
                }
                else if (selectedFloor == QUIT.ToLower())
                {
                    Console.WriteLine("Thank you for using our elevators, have a nice day");
                }
                else
                    Console.WriteLine("There is no such floor, Please try again (or use the stairs)");
            }
        }

        private static void UpdateStatuses(Elevator[] elevatorsList)
        {
            Console.Clear();

            foreach (var item in elevatorsList)
            {
                if (item.Status == ElevatorStatus.STOPPED)
                    Console.WriteLine("Elevator status: {0} at floor {1} with {2} people inside", item.Status, item.CurrentFloor, item.PassangerCount);
                else
                    Console.WriteLine("Elevator status: Moving {0} from {1} to {2} with {3} people inside", item.Status, item.CurrentFloor, item.MovingToFloor, item.PassangerCount);
            }

            Console.WriteLine();
        }

        private static void DisplayError()
        {
            Console.WriteLine("That' doesn't make sense...");
            Console.Beep();
            Thread.Sleep(2000);
            Console.Clear();
        }

        private static Elevator CheckElevator(Elevator[] elevatorList, int currentFloor, int selectedFloor)
        {
            Elevator elevator = elevatorList.Where(f => f.Status == ElevatorStatus.STOPPED)?.OrderBy(x => Math.Abs(x.CurrentFloor - currentFloor))?.FirstOrDefault();

            if (elevator == null)
            {
                //Check for nearest elevator going in similar direction
                int upOrDown = selectedFloor - currentFloor;

                if (upOrDown > 0)
                {
                    //up
                    elevator = elevatorList.Where(f => (f.Status == ElevatorStatus.UP) && f.CurrentFloor <= currentFloor).FirstOrDefault();
                }
                else
                {
                    //down
                    elevator = elevatorList.Where(f => (f.Status == ElevatorStatus.DOWN) && f.CurrentFloor >= currentFloor).FirstOrDefault();
                }
            }

            return elevator;
        }
    }

    public class Elevator
    {
        private bool[] floorReady;
        public int CurrentFloor = 1;
        public int MovingToFloor = 1;
        private int topfloor;
        public ElevatorStatus Status = ElevatorStatus.STOPPED;
        public int PassangerCount = 0;
        public int PassangerLimit = 5;

        public Elevator(int NumberOfFloors = 0)
        {
            floorReady = new bool[NumberOfFloors + 1];
            topfloor = NumberOfFloors;
        }

        private void Stop(int floor)
        {
            PassangerCount = 0;
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

        public async void GoToFloor(int floor)
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

        public enum ElevatorStatus
        {
            UP,
            STOPPED,
            DOWN
        }
    }
}