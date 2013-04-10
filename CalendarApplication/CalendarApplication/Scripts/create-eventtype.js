var field_count = 0;

/* Sets the counter */
function setIdCounter(counter) {
    field_count = counter;
}

/* Removes field */
function removeField(id,mid) {
    var field = document.getElementById("field_" + id);
    field.parentElement.removeChild(field);
    if (mid != -1) {
        var list = document.getElementById('fields');
        var newField = document.createElement('div');
        var fieldId = 'field_' + id;
        newField.id = fieldId;
        newField.innerHTML = "<input type='hidden' id='viewid_rem_" + id + "' name='TypeSpecific[" + id + "].ViewID' value='-1' />"
                                + "<input type='hidden' id='id_rem_" + id + "' name='TypeSpecific[" + id + "].ID' value='"+mid+"'/>";
        list.appendChild(newField);
    }
}

/* Inserts new field */
function newField() {
    var list = document.getElementById('fields');
    var newField = document.createElement('div');
    var fieldId = 'field_' + field_count;
    newField.id = fieldId;
    list.appendChild(newField);

    // Get partial view from server, using ajax
    $.ajax({
        url: "/EventType/GetPartial/",
        type: 'GET',
        data: { id: field_count },
        dataType: 'html',
        success: function (result) {
            $('#'+fieldId).html(result);
        }
    });
        
    field_count++;
}

/* Function used for inserting input field for varchar size */
function updateVarChar(id) {
    var value = document.getElementById('Type_' + id).selectedIndex;
    var label = document.getElementById('varchar_label_' + id);
    var input = document.getElementById('varchar_input_' + id);
    if (value == 1) {
        // It is a text field.
        label.innerHTML = "<label>Max size of input:</label>";
        input.innerHTML = "<input type='text' id='varchar_size_" + id + "' name='TypeSpecific[" + id + "].VarcharLength'>";
    }
    else {
        // It is not a text field
        label.innerHTML = "";
        input.innerHTML = "";
    }
}

/* Function used for updating the counter for the description text area */
function updateCounter_test(id) {
    var chars = 99 - document.getElementById('Desc_' + id).value.length;
    var label = document.getElementById('desc_count_' + id);
    var content = "Characters left: ";
    if (chars < 0) {
        // If there are too many chars, make a red number
        content += '<span style="color:red">'+chars+'</span>';
    }
    else {
        content += chars;
    }
    label.innerHTML = content;
}