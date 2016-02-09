using System;
using System.Linq;
using Akka.Actor;
using Akka.Routing;
using API;

namespace Client
{
   class Program
   {
      private static void Main(string[] _)
      {
         var system = ActorSystem.Create("globomantics");

         var api = system.ActorOf(Props.Empty.WithRouter(FromConfig.Instance), "api");
         var printer = system.ActorOf<Printer>();

         var rand = new Random();

         system.Scheduler.Advanced.ScheduleRepeatedly(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), () =>
            {
               if(api.Ask<Routees>(new GetRoutees()).Result.Members.Any())
                  api.Tell(new VideoWatchedEvent(rand.Next(16), rand.Next(11)));
            });
         
         system.Scheduler.Advanced.ScheduleRepeatedly(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4), () =>
            {
               if (api.Ask<Routees>(new GetRoutees()).Result.Members.Any())
                  api.Tell(new LoginEvent(rand.Next(11)), printer);
            });


         system.WhenTerminated.Wait();
      }
   }

   /// <summary>
   /// Prints recommendations out to the console
   /// </summary>
   public class Printer : ReceiveActor
   {
      public Printer()
      {
         Receive<Recommendation>(res =>
         {
            var results = string.Join(Environment.NewLine, res.RecommendedVideos.Select(x => x.Title));

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Recommendations: {Environment.NewLine}{results}");
            Console.ResetColor();
         });
      }
   }
}
