using System;
using System.Threading.Tasks;
using System.Linq;
using Akka.Actor;
using Akka.Routing;
using API.Helpers;
using API.StatefulWorkers;
using API.StatelessWorkers;

namespace API
{
   /// <summary>
   /// Initiates the recommendation workflow for a user
   /// </summary>
   internal class RecommendationJob
   {
      public int UserId { get; }

      public IActorRef Client { get; }

      public RecommendationJob(int userId, IActorRef client)
      {
         UserId = userId;
         Client = client;
      }
   }

   /// <summary>
   /// Performs all the collaboration required to create a recommendation.
   /// One recommendation per actor, lives for the lifetime of the lifecycle
   /// 
   /// 1. Wait until all required actors are up
   /// 2. Query view store for previously viewed videos
   /// 3. Query video details fetcher for unseen videos
   /// 4. Build and send recommendation
   /// </summary>
   internal class RecommendationWorkflow : ReceiveActor
   {
      private class BeginAttempt
      {
         public RecommendationJob Job { get; }

         public BeginAttempt(RecommendationJob job)
         {
            Job = job;
         }
      }

      private class JobAttempt
      {
         public RecommendationJob Job { get; }

         public bool CanStart { get; }

         public JobAttempt(RecommendationJob job, bool canStart)
         {
            Job = job;
            CanStart = canStart;
         }
      }
      
      private readonly IActorRef _videoDetails;
      private readonly IActorRef _viewsRepo;
      private ICancelable _startAttempts;

      public RecommendationWorkflow(IActorRef viewsRepo, IActorRef videoDetails)
      {
         _viewsRepo = viewsRepo;
         _videoDetails = videoDetails;

         AcceptingJob();
      }

      private void AcceptingJob()
      {
         Receive<RecommendationJob>(job =>
         {
            Console.WriteLine($"Starting recommendation workflow for user {job.UserId}");

            _startAttempts = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(
               TimeSpan.Zero, TimeSpan.FromMilliseconds(200), Self, new BeginAttempt(job), ActorRefs.NoSender);

            Become(Working);
         });
      }

      private void Working()
      {
         Receive<BeginAttempt>(begin =>
         {
            var viewsRoutees = _viewsRepo.Ask<Routees>(new GetRoutees());
            var videoDetailsRoutees = _videoDetails.Ask<Routees>(new GetRoutees());

            Task.WhenAll(viewsRoutees, videoDetailsRoutees)
               .ContinueWith(allRoutees => new JobAttempt(begin.Job, allRoutees.Result.All(r => r.Members.Any())))
               .PipeTo(Self);
            //Each router has at least one routee
         });

         Receive<JobAttempt>(attempt => !attempt.CanStart, attempt => Console.WriteLine("JobAttempt failed"));

         Receive<JobAttempt>(attempt =>  attempt.CanStart, attempt =>
         {
            _startAttempts.Cancel();

            _viewsRepo
               .Ask<PreviouslyWatchedVideosResponse>(new PreviouslyWatchedVideosRequest(attempt.Job), TimeSpan.FromSeconds(20))
               .PipeTo(Self);
         });

         Receive<PreviouslyWatchedVideosResponse>(resp =>
         {
            _videoDetails
               .Ask<UnwatchedVideosResponse>(new UnwatchedVideosRequest(resp.Job, resp.PreviouslySeenVideoIds), TimeSpan.FromSeconds(20))
               .PipeTo(Self);
         });

         Receive<UnwatchedVideosResponse>(unseen =>
         {
            var recommendedVideos = unseen.UnseenVideos
                  .OrderByDescending(v => v.Rating)
                  .Take(GlobomanticsConfiguration.NumberOfRecommendations)
                  .ToArray();

            unseen.Job.Client.Tell(new Recommendation(recommendedVideos));

            //My work here is done
            Self.Tell(PoisonPill.Instance);
            Console.WriteLine("Workflow finished");
         });

         Receive<Status.Failure>(f => Console.WriteLine("Error contacting other nodes"));
      }
   }
}