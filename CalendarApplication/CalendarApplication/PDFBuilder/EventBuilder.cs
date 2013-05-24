using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;
using System.Windows.Forms;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

using CalendarApplication.Models.Event;
using CalendarApplication.Models.EventType;
using CalendarApplication.Models.User;
using CalendarApplication.Models.Group;
using CalendarApplication.Database;

namespace CalendarApplication.PDFBuilder
{
    public class EventBuilder
    {
        /// <summary>
        /// Build and open pdf version of an event
        /// </summary>
        /// <param name="basicevent">Model of the event to be built as pdf</param>
        public static string BuildPDF(EventWithDetails evm)
        {
            string homepath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string name = "";
            string[] arr = evm.Name.Split(' ');
            for (int i = 0; i < arr.Length; i++)
            {
                name += arr[i];
            }
            string file = name + ".tex";
            //path to pdflatex.exe
            string pdflatexpath = "\"C:\\Program Files\\MiKTeX 2.9\\miktex\\bin\\pdflatex.exe\"";
            //folder to temporarily store files used in compilation of the pdf document
            string auxdir = homepath + @"\tmp\";
            //output folder. substitute with "<path to CalendarApplication.sln>\CalendarApplication\"
            string outputdir = homepath + @"\Downloads\KU\PKSU\PKSU-Gruppearbejde\CalendarApplication\CalendarApplication\";

            //parse file
            List<String> input = ParseFile(evm);

            //write to file
            System.IO.File.WriteAllLines(auxdir + file, input);

            //build pdf
            string command = pdflatexpath + " -aux-directory=" + auxdir +
                                " -interaction=nonstopmode -output-directory=" + outputdir + " " + auxdir + file;
            RunCommand(command,
                       "An error has occured while building " + file,
                       "An error has occured while trying to build " + file + "\n");

            //cleanup auxdir
            RunCommand("del " + auxdir + "*.aux; del " + auxdir + "*.tex; del " + auxdir + "*.log",
                       "An error has occured during cleanup after " + file,
                       "An error has occured while trying to cleanup after " + file + "\n");

            return @"..\..\" + name + ".pdf";
        }

        /// <summary>
        /// Parse through template .tex file and insert all keywords at their respective positions
        /// keyword syntax in template .tex file: ¤¤=keyword=option=option=¤¤
        /// </summary>
        /// <param name="evm">model of the event to be parsed</param>
        /// <returns></returns>
        private static List<string> ParseFile(EventWithDetails evm)
        {
            string templatepath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Desktop\templates\";
            string[] lines;

            if (File.Exists(templatepath + evm.TypeName + ".tex"))
            {
                lines = File.ReadAllLines(templatepath + evm.TypeName + ".tex");
            }
            else
            {
                lines = File.ReadAllLines(templatepath + "Basic Event.tex");
            }

            for (int i = 0; i < lines.Length; i++)
            {
                StringBuilder sb = new StringBuilder();
                if (Regex.Match(lines[i], "¤¤.*¤¤").Success)
                {
                    string[] tokens = Regex.Split(lines[i], "¤¤");
                    for (int j = 0; j < tokens.Length; j++)
                    {
                        string[] keys = tokens[j].Split('=');
                        if (keys.Length == 1)
                        {
                            sb.Append(keys[0]);
                        }
                        else
                        {
                            switch (keys[1])
                            {
                                case "eventname": sb.Append(evm.Name); break;
                                case "eventstart": sb.Append(evm.Start); break;
                                case "eventend": sb.Append(evm.End); break;
                                case "eventtype": sb.Append(evm.TypeName); break;
                                case "eventapproved": sb.Append(evm.Approved); break;
                                case "eventrooms": sb.Append(EventBuilder.ListRooms(evm)); break;
                                case "eventcreator": sb.Append(evm.Creator); break;
                                case "eventduration": sb.Append(evm.getDuration()); break;
                                case "eventstatetext": sb.Append(evm.getStateText()); break;
                                case "eventspecific":
                                    if (keys.Length == 4)
                                    { sb.AppendLine(EventBuilder.ListEventSpecific(evm, keys[2], false)); }
                                    else if (keys.Length == 5)
                                    { sb.AppendLine(EventBuilder.ListEventSpecific(evm, keys[2], bool.Parse(keys[3]))); }
                                    break;
                                case "": break;
                                default: sb.Append(tokens[j]); break;
                            }
                        }
                        lines[i] = sb.ToString();
                    }
                }
            }
            return lines.OfType<string>().ToList();
        }

