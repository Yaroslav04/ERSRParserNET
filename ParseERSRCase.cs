using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ERSRParserNET
{
    public static class ParseERSRCase
    {
        public static async Task<List<ERSRCaseClass>> GetERSRCaseList(List<ERSRClass> _list)
        {
            List<ERSRCaseClass> result = new List<ERSRCaseClass>();
            foreach (var item in _list)
            {
                result.Add(await GetERSRCase(item));
            }
            return result;
        }
        private static async Task<ERSRCaseClass> GetERSRCase(ERSRClass _ersr)
        {
            ERSRCaseClass result = new ERSRCaseClass(_ersr);
            string text = await Servises.GetResponseHTML(_ersr.URL);
            var lines = text.Split('\n');
            foreach (var line in lines)
            {
                if (line.Contains("Номер кримінального провадження в ЄРДР"))
                {
                    Regex regex = new Regex(@"\d\d\d\d\d\d\d\d\d\d\d\d\d\d\d\d\d");
                    MatchCollection matches = regex.Matches(line);
                    if (matches.Count > 0)
                    {
                        result.CriminalNumber = matches[0].Value;
                    }
                    else
                    {
                        regex = new Regex(@"\d(20)\d\d\d\d\d\d\d\d\d\d\d\d\d\d");
                        matches = regex.Matches(text);
                        if (matches.Count > 0)
                        {
                            result.CriminalNumber = matches[0].Value;
                        }
                    }
                }

                if (line.Contains("Дата набрання законної сили"))
                {
                    Regex regex = new Regex(@"\d\d(.)\d\d(.)\d\d\d\d");
                    MatchCollection matches = regex.Matches(line);
                    if (matches.Count > 0)
                    {
                        result.LegalDate = Convert.ToDateTime(matches[0].Value);
                    }
                }

                if (line.Contains("Категорія&nbsp;справи"))
                {
                    string subresult = "";
                    Regex regex = new Regex(@"(</form>:)[\w, \W]*");
                    MatchCollection matches = regex.Matches(line);
                    if (matches.Count > 0)
                    {
                        subresult = matches[0].Value.Replace(".</b></td></tr>", "").Replace("</form>:", "").Replace("\t", "").Replace("\n", "").Replace("\r", "");
                    }

                    if (subresult != "")
                    {
                        foreach(var s in subresult.Split(";"))
                        {
                            result.Category.Add(s.Trim());
                        }
                    }
                }
            }

            bool sw = false;
            List<string> grab = new List<string>();
            foreach (var line in lines)
            {
                if (sw)
                {
                    grab.Add(line);
                }

                if (line.Contains("<body>"))
                {
                    sw = true;
                }

                if (line.Contains("</body>"))
                {
                    sw = false;
                }

            }

            string content = "";
            foreach (var line in grab)
            {
                content = content + line + "\n";
            }

            var sx = content.Replace("&nbsp;", " ").Replace("&#171;", "").Replace("&#187;", "");

            string res = "";

            sw = false;

            foreach (char x in sx)
            {
                if (x == '>')
                {
                    sw = true;
                }
                
                if (x == '<')
                {
                    sw = false;
                }

                if (x != '>')
                {
                    if (sw)
                    {
                        res = res + x;
                    }               
                }
            }


            result.Content = res.Replace("\t", "");

            return result;
        }
    }
}
