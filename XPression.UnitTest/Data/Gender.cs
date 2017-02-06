using System;

namespace XPression.UnitTest.Data
{
   // just for testing, normally Gender never would be a flag
   [Flags]
   public enum Gender : short
   {
      Unknown,
      Male = 1,
      Female = 2
   }
}
