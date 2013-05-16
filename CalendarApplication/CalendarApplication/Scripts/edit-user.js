/* Function for sending data when updating users from the
   user manage page. */
function sendData(type) {
    var fromList = "";
    var toList = "";
    var field = "";
    var value = false;
    if (type == "active-add") {
        fromList = "UISelect";
        toList = "UASelect";
        field = "active";
        value = true;
    }
    else if (type == "active-rem") {
        fromList = "UASelect";
        toList = "UISelect";
        field = "active";
    }
    else if (type == "approval") {
        fromList = "UNASelect";
        toList = "UASelect";
        field = "needsApproval";
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
function moveList(list1,list2,id) {
    var option = $("#" + list1 + " option[value='" + id + "']").remove();
    $("#" + list2).append(option);
    var old1 = parseInt(document.getElementById(list1 + "_counter").value,10);
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

function showUser(list) {
    var list = document.getElementById(list);
    var id = list.options[list.selectedIndex].value;
    window.location.href = "/User/Index?userId=" + id;
}