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
        string jsonString = File.ReadAllText(@"D:\\VisualStudioProjects\\JsonToHtmlConverter\\pageNotFoundV2.json");
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
        htmlStringBuilder.AppendLine($"<head>");

        //loop through list of tags in the head 

        foreach (var tag in htmlObj.Head)
        {
            if (tag is SelfClosingTag)
            {
                string attributeString = "";
                //loop through attributes
                foreach (TagAttribute att in tag.Attributes)
                {

                    attributeString += $"{att.Name}=\"{att.Value}\" ";
                }

                htmlStringBuilder.AppendLine($"<{tag.TagName} {attributeString} >");
            }
            else if (tag is PairedTag pairedTag)
            {
                string attributeString = "";
                //loop through attributes
                foreach (TagAttribute att in tag.Attributes)
                {
                    attributeString += $"{att.Name}=\"{att.Value}\" ";
                }

                htmlStringBuilder.AppendLine($"<{tag.TagName} {attributeString} >{pairedTag.Content}</{tag.TagName}>");

            }

        }
        htmlStringBuilder.AppendLine($"</head>");

        htmlStringBuilder.AppendLine(GenerateTag(htmlObj.Body).ToString());


        //close html tag
        htmlStringBuilder.AppendLine($"</html>");
        return htmlStringBuilder;

    }


    public static StringBuilder GenerateTag(Tag tag)
    {
        StringBuilder tagStringBuilder = new StringBuilder();



        /*
        ////////////////////////
        CREATE ATTRIBUTES
        /////////////////////////
        */

        //build attribute string
        string attributeString = "";

        //loop through attributes
        foreach (TagAttribute att in tag.Attributes)
        {

            attributeString += $"{att.Name}=\"{att.Value}\" ";
        }

        //check if style list is not empty
        if (tag.Styles.Count != 0)
        {

            //create style string
            string styleString = "style=\"";
            foreach (Style style in tag.Styles)
            {
                styleString += $"{style.StyleName}:{style.StyleValue};";
            }

            //close the style attribute and add to attribute string
            styleString += "\"";
            attributeString += styleString;
        }



        /*
        ////////////////////////
        CREATE TAGS
        /////////////////////////
        */


        if (tag is SelfClosingTag)
        {
            tagStringBuilder.AppendLine($"<{tag.TagName} {attributeString}>");
        }
        else if (tag is PairedTag pairedTag)
        {

            if (pairedTag.Content is string)
            {

                tagStringBuilder.AppendLine($"<{pairedTag.TagName} {attributeString}>{pairedTag.Content}</{pairedTag.TagName}>");
            }
            else if (pairedTag.Content is List<Tag> nestedContent)
            {

                StringBuilder nestedContentStringBuilder = new StringBuilder();
                foreach (var nestedTag in nestedContent)
                {
                    nestedContentStringBuilder.AppendLine(GenerateTag(nestedTag).ToString());
                }
                tagStringBuilder.AppendLine($"<{pairedTag.TagName} {attributeString}>{nestedContentStringBuilder.ToString()}</{pairedTag.TagName}>");
            }
        }



        return tagStringBuilder;
    }


}