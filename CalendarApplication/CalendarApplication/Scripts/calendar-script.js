/* Function for switching between pages in lists */
function gotoOtherPage(from) {
    $('#EventFrom').val(from);
    $('#list_form').submit();
}

/* Function for sorting */
function sortBy(eventOrder) {
    var old = $('#Order').val();
    if (old == eventOrder) {
        var desc = $('#Descending').val().toLowerCase() == "true";
        $('#Descending').val(!desc);
    }
    else {
        $('#Descending').val(false);
    }
    $('#Order').val(eventOrder);
    $('#EventFrom').val(0);
    $('#list_form').submit();
}

/* Function for navigating calendar (month and days).
   If date is of format yyyy-MM, the calendar month is
   shown. If it has format yyyy-MM-dd, the calendar day
   is shown. */
function goto(date) {
    var data = date.split("-");
    if (data.length < 2 || data.length > 3) { return; }
    else if (data.length == 2) {
        $("#Date").val(date + "-01 00:00:00");
        $("#Mode").val("MONTH");
    }
    else {
        $("#Date").val(date + " 00:00:00");
        $("#Mode").val("DAY");
    }
    $("#cal_form").submit();
}

/* Functions for showing/hideing event pop-ups in the calendar. */
function showPopup(name) {
    // Show full name
    $('#' + name + '_small').show();
    // Set time
    setTimeout(function () {
        // Show big if small is still visible == cursor is still on event
        if ($('#' + name + '_small').is(":visible")) {
            $('#' + name + '_big').fadeIn();
        }
    }, 700);
}

function hidePopup(name) {
    // Hide both big and small on cursor exit
    $('#' + name + '_small').hide();
}

function hidePopupBig(name) {
    $('#' + name + '_big').hide();
}

/* Function for un/checking all boxes for a given list (Eventtypes or Rooms) */
function changeCheckboxes(list,element) {
    for (var i = 0; $("#Filter_" + list + "_" + i + "__Selected").length > 0
            && $("#Filter_" + list + "_" + i + "__Selected") != null; i++) {
        $("#Filter_" + list + "_" + i + "__Selected").attr("checked", element.checked);
    }
}

/* Function to be run on filter load. Checks the checkAll box, based on the number of checked boxes (Eventtypes and Rooms) */
function checkBoxes() {
    var checked = 0;
    var unchecked = 0;
    for (var i = 0; $("#Filter_Eventtypes_" + i + "__Selected").length > 0 && $("#Filter_Eventtypes_" + i + "__Selected") != null; i++) {
        if ($("#Filter_Eventtypes_" + i + "__Selected").attr("checked")) {
            checked++;
        }
        else {
            unchecked++;
        }
    }
    $("#Eventtypes_checkAll").attr("checked", checked > unchecked);

    checked = 0;
    unchecked = 0;
    for (var i = 0; $("#Filter_Rooms_" + i + "__Selected").length > 0 && $("#Filter_Rooms_" + i + "__Selected") != null; i++) {
        if ($("#Filter_Rooms_" + i + "__Selected").attr("checked")) {
            checked++;
        }
        else {
            unchecked++;
        }
    }
    $("#Rooms_checkAll").attr("checked", checked > unchecked);
}

var roomList = [];
var roomIds = [];
var selection = false
var s_top = 0;
var s_bottom = 0;
var offset = 0;

function addRoom(id) {
    roomList[id] = new Object();
    roomList[id].select = false;
    roomList[id].start = new Array();
    roomList[id].end = new Array();
    roomIds.push(id);
    addSelectionFunction(id);
}

function addEvent(roomId, start, end) {
    var spix = dateStringToPixel(start);
    var epix = dateStringToPixel(end);
    roomList[roomId].start.push(spix);
    roomList[roomId].end.push(epix);
}

function dateStringToPixel(dateStr) {
    var d = new Date(dateStr);
    var t = new Date(date);
    t.setDate(date.getDate() + 1);
    if (d < date) { return 0; }
    else if (d > t) { return 720; }
    return (d - date) / (120000);
}

function addSelectionFunction(roomId) {
    $("#room_" + roomId)
    .unbind("mousemove")
    .unbind("mouseup")
    .unbind("mouseleave")
    .mousedown(function (e) {
        if (!selection) {
            s_top = e.pageY - $("#room_wrap_" + roomId).position().top;
            s_top = s_top > 660 ? 660 : s_top;
            s_bottom = s_top + 60;
            $(this).html($(this).html() + createSelection(roomId, s_top, 60));
            $("#clear_button").removeAttr("disabled");
            selection = true;
            addDrawFunctions(roomId);
        }
        else if (!roomList[roomId].select) {
            $(this).html($(this).html() + createSelection(roomId, s_top, s_bottom - s_top));
            addDrawFunctions(roomId);
        }
        roomList[roomId].select = true;
        checkRoom(roomId);
    });
}

