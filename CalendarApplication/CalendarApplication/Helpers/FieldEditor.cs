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

            builder.AppendLine("<label for='" + name + "' style='font-weight:bold'>" + model.Name + "</label>");
            builder.AppendLine(model.RequiredCreate ? "<span style='color:red'>**</span>" : model.RequiredApprove ? "<span style='color:red'>*</span>" : "");
            builder.AppendLine("<br>");
            if (!string.IsNullOrEmpty(model.Description))
            {
                builder.AppendLine("<span style='color:grey;font-size:80%;text-align:left'>" + model.Description + "</span><br>");
            }
            
            if (model.Datatype == Fieldtype.Float)
            {
                //builder.AppendLine("<input type='number' name='" + name + ".FloatValue' value='" + model.FloatValue + "' step='any' >");
                builder.AppendLine("<div id='" + id + "_number_warning'>Input other than numbers will be disregarded. Use commas as decimal points.</div>");
                builder.Append("<input id='" + id + "' type='text' name='" + name + ".FloatValue' value='" + model.FloatValue);
                builder.AppendLine("' onchange=\"setState(); validateInput('" + id + "','Float',true,true)\">");
            }
            else if (model.Datatype == Fieldtype.Text)
            {
                builder.AppendLine("<div id='" + id + "_char_counter'>Characters left: " + (model.VarcharLength - (string.IsNullOrWhiteSpace(model.StringValue) ? 0 : model.StringValue.Length)) + "</div>");
                string counterInc = "onkeyup=\"updateCounter('" + id + "'," + model.VarcharLength + "); setState(); validateInput('" + id + "','Text',true,true)\"";
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
                builder.AppendLine("<input type='checkbox' name='" + name + ".BoolValue' value='true'" + (model.BoolValue ? " checked='checked'>" : ">"));
            }
            else if (model.Datatype == Fieldtype.Datetime)
            {
                builder.AppendLine(DateTimeEditor.DateTimeEditorFor(model.DateValue, name + ".DateValue",
                                                        DateTimeEditor.DATE_TIME_ALL_FIELDS, "", "").ToString());
            }
            else if (model.Datatype == Fieldtype.User)
            {
                builder.Append("<select name='" + name + ".IntValue' id='" + id);
                builder.AppendLine("' onchange=\"setState(); validateInput('" + id + "','User',true,true)\">");
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
                builder.Append("<select name='" + name + ".IntValue' id='" + id);
                builder.AppendLine("' onchange=\"setState(); validateInput('" + id + "','Group',true,true)\">");
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
                if (model.File == null) { model.File = new FileModel(); }
                if (model.File.ID > 0)
                {
                    builder.AppendLine("<div id='" + id + "'>");
                    builder.AppendLine("<div id='" + id + "_Current'>");
                    builder.AppendLine("<text>" + model.File.CurrentFileName + "</text>");
                    builder.AppendLine("<input type='hidden' id='" + id + "_Active' ");
                    builder.AppendLine("name='" + name + ".File.Active' value='true'>");
                    builder.AppendLine("<input type='hidden' id='" + id + "_Delete' ");
                    builder.AppendLine("name='" + name + ".File.Delete' value='false'>");
                    builder.AppendLine("<input type='hidden' id='" + id + "_ID' ");
                    builder.AppendLine("name='" + name + ".File.ID' value='" + model.File.ID + "'>");
                    builder.AppendLine("<input type=\"button\" value=\"Delete file permanently\" onclick=\"deleteFile('" + id + "'); setState();\" />");
                    builder.AppendLine("<input type=\"button\" value=\"Remove only\" onclick=\"removeFile('" + id + "'); setState();\" />");
                    builder.AppendLine("</div>");
                    builder.AppendLine("<div id='" + id + "_New' style='display:none' >");
                    builder.AppendLine("<input type='file' id='" + id + "_Input' name='" + name + ".File.InputFile'>");
                    builder.AppendLine("</div>");
                    builder.AppendLine("</div>");
                }
                else
                {
                    builder.AppendLine("<div id='" + id + "'>");
                    builder.AppendLine("<input type='file' id='" + id + "_Input' name='" + name + ".File.InputFile'");
                    builder.AppendLine("onchange=\"setState(); validateInput('" + id + "','File',true,true);\" >");
                    builder.AppendLine("<input type='hidden' id='" + id + "_ID' ");
                    builder.AppendLine("name='" + name + ".File.ID' value='" + 0 + "'>");
                    builder.AppendLine("<input type='hidden' id='" + id + "_Active' ");
                    builder.AppendLine("name='" + name + ".File.Active' value='true'>");
                    builder.AppendLine("</div>");
                }
            }
            else if (model.Datatype == Fieldtype.UserList)
            {
                builder.AppendLine(DoubleListEditor.ListEditorFor(model.List, id, name + ".List",
                                     "Users selected", "Users available",
                                     "setState(); validateInput('" + id + "','List',true,true)", "setState()").ToString());
            }
            else if (model.Datatype == Fieldtype.GroupList)
            {
                builder.AppendLine(DoubleListEditor.ListEditorFor(model.List, id, name + ".List",
                                     "Groups selected", "Groups available",
                                     "setState(); validateInput('" + id + "','List',true,true)", "setState()").ToString());
            }
            else if (model.Datatype == Fieldtype.FileList)
            {
                builder.AppendLine("<div id='" + id + "'>");                
                if (model.FileList == null) { model.FileList = new List<FileModel>(); }
                string viewName = name + ".FileList";
                for (int i = 0; i < model.FileList.Count; i++)
                {
                    builder.AppendLine("<div id='" + id + "_" + i + "'>");
                    builder.AppendLine("<text>" + (model.FileList[i].CurrentFileName as string) + "</text>");
                    builder.AppendLine("<input type='hidden' id='" + id + "_" + i + "_Active' ");
                    builder.AppendLine("name='" + viewName + "[" + i + "].Active' value='true'>");
                    builder.AppendLine("<input type='hidden' id='" + id + "_" + i + "_Delete' ");
                    builder.AppendLine("name='" + viewName + "[" + i + "].Delete' value='false'>");
                    builder.AppendLine("<input type='hidden' id='" + id + "_" + i + "_ID' ");
                    builder.AppendLine("name='" + viewName + "[" + i + "].ID' value='" + model.FileList[i].ID + "'>");
                    builder.AppendLine("<input type=\"button\" value=\"Delete file permanently\" onclick=\"deleteFileList('" + id + "_" + i + "'); setState();\" />");
                    builder.AppendLine("<input type=\"button\" value=\"Remove only\" onclick=\"removeFileList('" + id + "_" + i + "'); setState();\" />");
                    builder.AppendLine("</div>");

                }
                builder.AppendLine("</div>");
                builder.AppendLine("<input type='button' value='Add' onclick='addFileListField(\"" + id + "\",\"" + viewName + "\")' />");
                builder.AppendLine("<script>fileCounter['" + id + "'] = " + model.FileList.Count + ";</script>");
            }
            else if (model.Datatype == Fieldtype.TextList)
            {
                builder.AppendLine("<div id='" + id + "'>");
                if (model.StringList == null) { model.StringList = new List<StringListModel>(); }
                string viewName = name + ".StringList";
                for (int i = 0; i < model.StringList.Count; i++)
                {
                    builder.AppendLine("<div id='" + id + "_" + i + "'>");
                    // the following could possibly be replaced with repeated usage of StringListPartial view
                    // divs, inputs and so on are the same
                    builder.AppendLine("<div id='" + id + "_" + i + "_Text_char_counter'>Characters left: " + (250 - model.StringList[i].Text.Length) + "</div>");
                    builder.AppendLine("<input type='text' id='" + id + "_" + i +"_Text' ");
                    builder.AppendLine("name='" + viewName + "[" + i + "].Text' value='" + model.StringList[i].Text + "' "); 
                    builder.AppendLine("onkeyup=\"updateCounter('" + id + "_" + i + "_Text', 250); setState(); validateInput('" + id + "','StringList',true,true);\" />");
                    builder.AppendLine("<input type='hidden' id='" + id + "_" + i + "_Active' ");
                    builder.AppendLine("name='" + viewName + "[" + i + "].Active' value='true'>");
                    builder.AppendLine("<input type='hidden' id='" + id + "_" + i + "_ID' ");
                    builder.AppendLine("name='" + viewName + "[" + i + "].ID' value='" + model.StringList[i].ID + "'>");
                    builder.AppendLine("<input type=\"button\" value=\"Remove\" onclick=\"removeStringListField('" + id + "_" + i + "'); setState();\" />");
                    builder.AppendLine("</div>");
                    
                }
                builder.AppendLine("</div>");
                builder.AppendLine("<input type='button' value='Add' onclick='addStringListField(\"" + id + "\",\"" + viewName + "\")' />");
                builder.AppendLine("<script>stringCounter['" + id + "'] = " + model.StringList.Count + ";</script>");
            }
            else
            {
                builder.AppendLine("<span style='color:red'>Error. Could not create input field. Not implemented...</span>");
            }

            if (model.RequiredCreate)
            {
                builder.AppendLine("<div id='" + id + "_Error' class='validation-summary-errors' style='display:none'>");
                builder.Append(model.Name + " is required!");
                builder.Append("</div>");
            }

            return MvcHtmlString.Create(builder.ToString());
        }
    }
}