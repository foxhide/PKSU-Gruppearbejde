/*
  This js-script contains the js-functions needed for event create/edit.
 */

var numberOfFields = 0;

/* Get the partial view for the given event type. If type == 0 (non selected),
   the event_specifics div will be set empty */
function getSpecifics(type) {
    if (type != 0) {
        $.ajax({
            url: "/Event/GetEventSpecific/",
            type: 'GET',
            data: { type: type },
            dataType: 'html',
            success: function (result) {
                $('#event_specifics').html(result);
            }
        });
    }
    else {
        $('#event_specifics').html("");
        numberOfFields = 0;
    }
}

/* Sets this event as approved. Updates the state by calling set state. */
function setApproved() {
    var approved = $("#Approved").val().toLowerCase() == "false" ? true : false;
    $("#Approved").val(approved);
    $("#approve_button").val(!approved ? "Approve" : "Disapprove");
    setState();
}

/* Sets the state of the event. This is called when approval is set, or when a nullable
   field is set (so far: User, Group and Text). */
function setState() {
    var approved = $("#Approved").val().toLowerCase() == "true" ? true : false;
    if (!approved) {
        $("#State").val(0);
        $("#state_text").html("Incomplete");
        $("#state_text").css("color", "red");
    }
    else {
        /* Note: RequiredApprove should be set whenever RequiredCreate is set (so we avoid to run both checks) */
        if (!checkApprove()) {
            $("#State").val(1);
            $("#state_text").html("Approved");
            $("#state_text").css("color", "yellow");
        }
        else {
            $("#State").val(2);
            $("#state_text").html("Finished");
            $("#state_text").css("color", "#40FF00");
        }
    }
}

/* Checks all the nullable type specific fields to see if they are required
   for approval and if they have been set.
 */
function checkApprove() {
    if ($("#Name").val() == "") { return false; }
    for (var i = 0; i < numberOfFields; i++) {
        var id = "#TypeSpecifics\\[" + i + "\\]";
        var req = $(id + "_RequiredApprove").val();
        if (req.toLowerCase() == "true") {
            var dataType = $(id + "_Datatype").val();
            if (dataType == "User" || dataType == "Group") {
                if ($(id).val() == "0") { return false; }
            }
            else if (dataType == "Text") {
                if ($(id).val() == "") { return false; }
            }
        }
    }
    return true;
}

/* Check if all fields required for creation are filled out. */
function checkCreate() {
    var ok = true;
    if ($("#Name").val() == "") {
        ok = false;
        $("#Name").addClass("input-validation-error");
        $("#Name_Error").html("The event must have a Name!");
    }
    for (var i = 0; i < numberOfFields; i++) {
        var id = "#TypeSpecifics\\[" + i + "\\]";
        var req = $(id + "_RequiredCreate").val();
        if (req.toLowerCase() == "true") {
            var dataType = $(id + "_Datatype").val();
            if (((dataType == "User" || dataType == "Group") && $(id).val() == "0")
                || (dataType == "Text" && $(id).val() == "")) {

                ok = false;
                var name = $(id + "_Name").val();
                $(id).addClass("input-validation-error");
                $(id + "_Error").html("The field " + name + " must be filled!");
            }
        }
    }
    return ok;
}

/* Is called on change - removes error messages, if any. */
function updateSelf(id,type) {

    id = "#" + id;

    // Get datatype and input elements
    var input = $(id);
    if (((type == "User" || type == "Group") && input.val() != "0")
                || (type == "Text" && input.val() != "")) {
        input.removeClass("input-validation-error");
        $(id + "_Error").html("");
    }
}