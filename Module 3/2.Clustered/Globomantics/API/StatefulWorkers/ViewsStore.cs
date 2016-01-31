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
   internal class PreviouslyViewedVideosRequest
   {
      public RecommendationJob Job { get; }

      public PreviouslyViewedVideosRequest(RecommendationJob job)
      {
         Job = job;
      }
   }

   /// <summary>
   /// Response containing all videos a user has viewed
   /// </summary>
   internal class PreviouslyViewedVideosResponse
   {
      public RecommendationJob Job { get; }

      public int[] PreviouslyViewedVideoIds { get; }

      public PreviouslyViewedVideosResponse(RecommendationJob job, int[] previouslyViewedVideoIds)
      {
         Job = job;
         PreviouslyViewedVideoIds = previouslyViewedVideoIds;
      }
   }


   /// <summary>
   /// Durable store of which users have viewed which videos
   /// Can be queried for which videos a user has viewed
   /// </summary>
   internal class ViewsStore : PersistentActor//Part of Akka.Persistence
   {
      private class Save { }

      private List<VideoView> _store = new List<VideoView>();

      private readonly TimeSpan _snapshotInterval = TimeSpan.FromSeconds(10);

      public override string PersistenceId { get; } = Context.Self.Path.ToString();

      public ViewsStore()
      {
         Context.System.Scheduler.ScheduleTellRepeatedly(
            _snapshotInterval, _snapshotInterval, Self, new Save(), ActorRefs.NoSender);
      }

      protected override bool ReceiveRecover(object message)
      {
         return message.Match()
            .With<VideoView>(view => _store.Add(view))
            .With<SnapshotOffer>(offer =>
            {
               _store = (List<VideoView>)offer.Snapshot;
               Console.WriteLine($"Recovered state with {_store.Count} views");
            })
            .WasHandled;
      }

      protected override bool ReceiveCommand(object message)
      {
         return message.Match()
            .With<Save>(_ => SaveSnapshot(_store))
            .With<VideoView>(view =>
            {
               Persist(view, _store.Add);
               Console.WriteLine($"Persisting view. video: {view.VideoId} user: {view.UserId}");
            })
            .With<PreviouslyViewedVideosRequest>(req =>
            {
               Console.WriteLine(nameof(PreviouslyViewedVideosRequest) + $" for user {req.Job.UserId}");

               var result = _store
                  .Where(e => e.UserId == req.Job.UserId)
                  .Select(e => e.VideoId)
                  .Distinct()
                  .ToArray();

               Sender.Tell(new PreviouslyViewedVideosResponse(req.Job, result));
            })
            .WasHandled;
      }
   }
}