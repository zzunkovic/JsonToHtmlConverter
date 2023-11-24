using JsonToHtmlConverter.Models;
using JsonToHtmlConverter.Utils;
using Newtonsoft.Json.Linq;

public static class Program
{


    public static void Main(string[] args)
    {
        //Convert JSON to JObject
        string jsonString = File.ReadAllText(@"D:\\VisualStudioProjects\\JsonToHtmlConverter\\helloWorld.json");
        JObject jsonObject = JObject.Parse(jsonString);


        //Create instance of an object that holds the html related data from the JSON file
        Html html = new Html();

        //Store Doctype if it exists
        if (jsonObject["doctype"] != null)
        {
            html.Doctype = (string)jsonObject["doctype"]!;
        }

        //Store Language if it exists
        if (jsonObject["language"] != null)
        {
            html.Language = (string)jsonObject["language"]!;
        }

        html.Head = Utils.ConvertJObjectHeadToListOfTags((JObject)jsonObject["head"]);


        //foreach (var tag in html.Head)
        //{
        //    Console.Write(tag.TagName);
        //    foreach (var item in tag.Attributes)
        //    {
        //        Console.Write($" with an attribute {item.Name} with the value of {item.Value}\n");
        //    }
        //    Console.WriteLine();
        //}


    }







}