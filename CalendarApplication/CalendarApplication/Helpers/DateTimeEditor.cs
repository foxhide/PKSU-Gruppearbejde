using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Text;

namespace CalendarApplication.Helpers
{
    public static class DateTimeEditor
    {
        // String[] used for std. field -> display all dates.
        public static readonly string[] DATE_TIME_ALL_FIELDS = new string[] { "Day", "Month", "Year", "Hour", "Minute" };

        /// <summary>
        /// Create a editor for an EditableDateTime. Displays all fields and makes standard validation (remember to include
        /// date-picker-validator.js).
        /// </summary>
        public static MvcHtmlString DateTimeEditorFor<TModel, TValue>(this HtmlHelper<TModel> helper,
            Expression<Func<TModel, TValue>> expression)
        {
            return DateTimeEditor.DateTimeEditorFor(helper, expression, DATE_TIME_ALL_FIELDS,"","");
        }

        /// <summary>
        /// Create a editor for an EditableDateTime. Displays all fields and applies the given validation, together with
        /// the standard validation. (remember to include date-picker-validator.js).
        /// </summary>
        /// <param name="compare">String of compares, ex: 'g.today,l.otherName, ...' -> date should be greater than
        /// today, but less than date named 'otherName'</param>
        public static MvcHtmlString DateTimeEditorFor<TModel, TValue>(this HtmlHelper<TModel> helper,
            Expression<Func<TModel, TValue>> expression, string compare)
        {
            return DateTimeEditor.DateTimeEditorFor(helper, expression, DATE_TIME_ALL_FIELDS, compare, "");
        }

        /// <summary>
        /// Returns the html for a datetime editor
        /// </summary>
        /// <param name="compare">String of compares, ex: 'g.today,l.otherName, ...' -> date should be greater than
        /// today, but less than date named 'otherName'</param>
        /// <param name="onchange">JS-function to be called onchange</param>
        /// <returns></returns>
        public static MvcHtmlString DateTimeEditorFor<TModel, TValue>(this HtmlHelper<TModel> helper,
            Expression<Func<TModel, TValue>> expression, string compare, string onchange)
        {
            return DateTimeEditor.DateTimeEditorFor(helper, expression, DATE_TIME_ALL_FIELDS, compare, onchange);
        }

        /// <summary>
        /// Create a editor for an EditableDateTime. Displays given fields and applies the given validation, together with
        /// the standard validation. (remember to include date-picker-validator.js).
        /// </summary>
        /// <param name="fields">Array of names of fields. Possible fields are 'Year','Month',... Printed in the order given
        /// in this array.</param>
        /// <param name="compare">String of compares, ex: 'g.today,l.otherName, ...' -> date should be greater than
        /// today, but less than date named 'otherName'</param>
        /// <param name="onchange">"JS-function to be called onchange"</param>
        public static MvcHtmlString DateTimeEditorFor<TModel, TValue>(this HtmlHelper<TModel> helper,
            Expression<Func<TModel, TValue>> expression, string[] fields, string compare, string onchange)
        {
            // Get model
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            DateTime edt = (DateTime)metadata.Model;

            // Get Name
            string name = ExpressionHelper.GetExpressionText(expression).Split('.').Last();

            return DateTimeEditorFor(edt, name, fields, compare, onchange);
        }

        public static MvcHtmlString DateTimeEditorFor(DateTime edt, string name, string[] fields, string compare, string onchange)
        {
            string[] tmp = name.Split(new[] { '[', ']', '.' });
            string id = tmp[0];
            for (int i = 1; i < tmp.Length; i++) { id += "_" + tmp[i]; }

            StringBuilder builder = new StringBuilder();

            // Build validation string.
            compare = compare == null ? "" : compare;
            string validateStr = "onchange=\"validateDate('" + id + "','" + compare + "'); " + onchange + "\"";

            builder.AppendLine("<input type='hidden' name='" + name + "' id='" + id + "' value='"
                                + edt.ToString("dd-MM-yyyy HH:mm:ss") + "'>");
            builder.AppendLine("<table class='custom-style-1'>");

            // Create headers
            builder.AppendLine("<tr>");
            foreach (string f in fields)
            {
                builder.Append("<td style='color:grey;font-size:80%;text-align:left'>");
                builder.Append(f);
                builder.Append("</td>");
            }
            builder.Append("</tr>");

            // Create input fields
            builder.AppendLine("<tr>");
            foreach (string f in fields)
            {
                builder.AppendLine("<td>");
                int w = f.Equals("Year") ? 60 : 40;  // Size
                int val = f.Equals("Year") ? edt.Year :
                    f.Equals("Month") ? edt.Month :
                    f.Equals("Day") ? edt.Day :
                    f.Equals("Hour") ? edt.Hour :
                    f.Equals("Minute") ? edt.Minute : 0;  // Current value
                AddField(builder, id, f, val, w, validateStr); // Add the field
                builder.Append("</td>");
            }
            string function = string.IsNullOrEmpty(onchange) ? "null" : onchange.Substring(0, onchange.Length - 2);
            builder.Append("<td><script>createDatePicker('" + id + "','" + compare + "'," + function + ")</script>");
            builder.Append("<input type='hidden' id='" + id + "_picker'>");
            builder.Append("<input type='button' value='Select Date' id='" + id + "_but' onclick=showDatePicker('" + id + "_picker') /></td>");
            builder.AppendLine("</tr></table>");
            return MvcHtmlString.Create(builder.ToString());
        }

        // Function for adding a input field to a builder
        private static void AddField(StringBuilder builder, string name, string field, int value, int width, string validate)
        {
            builder.Append("<input class='text-box single-line' type='number' id='");
            builder.Append(name);    //
            builder.Append("_");     // id='name_field'
            builder.Append(field);   //
            builder.Append("' data-val='true' value='");
            builder.Append(value);
            builder.Append("' style='width:");
            builder.Append(width);
            builder.Append("px' ");
            builder.Append(validate);
            builder.Append(">");
        }
    }
}