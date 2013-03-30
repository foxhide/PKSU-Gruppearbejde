var field_count = 0;

function setIdCounter(counter) {
    field_count = counter;
}

function removeField(id) {
    var field = document.getElementById("field_" + id);
    field.parentElement.removeChild(field);
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
            dataType: 'html', // <-- to expect an html response
            success: function (result) {
                $('#'+fieldId).html(result);
            }
        });
        
        field_count++;
}