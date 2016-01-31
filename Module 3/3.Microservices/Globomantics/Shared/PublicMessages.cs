using Akka.Actor;
using Shared.Domain;

namespace Shared
{
   /// <summary>
   /// Messages available outside of this assembly
   /// </summary>

   public class Recommendation
   {
      public Video[] RecommendedVideos { get; }

      public Recommendation(Video[] recommendedVideos)
      {
         RecommendedVideos = recommendedVideos;
      }
   }

   public class Login
   {
      public int UserId { get; }

      public Login(int userId)
      {
         UserId = userId;
      }
   }

   public class VideoView
   {
      public int VideoId { get; }

      public int UserId { get; }

      public VideoView(int videoId, int userId)
      {
         VideoId = videoId;
         UserId = userId;
      }
   }

   /// <summary>
   /// Initiates the recommendation workflow for a user
   /// </summary>
   public class RecommendationJob
   {
      public int UserId { get; }

      public IActorRef Client { get; }

      public RecommendationJob(int userId, IActorRef client)
      {
         UserId = userId;
         Client = client;
      }
   }
}
