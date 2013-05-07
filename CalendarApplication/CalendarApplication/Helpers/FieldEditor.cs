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

namespace CalendarApplication.Helpers
{
    public static class FieldEditor
    {
        public static MvcHtmlString FieldEditorFor<TModel, TValue>(this HtmlHelper<TModel> helper,
            Expression<Func<TModel, TValue>> expression)
        {
            // Get model
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            FieldModel model = (FieldModel)metadata.Model;

            // Get Name
            string name = ExpressionHelper.GetExpressionText(expression).Split('.').Last();


            StringBuilder builder = new StringBuilder();
            builder.AppendLine("<input type='hidden' name='" + name + ".Datatype' value='" + model.Datatype + "'>");
            builder.AppendLine("<input type='hidden' name='" + name + ".ID' value='" + model.ID + "'>");

            builder.AppendLine("<label for='"+name+"'>"+model.Name+"</label><br>");
            builder.AppendLine("<span style='color:grey;font-size:80%;text-align:left'>"+model.Description+"</span><br>");
            
            if (model.Datatype == Fieldtype.Float)
            {
                builder.AppendLine("<input type='number' name='" + name + ".FloatValue' value='" + model.FloatValue + "' step='any' >");
            }
            else if (model.Datatype == Fieldtype.Text)
            {
                builder.AppendLine("<div id='" + name + "_StringValue_char_counter'>Characters left: " + model.VarcharLength + "</div>");
                string counterInc = "onkeyup=updateCounter('" + name + "_StringValue'," + model.VarcharLength + ");";
                if (model.VarcharLength < 50)
                {
                    builder.AppendLine("<input type='text' id='" + name + "_StringValue' name='" + name +
                                        ".StringValue' value='" + model.StringValue);
                    builder.AppendLine("' " + counterInc + ">");
                }
                else
                {
                    builder.AppendLine("<textarea type='text' id='" + name + "_StringValue' name='" + name +
                                        ".StringValue' cols='40' rows='");
                    builder.Append((model.VarcharLength/40+1) + "' " + counterInc + ">" + model.StringValue + "</textarea>");
                }
            }
            else if (model.Datatype == Fieldtype.Bool)
            {
                builder.AppendLine("<input type='checkbox' name='" + name + ".BoolValue' value='true'"+(model.BoolValue?" selected>":">"));
            }
            else if (model.Datatype == Fieldtype.Datetime)
            {
                builder.AppendLine(DateTimeEditor.DateTimeEditorFor(model.DateValue, name + ".DateValue",
                                                        DateTimeEditor.DATE_TIME_ALL_FIELDS, "").ToString());
            }
            else if (model.Datatype == Fieldtype.User)
            {
                builder.AppendLine("<select name='" + name + ".IntValue'>");
                foreach (SelectListItem user in model.List)
                {
                    builder.Append("<option value='" + user.Value);
                    if (int.Parse(user.Value) == model.IntValue) { builder.Append(" selected"); }
                    builder.Append("'>" + user.Text + "</option>");
                }
                builder.AppendLine("</select>");
            }
            else if (model.Datatype == Fieldtype.Group)
            {
                builder.AppendLine("<select name='" + name + ".IntValue'>");
                foreach (SelectListItem group in model.List)
                {
                    builder.Append("<option value='" + group.Value);
                    if (int.Parse(group.Value) == model.IntValue) { builder.Append(" selected"); }
                    builder.Append("'>" + group.Text + "</option>");
                }
                builder.AppendLine("</select>");
            }
            else if (model.Datatype == Fieldtype.File)
            {
                //////////////////////////// needs work ////////////////////////
                builder.AppendLine("<input type='file'>");
            }
            else if (model.Datatype == Fieldtype.UserList)
            {
                builder.AppendLine(DoubleListEditor.ListEditorFor(model.List, name + ".List",
                                     "Users selected", "Users available", "", "").ToString());
            }
            else if (model.Datatype == Fieldtype.GroupList)
            {
                builder.AppendLine(DoubleListEditor.ListEditorFor(model.List, name + ".List",
                                     "Groups selected", "Groups available", "", "").ToString());
            }
            else if (model.Datatype == Fieldtype.FileList)
            {
                //////////////////////////// needs work ////////////////////////
                builder.AppendLine("<text>File list</text>");
            }
            else
            {
                builder.AppendLine("<span style='color:red'>Error. Could not create input field. Not implemented...</span>");
            }

            return MvcHtmlString.Create(builder.ToString());
        }
    }
}