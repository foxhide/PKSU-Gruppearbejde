﻿/*
 *  The tools script includes js-functions used for different
 *  tasks, among other: counting chars in a text field and
 *  for implementing the functionality of double select lists
 */

/* Function used for updating the counter for a text area */
function updateCounter(name, maxSize) {
    var input = document.getElementById(name).value;
    var chars = input.length;
    var label = document.getElementById(name + '_char_counter');
    var content = "Characters left: ";
    if (chars > maxSize) {
        // If there are too many chars, make a red 0 with an error message
        content += '<span style="color:red">0 - too many characters...</span>';
        // Remove chars from the back of the input
        input = input.substring(0, maxSize);
        document.getElementById(name).value = input;
    }
    else {
        // Set the counter
        content += maxSize-chars;
    }
    label.innerHTML = content;
}

/* Function used for selecting/deselecting items in the double select list */
function moveSelected(name, add) {
    // Get lists, depending on add / rem (!add)
    var name1 = add ? name + '_available' : name + '_select';
    var name2 = add ? name + '_select' : name + '_available';
    var list1 = document.getElementById(name1);
    var list2 = document.getElementById(name2);
    // Run through the list until selected option is found. Remove it
    // and add it to the other list.
    for (var i = 0; i < list1.length; i++) {
        if(list1.options[i].selected) {
            var id = list1.options[i].value;
            var roomName = list1.options[i].innerHTML;
            list1.remove(i);
            list2.options[list2.length] = new Option(roomName,id);
            document.getElementById(name + '_' + id).value = add;
            break;
         }
    }
}

/* Function for disableing double lists */
function disableLists(listName, disable) {
    // simply disable/enable lists and buttons
    document.getElementById(listName + "_available").disabled = disable;
    document.getElementById(listName + "_select").disabled = disable;
    document.getElementById(listName + "_add_button").disabled = disable;
    document.getElementById(listName + "_rem_button").disabled = disable;
}

/* Get a list of SelectListItem for all selected items in a double list */
function getListSelected(listName) {
    var list = [];
    var i = 1;
    var text = $('#' + listName + '_0_Text');
    var value = $('#' + listName + '_0_Value');
    var select = $('#' + listName + '_' + value.val());

    while (value.length > 0) {
        if (select.val().toLowerCase() == "true") {
            list.push({
                Text: text.val(),
                Value: value.val(),
                Selected: true
            });
        }

        text = $('#' + listName + '_' + i + '_Text');
        value = $('#' + listName + '_' + i + '_Value');
        select = $('#' + listName + '_' + value.val());
        i++;
    }

    return list;
}

/* Function used for editing a user string or bool value, used in several pages */
function updateUser(field, id, value) {
    var url = "/Account/EditUser";
    if (typeof value === "string") { url += "String"; }
    else { url += "Bool"; }
    $.ajax({
        url: url,
        type: 'POST',
        data: { field: field, userId: id, value: value }
    });
}

/* Resets the user password (only allowed if admin) */
function resetPassword(id) {
    $.ajax({
        url: "/Account/PasswordReset",
        type: 'POST',
        data: { userId: id },
        dataType: 'html',
        success: function (result) {
            if (result && result != "null") {
                result = "New password: " + result;
                $("#new_pass").html(result);
            }
            else {
                $("#new_pass").html("An error occurred, could not reset password!");
            }
            $("#new_pass").show();
        }
    });
}

/* Redirect to the given url */
function gotoUrl(url) {
    window.location = url;
}