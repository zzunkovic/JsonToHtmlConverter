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


    }
}
