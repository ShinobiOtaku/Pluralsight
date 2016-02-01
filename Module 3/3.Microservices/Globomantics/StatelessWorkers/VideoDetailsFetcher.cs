using System;
using Akka.Actor;
using Shared;
using Shared.Domain;

namespace StatelessWorkers
{
   /// <summary>
   /// Request to see which videos have not been viewed by a user
   /// </summary>
   public class UnseenVideosRequest
   {
      public RecommendationJob Job { get; }

      public int[] PreviouslySeenVideoIds { get; }

      public UnseenVideosRequest(RecommendationJob job, int[] previouslySeenVideoIds)
      {
         Job = job;
         PreviouslySeenVideoIds = previouslySeenVideoIds;
      }
   }

   /// <summary>
   /// Response containing which videos have not been viewed by a user
   /// </summary>
   public class UnseenVideosResponse
   {
      public RecommendationJob Job { get; }

      public Video[] UnseenVideos { get; }

      public UnseenVideosResponse(RecommendationJob job, Video[] unseenVideos)
      {
         Job = job;
         UnseenVideos = unseenVideos;
      }
   }

   /// <summary>
   /// Actor which wraps remote API calls to fetch details of videos
   /// Only allows one in-flight request at once
   /// </summary>
   public class VideoDetailsFetcher : ReceiveActor, IWithUnboundedStash
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
         Receive<UnseenVideosRequest>(req =>
         {
            Console.WriteLine(nameof(UnseenVideosRequest) + $" for user {req.Job.UserId}");

            _remoteAPI
               .GetUnseenVideosAsync(req.PreviouslySeenVideoIds)
               .PipeTo(Self, Sender, result => new UnseenVideosResponse(req.Job, result));

            Become(Busy);
         });
      }

      //Limit in-flight requests
      private void Busy()
      {
         Receive<UnseenVideosResponse>(resp =>
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