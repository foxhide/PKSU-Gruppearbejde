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

            builder.AppendLine("<label for='"+name+"'>"+model.Name+"</label>");
            builder.AppendLine("<input type='hidden' name='"+name+".Datatype' value='" + model.Datatype + "'>");
            builder.AppendLine("<input type='hidden' name='"+name+".ID' value='" + model.ID + "'>");
            builder.AppendLine("<br>");
            
            if (model.Datatype == Fieldtype.Integer)
            {
                builder.AppendLine("<input type='number' name='" + name + ".IntValue' value='" + model.IntValue + "'>");
            }
            else if (model.Datatype == Fieldtype.Text)
            {
                if (model.VarcharLength < 50)
                {
                    builder.AppendLine("<input type='text' name='" + name + ".StringValue' value='" + model.StringValue + "'>");
                }
                else
                {
                    builder.AppendLine("<textarea type='text' name='" + name + ".StringValue' cols='40' rows='");
                    builder.Append((model.VarcharLength/40+1) + "'>" + model.StringValue + "</textarea>");
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
            else
            {
                builder.AppendLine("<span style='color:red'>Error. Could not create input field. Not implemented...</span>");
            }
            return MvcHtmlString.Create(builder.ToString());
        }
    }
}