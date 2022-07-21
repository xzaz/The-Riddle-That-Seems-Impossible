using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            int cRooms = 100;
            int cPrisoners = 100;
            int cOpenRooms = 50;
            int runs = 1000;

            List<Prisoner> prisoners = new List<Prisoner>(cRooms);
            // create prisoners
            for (int i = 1; i <= cPrisoners; i++)
            {
                prisoners.Add(new Prisoner()
                {
                    Number = i
                });
            }

            // No strat method
            var noStratStats = NoStrat(prisoners, cRooms, cOpenRooms, runs, false);

            Console.WriteLine(noStratStats);

            WriteFile("random.csv", noStratStats.ByPercentage().Select(v => $"{v.Key};{v.Value}").ToArray());

            // loop strat
            var loopStatStrat = LoopStrat(prisoners, cRooms, cOpenRooms, runs, false);
            
            Console.WriteLine(loopStatStrat);

            WriteFile("loop.csv", loopStatStrat.ByPercentage().Select(v => $"{v.Key};{v.Value}").ToArray());
        }

        private static void WriteFile(string name, string[] lines)
        {
            File.WriteAllLines(name, lines);
        }

        private static List<Room> CreateRooms(int amount, bool verbose = false)
        {

            List<Room> rooms = new List<Room>(amount);
            var rndList = new Random();
            int[] roomPrisList = Enumerable.Range(1, amount).OrderBy(v => rndList.Next(1, 100)).ToArray();

            // create rooms
            for (int i = 1; i <= amount; i++)
            {
                rooms.Add(new Room()
                {
                    PrisonerNumber = roomPrisList[i - 1],
                    Number = i
                });
            }

            // print out the rooms
            if(verbose) Console.WriteLine($"Created {amount} rooms: {string.Join(",", rooms.Select(v => $"[{v.Number},{v.PrisonerNumber}]"))}");

            return rooms;
        }

        private static Stats NoStrat(List<Prisoner> prisoners, int cRooms, int cOpenRooms, int runs, bool verbose = false)
        {
            // result
            var st = new Stats(runs);
            st.StratName = $"NoStrat, Random select {cRooms} amount of rooms";

            for(int r = 1; r <= st.AmountOfRuns; r++) {
                var run = new StatsRun();
                run.RunNumber = r;
                var rooms = CreateRooms(cRooms);

                foreach (var pris in prisoners)
                {
                    Room fRoom = null; // room number prisoner is the same

                    var listOfRoomsToOpen = createRandomList(cOpenRooms, cRooms);
                    // doors to open
                    foreach (var rn in listOfRoomsToOpen)
                    {
                        var room = rooms[rn - 1];
                        if (verbose) Console.WriteLine($"Prisoner {pris.Number} tried { Array.IndexOf(listOfRoomsToOpen, rn) + 1 } times and opened {rn - 1} that contains number: {room.PrisonerNumber}");

                        if (pris.Number == room.PrisonerNumber)
                        {
                            fRoom = room;
                            // found the number next prisoner
                            break;
                        }
                    }

                    // check if was not found
                    if (fRoom == null)
                    {
                        if (verbose)  Console.WriteLine($"[X] Prisoner {pris.Number} could not find its room");

                        run.NotFound++;
                    }
                    else
                    {
                        if (verbose) Console.WriteLine($"[V] Prisoner {pris.Number} found its room '{fRoom.Number}'");

                        run.Found++;
                    }
                }

                st.Runs.Add(run);
            }

            return st;
        }

        private static Stats LoopStrat(List<Prisoner> prisoners, int cRooms, int cOpenRooms, int runs, bool verbose = false)
        {
            var stats = new Stats(runs);
            for (int r = 1; r <= stats.AmountOfRuns; r++)
            {
                var rooms = CreateRooms(cRooms);
                var run = new StatsRun();
                run.RunNumber = r;
                foreach (var prisioner in prisoners)
                {
                    Room room = null;
                    int number = prisioner.Number;
                    int it = 0;
                    do
                    {
                        if (it == cOpenRooms)
                            break;

                        number = room != null ? room.PrisonerNumber : prisioner.Number;
                        room = rooms.First(r => r.Number == number);

                        if (verbose)
                            Console.WriteLine($"Prisioner with number {prisioner.Number} searching room #{room.Number} => #{room.PrisonerNumber}, iteration {it}");

                        it++;

                    } while (room.PrisonerNumber != prisioner.Number);

                    // check if we found it
                    if (room.PrisonerNumber == prisioner.Number)
                    {
                        if (verbose)
                            Console.WriteLine($"[V] Prisoner {prisioner.Number} found its room '{room.Number}'");

                        run.Found++;
                    } else
                    {
                        if (verbose)
                            Console.WriteLine($"[X] Prisoner {prisioner.Number} could not find its room");

                        run.NotFound++;
                    }
                }

                stats.Runs.Add(run);
            }

            return stats;
        }

        private static int[] createRandomList(int amount, int doors)
        {
            var r = new Random();
            List<int> result = new List<int>();

            while(result.Count < amount)
            {
                var rnd = r.Next(1, doors);
                if (!result.Contains(rnd))
                    result.Add(rnd);
            }

            return result.ToArray();
        }
    }

    class Room : IRP
    {
        public int PrisonerNumber { get; set; }
        public int Number { get; set; }
    }

    class Prisoner : IRP
    {
        public int Number { get; set; }
    }

    class Stats
    {
        public string StratName { get; set; }
        public List<StatsRun> Runs { get; set; }
        public int AmountOfRuns { get; set; }
        
        public Stats(int runs)
        {
            AmountOfRuns = runs;
            Runs = new List<StatsRun>(runs);
        }

        public override string ToString()
        {
            return $"{StratName}: {Environment.NewLine} {String.Join(Environment.NewLine, Runs.Select(v => v.ToString()))}";
        }

        public Dictionary<int, int> ByPercentage()
        {
            var dic = new Dictionary<int, int>();

            for(int i = 0; i <= 100; i++)
            {
                dic.Add(i, this.Runs.Where(v => Convert.ToInt32(v.PercentageFound * 100) == i).Count());
            }

            return dic;
        }
    }

    class StatsRun
    {
        public int RunNumber { get; set; }
        public int Found { get; set; }
        public int NotFound { get; set; }

        public decimal PercentageFound => (decimal)Found / (decimal)(Found + NotFound);

        public override string ToString()
        {
            return $"{RunNumber};{Found};{NotFound};{PercentageFound}";
        }
    }

    interface IRP
    {
        int Number { get; set; }
    }
}
