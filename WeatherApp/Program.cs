using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Data.SQLite;
using Newtonsoft.Json;

namespace ConsoleApp1
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();
        private static string url = "http://api.openweathermap.org/data/2.5/forecast?id=524901&appid=3185b978c0671beea080ac05cad84534";

        static SQLiteConnection CreateConnection()
        {
            SQLiteConnection sqlite_conn;
            sqlite_conn = new SQLiteConnection("Data Source=database.db; Version = 3; New = True; Compress = True; ");
            try
            {
                sqlite_conn.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return sqlite_conn;
        }

        static void CreateTable(SQLiteConnection conn)
        {
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();

            string Createsql = "CREATE TABLE IF NOT EXISTS WeatherData (" +
                "DateTime DATETIME," +
                "Temperature REAL," +
                "WeatherDescription TEXT," +
                "Rain REAL," +
                "Clouds INT," +
                "WindSpeed REAL," +
                "WindDeg INT," +
                "MainTemp REAL," +
                "MainFeelsLike REAL," +
                "MainTempMin REAL," +
                "MainTempMax REAL," +
                "MainPressure INT," +
                "MainSeaLevel INT," +
                "MainGrndLevel INT," +
                "MainHumidity INT," +
                "MainTempKf REAL" +
                ")";

            sqlite_cmd.CommandText = Createsql;
            sqlite_cmd.ExecuteNonQuery();
        }


        static void InsertData(SQLiteConnection conn, Weather weather)
        {
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();

            DateTime today = DateTime.Now.Date;

            foreach (var data in weather.List)
            {
                DateTime dataDate = DateTime.Parse(data.Dt_txt);

                if (dataDate.Date == today)
                {
                    string insertSql = $@"
            INSERT INTO WeatherData (
                DateTime, Temperature, WeatherDescription, Rain, Clouds, WindSpeed, WindDeg, 
                MainTemp, MainFeelsLike, MainTempMin, MainTempMax, 
                MainPressure, MainSeaLevel, MainGrndLevel, MainHumidity, MainTempKf
            ) VALUES (
                '{data.Dt_txt}', {data.Main.Temp}, '{data.Weather[0].Description}', 
                {data.Rain?._3h ?? 0}, {data.Clouds.All}, {data.Wind.Speed}, {data.Wind.Deg}, 
                {data.Main.Temp}, {data.Main.Feels_like}, {data.Main.Temp_min}, {data.Main.Temp_max}, 
                {data.Main.Pressure}, {data.Main.Sea_level}, {data.Main.Grnd_level}, {data.Main.Humidity}, {data.Main.Temp_kf}
            )";

                    sqlite_cmd.CommandText = insertSql;
                    sqlite_cmd.ExecuteNonQuery();
                }
            }
        }

        static void ReadData(SQLiteConnection conn)
        {
            SQLiteDataReader sqlite_datareader;
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = "SELECT * FROM WeatherData";
            sqlite_datareader = sqlite_cmd.ExecuteReader();
            while (sqlite_datareader.Read())
            {
                DateTime dateTime = sqlite_datareader.GetDateTime(0);
                double temperature = sqlite_datareader.GetDouble(1);
                string weatherDescription = sqlite_datareader.GetString(2);
                double rain = sqlite_datareader.GetDouble(3);
                int clouds = sqlite_datareader.GetInt32(4);
                double windSpeed = sqlite_datareader.GetDouble(5);
                int windDeg = sqlite_datareader.GetInt32(6);
                double mainTemp = sqlite_datareader.GetDouble(7);
                double mainFeelsLike = sqlite_datareader.GetDouble(8);
                double mainTempMin = sqlite_datareader.GetDouble(9);
                double mainTempMax = sqlite_datareader.GetDouble(10);
                int mainPressure = sqlite_datareader.GetInt32(11);
                int mainSeaLevel = sqlite_datareader.GetInt32(12);
                int mainGrndLevel = sqlite_datareader.GetInt32(13);
                int mainHumidity = sqlite_datareader.GetInt32(14);
                double mainTempKf = sqlite_datareader.GetDouble(15);

                Console.WriteLine($"Date and Time: {dateTime}, Temperature: {temperature}°F, Weather: {weatherDescription}");
                Console.WriteLine($"Rain: {rain}, Clouds: {clouds}, Wind Speed: {windSpeed}, Wind Degree: {windDeg}");
                Console.WriteLine($"Main Temp: {mainTemp}°F, Feels Like: {mainFeelsLike}°F, Temp Min: {mainTempMin}°F, Temp Max: {mainTempMax}°F");
                Console.WriteLine($"Pressure: {mainPressure} hPa, Sea Level: {mainSeaLevel} hPa, Ground Level: {mainGrndLevel} hPa, Humidity: {mainHumidity}%");
                Console.WriteLine($"Temp Kf: {mainTempKf}");
                Console.WriteLine();
            }
        }

        static void DeleteData(SQLiteConnection conn)
        {
            try
            {
                SQLiteCommand sqlite_cmd = conn.CreateCommand();
                sqlite_cmd.CommandText = $"DELETE FROM WeatherData";
                sqlite_cmd.ExecuteNonQuery();
                Console.WriteLine("Data deleted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting data: " + ex.Message);
            }
        }


        async static Task Main(string[] args)
        {
            string userInput = "";
            while (userInput != "E")
            {
                Console.WriteLine("Enter 'T' to get today's weather, 'A' to view all records, 'D' to delete all records, or 'E' to exit:");
                userInput = Console.ReadLine().ToUpper();

                switch (userInput)
                {
                    case "T":
                        await GetTodayWeather();
                        break;
                    case "A":
                        ViewAllRecords();
                        break;
                    case "E":
                        Console.WriteLine("Exiting the program.");
                        break;
                    case "D":
                        DeleteAllRecords();
                        break;
                    default:
                        Console.WriteLine("Invalid input. Please try again.");
                        break;
                }
            }
        }

        async static Task GetTodayWeather()
        {
            string result = await client.GetStringAsync(url);
            var weather = JsonConvert.DeserializeObject<Weather>(result);

            SQLiteConnection conn = CreateConnection();
            CreateTable(conn);
            InsertData(conn, weather);
            ReadData(conn);
            conn.Close();
        }

        static void ViewAllRecords()
        {
            SQLiteConnection conn = CreateConnection();
            ReadData(conn);
            conn.Close();
        }

        static void DeleteAllRecords()
        {
            SQLiteConnection conn = CreateConnection();
            DeleteData(conn);
            conn.Close();
        }
    }
}