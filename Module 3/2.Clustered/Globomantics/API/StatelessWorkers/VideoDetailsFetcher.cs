using System;
using Akka.Actor;
using API.Domain;

namespace API.StatelessWorkers
{
   /// <summary>
   /// Request to see which videos have not been viewed by a user
   /// </summary>
   internal class UnviewedVideosRequest
   {
      public RecommendationJob Job { get; }

      public int[] PreviouslySeenVideoIds { get; }

      public UnviewedVideosRequest(RecommendationJob job, int[] previouslySeenVideoIds)
      {
         Job = job;
         PreviouslySeenVideoIds = previouslySeenVideoIds;
      }
   }

   /// <summary>
   /// Response containing which videos have not been viewed by a user
   /// </summary>
   internal class UnviewedVideosResponse
   {
      public RecommendationJob Job { get; }

      public Video[] UnseenVideos { get; }

      public UnviewedVideosResponse(RecommendationJob job, Video[] unseenVideos)
      {
         Job = job;
         UnseenVideos = unseenVideos;
      }
   }

   /// <summary>
   /// Actor which wraps remote API calls to fetch details of videos
   /// Only allows one in-flight request at once
   /// </summary>
   internal class VideoDetailsFetcher : ReceiveActor, IWithUnboundedStash
   {
      private readonly RemoteAPI _remoteAPI = new RemoteAPI();

      public IStash Stash { get; set; }

      public VideoDetailsFetcher()
      {
         Console.WriteLine(nameof(VideoDetailsFetcher) + " started");
         Ready();
      }

      private void Ready()
      {
         Receive<UnviewedVideosRequest>(req =>
         {
            Console.WriteLine(nameof(UnviewedVideosRequest) + $" for user {req.Job.UserId}");

            _remoteAPI
               .GetUnseenVideosAsync(req.PreviouslySeenVideoIds)
               .PipeTo(Self, Sender, result => new UnviewedVideosResponse(req.Job, result));

            Become(Busy);
         });
      }

      //Limit in-flight requests
      private void Busy()
      {
         Receive<UnviewedVideosResponse>(resp =>
         {
            Sender.Tell(resp);
            GetReady();
         });

         ReceiveAny(_ => Stash.Stash());
      }

      private void GetReady()
      {
         Stash.UnstashAll();
         Become(Ready);
      }
   }
}