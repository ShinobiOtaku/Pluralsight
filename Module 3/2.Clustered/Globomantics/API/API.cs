using Akka.Actor;
using Akka.Routing;

namespace API
{
   /// <summary>
   /// Top-level actor, all client messages will flow through this actor
   /// </summary>
   internal class API : ReceiveActor
   {
      private readonly IActorRef _videoDetails;
      private readonly IActorRef _viewsRepo;

      public API(IActorRef viewsRepo, IActorRef videoDetails)
      {
         _viewsRepo = viewsRepo;
         _videoDetails = videoDetails;

         Start();
      }

      private void Start()
      {
         Receive<LoginEvent>(login =>
         {
            var master = Context.ActorOf(Props.Create(() => new RecommendationWorkflow(_viewsRepo, _videoDetails)));
            master.Tell(new RecommendationJob(login.UserId, Sender));
         });

         //Best effort
         Receive<VideoWatchedEvent>(view => _viewsRepo.Tell(new Broadcast(view)));
      }
   }
}