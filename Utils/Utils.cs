using System.Text;
using JsonToHtmlConverter.Models;
using JsonToHtmlConverter.Models.Tags;
using Newtonsoft.Json.Linq;

namespace JsonToHtmlConverter.Utils
{
    public class Utils
    {

        /// <summary>
        /// Converts the JOBject with the head data into a list of Tags.
        /// </summary>
        /// <param name="jObject">JObject containing head data.</param>
        /// <returns>A list of html tags nested inside the head tag.</returns>
        public static List<Tag> ConvertJObjectHeadToListOfTags(JObject jObject)
        {
            var tagList = new List<Tag>();

            //convert meta if it exists
            if (jObject["meta"] != null)
            {

                foreach (JProperty property in jObject["meta"]!.Children<JProperty>())
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


                        tag.Attributes!.Add(new TagAttribute { Name = "name", Value = "viewport" });
                        tag.Attributes.Add(new TagAttribute { Name = "content", Value = viewportContentString });
                    }
                    else
                    {
                        if (property.Value.Type == JTokenType.String)
                        {
                            tag.Attributes!.Add(new TagAttribute { Name = property.Name, Value = (string)property.Value });
                        }

                    }
                    tagList.Add(tag);

                }



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
                        if (property.Value.Type == JTokenType.String)
                        {

                            tag.Attributes!.Add(new TagAttribute { Name = property.Name, Value = (string)property.Value });

                        }

                    }
                    tagList.Add(tag);

                }


            }

            //convert title

            PairedTag titleTag = new PairedTag { TagName = "title", Content = jObject["title"] };
            tagList.Add(titleTag);

            return tagList;
        }



        /// <summary>
        /// Converts the JObject with tag data into a Tag object. The method is called recursively if it detects a nested tag.
        /// </summary>
        /// <param name="jsonTag">JObject containing tag data.</param>
        /// <param name="tagReference">A reference to the tag object.</param>
        public static void ConvertTag(JObject jsonTag, PairedTag tagReference)
        {

            foreach (JProperty property in jsonTag.Children<JProperty>())
            {
                //If the property contains attribute data, add that data to the Atributes list, otherwise add data to Tag list
                if (property.Name == "attributes")
                {



                    foreach (var attributeProperty in property.Value.Children<JProperty>())
                    {


                        if (attributeProperty.Value.Type == JTokenType.String)
                        {

                            tagReference.Attributes!.Add(new TagAttribute { Name = attributeProperty.Name, Value = (string)attributeProperty.Value });
                        }
                        else
                        {
                            //attribute contains another object which makes it a style attribute, in this case save it to the styles list
                            foreach (var styleProp in attributeProperty.Value.Children<JProperty>())
                            {

                                if (styleProp.Value.Type == JTokenType.String)
                                {

                                    tagReference.Styles.Add(new Style { StyleName = styleProp.Name, StyleValue = (string)styleProp.Value });

                                }
                            }

                        }


                    }

                }
                else if (property.Name == "img" || property.Name == "input")
                {
                    SelfClosingTag selfClosingTag = new SelfClosingTag { TagName = property.Name };
                    foreach (var attributeProperty in property.Value.Children<JProperty>())
                    {
                        selfClosingTag.Attributes.Add(new TagAttribute { Name = attributeProperty.Name, Value = attributeProperty.Value.ToString() });

                    }
                    if (tagReference.Content == null)
                    {
                        tagReference.Content = new List<Tag> { selfClosingTag };

                    }
                    else if (tagReference.Content is List<Tag> tagList)
                    {
                        tagList.Add(selfClosingTag);

                    }

                }
                else
                {
                    //if the property contains a string it means it is not nested, in that case just store it in the tag list. Otherwise it is a nested tag
                    if (property.Value.Type == JTokenType.String)
                    {
                        PairedTag stringTag = new PairedTag { TagName = property.Name, Content = property.Value.ToString() };

                        //Check if the tag list is available, if not first create the list and then add the tag
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


                        //If the tag is nested,create a new tag in the current tags list and pass the inserted tags
                        //reference and the corresponding JObject to the ConvertTag function (recursion)

                        //Check if the tag list is available, if not first create the list and then add the tag

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


        /// <summary>
        /// Converts a Tag object into a string
        /// </summary>
        /// <param name="tag">a Tag object which is to be converted.</param>
        /// <param name="numberOfNestings">Keeps track of the current depth of nesting. Used for indentation.</param>
        /// <returns>A string builder of the generated tag</returns>
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


            foreach (TagAttribute att in tag.Attributes!)
            {

                attributeString += $"{att.Name}=\"{att.Value}\" ";
            }


            if (tag.Styles.Count != 0)
            {

                //create style string
                string styleString = "style=\"";
                foreach (Style style in tag.Styles)
                {
                    styleString += $"{style.StyleName}:{style.StyleValue};";
                }


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
                tagStringBuilder.Append($"<{tag.TagName} {attributeString}/>");
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

                    //add indentation with the tab added to the child tag, depending on current depth of nesting
                    string indentationStart = string.Concat(Enumerable.Repeat("\t", numOfNests));

                    //1 is subtracted here because this indentation refers to the parent tag
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


        /// <summary>
        /// Generates a string builder from the html object
        /// </summary>
        /// <param name="htmlObj">Html object</param>
        /// <returns>A string builder of the generated html file</returns>
        public static StringBuilder GenerateHtmlString(Html htmlObj)
        {
            StringBuilder htmlStringBuilder = new StringBuilder();
            htmlStringBuilder.AppendLine($"<!DOCTYPE {htmlObj.Doctype}>");


            htmlStringBuilder.AppendLine($"<html lang=\"{htmlObj.Language.ToLower()}\">");
            htmlStringBuilder.AppendLine($"\t<head>");



            foreach (var tag in htmlObj.Head!)
            {
                string attributeString = "";
                if (tag is SelfClosingTag)
                {

                    foreach (TagAttribute att in tag.Attributes!)
                    {

                        attributeString += $"{att.Name}=\"{att.Value}\" ";
                    }

                    htmlStringBuilder.AppendLine($"\t\t<{tag.TagName} {attributeString} >");
                }
                else if (tag is PairedTag pairedTag)
                {


                    foreach (TagAttribute att in tag.Attributes!)
                    {
                        attributeString += $"{att.Name}=\"{att.Value}\" ";
                    }

                    htmlStringBuilder.AppendLine($"\t\t<{tag.TagName} {attributeString} >{pairedTag.Content}</{tag.TagName}>");

                }

            }
            htmlStringBuilder.AppendLine($"\t</head>");

            int numOfNests = 1;
            htmlStringBuilder.AppendLine(GenerateTag(htmlObj.Body!, numOfNests).ToString());



            htmlStringBuilder.AppendLine($"</html>");
            return htmlStringBuilder;

        }


    }
}
