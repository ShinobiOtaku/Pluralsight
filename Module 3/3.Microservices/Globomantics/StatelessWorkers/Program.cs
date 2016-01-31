using Akka.Actor;

namespace StatelessWorkers
{
   class Program
   {
      private static void Main(string[] _) =>
         ActorSystem.Create("globomantics").WhenTerminated.Wait();
   }
}
