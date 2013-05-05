/* Function for sending data when updating users from the
   user manage page. */
function sendData(type) {
    var fromList = "";
    var toList = "";
    if (type == "active-add") {
        fromList = "UISelect";
        toList = "UASelect";
    }
    else if (type == "active-rem") {
        fromList = "UASelect";
        toList = "UISelect";
    }
    else if (type == "approval") {
        fromList = "UNASelect";
        toList = "UASelect";
    }
    var list = document.getElementById(fromList);
    var id = list.options[list.selectedIndex].value;
    $.ajax({
        url: "/Maintenance/EditUser",
        type: 'POST',
        data: { type: type, userId: id },
        success: moveList(fromList, toList, id)
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