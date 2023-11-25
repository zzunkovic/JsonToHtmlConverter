using System.Text;
using JsonToHtmlConverter.Models;
using JsonToHtmlConverter.Models.Tags;
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

        Utils.ConvertTag((JObject)jsonObject["body"], html.Body);


        string outputHtml = GenerateHtmlString(html).ToString();
        File.WriteAllText(@"D:\\VisualStudioProjects\\JsonToHtmlConverter\\output.html", outputHtml);





    }

    public static StringBuilder GenerateHtmlString(Html htmlObj)
    {
        StringBuilder htmlStringBuilder = new StringBuilder();
        htmlStringBuilder.AppendLine($"<!DOCTYPE {htmlObj.Doctype}>");

        //open html tag
        htmlStringBuilder.AppendLine($"<html lang=\"{htmlObj.Language.ToLower()}\">");


        //loop through list of tags in the head 




        //close html tag
        htmlStringBuilder.AppendLine($"</html>");
        return htmlStringBuilder;

    }


    public static StringBuilder GenerateTag(Tag tag)
    {
        StringBuilder tagStringBuilder = new StringBuilder();

        string line = $"<{tag.TagName.ToLower()}";

        return tagStringBuilder;
    }


}