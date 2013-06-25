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

            //sanitize illegal latex characters
            StringBuilder sb;
            for (int i = 0; i < input.Count; i++)
            {
                //underscore
                if (Regex.Match(input[i], "_").Success)
                {
                    sb = new StringBuilder();
                    string[] tmp = input[i].Split('_');
                    for (int j = 0; j < tmp.Length; j++)
                    {
                        sb.Append(tmp[j]);
                        if (j != tmp.Length - 1)
                        {
                            sb.Append(@"\_");
                        }
                    }
                    input[i] = sb.ToString();
                }
                //hat
                if (Regex.Match(input[i], "^").Success)
                {
                    sb = new StringBuilder();
                    string[] tmp = input[i].Split('^');
                    for (int j = 0; j < tmp.Length; j++)
                    {
                        sb.Append(tmp[j]);
                        if (j != tmp.Length - 1)
                        {
                            sb.Append(@"\^{}");
                        }
                    }
                    input[i] = sb.ToString();
                }
            }

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
            }
            else
            {
                lines = EventBuilder.Default(evm);
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
            FieldModel fm = null;
            for (int i = 0; i < evm.TypeSpecifics.Count; i++)
            {
                fm = evm.TypeSpecifics[i];
                if (fieldname.Equals(evm.TypeSpecifics[i].Name)) { break; }
                if (i == evm.TypeSpecifics.Count - 1) { return ""; }
            }
            return printField(fm, fileappend);
        }

        private static string printField(FieldModel fm, bool fileappend)
        {
            StringBuilder sb = new StringBuilder();
            switch (fm.Datatype)
            {
                case Fieldtype.Float: return fm.FloatValue.ToString();
                case Fieldtype.Text: return fm.StringValue;
                case Fieldtype.Datetime: return fm.DateValue.ToString();
                case Fieldtype.User: return EventBuilder.printUser(UserModel.GetUser(fm.IntValue), true);
                case Fieldtype.Group: return EventBuilder.printGroup(MySqlGroup.getGroup(fm.IntValue), true);
                case Fieldtype.File: return fileappend ? @" \includepdf{" + fm.StringValue + "} " : fm.StringValue;
                case Fieldtype.Bool: return fm.BoolValue.ToString();
                case Fieldtype.UserList:
                    for (int i = 0; i < fm.List.Count; i++)
                    {
                        sb.AppendLine(EventBuilder.printUser(UserModel.GetUser(int.Parse(fm.List[i].Value)), false));
                    }
                    return sb.ToString();
                case Fieldtype.GroupList:
                    for (int i = 0; i < fm.List.Count; i++)
                    {
                        sb.AppendLine(EventBuilder.printGroup(MySqlGroup.getGroup(int.Parse(fm.List[i].Value)), false));
                    }
                    return sb.ToString();
                case Fieldtype.FileList:
                    // do something once files are implemented
                    return "";
                default: return "";
            }
        }

        public static string printUser(UserModel um, bool b)
        {
            if (b)
            {
                return um.UserName;
            }

            StringBuilder result = new StringBuilder();

            result.AppendLine(@"\begin{tabular}{|c|c|}\hline");
            result.AppendLine(@"User Name & " + um.UserName + @"\\\hline");
            result.AppendLine(@"Real Name & " + um.RealName + @"\\\hline");
            result.AppendLine(@"Admin & " + um.Admin + @"\\\hline");
            result.AppendLine(@"Active & " + um.Active + @"\\\hline");
            result.AppendLine(@"Email & " + um.Email + @"\\\hline");
            result.AppendLine(@"Phone Number & " + um.Phone + @"\\\hline");
            result.AppendLine(@"\end{tabular}\\");

            return result.ToString();
        }

        public static string printGroup(GroupModel gm, bool b)
        {
            if (b)
            {
                return gm.Name;
            }
            StringBuilder result = new StringBuilder();

            result.AppendLine(@"\begin{tabular}{|c|}\hline");
            result.AppendLine(@"Group Name\\\hline");
            result.AppendLine(gm.Name + @"\\\hline");
            result.AppendLine(@"\end{tabular}\\");

            return result.ToString();
        }

        /// <summary>
        /// Default template for print event. This is invoked whenever a printed event has no template tex file. 
        /// </summary>
        /// <param name="evm">Event Model to build the pdf from</param>
        /// <returns>string array containing each line of the tex file</returns>
        private static string[] Default(EventWithDetails evm)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(@"\documentclass[a4paper,12pt]{article}");
            sb.AppendLine(@"\usepackage[utf8]{inputenc}");
            sb.AppendLine(@"\usepackage{pdfpages}");
            sb.AppendLine(@"\begin{document}");

            //basic event info
            sb.AppendLine(@"\section*{Productionplan");
            sb.AppendLine(@"\section*{" + evm.Name + @"}");
            sb.AppendLine(@"\begin{tabular}{|l|l|l|}\hline");
            sb.AppendLine(@"Type: " + evm.TypeName + @" & Creator: " + evm.Creator + @" & " +
                (evm.Approved ? @"Event Approved" : @"Event Not Approved") + @"\\\hline");
            sb.AppendLine(@"Start: " + evm.Start + @" & End: " + evm.End + @" & State: " + evm.getStateText() + @"\\\hline");
            sb.AppendLine(@"\end{tabular}\vspace{1cm}");
            sb.AppendLine(@"Rooms Used:\qquad" + EventBuilder.ListRooms(evm) + @"\vspace{1cm}");
            
            //additional event info
            sb.AppendLine(@"\begin{tabular}{|l|l|}\hline ");
            for (int i = 0; i < evm.TypeSpecifics.Count; i++)
            {
                sb.AppendLine(evm.TypeSpecifics[i].Name + " & ");
                sb.AppendLine(@"\begin{tabular}{l}");
                sb.Append(printField(evm.TypeSpecifics[i], false));
                sb.AppendLine(@"\end{tabular}\\\hline");
            }
            sb.AppendLine(@"\end{tabular}");

            sb.AppendLine(@"\end{document}");

            return sb.ToString().Split('\n');
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
