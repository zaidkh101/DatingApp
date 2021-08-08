using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Extensions
{
    public static class DateTimeExtensions
    {
        public static int CalculateAge(this DateTime DoB)
        {
            var Today = DateTime.Today;
            var Age = Today.Year - DoB.Year;
            if (DoB.Date > Today.AddYears(-Age)) Age--;
            return Age;
        }
    }
}
