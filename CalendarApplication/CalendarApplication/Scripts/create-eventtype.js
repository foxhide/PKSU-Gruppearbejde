/*
 *  This script is used for the event type creation page. It adds the html for each
 *  field when the user presses the 'Add field'-button by using ajax
 */

// Counter for naming the added fields.
var field_count = 0;

var id2field = [];
var field2id = [];

/* Sets the counter */
function setIdCounter(counter) {
    field_count = counter;
    for (var i = 0; i < field_count; i++) {
        id2field.push(i);
        field2id.push(i);
    }
}

/* Removes field */
function removeField(viewId, mid, datatype) {
    var field_number = id2field[viewId];
    var field = document.getElementById("field_" + field_number);
    field.parentElement.removeChild(field);
    id2field[viewId] = -1;
    field2id[field_number] = -1;
    if (mid != -1) {
        var list = document.getElementById('fields');
        var newField = document.createElement('div');
        var fieldId = 'field_' + field_number;
        newField.id = fieldId;
        newField.innerHTML = "<input type='hidden' id='ViewID_" + viewId + "' name='TypeSpecific[" + viewId + "].ViewID' value='-1' />"
                                + "<input type='hidden' id='id_rem_" + viewId + "' name='TypeSpecific[" + viewId + "].ID' value='" + mid + "'/>"
                                + "<input type='hidden' id='id_rem_" + viewId + "' name='TypeSpecific[" + viewId + "].Datatype' value='" + datatype + "'/>";
        list.appendChild(newField);
    }
}

/* Removes field and sets files for deletion */
function removeField(viewId, mid, datatype, del) {
    var field_number = id2field[viewId];
    var field = document.getElementById("field_" + field_number);
    field.parentElement.removeChild(field);
    id2field[viewId] = -1;
    field2id[field_number] = -1;
    if (mid != -1) {
        var list = document.getElementById('fields');
        var newField = document.createElement('div');
        var fieldId = 'field_' + field_number;
        newField.id = fieldId;
        newField.innerHTML = "<input type='hidden' id='ViewID_" + viewId + "' name='TypeSpecific[" + viewId + "].ViewID' value='-1' />"
                                + "<input type='hidden' id='id_rem_" + viewId + "' name='TypeSpecific[" + viewId + "].ID' value='" + mid + "'/>"
                                + "<input type='hidden' id='id_rem_" + viewId + "' name='TypeSpecific[" + viewId + "].Datatype' value='" + datatype + "'/>"
                                + "<input type='hidden' id='id_rem_" + viewId + "' name='TypeSpecific[" + viewId + "].FileDelete' value='" + del + "'/>";
        list.appendChild(newField);
    }
}

/* Inserts new field */
function newField() {
    var list = document.getElementById('fields');
    var newField = document.createElement('div');
    var fieldId = 'field_' + field_count;
    var field_number = field_count;
    newField.id = fieldId;
    list.appendChild(newField);

    // Get partial view from server, using ajax
    $.ajax({
        url: "/Maintenance/GetPartial/",
        type: 'GET',
        data: { id: field_count },
        dataType: 'html',
        success: function (result) {
            $('#' + fieldId).html(result);
            id2field.push(field_number);
            field2id.push(field_number);
        }
    });
        
    field_count++;
}

/* Move the given field one down */
function moveFieldDown(viewId) {
    var field_number = id2field[viewId];
    if (field_number == field_count - 1) { return; }
    else {
        var below = field_number + 1;
        while (below < field_count && field2id[below] == -1) { below++; }
        if (below == -1) { return; }
        else {
            swapField(field_number, below);
        }
    }
}

/* Move the given field one up */
function moveFieldUp(viewId) {
    var field_number = id2field[viewId];
    if (field_number == 0) { return; }
    else {
        var above = field_number - 1;
        while (above >= 0 && field2id[above] == -1) { above--; }
        if (above == -1) { return; }
        else {
            swapField(field_number, above);
        }
    }
}

/* Swap two fields */
function swapField(field_1, field_2) {
    var id_1 = field2id[field_1];
    var id_2 = field2id[field_2];

    // Update all DOM values, that may have changed (this is not done automatically)
    updateValues(id_1);
    updateValues(id_2);

    // Update the html
    var dom_field_1 = $("#field_" + field_1);
    var dom_field_2 = $("#field_" + field_2);
    var tmpHtml = dom_field_1.html();
    dom_field_1.html(dom_field_2.html());
    dom_field_2.html(tmpHtml);

    // Update the mapping tables
    field2id[field_1] = id_2;
    id2field[id_2] = field_1;
    field2id[field_2] = id_1;
    id2field[id_1] = field_2;

    // Update ViewId for keeping order when posting to server
    $("#ViewID_" + id_1).val(field_2);
    $("#ViewID_" + id_2).val(field_1);
}

/* Update all values in the html, allowing for swapping the fields */
function updateValues(id) {
    // Update name
    var name = document.getElementById("Name_" + id);
    name.setAttribute('value', name.value);

    // Update description
    var desc = document.getElementById("Desc_" + id);
    desc.innerHTML = desc.value;

    // Update required for creation
    var reqc = document.getElementById("Reqc_" + id);
    if (reqc.checked) { reqc.setAttribute('checked', 'checked'); }
    else { reqc.removeAttribute('checked'); }

    // Update required for approval
    var reqa = document.getElementById("Reqa_" + id);
    if (reqa.checked) { reqa.setAttribute('checked', 'checked'); }
    else { reqa.removeAttribute('checked'); }

    // Update varchar input
    var vc = document.getElementById("varchar_size_" + id);
    if (vc != null) { vc.setAttribute('value', vc.value); }

    // Update type
    var type = document.getElementById("Type_" + id);
    if (type != null) {
        // Remove previous selections.
        var selected = type.selectedIndex;
        for (var i = 0; i < type.options.length; i++) {
            type.options[i].removeAttribute('selected');
        }
        type.selectedIndex = selected;
        type.options[type.selectedIndex].setAttribute('selected', 'selected');
    }
}

/* Function used for inserting input field for varchar size */
function updateVarChar(id) {
    var value = document.getElementById('Type_' + id).selectedIndex;
    var label = document.getElementById('varchar_label_' + id);
    var input = document.getElementById('varchar_input_' + id);
    if (value == 1) {
        // It is a text field.
        label.innerHTML = "<label>Max size of input:</label>";
        input.innerHTML = "<input type='text' id='varchar_size_" + id + "' name='TypeSpecific[" + id + "].VarcharLength'"
                            + " style='width:50px' value='250'>";
    }
    else {
        // It is not a text field
        label.innerHTML = "";
        input.innerHTML = "";
    }
}

// Limit the varchar -> a varchar must never be changed to something less than it is now..
function varCharLimit(id, limit) {
    var input = document.getElementById('varchar_size_' + id);
    var value = input.value;
    if (value < limit) {
        input.value = limit;
    }
}

function checkFields(form) {
    var ok = true;
    for (var i = 0; i < field_count; i++) {
        if ($("#ViewID_" + i).attr("id") != -1) {
            if ($("#Name_" + i).attr("value") == "") {
                $("#Name_" + i).addClass("input-validation-error");
                ok = false;
            }
        }
    }
    if (ok) {
        form.submit();
    }
    else {
        $("#event-type-submit-error").show();
    }
}

function updateReqForAppr(id) {
    if ($("#Reqc_" + id).attr("checked")) {
        $("#Reqa_" + id).attr("checked", "checked");
        $("#Reqa_" + id).attr("disabled", true);
    }
    else {
        $("#Reqa_" + id).removeAttr("disabled");
    }
}