function createSelection(roomId, top, height) {
    var div = "<div id='selection_" + roomId + "' class='day-event selection'";
    div += "style='top:" + top + "px;height:" + height + "px;'>"

    div += "<div class='drawer' id='" + roomId + "_t_drawer' style='top:2px'></div>";
    div += "<div class='remove' onclick='removeSelection(" + roomId + ")'>X</div>"
    div += "<div class='drawer' id='" + roomId + "_b_drawer' style='bottom:2px'></div>";

    div += "</div>";
    return div;
}

function getTime(y) {
    var hour = y / 30;
    hour = (hour - (hour % 1) + 6) % 24;
    var min = (y % 30) * 2;
    min = min - (min % 1);
    return (hour < 10 ? "0" + hour : hour) + ":" + (min < 10 ? "0" + min : min);
}

var flagDown = false;
var flagUp = false;

function addDrawFunctions(roomId) {
    $("#" + roomId + "_b_drawer")
    .mousedown(function (e) {
        flagDown = true;
        $("#time_counter").html(getTime(s_bottom));
        $("#time_counter").show();
    })
    $("#" + roomId + "_t_drawer")
    .mousedown(function (e) {
        flagUp = true;
        $("#time_counter").html(getTime(s_top));
        $("#time_counter").show();
    })
    $("#room_" + roomId)
    .unbind("mousedown")
    .mousemove(function (e) {
        if (flagDown) {
            s_bottom = e.pageY - $("#room_wrap_" + roomId).position().top;
            if (s_bottom < s_top + 15) { s_bottom = s_top + 15; }
            else if (s_bottom > 720) { s_bottom = 720; }
            $(".selection").height(s_bottom - s_top);
            $("#time_counter").html(getTime(s_bottom));
        }
        else if (flagUp) {
            s_top = e.pageY - $("#room_wrap_" + roomId).position().top;
            if(s_top > s_bottom - 15) { s_top = s_bottom - 15; }
            else if (s_top < 0) { s_top = 0; }
            $(".selection").css({ top: s_top + "px" });
            $(".selection").height(s_bottom - s_top);
            $("#time_counter").html(getTime(s_top));
        }
        $("#time_counter").css({ top: (e.pageY - 5) + "px", left: (e.pageX + 10) + "px" });
    })
    .mouseup(function (e) {
        flagDown = false;
        flagUp = false;
        $("#time_counter").hide();
        checkAllRooms();
    })
    .mouseleave(function (e) {
        if (flagUp || flagDown) {
            checkAllRooms();
        }
        flagUp = false;
        flagDown = false;
        $("#time_counter").hide();
    });
}

function removeSelection(roomId) {
    $("#selection_" + roomId).remove();
    roomList[roomId].select = false;
    addSelectionFunction(roomId);
    for (var i = 0; i < roomIds.length; i++) {
        if (roomList[roomIds[i]].select) { return; }
    }
    resetSelection();
}

function removeAllSelections() {
    $(".selection").remove();
    for (var i = 0; i < roomIds.length; i++) {
        roomList[roomIds[i]].select = false;
        addSelectionFunction(roomIds[i]);
    }
    resetSelection();
}

function resetSelection() {
    s_top = 0;
    s_bottom = 0;
    selection = false;
    $("#clear_button").attr("disabled", "disabled");
}

var date = new Date();

function setDate(strDate) {
    date = new Date(strDate);
}

function getDate(y) {
    var tmpDate = date;
    if (y > 720 - 30 * offset) {
        tmpDate.setDate(date.getDate() + 1);
    }
    var result = tmpDate.getFullYear() + "-";
    result += tmpDate.getMonth() < 9 ? "0" + (tmpDate.getMonth() + 1) : (tmpDate.getMonth() + 1);
    result += "-";
    result += tmpDate.getDate() < 10 ? "0" + tmpDate.getDate() : tmpDate.getDate();
    result += "T" + getTime(y);
    return result;
}

function createNewEvent() {
    var url = "/Event/EditEvent?eventId=-1";
    if (selection) {
        url += "&from=" + getDate(s_top);
        url += "&to=" + getDate(s_bottom);
        url += "&rooms=";
        var first = true;
        for (var i = 0; i < roomIds.length; i++) {
            if (roomList[roomIds[i]].select) {
                if (!first) { url += ":"; }
                url += roomIds[i];
                first = false;
            }
        }
    }
    window.location = url;
}

function addEventGoto(id) {
    $("#event_" + id).unbind("mousedown")
    .mousedown(function (e) {
        e.stopImmediatePropagation()
        window.location = "/Event?eventId=" + id;
    });
}

function checkAllRooms() {
    for (var i = 0; i < roomIds.length; i++) {
        if (roomList[roomIds[i]].select) {
            checkRoom(roomIds[i]);
        }
    }
}

function checkRoom(roomId) {
    for (var i = 0; i < roomList[roomId].start.length; i++) {
        var start = roomList[roomId].start[i];
        var end = roomList[roomId].end[i];
        if ((start > s_top && end < s_bottom)
            || (start < s_top && end > s_top)
            || (start < s_bottom && end > s_bottom)) {
            $("#selection_" + roomId).addClass("selection-bad");
            return;
        }
    }
    $("#selection_" + roomId).removeClass("selection-bad");
}