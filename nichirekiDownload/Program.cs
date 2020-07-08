using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;

namespace nichirekiDownload
{
    class Program
    {
        static void Main(string[] args)
        {
            // 全24巻
            // nichireki01から21
            // nichirekia1からa3
            List<String> targets = new List<String>();
            for (int i = 1; i <= 21; i++)
            {
                targets.Add($"{i:00}");
            }
            for (int i = 1; i <= 3; i++)
            {
                targets.Add($"a{i}");
            }

            var wc = new WebClient();
            string json_uri;
            string json;
            foreach (string target in targets)
            {
                json_uri = $"https://kids-km3.shogakukan.co.jp/books/nichireki{target}/json";
                json = wc.DownloadString(json_uri);
                //Console.WriteLine(json);
                var serializeOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance,
                    WriteIndented = true
                };
                NichiReki json_obj = JsonSerializer.Deserialize<NichiReki>(json, serializeOptions);

                int page = 0;
                string uri;
                byte[] img;
                string reply;
                string fileName;
                foreach (Content c in json_obj.Contents)
                {
                    uri = $"https://kids-km3.shogakukan.co.jp/contents/nichireki{target}/{page}/base64";
                    Console.WriteLine(uri);
                    reply = wc.DownloadString(uri);
                    img = Convert.FromBase64String(reply);
                    fileName = $"nichireki{target}_{page:000}.jpg";
                    using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
                    {
                        writer.Write(img);
                    }
                    page++;
                }
            }
            Console.WriteLine("finish!");
            Console.ReadLine();
        }
    }

    public class NichiReki
    {
        public String Slug { get; set; }
        public String Title { get; set; }
        public String IsbnCd { get; set; }
        public String JanCd { get; set; }
        public String Introduction { get; set; }
        public String HatsubaiDate { get; set; }
        public String Hiraki { get; set; }
        public String Cover { get; set; }
        public List<Content> Contents { get; set; }
        public List<Landmark> Landmarks { get; set; }

    }

    public class Content
    {
        public int SortNo { get; set; }
        public String MimeType { get; set; }
        public String Width { get; set; }
        public String Height { get; set; }
    }

    public class Landmark
    {
        public int Page { get; set; }
        public String Text { get; set; }
    }

    // via https://stackoverflow.com/questions/58570189/is-there-a-built-in-way-of-using-snake-case-as-the-naming-policy-for-json-in-asp
    public class SnakeCaseNamingPolicy : JsonNamingPolicy
    {
        private readonly SnakeCaseNamingStrategy _newtonsoftSnakeCaseNamingStrategy
            = new SnakeCaseNamingStrategy();

        public static SnakeCaseNamingPolicy Instance { get; } = new SnakeCaseNamingPolicy();

        public override string ConvertName(string name)
        {
            /* A conversion to snake case implementation goes here. */

            return _newtonsoftSnakeCaseNamingStrategy.GetPropertyName(name, false);
        }
    }
}
