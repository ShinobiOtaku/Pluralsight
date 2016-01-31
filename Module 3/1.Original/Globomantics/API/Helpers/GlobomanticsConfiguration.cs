using System.Configuration;
using Akka.Configuration.Hocon;

namespace API.Helpers
{
   /// <summary>
   /// Helper to provide settings specified in the globomantics section of HOCON
   /// </summary>
   internal static class GlobomanticsConfiguration
   {
      public static int NumberOfRecommendations { get; } = 5;

      static GlobomanticsConfiguration()
      {
         var section = (AkkaConfigurationSection)ConfigurationManager.GetSection("akka");
         var akkaConfig = section.AkkaConfig;

         var globomantics = akkaConfig.GetConfig("globomantics");
         if (globomantics != null)
            NumberOfRecommendations = globomantics.GetInt("number-of-recommendations", NumberOfRecommendations);
      }
   }
}