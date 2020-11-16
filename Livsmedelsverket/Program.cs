using RestSharp;
using System;
using System.Xml;

namespace Livsmedelsverket
{
    class Program
    {
        #region Fields
        private static string _apiUrl => "http://www7.slv.se/apilivsmedel/LivsmedelService.svc/Livsmedel/";
        private static string _rootPath => Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        #endregion

        #region Methods
        static void Main()
        {
            Console.WriteLine("Livsmedelsverket API");
            Console.WriteLine("Välj data att hämta: (1) Näringsvärde, (2) Klassificering");

            string data = Console.ReadLine();
            bool success = int.TryParse(data, out int category);

            if (success)
            {
                Console.WriteLine("Vilket datum vill du hämta data ifrån? Ange yyyy-MM-dd");
                string dateval = Console.ReadLine();

                var dateSuccess = DateTime.TryParse(dateval, out DateTime date);

                if (dateSuccess)
                {
                    var type = string.Empty;

                    switch (category)
                    {
                        case 1:
                            type = "Naringsvarde";
                            break;
                        case 2:
                            type = "Klassificering";
                            break;
                    }

                    var client = new RestClient(_apiUrl);
                    var request = new RestRequest($"{type}/{date:yyyy-MM-dd}", DataFormat.Xml);
                    Console.WriteLine($"Trying to GET {type} data from {date:yyyy-MM-dd}");

                    try
                    {
                        var response = client.Get(request);

                        if (response.IsSuccessful)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("GET Data succeded.");
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("Preparing XML.");

                            var xmlDoc = new XmlDocument();
                            xmlDoc.LoadXml(response.Content);
                            xmlDoc.Save($"{_rootPath}\\Livsmedelsverket\\{type}.xml");

                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Success! Saved file to {_rootPath}\\Livsmedelsverket\\{type}.xml");
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Getting Data failed");
                        }

                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Getting Data failed, Error: " + e.InnerException);
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Incorrect Date input: '{dateval}' yyyy-MM-dd");
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Incorrect Category input: '{data}' should be 1 or 2.");
            }
            Console.ResetColor();
        }
        #endregion
    }
}