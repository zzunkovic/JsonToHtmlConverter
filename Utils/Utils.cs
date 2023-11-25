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
                tag.Attributes.Add(new TagAttribute { Name = property.Name, Value = (string)property.Value });
                tagList.Add(tag);

            }

            //convert link

            JArray linkArray = jObject["link"] as JArray;
            foreach (JObject linkObject in linkArray)
            {
                foreach (JProperty property in linkObject.Properties())
                {
                    SelfClosingTag tag = new SelfClosingTag();
                    tag.TagName = "link";
                    tag.Attributes.Add(new TagAttribute { Name = property.Name, Value = (string)property.Value });
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
}