        private static string ListRooms(BasicEvent evm)
        {
            StringBuilder result = new StringBuilder(@"\begin{tabular}{|c|}\hline ");
            for (int i = 0; i < evm.Rooms.Count; i++)
            {
                result.AppendLine(evm.Rooms[i].Name + @"\\\hline ");
            }
            result.AppendLine(@"\end{tabular}");
            return result.ToString();
        }

        private static string ListEventSpecific(EventWithDetails evm, string fieldname, bool fileappend)
        {
            StringBuilder result = new StringBuilder();
            FieldModel fm = null;
            for (int i = 0; i < evm.TypeSpecifics.Count; i++)
            {
                fm = evm.TypeSpecifics[i];
                if (fieldname.Equals(evm.TypeSpecifics[i].Name)) { break; }
                if (i == evm.TypeSpecifics.Count - 1) { return ""; }
            }
            switch (fm.Datatype)
            {
                case Fieldtype.Float: result.AppendLine(fm.FloatValue.ToString()); break;
                case Fieldtype.Text: result.AppendLine(fm.StringValue); break;
                case Fieldtype.Datetime: result.AppendLine(fm.DateValue.ToString()); break;
                case Fieldtype.User: result.AppendLine(EventBuilder.printUser(UserModel.GetUser(fm.IntValue))); break;
                case Fieldtype.Group: result.AppendLine(EventBuilder.printGroup(MySqlGroup.getGroup(fm.IntValue))); break;
                case Fieldtype.File: result.AppendLine(fileappend ? @" \includepdf{" + fm.StringValue + "}" : fm.StringValue); break;
                case Fieldtype.Bool: result.AppendLine(fm.BoolValue.ToString()); break;
                case Fieldtype.UserList: for (int i = 0; i < fm.List.Count; i++)
                                         {
                                            result.AppendLine(EventBuilder.printUser(UserModel.GetUser(int.Parse(fm.List[i].Value))));
                                         }
                                         break;
                case Fieldtype.GroupList: for (int i = 0; i < fm.List.Count; i++)
                                          {
                                              result.AppendLine(EventBuilder.printGroup(MySqlGroup.getGroup(int.Parse(fm.List[i].Value))));
                                          }
                                          break;
                case Fieldtype.FileList: /*for (int i = 0; i < fm.List.Count; i++)
                                         {
                                             result.AppendLine(fileappend ? @"\includepdf{" + fm.List[i]. + "}" : fm.StringValue);
                                         }
                                         break;*/
                default: return "";
            }
            return result.ToString();
        }

        public static string printUser(UserModel um)
        {
            StringBuilder result = new StringBuilder();

            result.AppendLine(@"\begin{tabular}{|c|c|}\hline");
            result.AppendLine(@"User Name & " + um.UserName + @"\\\hline");
            result.AppendLine(@"Real Name & " + um.RealName + @"\\\hline");
            result.AppendLine(@"Admin & " + um.Admin + @"\\\hline");
            result.AppendLine(@"Active & " + um.Active + @"\\\hline");
            result.AppendLine(@"Email & " + um.Email + @"\\\hline");
            result.AppendLine(@"\end{tabular}");

            return result.ToString();
        }

        public static string printGroup(GroupModel gm)
        {
            StringBuilder result = new StringBuilder();

            result.AppendLine(@"\begin{tabular}{|c|}\hline");
            result.AppendLine(@"Group Name\\\hline");
            result.AppendLine(gm.Name + @"\\\hline");
            result.AppendLine(@"\end{tabular}");

            return result.ToString();
        }

        /// <summary>
        /// Run a given command in cmd.exe, assumes that user is using windows
        /// </summary>
        /// <param name="command">Command to be executed</param>
        /// <param name="error1">Message to be shown upon error on process exit</param>
        /// <param name="error2">Message to be shown upon error while trying to run the command</param>
        private static void RunCommand(string command, string error1, string error2)
        {
            int ExitCode = 0;
            Process process;
            try
            {
                ProcessStartInfo processInfo;

                processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);

                processInfo.CreateNoWindow = true;
                processInfo.UseShellExecute = false;

                process = Process.Start(processInfo);
                process.WaitForExit();

                ExitCode = process.ExitCode;

                process.Close();

                if (ExitCode != 0)
                {
                    MessageBox.Show(error1 + "\n" + command, "ExecuteCommand");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(error2 + "\n" + command + "\n" + e.Message, "Error");
            }
        }
    }
}
