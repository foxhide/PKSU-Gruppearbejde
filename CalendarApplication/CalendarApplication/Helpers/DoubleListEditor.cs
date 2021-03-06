﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Text;

namespace CalendarApplication.Helpers
{
    public static class DoubleListEditor
    {
        /// <summary>
        /// Create a DoubleListEditor for the given list of SelectListItems
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="helper"></param>
        /// <param name="expression">The list as a lambda expression</param>
        /// <param name="labelList1">Label of the selected list</param>
        /// <param name="labelList2">Label of the available list</param>
        /// <returns>A MvcHtmlString with the html for the editor</returns>
        public static MvcHtmlString ListEditorFor<TModel, TValue>(this HtmlHelper<TModel> helper,
            Expression<Func<TModel, TValue>> expression, string selectedLabel, string availableLabal)
        {
            return DoubleListEditor.ListEditorFor(helper, expression, selectedLabel, availableLabal, "", "");
        }

        /// <summary>
        /// Create a DoubleListEditor for the given list of SelectListItems (with onaction js-functions)
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="helper"></param>
        /// <param name="expression">The list as a lambda expression</param>
        /// <param name="labelList1">Label of the selected list</param>
        /// <param name="labelList2">Label of the available list</param>
        /// <param name="onAdd">JS-function to be called on add</param>
        /// <param name="onRem">JS-function to be called on remove</param>
        /// <returns>A MvcHtmlString with the html for the editor</returns>
        public static MvcHtmlString ListEditorFor<TModel, TValue>(this HtmlHelper<TModel> helper,
            Expression<Func<TModel, TValue>> expression, string selectedLabel, string availableLabal, string onAdd, string onRem)
        {
            // Get model
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            List<SelectListItem> list = (List<SelectListItem>)metadata.Model;

            // Get Name
            string name = ExpressionHelper.GetExpressionText(expression).Split('.').Last();

            return ListEditorFor(list, name, name, selectedLabel, availableLabal, onAdd, onRem);
        }

        public static MvcHtmlString ListEditorFor(List<SelectListItem> list, string id, string name,
                                                  string selectedLabel, string availableLabal, string onAdd, string onRem)
        {

            if (list == null)
            {
                list = new List<SelectListItem>();
            }
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("<span style='color:grey;font-size:80%;text-align:left'>");
            for (int i = 0; i < list.Count; i++)
            {
                builder.AppendLine("<input type='hidden' id='" + id + "_" + i + "_Value' name='" + name +
                                    "[" + i + "].Value' value='" + list[i].Value + "'>");
                builder.AppendLine("<input type='hidden' id='" + id + "_" + i + "_Text' name='" + name +
                                    "[" + i + "].Text' value='" + list[i].Text + "'>");
                builder.AppendLine("<input type='hidden' id='" + id + "_" + list[i].Value + "' name='" + name +
                                    "[" + i + "].Selected' value=" + (list[i].Selected ? "true" : "false") + ">");
            }

            builder.AppendLine("<table class='custom-style-1'><tr>");

            builder.AppendLine("<td>" + availableLabal + "</td><td></td><td>" + selectedLabel + "</td>");

            builder.AppendLine("</tr><tr>");

            builder.AppendLine("<td>");
            builder.AppendLine("<select id='" + id + "_available' multiple='multiple' name='" + name + "' size='5' style='min-width:100px'>");
            foreach (SelectListItem sli in list)
            {
                if (!sli.Selected) builder.AppendLine("<option value='" + sli.Value + "'>" + sli.Text + "</option>");
            }
            builder.AppendLine("</select>");
            builder.AppendLine("</td>");

            builder.AppendLine("<td style='vertical-align:middle'>");
            builder.Append("<input type='button' id='" + name + "_add_button' value='-->' onclick=\"moveSelected('" + id + "',true);");
            builder.Append(onAdd + "\" ><br>");
            builder.Append("<input type='button' id='" + name + "_rem_button' value='<--' onclick=\"moveSelected('" + id + "',false);");
            builder.Append(onRem + "\" >");
            builder.AppendLine("</td>");

            builder.AppendLine("<td>");
            builder.AppendLine("<select id='" + id + "_select' multiple='multiple' name='" + name + "' size='5' style='min-width:100px'>");
            foreach (SelectListItem sli in list)
            {
                if (sli.Selected) builder.AppendLine("<option value='" + sli.Value + "'>" + sli.Text + "</option>");
            }
            builder.AppendLine("</select>");
            builder.AppendLine("</td>");

            builder.Append("</tr></table></span>");

            return MvcHtmlString.Create(builder.ToString());
        }
    }
}