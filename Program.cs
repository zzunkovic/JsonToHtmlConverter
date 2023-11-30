using JsonToHtmlConverter.Models;
using JsonToHtmlConverter.Utils;
using Newtonsoft.Json.Linq;

public static class Program
{


    public static void Main(string[] args)
    {

        //Reading and validating user input

        string? filename;
        bool inputIsValid = false;


        do
        {

            Console.WriteLine("Please provide the complete filename of the JSON file you want to convert.");
            filename = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(filename))
            {
                Console.WriteLine($"Filename cannot be empty, please provide a valid filename ");
            }
            else if (!filename.EndsWith(".json"))
            {
                Console.WriteLine($"Filename must end with a .json extension, please provide a valid filename ");
            }

            else if (!File.Exists($"./{filename}"))
            {
                Console.WriteLine($"File does not exist, please try again ");

            }
            else
            {
                inputIsValid = true;
            }

        } while (!inputIsValid);



        ////////////////////////////
        /// PROVIDE ABSOLUTE PATH TO INPUT FILE HERE IF RUNNING FROM VS (in solution explorer right click on file, copy full path) , ALSO COMMENT OUT ALL THE CODE ABOVE THIS COMMENT IN Main
        ///////////////////////////
        //Convert JSON to JObject
        string jsonString = File.ReadAllText($"./{filename}");
        JObject jsonObject = JObject.Parse(jsonString);


        //Create instance of an object that holds the html related data received from the JSON file read above
        Html html = new Html();

        //Store Doctype if it exists
        if (jsonObject["doctype"] != null && jsonObject["doctype"].Type == JTokenType.String)
        {
            html.Doctype = (string)jsonObject["doctype"]!;
        }

        //Store Language if it exists
        if (jsonObject["language"] != null && jsonObject["language"].Type == JTokenType.String)
        {
            html.Language = (string)jsonObject["language"]!;
        }

        //Convert head to desired format and store inside the instance
        html.Head = Utils.ConvertJObjectHeadToListOfTags((JObject)jsonObject["head"]!);


        //Convert body data to desired format and store inside the instance
        Utils.ConvertTag((JObject)jsonObject["body"]!, html.Body!);


        //Convert html object to string and output to file
        string outputHtml = Utils.GenerateHtmlString(html).ToString();


        ////////////////////////////
        /// PROVIDE ABSOLUTE PATH TO OUTPUT FILE HERE IF RUNNING FROM VS
        ///////////////////////////

        File.WriteAllText("./output.html", outputHtml);

        Console.WriteLine("File converted, press Enter to exit");
        Console.Read();


    }

}