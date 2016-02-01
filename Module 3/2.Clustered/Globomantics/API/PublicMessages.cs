using API.Domain;

namespace API
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

   public class LoginEvent
   {
      public int UserId { get; }

      public LoginEvent(int userId)
      {
         UserId = userId;
      }
   }

   public class VideoWatchedEvent
   {
      public int VideoId { get; }

      public int UserId { get; }

      public VideoWatchedEvent(int videoId, int userId)
      {
         VideoId = videoId;
         UserId = userId;
      }
   }
}
