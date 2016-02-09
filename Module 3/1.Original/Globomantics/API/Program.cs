using System;
using System.Linq;
using Akka.Actor;
using Akka.Routing;
using API.StatefulWorkers;
using API.StatelessWorkers;

namespace API
{
   public class Program
   {
      private static void Main(string[] _)
      {
         var system = ActorSystem.Create("globomantics");

         var viewsPool = system.ActorOf(Props.Create<VideosWatchedStore>().WithRouter(FromConfig.Instance), "views");

         var videoDetailsPool = system.ActorOf(Props.Create<VideoDetailsFetcher>().WithRouter(FromConfig.Instance), "videoDetails");

         var api = system.ActorOf(Props.Create(() => new API(viewsPool, videoDetailsPool)), "api");

         var rand = new Random();

         var printer = system.ActorOf<Printer>();

         system.Scheduler.Advanced.ScheduleRepeatedly(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(1),
            () => api.Tell(new VideoWatchedEvent(rand.Next(16), rand.Next(11))));

         system.Scheduler.Advanced.ScheduleRepeatedly(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5),
            () => api.Tell(new LoginEvent(rand.Next(11)), printer));


         system.WhenTerminated.Wait();
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
}