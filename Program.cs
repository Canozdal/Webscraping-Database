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
class Program
{
    static async Task Main()
    {
        string connection_string = "server=localhost;port=3306;database=news;user=root;password=password;";
        ArrayList items = GetNews("https://www.sondakika.com/", "//*[@id=\"bx-pager\"]");
        using (var connection = new MySqlConnection(connection_string))
        {
            connection.Open();
            foreach(var item in items)
            {
                string query = "INSERT IGNORE INTO news(text) VALUES(@item)";

                using(var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@item", item);
                    command.ExecuteNonQuery();
                }
            }
            connection.Close(); 
        }


        Console.WriteLine(items.Count);

    }
    static string HandleTurkishCharacters(string text)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        byte[] bytes = Encoding.GetEncoding("iso-8859-9").GetBytes(text);

        return Encoding.UTF8.GetString(bytes); 
    }
    static ArrayList GetNews(string url,string xpath)
    {
        var web = new HtmlWeb();
        var doc = web.Load(url);
        var nodes = doc.DocumentNode.SelectNodes(xpath);
        ArrayList news = new ArrayList();
        if (nodes != null)
        {
            var anchorElements = nodes.Descendants("a");

            foreach (var anchorElement in anchorElements)
            {
                string titleAttribute = anchorElement.GetAttributeValue("title", "");
                string decodedTitle = WebUtility.HtmlDecode(titleAttribute);
                string handledText = HandleTurkishCharacters(decodedTitle);
                Console.WriteLine(handledText);
                news.Add(handledText);
            }
        }
        return news;
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