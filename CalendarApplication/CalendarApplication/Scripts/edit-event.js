/*
  This js-script contains the js-functions needed for event create/edit.
 */

// Has the popup appeared?
var popup = false;

/* Function for showing popup, if not disabled, or getting type specifics if disabled */
function typeCheck(type) {
    if (popup) {
        getSpecifics(type);
    }
    else {
        $("#type_warning").show();
    }
}

/* Response from type warning. Hides the popup and disableds the popup on true */
function typeCheckResponse(response,old) {
    $("#type_warning").hide();
    popup = response;
    if (!response) {
        $("#SelectedEventType").val(old);
    }
    else {
        getSpecifics($("#SelectedEventType").val());
    }
}

// Number of fields variable
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
        numberOfFields = 0;
        $('#event_specifics').html("");
        setState();
        validateInput('SelectedEventType', 'Type', true, true);
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
    if (!validateInput("Name", "Text", false, false)
        || !validateInput("SelectedEventType", "Type", false, false)
        || !validateInput("RoomSelectList", "List", false, false)) {
        return false;
    }

    for (var i = 0; i < numberOfFields; i++) {
        var id = "TypeSpecifics_" + i;
        var jid = "#" + id;
        var req = $(jid + "_RequiredApprove").val();
        if (req.toLowerCase() == "true") {
            var dataType = $(jid + "_Datatype").val();
            dataType = dataType.substring(dataType.length - 4) == "List" ? "List" : dataType;
            if (!validateInput(id, dataType, false, false)) { return false; }
        }
    }
    return true;
}

/* Check if all fields required for creation are filled out. */
function checkCreate() {
    var ok = validateInput("Name", "Text",true, false);
    ok = validateInput("SelectedEventType", "Type",true, false) && ok;
    ok = validateInput("RoomSelectList", "List", true, false) && ok;

    for (var i = 0; i < numberOfFields; i++) {
        var id = "TypeSpecifics_" + i;
        var jid = "#" + id;
        var req = $(jid + "_RequiredCreate").val();
        if (req.toLowerCase() == "true") {
            var dataType = $(jid + "_Datatype").val();
            dataType = dataType.substring(dataType.length - 4) == "List" ? "List" : dataType;
            ok = validateInput(id, dataType, true, false) && ok;
        }
    }
    return ok;
}

/* Validate the input - returns true if valid, false if invalid
   id is the HTML id of the input field, type is the data type
   (e.g. 'List', 'Text',...), if showError is set, the validation
   message is shown, and if removeOnly is set, this function simply
   removes the validation error for the given field */
function validateInput(id,type,showError,removeOnly) {
    var jid = "#" + id;
    if (type == "User" || type == "Group" || type == "Type") {
        var result = $(jid).val() == "0";
        if (showError) { setValidationMessage(jid, jid + "_Error", result && !removeOnly); }
        return !result;
    }
    else if (type == "Text") {
        var result = $(jid).val() == "";
        if (showError) { setValidationMessage(jid, jid + "_Error", result && !removeOnly); }
        return !result;
    }
    else if (type == "List") {
        var result = getListSelected(id).length == 0;
        if (showError) { setValidationMessage(jid + "_select", jid + "_Error", result && !removeOnly); }
        return !result;
    }
    return true;
}

/* Show/hide the error message for the object
   id = HTML id of object, errId = HTML id of error div,
   isError = if set, displays the error, else it hides it */
function setValidationMessage(id, errId, isError) {
    if (isError) {
        $(id).addClass("input-validation-error");
        $(errId).show();
    }
    else {
        $(id).removeClass("input-validation-error");
        $(errId).hide();
    }
}

/* Check if the rooms are free at the time specified (using AJAX) */
function checkRooms() {
    var rooms = getListSelected("RoomSelectList");

    var eventId = $("#ID").val();
    var start = $("#Start").val();
    var end = $("#End").val();

    var jsonText = JSON.stringify({ eventId: eventId, start: start, end: end, rooms: rooms });

    $.ajax({
        type: "POST",
        url: "/Event/CheckDatesJson",
        data: jsonText,
        contentType: "application/json; charset=utf-8",
        success: function (result) {
            var list = JSON.parse(result);
            if (list.length == 0) {
                $("#rd_but").css("background-color", "lightgreen");
                $("#rd_but").val("OK!");
                $("#room_date_feedback").html("");
            }
            else {
                var html = "";
                for (var i = 0; i < list.length; i++) {
                    html += "The room " + list[i].Name
                            + " is occupied in the period from " + list[i].Start
                            + " to " + list[i].End + "<br>";
                }
                $("#rd_but").css("background-color", "red");
                $("#rd_but").val("Error!");
                $("#room_date_feedback").html(html);
            }
        }
    });
}

/* Update the date-room-check button to show check message */
function dateRoomUpdate() {
    $("#rd_but").css("background-color", "yellow");
    $("#rd_but").val("Check rooms and dates");
}