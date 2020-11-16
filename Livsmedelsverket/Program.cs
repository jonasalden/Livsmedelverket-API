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

            if (success)
            {
                DoLog("Specify date for data: yyyy-MM-dd");

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
                    DoLog($"Trying to GET {type} data from {date:yyyy-MM-dd}");

                    try
                    {
                        var response = client.Get(request);

                        if (response.IsSuccessful)
                        {
                            DoLog("GET Data succeded.", ConsoleColor.Green);
                            DoLog("Preparing XML.");

                            var xmlDoc = new XmlDocument();
                            xmlDoc.LoadXml(response.Content);

                            bool directoryExist = Directory.Exists($"{_rootPath}\\Livsmedelsverket");

                            if (!directoryExist)
                            {
                                DoLog($"Directory {_rootPath}\\Livsmedelsverket not found.", ConsoleColor.Yellow);
                                DoLog("Do you want to create folder for the retrieved data? Y/N", ConsoleColor.Yellow);

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

                            if (File.Exists($"{_rootPath}\\Livsmedelsverket\\{type.ToLower()}.xml"))
                            {
                                DoLog("File exists, Do you want to override it? Y/N");
                                var answer = Console.ReadLine();
                                answer = answer.ToLower().Trim();
                                DoLog("--------------", ConsoleColor.Green);

                                if (answer == "y")
                                {
                                    xmlDoc.Save($"{_rootPath}\\Livsmedelsverket\\{type.ToLower()}.xml");
                                    DoLog($"Success! Overrided {_rootPath}\\Livsmedelsverket\\{type.ToLower()}.xml", ConsoleColor.Green);
                                }
                                else if (answer == "n")
                                {
                                    xmlDoc.Save($"{_rootPath}\\Livsmedelsverket\\{type.ToLower()}_{DateTime.Now.ToString("yyyyMMddHHmmssFFF")}.xml");
                                    DoLog($"Success! Created {_rootPath}\\Livsmedelsverket\\{type.ToLower()}_{DateTime.Now.ToString("yyyyMMddHHmmssFFF")}.xml", ConsoleColor.Green);
                                }
                            }
                            else
                            {
                                xmlDoc.Save($"{_rootPath}\\Livsmedelsverket\\{type.ToLower()}.xml");
                                DoLog($"Success! Created {_rootPath}\\Livsmedelsverket\\{type.ToLower()}.xml", ConsoleColor.Green);
                            }
                        }
                        else
                        {
                            DoLog("Getting Data failed", ConsoleColor.Red);
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
        #endregion
    }
}