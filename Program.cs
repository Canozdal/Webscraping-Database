// See https://aka.ms/new-console-template for more information
using System;
using System.Net;
using System.Text;
using System.Linq;
using HtmlAgilityPack;
using System.Collections;
using MySql.Data.MySqlClient;
using System.Net.Http;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq.Expressions;

class NewsItem
{
    private string title;
    private string hrefString;
    private Uri href;
    private DateTime registryTime;
    public string Title { get => title; set => title = value; }
    public string HrefString { get => hrefString; set => hrefString = value; }
    public Uri Href { get => href; set => href = value; }

    public DateTime RegistryTime { get => registryTime; set => registryTime = value; } 

    public NewsItem(string Title, string HrefString)
    {
        this.Title = Title;
        this.HrefString = "https://www.sondakika.com/" + HrefString;
        if(Uri.TryCreate(hrefString,UriKind.Absolute,out Uri resultUri))
        {
           Href = resultUri;
        }
        registryTime = DateTime.Now;
    }
}
class Program
{
    static async Task Main()
    {
        
        List<NewsItem> items = GetNews("https://www.sondakika.com/", "//*[@id=\"bx-pager\"]");
        WriteToDatabase(items);
        Console.WriteLine(items.Count);

        ReadFromDatabase("descending");


    }
    static void WriteToDatabase(List<NewsItem> items)
    {
        string connection_string = "server=localhost;port=3306;database=news;user=root;password=password;";
        using (var connection = new MySqlConnection(connection_string))
        {
            connection.Open();
            foreach (var item in items)
            {
                string query = "INSERT IGNORE INTO news(text,registryTime,hRef) VALUES(@Title,@RegistryTime,@HrefString)";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Title", item.Title);
                    command.Parameters.AddWithValue("@HrefString", item.HrefString);
                    command.Parameters.AddWithValue("@RegistryTime", item.RegistryTime.ToString());
                    command.ExecuteNonQuery();
                }
            }
            connection.Close();
        }
    }
    static string HandleTurkishCharacters(string text)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        byte[] bytes = Encoding.GetEncoding("iso-8859-9").GetBytes(text);

        return Encoding.UTF8.GetString(bytes); 
    }
    static List<NewsItem> GetNews(string url,string xpath)
    {
        var web = new HtmlWeb();
        var doc = web.Load(url);
        var nodes = doc.DocumentNode.SelectNodes(xpath);
        List<NewsItem> news = new List<NewsItem>();
        if (nodes != null)
        {
            var anchorElements = nodes.Descendants("a");

            foreach (var anchorElement in anchorElements)
            {
                string titleAttribute = anchorElement.GetAttributeValue("title", "");
                string newsLink = anchorElement.GetAttributeValue("href", "");
                string decodedTitle = WebUtility.HtmlDecode(titleAttribute);
                string handledText = HandleTurkishCharacters(decodedTitle);
                news.Add(new NewsItem(handledText, newsLink));
            }
        }
        return news;
    }
    static void ReadFromDatabase(string rule)
    {
        string connection_string = "server=localhost;port=3306;database=news;user=root;password=password;";
        try
        {
            using (var connection = new MySqlConnection(connection_string))
            {
                connection.Open();
                string query = "";

                switch (rule)
                {
                    case "descending":
                        query = "SELECT * FROM news ORDER BY registryTime DESC";
                        Console.WriteLine("Descending");
                        break;
                    case "ascending":
                        query = "SELECT * FROM news ORDER BY registryTime ASC";
                        Console.WriteLine("Ascending");
                        break;
                    default:
                        query = "SELECT * FROM news";
                        Console.WriteLine("Default Rule");
                        break;
                }
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine(reader.GetString(reader.GetOrdinal("text")) + " " + reader.GetString(reader.GetOrdinal("href")));
                        }
                    }

                }
                connection.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while reading {ex}");
        }

    }
    static async Task GetCheapestFlights(string apiKey,string origin, string destination,DateTime departureDate)
    {
        string base_url = "https://partners.api.skyscanner.net/apiservices/v3/flights/live/search/create";
        string formattedDate = departureDate.ToString("yyyy-MM-dd");

        string url = $"{base_url}?x-api-key={apiKey}&country=DE&currency=EUR&locale=en-US&originPlace={origin}&destinationPlace={destination}&outboundDate={formattedDate}&adults=1";
        using (var httpClient = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();

                    Console.WriteLine(responseBody);
                }
                else
                {
                    Console.WriteLine($"Error code {response.StatusCode} : {response.ReasonPhrase}");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}