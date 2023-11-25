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


    public static void ConvertTag(JObject jsonTag, PairedTag tagReference)
    {

        foreach (JProperty property in jsonTag.Children<JProperty>())
        {
            if (property.Name == "attributes")
            {
                Console.WriteLine("entered atributes");

                //store to attributes
                foreach (var attributeProperty in property.Value.Children<JProperty>())
                {
                    Console.WriteLine("started attribute for loop");

                    if (attributeProperty.Value.Type == JTokenType.String)
                    {
                        Console.WriteLine("Contained string");

                        //attribute contains a string
                        tagReference.Attributes.Add(new TagAttribute { Name = attributeProperty.Name, Value = (string)attributeProperty.Value });
                    }
                    else
                    {
                        //attribute contains another object (style)
                        foreach (var styleProp in attributeProperty.Value.Children<JProperty>())
                        {
                            tagReference.Styles.Add(new Style { StyleName = styleProp.Name, StyleValue = (string)styleProp.Value });
                        }

                    }


                }

            }
            else
            {
                //store to tag if string, otherwise recurse
                if (property.Value.Type == JTokenType.String)
                {
                    PairedTag stringTag = new PairedTag { TagName = property.Name, Content = property.Value };

                    //store the tag
                    if (tagReference.Content == null)
                    {
                        tagReference.Content = new List<Tag> { stringTag };

                    }
                    else if (tagReference.Content is List<Tag> tagList)
                    {
                        tagList.Add(stringTag);

                    }

                }
                else
                {
                    //add a new tag,recurse, pass the reference to new tag

                    //Also check before that if it is a selfclosing type like img, then just store it in the list


                    if (tagReference.Content == null)
                    {
                        var insertedTag = new PairedTag { TagName = property.Name };
                        tagReference.Content = new List<Tag> { insertedTag };

                        ConvertTag((JObject)property.Value, insertedTag);

                    }
                    else if (tagReference.Content is List<Tag> tagList)
                    {
                        var insertedTag = new PairedTag { TagName = property.Name };
                        tagList.Add(insertedTag);

                        ConvertTag((JObject)property.Value, insertedTag);
                    }

                }


            }


        }

    }

}