using Akka.Actor;

namespace MyActorSystem
{
   class Program
   {
      static void Main(string[] args)
      {
         ActorSystem
            .Create("MyActorSystem")
            .AwaitTermination();
      }
   }
}
