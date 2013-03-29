var i = 0;

var fields = [];

function addNewField() {
    var list = document.getElementById('fields');
    var newField = document.createElement('div');

    fields.push(true);

    var code = "<fieldset><legend>Field "+i+"</legend><table>" +
                "<tr><td><label for='Name_" + i + "'>Name of field</label></td>" +
                "<td><label for='Description_" + i + "'>Description for field:</label></td>" +
                "<td><label for='Datatype_" + i + "'>Type of field:</label></td>" +
                "<td><label for='Required_" + i + "'>Required:</label></td></tr>" +
                "<tr><td><input type='text' name='Name_" + i + "' id='Name_" + i + "'></td>" +
                "<td><input type='text' name='Description_" + i + "' id='Description_" + i + "'></td>" +
                "<td><input type='text' name = 'Datatype_" + i + "' id='Datatype_" + i + "'></td>" +
                "<td><input type='checkbox' name = 'Required_" + i + "' id='Required_" + i + "'></td></tr>" +
                "</table><button type=\"button\" onclick=\"removeField(" + i + ")\">Remove</button>" +
                "</fieldset>";

    newField.id = 'field_'+i;
    newField.innerHTML = code;
    i++;
    list.appendChild(newField);
}

function removeField(id) {
    var field = document.getElementById("field_" + id);
    field.parentElement.removeChild(field);
    fields[id] = false;
}

function getAll() {
    var result = "";
    for (var i = 0; i < fields.length; i++) {
        if (fields[i]) {
            if (result != "") { result += "|" }
            result += document.getElementById("Name_" + i).value + "::"
                        + document.getElementById("Description_" + i).value + "::"
                        + document.getElementById("Datatype_" + i).value + "::"
                        + document.getElementById("Required_" + i).value;
        }
    }
    alert(result);
}