using System.Collections.Generic;

namespace ConsoleApp1
{
    public class WeatherData
    {
        public MainData Main { get; set; }
        public List<WeatherInfo> Weather { get; set; }
        public Clouds Clouds { get; set; }
        public Wind Wind { get; set; }
        public int Visibility { get; set; }
        public Rain Rain { get; set; }
        public string Dt_txt { get; set; }
    }
}