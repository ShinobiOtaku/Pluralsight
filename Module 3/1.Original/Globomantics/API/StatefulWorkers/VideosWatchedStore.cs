using System;
using System.Collections.Generic;
using System.Linq;
using Akka;
using Akka.Actor;
using Akka.Persistence;

namespace API.StatefulWorkers
{
   /// <summary>
   /// Request for all the videos a user has viewed
   /// </summary>
   internal class PreviouslyWatchedVideosRequest
   {
      public RecommendationJob Job { get; }

      public PreviouslyWatchedVideosRequest(RecommendationJob job)
      {
         Job = job;
      }
   }

   /// <summary>
   /// Response containing all videos a user has viewed
   /// </summary>
   internal class PreviouslyWatchedVideosResponse
   {
      public RecommendationJob Job { get; }

      public int[] PreviouslySeenVideoIds { get; }

      public PreviouslyWatchedVideosResponse(RecommendationJob job, int[] previouslySeenVideoIds)
      {
         Job = job;
         PreviouslySeenVideoIds = previouslySeenVideoIds;
      }
   }


   /// <summary>
   /// Durable store of which users have viewed which videos
   /// Can be queried for which videos a user has viewed
   /// </summary>
   internal class VideosWatchedStore : PersistentActor//Part of Akka.Persistence
   {
      private List<VideoWatchedEvent> _store = new List<VideoWatchedEvent>();

      public override string PersistenceId { get; } = "ViewsStore";

      public VideosWatchedStore()
      {
         Console.WriteLine(nameof(VideosWatchedStore) + " started");
      }

      protected override bool ReceiveRecover(object message)
      {
         return message.Match()
            .With<VideoWatchedEvent>(view => _store.Add(view))
            .With<SnapshotOffer>(offer =>
            {
               _store = (List<VideoWatchedEvent>)offer.Snapshot;
               Console.WriteLine($"Recovered state with {_store.Count} views");
            })
            .WasHandled;
      }

      protected override bool ReceiveCommand(object message)
      {
         return message.Match()
            .With<VideoWatchedEvent>(view =>
            {
               Persist(view, v =>
               {
                  _store.Add(v);
                  SaveSnapshot(_store);
               });
               Console.WriteLine($"Persisting {nameof(VideoWatchedEvent)}. video: {view.VideoId} user: {view.UserId}");
            })
            .With<PreviouslyWatchedVideosRequest>(req =>
            {
               Console.WriteLine(nameof(PreviouslyWatchedVideosRequest) + $" for user {req.Job.UserId}");

               var result = _store
                  .Where(e => e.UserId == req.Job.UserId)
                  .Select(e => e.VideoId)
                  .Distinct()
                  .ToArray();

               Sender.Tell(new PreviouslyWatchedVideosResponse(req.Job, result));
            })
            .WasHandled;
      }
   }
}