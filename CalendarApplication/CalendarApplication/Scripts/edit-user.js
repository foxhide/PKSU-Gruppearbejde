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

/* Function for setting status changed */
function statusChanged(id) {
    $("#" + id).removeClass();
    $("#" + id).addClass("yellow_triangle");
}

/* Function for updating name */
function updateName(userId, value) {
    $("#name-input").removeClass();
    $.ajax({
        url: "/Account/EditUserString",
        type: 'POST',
        data: { field: 2, userId: userId, value: value },
        success: function (result) {
            if (result) { $("#name-input").addClass("tick"); }
            else { $("#name-input").addClass("red_cross"); }
        },
        error: function (result) {
            $("#name-input").addClass("red_cross");
        }
    });
}

/* Function for updating email */
function updateEmail(userId, value) {
    $("#email-input").removeClass();
    $.ajax({
        url: "/Account/EditUserString",
        type: 'POST',
        data: { field: 0, userId: userId, value: value },
        success: function (result) {
            if (result) { $("#email-input").addClass("tick"); }
            else { $("#email-input").addClass("red_cross"); }
        },
        error: function (result) {
            $("#email-input").addClass("red_cross");
        }
    });
}

/* Function for updating phone number */
function updatePhone(userId, value) {
    $("#phone-input").removeClass();
    $.ajax({
        url: "/Account/EditUserString",
        type: 'POST',
        data: { field: 1, userId: userId, value: value },
        success: function (result) {
            if (result) { $("#phone-input").addClass("tick"); }
            else { $("#phone-input").addClass("red_cross"); }
        },
        error: function (result) {
            $("#phone-input").addClass("red_cross");
        }
    });
}

/* Function used for editing password in edit profile */
function updatePassword(id) {
    var op = $("#OldPassword").val();
    var np = $("#Password").val();
    var rep = $("#PasswordConfirm").val();
    $("#password-input").removeClass();
    if (np != rep) {
        $("#match_error").html("Confirmation password does not match!");
        $("#PasswordConfirm").addClass("input-validation-error");
        $("#password-input").addClass("red_cross");
        return;
    }
    $("#match_error").html("");
    $("#PasswordConfirm").removeClass("input-validation-error");
    $.ajax({
        url: "/Account/EditUserPassword",
        type: 'POST',
        data: { userId: id, oldPass: op, newPass: np },
        success: function (result) {
            var i = parseInt(result, 10);
            switch (i) {
                case 0:
                    $('#basic_error').html("");
                    $('#old_pass_error').html("");
                    break; // OK
                case -1:
                    $('#basic_error').html("User validation error!");
                    break;
                case -2:
                    $('#old_pass_error').html("Incorrect password!");
                    break;
                case -11:
                    $('#basic_error').html("Some database error occured!");
                    break;
                case -12:
                    $('#basic_error').html("Profile not found!");
                    break;
                case -13:
                    $('#basic_error').html("Could not save new password!");
                    break;
            }
            $("#OldPassword").val("");
            $("#Password").val("");
            $("#PasswordConfirm").val("");
            $("#password-input").addClass(i != 0 ? "red_cross" : "tick");
        }
    });
}