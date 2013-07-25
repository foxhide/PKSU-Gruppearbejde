using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Text;

using CalendarApplication.Models.Event;
using CalendarApplication.Models.EventType;
using CalendarApplication.Helpers;
using System.IO;

namespace CalendarApplication.Helpers
{
    public static class ListView
    {
        /// <summary>
        /// Creates the Scroll-list structure. This method should be called with the using syntax:
        /// 
        /// @using(Html.BeginListView(id,labels))
        /// {
        ///     html tr-rows
        /// }
        /// </summary>
        /// <param name="id">A unique identifier, not used for any other element</param>
        /// <param name="labels">A string array with the labels for the rows</param>
        /// <returns>A scroll list. You should input the rows between the curly brackets as shown above</returns>
        public static ScrollList BeginListView<TModel>(this HtmlHelper<TModel> helper, string id, string[] labels)
        {
            return ListView.BeginListView(helper, id, labels, 200, "auto");
        }

        /// <summary>
        /// Creates the Scroll-list structure. This method should be called with the using syntax:
        /// 
        /// @using(Html.BeginListView(id))
        /// {
        ///     html tr-rows
        /// }
        /// </summary>
        /// <param name="id">A unique identifier, not used for any other element</param>
        /// <returns>A scroll list. You should input the rows between the curly brackets as shown above</returns>
        public static ScrollList BeginListView<TModel>(this HtmlHelper<TModel> helper, string id)
        {
            return ListView.BeginListView(helper, id, null, 200, "auto");
        }

        /// <summary>
        /// Creates the Scroll-list structure. This method should be called with the using syntax:
        /// 
        /// @using(Html.BeginListView(id,labels))
        /// {
        ///     html tr-rows
        /// }
        /// </summary>
        /// <param name="id">A unique identifier, not used for any other element</param>
        /// <param name="labels">A string array with the labels for the rows (use null for no header row)</param>
        /// <param name="height">css: height of list</param>
        /// <param name="width">css: width of list</param>
        /// <returns>A scroll list. You should input the rows between the curly brackets as shown above</returns>
        public static ScrollList BeginListView<TModel>(this HtmlHelper<TModel> helper,
                                                        string id,
                                                        string[] labels,
                                                        int maxPxHeight,
                                                        string cssWidth)
        {
            TextWriter w = helper.ViewContext.Writer;

            w.WriteLine("<div id='" + id + "' style='display:inline-block;width:" + cssWidth + ";border:1px black solid'>");

            if (labels != null)
            {
                w.WriteLine("<table class='scrollheader'><tr id='" + id + "_hrow'>");
                int i = 0;
                foreach (string label in labels)
                {
                    w.WriteLine("<th id='" + id + "_hd_" + i + "'>" + label + "</th>");
                    i++;
                }
                w.WriteLine("</tr></table>");
            }

            w.WriteLine("<div id='" + id + "_innerdiv' class='scroll'>");

            w.WriteLine("<table>");

            if (labels != null)
            {
                w.WriteLine("<tr>");
                for (int i = 0; i < labels.Length; i++)
                {
                    w.WriteLine("<td id='" + id + "_td_" + i + "' style='padding-top:0;padding-bottom:0'></td>");
                }
                w.WriteLine("</tr>");
            }

            return new ScrollList(id,labels != null ? labels.Length : 0,maxPxHeight,helper.ViewContext);
        }

        public static MvcHtmlString EndListView<TModel>(this HtmlHelper<TModel> helper, string id, int numberOfCols, int maxPxHeight)
        {
            string res = "</table></div><script>$(document).ready(updateList('" + id + "'," + numberOfCols + "," + maxPxHeight + "))</script></div>";
            return MvcHtmlString.Create(res);
        }
    }

    /// <summary>
    /// Class used for the curly bracket syntax of the html helper (@using).
    /// </summary>
    public class ScrollList : IDisposable
    {
        private string _id;
        private int _numberOfCols;
        private int _maxHeight;
        private readonly TextWriter _writer;

        // Constructor
        public ScrollList(string id, int numberOfCols, int maxHeight, ViewContext vc)
        {
            this._id = id;
            this._numberOfCols = numberOfCols;
            this._maxHeight = maxHeight;
            this._writer = vc.Writer;
        }

        // Called on close -> close tags and insert script
        public void Dispose()
        {
            string last = "</table></div><script>$(document).ready(updateList('"
                + this._id + "'," + this._numberOfCols + "," + this._maxHeight + "))</script></div>";
            this._writer.Write(last);
        }
    }
}