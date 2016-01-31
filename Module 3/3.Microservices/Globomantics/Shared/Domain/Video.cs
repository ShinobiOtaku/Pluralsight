using System;

namespace Shared.Domain
{
   public class Video
   {
      public int Id { get; }

      public string Title { get; }

      public Genre Genres { get; }

      public TimeSpan RunningTime { get; }

      public double Rating { get; }

      public Video(int id, string title, Genre genres, TimeSpan runningTime, double rating)
      {
         Id = id;
         Title = title;
         Genres = genres;
         RunningTime = runningTime;
         Rating = rating;
      }
   }
}