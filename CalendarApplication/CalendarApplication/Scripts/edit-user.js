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
        fromList = "UANSelect";
        toList = "UASelect";
    }
    var list = document.getElementById(fromList);
    var id = list.options[list.selectedIndex].value;
    moveList(fromList, toList, id);
/*    $.ajax({
        url: "/Maintenance/EditUser",
        type: 'POST',
        data: { type: type, userId: id },
        success: function () {
            moveList(fromList, toList, id);
        }
    });*/
}

function moveList(list1,list2,id) {
    var option = $("#" + list1 + " option[value='" + id + "']").remove();
    $("#" + list2).append(option);
    var old1 = parseInt(document.getElementById(list1 + "_counter").value,10);
    var old2 = parseInt(document.getElementById(list2 + "_counter").value, 10);
    document.getElementById(list1 + "_counter").value = (old1 - 1);
    document.getElementById(list2 + "_counter").value = (old2 + 1);
}