using Newtonsoft.Json;
using RestSharp;
using System;
using System.IO;
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
            DoLog("Livsmedelsverket API");
            DoLog("Select data to download: (1) Näringsvärde / (2) Klassificering");

            string selectedCategory = Console.ReadLine();
            bool success = int.TryParse(selectedCategory, out int category);

            if (!success)
            {
                DoLog("Wrong format, Try again between 1 and 2", ConsoleColor.Red);
                selectedCategory = Console.ReadLine();
                success = int.TryParse(selectedCategory, out category);
            }

            if (success)
            {
                DoLog("Specify date for data: yyyy-MM-dd");

                string dateval = Console.ReadLine();

                var dateSuccess = DateTime.TryParse(dateval, out DateTime date);

                if (!dateSuccess)
                {
                    DoLog("Wrong format, Try again: yyyy-MM-dd ", ConsoleColor.Red);
                    dateval = Console.ReadLine();
                    dateSuccess = DateTime.TryParse(dateval, out date);
                }

                if (dateSuccess)
                {
                    var type = string.Empty;

                    switch (category)
                    {
                        case 1:
                            type = ApiType.Nutrition;
                            break;
                        case 2:
                            type = ApiType.Classification;
                            break;
                        default:
                            DoLog("Not a valid data selection", ConsoleColor.Red);
                            break;
                    }

                    var client = new RestClient(_apiUrl);
                    var request = new RestRequest($"{type}/{date:yyyy-MM-dd}", DataFormat.Xml);
                    DoLog($"GET request: {type} from {date:yyyy-MM-dd}");

                    try
                    {
                        var response = client.Get(request);

                        if (response.IsSuccessful)
                        {
                            DoLog("GET request succeded.", ConsoleColor.Green);
                            DoLog("Preparing XML...");

                            var xmlDoc = new XmlDocument();
                            xmlDoc.LoadXml(response.Content);

                            bool directoryExist = Directory.Exists($"{_rootPath}\\Livsmedelsverket");

                            if (!directoryExist)
                            {
                                DoLog($"Directory {_rootPath}\\Livsmedelsverket not found.", ConsoleColor.Yellow);
                                DoLog("Do you want to create a folder for the retrieved data? Y/N", ConsoleColor.Yellow);

                                var answer = Console.ReadLine();

                                if (!string.IsNullOrWhiteSpace(answer))
                                {
                                    answer = answer.ToLower().Trim();

                                    if (answer == "y")
                                    {
                                        Directory.CreateDirectory($"{_rootPath}\\Livsmedelsverket");
                                        DoLog($"Directory {_rootPath}\\Livsmedelsverket created.", ConsoleColor.Green);
                                    }
                                    else if (answer == "n")
                                    {
                                        DoLog($"Directory {_rootPath}\\Livsmedelsverket not created.", ConsoleColor.Red);
                                    }
                                    else
                                    {
                                        DoLog("Not a legimate answer.", ConsoleColor.Red);
                                    }
                                }
                            }

                            DoLog("Do you want JSON or XML format? JSON/XML", ConsoleColor.Yellow);
                            string jsonOrXml = Console.ReadLine().ToLower().Trim();
                            var ext = "xml";

                            if (jsonOrXml == "json")
                            {
                                ext = jsonOrXml;
                            }

                            if (File.Exists($"{_rootPath}\\Livsmedelsverket\\{type.ToLower()}.{ext}"))
                            {
                                DoLog("File exists, Do you want to override it? Y/N", ConsoleColor.Yellow);
                                DoLog("--------------");

                                var answer = Console.ReadLine().ToLower().Trim();

                                if (answer == "y")
                                {
                                    SaveFile(type, ext, xmlDoc);
                                    DoLog($"Success! Overrided {_rootPath}\\Livsmedelsverket\\{type.ToLower()}.{ext}", ConsoleColor.Green);
                                }
                                else if (answer == "n")
                                {
                                    SaveFile(type.ToLower() + $"_{DateTime.Now.ToString("yyyyMMddHHmmssFFF")}", ext, xmlDoc);
                                    DoLog($"Success! Created {_rootPath}\\Livsmedelsverket\\{type}_{DateTime.Now.ToString("yyyyMMddHHmmssFFF")}.{ext}", ConsoleColor.Green);
                                }
                            }
                            else
                            {
                                SaveFile(type, ext, xmlDoc);
                                DoLog($"Success! Created {_rootPath}\\Livsmedelsverket\\{type.ToLower()}.{ext}", ConsoleColor.Green);
                            }

                            DoLog("Testing to read file and deserialize it to an C# object.", ConsoleColor.Cyan);

                            // Print data from file.
                        }
                        else
                        {
                            DoLog($"Error, StatusCode: {response.StatusCode}, Date: {date:yyyy-MM-dd}, Type: {type}", ConsoleColor.Red);
                        }
                    }
                    catch (Exception e)
                    {
                        DoLog("Getting data failed, Error: " + e.Message, ConsoleColor.Red);
                    }
                }
                else
                {
                    DoLog($"Incorrect Date input: '{dateval}' yyyy-MM-dd", ConsoleColor.Red);
                }
            }
            else
            {
                DoLog($"Incorrect Category input: '{selectedCategory}, should be 1 or 2.'", ConsoleColor.Red);
            }
        }

        public static void DoLog(string text, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        public static void SaveFile(string type, string extension, XmlDocument xmlDoc)
        {
            switch (extension)
            {
                case "xml":
                    xmlDoc.Save($"{_rootPath}\\Livsmedelsverket\\{type.ToLower()}.{extension}");
                    break;
                case "json":
                    var json = JsonConvert.SerializeXmlNode(xmlDoc);
                    File.WriteAllText($"{_rootPath}\\Livsmedelsverket\\{type.ToLower()}.{extension}", json);
                    break;
            }
        }
        #endregion

        #region Classes
        public static class ApiType
        {
            public const string Nutrition = "Naringsvarde";
            public const string Classification = "Klassificering";
        }
        #endregion
    }
}