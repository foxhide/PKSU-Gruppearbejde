using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Text;

using CalendarApplication.Models.Event;
using CalendarApplication.Models.EventType;
using CalendarApplication.Models.Shared;
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

            builder.AppendLine("<label for='"+name+"'>"+model.Name+"</label>");
            builder.AppendLine("<input type='hidden' name='"+name+".Datatype' value='" + model.Datatype + "'>");
            builder.AppendLine("<input type='hidden' name='"+name+".ID' value='" + model.ID + "'>");
            builder.AppendLine("<br>");
            
            if (model.Datatype == Fieldtype.Integer)
            {
                builder.AppendLine("<input type='number' name='" + name + ".Value' value='" + (int)model.Value + "'>");
            }
            else if (model.Datatype == Fieldtype.Text)
            {
                if (model.VarcharLength < 50)
                {
                    builder.AppendLine("<input type='text' name='" + name + ".Value' value='" + (string)model.Value + "'>");
                }
                else
                {
                    builder.AppendLine("<textarea type='text' name='" + name + ".Value' cols='40' rows='");
                    builder.Append((model.VarcharLength/40+1) + "'>" + (string)model.Value + "</textarea>");
                }
            }
            else if (model.Datatype == Fieldtype.Bool)
            {
                builder.AppendLine("<input type='checkbox' name='" + name + ".Value' value='true'"+(model.Required?" selected>":">"));
            }
            else if (model.Datatype == Fieldtype.Datetime)
            {
                builder.AppendLine(DateTimeEditor.DateTimeEditorFor((EditableDateTime)model.Value, name + ".Value",
                                                        DateTimeEditor.DATE_TIME_ALL_FIELDS, "").ToString());
            }
            else
            {
                builder.AppendLine("<span style='color:red'>Error. Could not create input field. Not implemented...</span>");
            }
            return MvcHtmlString.Create(builder.ToString());
        }
    }
}