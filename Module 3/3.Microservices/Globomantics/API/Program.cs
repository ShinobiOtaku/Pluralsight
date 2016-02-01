using Akka.Actor;
using Akka.Routing;
using StatefulWorkers;
using StatelessWorkers;

namespace API
{
   public class Program
   {
      private static void Main(string[] _)
      {
         var system = ActorSystem.Create("globomantics");

         var viewsPool = system.ActorOf(Props.Create<VideosWatchedStore>().WithRouter(FromConfig.Instance), "views");

         var videoDetailsPool = system.ActorOf(Props.Create<VideoDetailsFetcher>().WithRouter(FromConfig.Instance), "videoDetails");

         system.ActorOf(Props.Create(() => new API(viewsPool, videoDetailsPool)), "api");

         system.WhenTerminated.Wait();
      }
   }
}