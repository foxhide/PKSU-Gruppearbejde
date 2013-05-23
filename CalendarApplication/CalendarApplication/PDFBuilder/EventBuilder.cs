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
                string[] tokens = Regex.Split(lines[i], "¤¤");
                for (int j = 0; j < tokens.Length; j++)
                {
                    switch (tokens[j])
                    {
                        case "=eventname=":       sb.Append(evm.Name); break;
                        case "=eventstart=":      sb.Append(evm.Start); break;
                        case "=eventend=":        sb.Append(evm.End); break;
                        case "=eventtype=":       sb.Append(evm.TypeName); break;
                        case "=eventapproved=":   sb.Append(evm.Approved); break;
                        case "=eventrooms=":      sb.Append(EventBuilder.ListRooms(evm)); break;
                        default:                sb.Append(tokens[j]); break;
                    }
                }
                lines[i] = sb.ToString();
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
