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

   public class VideoViewEvent
   {
      public int VideoId { get; }

      public int UserId { get; }

      public VideoViewEvent(int videoId, int userId)
      {
         VideoId = videoId;
         UserId = userId;
      }
   }
}
