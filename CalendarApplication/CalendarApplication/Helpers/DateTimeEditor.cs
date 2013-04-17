using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Text;

using CalendarApplication.Models.Shared;

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
            return DateTimeEditor.DateTimeEditorFor(helper, expression, DATE_TIME_ALL_FIELDS,"");
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
            return DateTimeEditor.DateTimeEditorFor(helper, expression, DATE_TIME_ALL_FIELDS, compare);
        }

        /// <summary>
        /// Create a editor for an EditableDateTime. Displays given fields and applies the given validation, together with
        /// the standard validation. (remember to include date-picker-validator.js).
        /// </summary>
        /// <param name="fields">Array of names of fields. Possible fields are 'Year','Month',... Printed in the order given
        /// in this array.</param>
        /// <param name="compare">String of compares, ex: 'g.today,l.otherName, ...' -> date should be greater than
        /// today, but less than date named 'otherName'</param>
        public static MvcHtmlString DateTimeEditorFor<TModel, TValue>(this HtmlHelper<TModel> helper,
            Expression<Func<TModel, TValue>> expression, string[] fields, string compare)
        {
            // Get model
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            EditableDateTime edt = (EditableDateTime)metadata.Model;

            // Get Name
            string name = ExpressionHelper.GetExpressionText(expression).Split('.').Last();

            StringBuilder builder = new StringBuilder();

            // Build validation string.
            compare = compare == null ? "" : compare;
            string validateStr = "onchange=validateDate('" + name + "','" + compare + "')";

            builder.AppendLine("<span style='color:grey;font-size:80%;text-align:left'>");
            builder.AppendLine("<table class='datetime-editor'>");

            // Create headers
            builder.AppendLine("<tr>");
            foreach (string f in fields)
            {
                builder.Append("<td>");
                builder.Append(f);
                builder.Append("</td>");
            }
            builder.Append("</tr>");

            // Create input fields
            builder.AppendLine("<tr>");
            foreach (string f in fields)
            {
                builder.Append("<td>");
                int w = f.Equals("Year") ? 60 : 40;  // Size
                int val = f.Equals("Year") ? edt.Year :
                    f.Equals("Month") ? edt.Month :
                    f.Equals("Day") ? edt.Day :
                    f.Equals("Hour") ? edt.Hour :
                    f.Equals("Minute") ? edt.Minute : 0;  // Current value
                AddField(builder, name, f, val, w, validateStr); // Add the field
                builder.Append("</td>");
            }
            builder.AppendLine("</tr></table>");
            builder.AppendLine("</span>");

            return MvcHtmlString.Create(builder.ToString());
        }

        // Function for adding a input field to a builder
        private static void AddField(StringBuilder builder, string name, string field, int value, int width, string validate)
        {
            builder.Append("<input class='text-box single-line' type='number' name='");
            builder.Append(name);    //
            builder.Append(".");     // name='name.field'
            builder.Append(field);   //
            builder.Append("' id='");
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