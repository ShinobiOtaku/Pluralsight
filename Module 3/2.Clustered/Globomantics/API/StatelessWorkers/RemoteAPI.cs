using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Domain;
using static API.Domain.Genre;
using static System.TimeSpan;

namespace API.StatelessWorkers
{
   // Shhhhh, Let's pretend this hits a 3rd party API to get video details
   public class RemoteAPI
   {
      private readonly IEnumerable<Video> _inMemoryStore;

      public RemoteAPI()
      {
         _inMemoryStore = new List<Video>
         {
            new Video(0, "The Shawshank Redemption", Crime | Drama, FromMinutes(142), 9.2),
            new Video(1, "The Godfather", Crime | Drama, FromMinutes(175), 9.2),
            new Video(2, "The Godfather: Part II", Crime | Drama, FromMinutes(202), 9),
            new Video(3, "The Dark Knight", Genre.Action | Crime | Drama, FromMinutes(152), 8.9),
            new Video(4, "Pulp Fiction", Crime | Drama, FromMinutes(154), 8.9),
            new Video(5, "Schindler's List", Biography | Drama | History, FromMinutes(195), 8.9),
            new Video(6, "12 Angry Men", Crime | Drama, FromMinutes(96), 8.9),
            new Video(7, "The Lord of the Rings: The Return of the King", Adventure | Drama | Fantasy, FromMinutes(201), 8.9),
            new Video(8, "The Good, the Bad and the Ugly", Western, FromMinutes(148), 8.9),
            new Video(9, "Fight Club", Drama, FromMinutes(139), 8.8),
            new Video(10,"The Lord of the Rings: The Fellowship of the Ring", Adventure | Drama | Fantasy, FromMinutes(178), 8.8),
            new Video(11,"Star Wars: Episode V - The Empire Strikes Back", Genre.Action | Adventure | Fantasy, FromMinutes(124), 8.7),
            new Video(12,"Forrest Gump", Romance | Drama, FromMinutes(142), 8.7),
            new Video(13,"Inception", Genre.Action | Mystery | SciFi, FromMinutes(148), 8.7),
            new Video(14,"One Flew Over the Cuckoo's Nest", Drama, FromMinutes(133), 8.7),
            new Video(15,"The Lord of the Rings: The Two Towers", Adventure | Drama | Fantasy, FromMinutes(179), 8.7)
         };
      }

      //Simulate a real remote api signature
      public async Task<Video[]> GetUnseenVideosAsync(int[] watchedVideoIds)
      {
         await Task.Yield();

         var result = _inMemoryStore
            .Where(vid => !watchedVideoIds.Contains(vid.Id))
            .ToArray();

         return result;
      }
   }
}
