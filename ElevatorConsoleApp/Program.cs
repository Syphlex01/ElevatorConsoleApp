using ElevatorConsoleApp;

namespace ElevatorApp
{
    public class Program
    {
        private const string QUIT = "quit";

        public static void Main(string[] args)
        {
        Start:
            string floorAmount;
            string elevatorAmount;
            Elevator elevator;
            Elevator[] elevatorsList;
            List<int> queue = new List<int>();

            Console.WriteLine("How many floors are in the building?");
            floorAmount = Console.ReadLine();

            if (int.TryParse(floorAmount, out int floor) && floor > 0)
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

            if (int.TryParse(elevatorAmount, out int elevators) && elevators > 0)
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
            
            while (selectedFloor != QUIT)
            {
                bool isValid = false;

                UpdateStatuses(elevatorsList);

                while (isValid == false)
                {
                    Console.WriteLine("Which floor are you on?");
                    string passengerFloor = Console.ReadLine();

                    if (!int.TryParse(passengerFloor, out currentFloor) && currentFloor > 0)
                    {
                        Console.WriteLine("There is no such floor, Please try again");
                    }
                    else
                    {
                        isValid = true;
                    }
                }

                Console.WriteLine("Which floor you would like to go to?");
                selectedFloor = Console.ReadLine();

                if (int.TryParse(selectedFloor, out floor))
                {
                    //Doing queue on first come due to time constraint
                    queue.Add(floor);
                    elevator = CheckElevator(elevatorsList, currentFloor, floor);

                    isValid = false;

                    while (isValid == false)
                    {
                        Console.WriteLine("How many people are getting on right now?");
                        string peopleGettingOn = Console.ReadLine();

                        if (!int.TryParse(peopleGettingOn, out amountOfPeople))
                        {
                            Console.WriteLine("Please enter a valid number of people");
                        }
                        else if (elevator.PassengerCount + amountOfPeople <= elevator.PassengerLimit)
                        {
                            elevator.PassengerCount += amountOfPeople;
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

                        Task task = Task.Run(async () =>
                            {
                                elevator.GoToFloor(floor);
                                queue.RemoveAt(0);
                            }
                        );
                    }
                    else
                    {
                        queue.Add(floor);
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
                    Console.WriteLine($"Elevator status: {item.Status} at floor {item.CurrentFloor} with {item.PassengerCount} people inside");
                else
                    Console.WriteLine($"Elevator status: Moving {item.Status} from {item.CurrentFloor} to {item.MovingToFloor} with {item.PassengerCount} people inside");
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

                elevator = (upOrDown > 0 ? elevatorList.FirstOrDefault(f => (f.Status == ElevatorStatus.UP) && f.CurrentFloor <= currentFloor) :
                    elevatorList.FirstOrDefault(f => (f.Status == ElevatorStatus.DOWN) && f.CurrentFloor >= currentFloor))!;
            }

            return elevator;
        }
    }

    
}