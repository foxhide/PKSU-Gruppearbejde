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

            // Make id-string for javascript functions.
            string[] tmp = name.Split(new[] { '[' , ']' });
            string id = tmp[0] + "_" + tmp[1]; // TypeSpecific_<number>

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("<input type='hidden' id='" + id + "_Datatype' name='" + name + ".Datatype' value='" + model.Datatype + "'>");
            builder.AppendLine("<input type='hidden' name='" + name + ".ID' value='" + model.ID + "'>");
            // Keep track of Name of server-side error message.
            builder.AppendLine("<input type='hidden' id='" + id + "_Name' name='" + name + ".Name' value='" + model.Name + "'>");
            // Keep track of required fields to perform client and server-side checks for approval/creation
            builder.AppendLine("<input type='hidden' id='" + id + "_RequiredCreate' name='" + name + ".RequiredCreate' value='" + model.RequiredCreate + "'>");
            builder.AppendLine("<input type='hidden' id='" + id + "_RequiredApprove' name='" + name + ".RequiredApprove' value='" + model.RequiredApprove + "'>");

            builder.AppendLine("<label for='"+name+"'>"+model.Name+"</label><br>");
            builder.AppendLine("<span style='color:grey;font-size:80%;text-align:left'>"+model.Description+"</span><br>");
            
            if (model.Datatype == Fieldtype.Float)
            {
                builder.AppendLine("<input type='number' name='" + name + ".FloatValue' value='" + model.FloatValue + "' step='any' >");
            }
            else if (model.Datatype == Fieldtype.Text)
            {
                builder.AppendLine("<div id='" + id + "_char_counter'>Characters left: " + model.VarcharLength + "</div>");
                string counterInc = "onkeyup=\"updateCounter('" + id + "'," + model.VarcharLength + "); setState(); updateSelf('" + id + "','Text')\"";
                if (model.VarcharLength < 50)
                {
                    builder.AppendLine("<input type='text' id='" + id + "' name='" + name +
                                        ".StringValue' value='" + model.StringValue);
                    builder.AppendLine("' " + counterInc + ">");
                }
                else
                {
                    builder.AppendLine("<textarea type='text' id='" + id + "' name='" + name +
                                        ".StringValue' cols='40' rows='");
                    builder.Append((model.VarcharLength/40+1) + "' " + counterInc + ">" + model.StringValue + "</textarea>");
                }
                // Keep track of varchar length, in case the page needs to be reloaded.
                builder.AppendLine("<input type='hidden' name='" + name + ".VarcharLength' value='" + model.VarcharLength + "'>");
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
                builder.AppendLine("<select name='" + name + ".IntValue' id='" + id + "' onchange=\"setState(); updateSelf('" + id + "','User')\">");
                foreach (SelectListItem user in model.List)
                {
                    builder.Append("<option value='" + user.Value + "'");
                    if (int.Parse(user.Value) == model.IntValue) { builder.Append(" selected='selected'"); }
                    builder.Append(">" + user.Text + "</option>");
                }
                builder.AppendLine("</select>");
            }
            else if (model.Datatype == Fieldtype.Group)
            {
                builder.AppendLine("<select name='" + name + ".IntValue' id='" + id + "' onchange=\"setState(); updateSelf('" + id + "','Group')\">");
                foreach (SelectListItem group in model.List)
                {
                    builder.Append("<option value='" + group.Value + "'");
                    if (int.Parse(group.Value) == model.IntValue) { builder.Append(" selected='selected'"); }
                    builder.Append(">" + group.Text + "</option>");
                }
                builder.AppendLine("</select>");
            }
            else if (model.Datatype == Fieldtype.File)
            {
                //////////////////////////// needs work ////////////////////////
                builder.AppendLine("<input type='file' id='" + id + "'>");
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

            if (model.RequiredCreate)
            {
                builder.Append("<span style='color:red'>**</span>");
                builder.AppendLine("<div id='" + id + "_Error' class='validation-summary-errors'></div>");
            }
            else if (model.RequiredApprove)
            {
                builder.Append("<span style='color:red'>*</span>");
            }

            return MvcHtmlString.Create(builder.ToString());
        }
    }
}