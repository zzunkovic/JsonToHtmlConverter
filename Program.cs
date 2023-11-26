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


        //Create instance of an object that holds the html related data received from the JSON file read above
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

        //Convert head to desired format and store inside the instance
        html.Head = Utils.ConvertJObjectHeadToListOfTags((JObject)jsonObject["head"]!);


        //Convert body data to desired format and store inside the instance
        Utils.ConvertTag((JObject)jsonObject["body"]!, html.Body!);


        //Convert html object to string and output to file
        string outputHtml = Utils.GenerateHtmlString(html).ToString();
        File.WriteAllText(@"D:\\VisualStudioProjects\\JsonToHtmlConverter\\output.html", outputHtml);

    }

}