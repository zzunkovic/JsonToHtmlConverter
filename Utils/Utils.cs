using System.Text;
using JsonToHtmlConverter.Models;
using JsonToHtmlConverter.Models.Tags;
using Newtonsoft.Json.Linq;

namespace JsonToHtmlConverter.Utils
{
    public class Utils
    {

        public static List<Tag> ConvertJObjectHeadToListOfTags(JObject jObject)
        {
            var tagList = new List<Tag>();

            //convert meta

            foreach (JProperty property in jObject["meta"].Children<JProperty>())
            {
                SelfClosingTag tag = new SelfClosingTag();
                tag.TagName = "meta";

                if (property.Name == "viewport")
                {
                    string viewportContentString = "";


                    foreach (JProperty viewportProp in property.Value.Children<JProperty>())
                    {
                        viewportContentString += $"{viewportProp.Name}={viewportProp.Value},";
                    }


                    tag.Attributes.Add(new TagAttribute { Name = "name", Value = "viewport" });
                    tag.Attributes.Add(new TagAttribute { Name = "content", Value = viewportContentString });
                }
                else
                {

                    tag.Attributes.Add(new TagAttribute { Name = property.Name, Value = (string)property.Value });

                }
                tagList.Add(tag);

            }

            //convert link
            if (jObject["link"] != null)
            {

                JArray linkArray = jObject["link"] as JArray;
                foreach (JObject linkObject in linkArray)
                {
                    SelfClosingTag tag = new SelfClosingTag();
                    tag.TagName = "link";
                    foreach (JProperty property in linkObject.Properties())
                    {
                        tag.Attributes.Add(new TagAttribute { Name = property.Name, Value = (string)property.Value });
                    }
                    tagList.Add(tag);

                }


            }

            //convert title

            PairedTag titleTag = new PairedTag { TagName = "title", Content = jObject["title"] };
            tagList.Add(titleTag);

            return tagList;
        }

        public static void ConvertTag(JObject jsonTag, PairedTag tagReference)
        {

            foreach (JProperty property in jsonTag.Children<JProperty>())
            {
                if (property.Name == "attributes")
                {


                    //store to attributes
                    foreach (var attributeProperty in property.Value.Children<JProperty>())
                    {


                        if (attributeProperty.Value.Type == JTokenType.String)
                        {


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
                        PairedTag stringTag = new PairedTag { TagName = property.Name, Content = property.Value.ToString() };

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

        public static StringBuilder GenerateTag(Tag tag, int numberOfNestings)
        {
            StringBuilder tagStringBuilder = new StringBuilder();

            int numOfNests = numberOfNestings;

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
                tagStringBuilder.Append($"<{tag.TagName} {attributeString}>");
            }
            else if (tag is PairedTag pairedTag)
            {

                if (pairedTag.Content is string)
                {

                    tagStringBuilder.Append($"<{pairedTag.TagName} {attributeString}>{pairedTag.Content}</{pairedTag.TagName}>");
                }
                else if (pairedTag.Content is List<Tag> nestedContent)
                {

                    StringBuilder nestedContentStringBuilder = new StringBuilder();
                    string indentationStart = string.Concat(Enumerable.Repeat("\t", numOfNests));
                    string indentationEnd = string.Concat(Enumerable.Repeat("\t", numOfNests - 1));
                    foreach (var nestedTag in nestedContent)
                    {
                        nestedContentStringBuilder.Append($"\n{indentationStart}" + GenerateTag(nestedTag, numOfNests + 1).ToString());
                    }

                    tagStringBuilder.Append($"<{pairedTag.TagName} {attributeString}>{nestedContentStringBuilder.ToString()}\n{indentationEnd}</{pairedTag.TagName}>");
                }
            }



            return tagStringBuilder;
        }


        public static StringBuilder GenerateHtmlString(Html htmlObj)
        {
            StringBuilder htmlStringBuilder = new StringBuilder();
            htmlStringBuilder.AppendLine($"<!DOCTYPE {htmlObj.Doctype}>");

            //open html tag
            htmlStringBuilder.AppendLine($"<html lang=\"{htmlObj.Language.ToLower()}\">");
            htmlStringBuilder.AppendLine($"\t<head>");

            //loop through list of tags in the head 

            foreach (var tag in htmlObj.Head)
            {
                string attributeString = "";
                if (tag is SelfClosingTag)
                {
                    //loop through attributes
                    foreach (TagAttribute att in tag.Attributes)
                    {

                        attributeString += $"{att.Name}=\"{att.Value}\" ";
                    }

                    htmlStringBuilder.AppendLine($"\t\t<{tag.TagName} {attributeString} >");
                }
                else if (tag is PairedTag pairedTag)
                {

                    //loop through attributes
                    foreach (TagAttribute att in tag.Attributes)
                    {
                        attributeString += $"{att.Name}=\"{att.Value}\" ";
                    }

                    htmlStringBuilder.AppendLine($"\t\t<{tag.TagName} {attributeString} >{pairedTag.Content}</{tag.TagName}>");

                }

            }
            htmlStringBuilder.AppendLine($"\t</head>");

            int numOfNests = 1;
            htmlStringBuilder.AppendLine(GenerateTag(htmlObj.Body, numOfNests).ToString());


            //close html tag
            htmlStringBuilder.AppendLine($"</html>");
            return htmlStringBuilder;

        }


    }
}
