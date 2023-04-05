using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Erkon.Classes
{
    public class AirconTemperature
    {
        private readonly IConfiguration _configuration;
        private readonly int _min;
        private readonly int _max;

        public AirconTemperature(IConfiguration configuration)
        {
            _configuration = configuration;
            _min = Convert.ToInt16(_configuration["Temperature:Min"]);
            _max = Convert.ToInt16(_configuration["Temperature:Max"]);
        }

        public List<int> Listing()
        {
            var l = new List<int>();
            for (var i = _min; i <= _max; i++)
            {
                l.Add(i);
            }
            return l;
        }
    }
}
