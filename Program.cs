// See https://aka.ms/new-console-template for more information
using System;
using System.Net;
using System.Text;
using System.Linq;
using HtmlAgilityPack;
using System.Collections;
using MySql.Data.MySqlClient;
class Program
{
    static void Main()
    {
        string connection_string = "server=localhost;port=3306;database=news;user=root;password=password;";
        ArrayList items = GetNews();
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
    static ArrayList GetNews()
    {
        var web = new HtmlWeb();
        var url = "https://www.sondakika.com/";
        var doc = web.Load(url);
        var nodes = doc.DocumentNode.SelectNodes("//*[@id=\"bx-pager\"]");
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
}