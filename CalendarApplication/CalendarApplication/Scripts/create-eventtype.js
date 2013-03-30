var field_count = 0;

function setIdCounter(counter) {
    field_count = counter;
}

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

function newField() {

        var list = document.getElementById('fields');
        var newField = document.createElement('div');
        var fieldId = 'field_' + field_count;
        newField.id = fieldId;
        list.appendChild(newField);

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