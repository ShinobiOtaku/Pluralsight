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
         var printer = system.ActorOf<PrinterActor>();

         var rand = new Random();

         system.Scheduler.Advanced.ScheduleRepeatedly(TimeSpan.FromMilliseconds(3), TimeSpan.FromSeconds(1), () =>
            {
               if(api.Ask<Routees>(new GetRoutees()).Result.Members.Any())
                  api.Tell(new VideoView(rand.Next(16), rand.Next(11)));
            });
         
         system.Scheduler.Advanced.ScheduleRepeatedly(TimeSpan.FromMilliseconds(5), TimeSpan.FromSeconds(5), () =>
            {
               if (api.Ask<Routees>(new GetRoutees()).Result.Members.Any())
                  api.Tell(new Login(rand.Next(11)), printer);
            });


         system.WhenTerminated.Wait();
      }
   }

   /// <summary>
   /// Prints recommendations out to the console
   /// </summary>
   public class PrinterActor : ReceiveActor
   {
      public PrinterActor()
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
