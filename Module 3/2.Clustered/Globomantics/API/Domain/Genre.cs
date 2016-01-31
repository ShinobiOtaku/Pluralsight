using System;

namespace API.Domain
{
   [Flags]
   public enum Genre
   {
      Crime = 1,
      Drama = 2,
      Action = 4,
      Biography = 8,
      History = 16,
      Adventure = 32,
      Fantasy = 64,
      Western = 128,
      Romance = 256,
      Mystery = 512,
      SciFi = 1024
   }
}