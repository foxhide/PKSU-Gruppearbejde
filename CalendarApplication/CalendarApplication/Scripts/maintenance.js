/*
 * This script contains functions for use in the maintenance pages.
 */

/* Function for sending data when updating users from the
   user manage page. */
function sendData(type) {
    var fromList = "";
    var toList = "";
    var field = -1;
    var value = false;
    if (type == "active-add") {
        fromList = "UISelect";
        toList = "UASelect";
        field = 0;
        value = true;
    }
    else if (type == "active-rem") {
        fromList = "UASelect";
        toList = "UISelect";
        field = 0;
    }
    else if (type == "approval") {
        fromList = "UNASelect";
        toList = "UASelect";
        field = 1;
    }
    var list = document.getElementById(fromList);
    var id = list.options[list.selectedIndex].value;
    $.ajax({
        url: "/Account/EditUserBool",
        type: 'POST',
        data: { field: field, userId: id, value: value },
        success: moveList(fromList, toList, id)
    });
}

/* Function for deleting user. A check is made server-side, that the user is not approved! */
function deleteUser() {
    var list = document.getElementById("UNASelect");
    var id = list.options[list.selectedIndex].value;
    $.ajax({
        url: "/Account/DeleteUser",
        type: 'POST',
        data: { userId: id },
        success: removeFromList("UNASelect", id)
    });
}

/* Function for moving users between lists and updating counters */
function moveList(list1, list2, id) {
    var option = $("#" + list1 + " option[value='" + id + "']").remove();
    $("#" + list2).append(option);
    var old1 = parseInt(document.getElementById(list1 + "_counter").value, 10);
    var old2 = parseInt(document.getElementById(list2 + "_counter").value, 10);
    document.getElementById(list1 + "_counter").value = (old1 - 1);
    document.getElementById(list2 + "_counter").value = (old2 + 1);
}

/* Function for removing user, that is not approved */
function removeFromList(list, id) {
    var option = $("#" + list + " option[value='" + id + "']").remove();
    var old = parseInt(document.getElementById(list + "_counter").value, 10);
    document.getElementById(list + "_counter").value = (old - 1);
}

/* Function for showing the user index page */
function showUser(listId) {
    var list = document.getElementById(listId);
    var id = list.options[list.selectedIndex].value;
    window.location.href = "/User/Index?userId=" + id;
}

/* Function for going to the event type edit page */
function editEventType(listId) {
    var list = document.getElementById(listId);
    var id = list.options[list.selectedIndex].value;
    window.location.href = "/Maintenance/EditEventType?eventTypeId=" + id;
}

/* Function for going to the event type edit page with a new event type */
function createEventType() {
    window.location.href = "/Maintenance/EditEventType?eventTypeId=-1";
}

/* Function for setting the active state of the selected event type */
function setEventTypeActive(active) {
    var list1 = active ? "inact" : "act";
    var list2 = active ? "act" : "inact";
    var list = document.getElementById(list1);
    var id = list.options[list.selectedIndex].value;
    $.ajax({
        url: "/Maintenance/SetEventTypeActive",
        type: 'POST',
        data: { eventTypeId: id, active: active },
        success: function (result) {
            if (result) {
                var option = $("#" + list1 + " option[value='" + id + "']").remove();
                $("#" + list2).append(option);
            }
        }
    });
}