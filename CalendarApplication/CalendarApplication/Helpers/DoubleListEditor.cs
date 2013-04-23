using System;
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
        public static readonly string SCRIPT = "<script>function moveSelected(name, add) {" +
                                                "var name1 = add ? name + '_select' : name + '_available';" +
                                                "var name2 = add ? name + '_available' : name + '_select';" +
                                                "var list1 = document.getElementById(name1);" +
                                                "var list2 = document.getElementById(name2);" +
                                                "for (var i = 0; i < list1.length; i++) {" +
                                                "if(list1.options[i].selected) {" +
                                                "var id = list1.options[i].value; var roomName = list1.options[i].innerHTML;" +
                                                "list1.remove(i); list2.options[list2.length] = new Option(roomName,id);" +
                                                "document.getElementById(name + '_' + id).value = add; break; }" +
                                                "}" +
                                                "}</script>";

        public static MvcHtmlString ListEditorFor<TModel, TValue>(this HtmlHelper<TModel> helper,
            Expression<Func<TModel, TValue>> expression, string labelList1, string labelList2)
        {
            // Get model
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            List<SelectListItem> list = (List<SelectListItem>)metadata.Model;

            // Get Name
            string name = ExpressionHelper.GetExpressionText(expression).Split('.').Last();

            StringBuilder builder = new StringBuilder();

            builder.AppendLine(DoubleListEditor.SCRIPT);

            builder.AppendLine("<span style='color:grey;font-size:80%;text-align:left'>");

            for (int i = 0; i < list.Count; i++)
            {
                builder.AppendLine("<input type='hidden' name='" + name + "[" + i + "].Value' value='"+list[i].Value+"'>");
                builder.AppendLine("<input type='hidden' name='" + name + "[" + i + "].Text' value='"+list[i].Text+"'>");
                builder.AppendLine("<input type='hidden' id='" + name + "_" + list[i].Value + "' name='" + name +
                                    "[" + i + "].Selected' value=" + (list[i].Selected ? "true" : "false") + ">");
            }

            builder.AppendLine("<table class='custom-style-1'><tr>");

            builder.AppendLine("<td>" + labelList1 + "</td><td></td><td>" + labelList2 + "</td>");

            builder.AppendLine("</tr><tr>");
            builder.AppendLine("<td>");
            builder.AppendLine("<select id='" + name + "_available' name='" + name + "' size='5' style='min-width:100px'>");
            foreach (SelectListItem sli in list)
            {
                if(sli.Selected) builder.AppendLine("<option value='" + sli.Value + "'>" + sli.Text + "</option>");
            }
            builder.AppendLine("</select>");
            builder.AppendLine("</td>");

            builder.AppendLine("<td style='vertical-align:middle'>");
            builder.Append("<input type='button' value='<--' onclick=moveSelected('" + name + "',true)><br>");
            builder.Append("<input type='button' value='-->' onclick=moveSelected('" + name + "',false)>");
            builder.AppendLine("</td>");

            builder.AppendLine("<td>");
            builder.AppendLine("<select id='" + name + "_select' name='" + name + "' size='5' style='min-width:100px'>");
            foreach (SelectListItem sli in list)
            {
                if (!sli.Selected) builder.AppendLine("<option value='" + sli.Value + "'>" + sli.Text + "</option>");
            }
            builder.AppendLine("</select>");
            builder.AppendLine("</td>");

            builder.Append("</tr></table></span>");

            return MvcHtmlString.Create(builder.ToString());
        }
    }
}